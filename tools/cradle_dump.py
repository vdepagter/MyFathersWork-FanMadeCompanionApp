"""Convert a decompiled Cradle story (MyFathersWorkConsole/.../Processor/*.txt) into compact, readable pseudo-Twine.

Usage:
  python tools/cradle_dump.py FearOfTheUnknown.txt > passages.txt          (all passages)
  python tools/cradle_dump.py FearOfTheUnknown.txt Mania S4Kill1           (only these passages)
  python tools/cradle_dump.py FearOfTheUnknown.txt --graph FearoftheUnknownStart   (BFS order from a passage)
  python tools/cradle_dump.py FearOfTheUnknown.txt --html Mania     (text as CSV-ready HTML: <b>, <i>, <br />)

Output notation:
  **bold**  _italic_  [style: ...]  (setupStyle / hubTitle / hook blocks)
  [[text -> Passage]]               link to a passage
  [[text => #Fragment]]             link replacing a hook with a fragment (printed below the passage as #Fragment)
  (goto: X) / (display: X)          base.abort / base.passage
  SETUP_IMAGE=Name                  $_SetupImage (the popup icon)
  SETUP_NEXT=...                    ViewItemObtain.SetupPassagename (passage opened when the setup popup is closed)
  NEXT_ROUND(hub -> passage)        PassageTracker.CheckProgress
"""
import re
import sys
import os
from collections import OrderedDict, deque

D = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'MyFathersWorkConsole', 'MyFathersWorkConsole', 'Processor')


def load(source):
    return open(os.path.join(D, source), encoding='utf-8-sig').read()


def parse(txt):
    # passage inits: name, tags, main method
    passages = OrderedDict()
    for m in re.finditer(r'base\.Passages\["((?:[^"\\]|\\.)*)"\] = new StoryPassage\("(?:[^"\\]|\\.)*", (new string\[0\]|new string\[\]\s*\{([^}]*)\}), new Func<IEnumerable<StoryOutput>>\(this\.(\w+)\)\);', txt):
        tags = re.findall(r'"([^"]*)"', m.group(3) or '')
        passages[m.group(1)] = {'tags': tags, 'main': m.group(4)}
    # method bodies
    methods = {}
    for m in re.finditer(r'\nprivate IEnumerable<StoryOutput> (\w+)\(\)\n\{\n(.*?)\n\}\n', txt, re.S):
        methods[m.group(1)] = m.group(2)
    return passages, methods


def unescape(s):
    return re.sub(r'\\(.)', lambda m: {'n': '\n', 't': '\t'}.get(m.group(1), m.group(1)), s)


def expr(e):
    e = e.strip()
    e = re.sub(r'this\.macros1\.(\w+)\(new StoryVar\[\]\s*\{(.*?)\}\)', lambda m: '%s(%s)' % (m.group(1), ', '.join(x.strip() for x in m.group(2).split(',\n'))), e, flags=re.S)
    e = e.replace('this.Vars.', '$')
    e = re.sub(r'\s*\n\s*', ' ', e)
    return e


def collapse(body):
    # collapse multi-line array initializers into one line
    body = re.sub(r'new StoryVar\[\]\s*\{\s*(.*?)\s*\}', lambda m: 'new StoryVar[]{' + re.sub(r'\s*\n\s*', ' ', m.group(1)) + '}', body, flags=re.S)
    return body


HTML = False


def render(body):
    body = collapse(body)
    B, I = ('<b>', '</b>'), ('<i>', '</i>')
    out = []
    line = ''
    indent = 0
    style_stack = []  # per brace depth: style name or None
    pending_style = None
    frags = []

    def flush():
        nonlocal line
        if line.strip():
            out.append('  ' * indent + line.strip())
        line = ''

    for raw in body.split('\n'):
        s = raw.strip()
        if not s or s == 'yield break;' or re.match(r'(StyleScope )?styleScope\d* = null;', s):
            continue
        m = re.match(r'using \(base\.styleScope\("(\w+)", (?:true|"(\w+)")\)\)', s)
        if m:
            pending_style = m.group(1)
            continue
        if s == '{':
            if pending_style:
                st = pending_style
                pending_style = None
                if st == 'bold':
                    line += B[0] if HTML else '**'
                elif st == 'italic':
                    line += I[0] if HTML else '_'
                else:
                    flush()
                    out.append('  ' * indent + '[%s:' % st)
                    indent += 1
                style_stack.append(st)
            else:
                flush()
                indent += 1
                style_stack.append(None)
            continue
        if s == '}':
            st = style_stack.pop() if style_stack else None
            if st == 'bold':
                line += B[1] if HTML else '**'
            elif st == 'italic':
                line += I[1] if HTML else '_'
            else:
                flush()
                indent -= 1
                if st:
                    out.append('  ' * indent + ']')
                else:
                    out.append('  ' * indent + '}')
            continue
        m = re.match(r'yield return base\.text\((.*)\);$', s)
        if m:
            a = m.group(1)
            if re.fullmatch(r'"(?:[^"\\]|\\.)*"', a):
                t = unescape(a[1:-1])
                t = re.sub(r'<sprite="(\w+)" index=0>', r'<icon=\1>', t)
                line += t
            else:
                line += '{' + expr(a) + '}'
            continue
        if s == 'yield return base.lineBreak();':
            line += '<br />' if HTML else ' <br>'
            continue
        m = re.match(r'yield return base\.link\("((?:[^"\\]|\\.)*)", "((?:[^"\\]|\\.)*)", null\);', s)
        if m:
            line += '[[%s -> %s]]' % (unescape(m.group(1)), m.group(2))
            continue
        m = re.match(r'yield return base\.link\((.*?), null, \(\) => base\.enchantHook\("(\w+)", HarloweEnchantCommand\.(\w+), new Func<IEnumerable<StoryOutput>>\(this\.(\w+)\), false\)\);', s)
        if m:
            label = m.group(1)
            label = unescape(label[1:-1]) if label.startswith('"') else '{' + expr(label) + '}'
            line += '[[%s => #%s]]' % (label, m.group(4))
            frags.append(m.group(4))
            continue
        m = re.match(r'yield return base\.enchant\((.*?), HarloweEnchantCommand\.(\w+), new Func<IEnumerable<StoryOutput>>\(this\.(\w+)\)\);', s)
        if m:
            flush()
            out.append('  ' * indent + '(enchant %s %s #%s)' % (m.group(1), m.group(2), m.group(3)))
            frags.append(m.group(3))
            continue
        m = re.match(r'yield return base\.abort\((?:goToPassage: )?"((?:[^"\\]|\\.)*)"\);', s)
        if m:
            flush()
            out.append('  ' * indent + '(goto: %s)' % m.group(1))
            continue
        m = re.match(r'yield return base\.passage\((.*?), (?:Array\.Empty<StoryVar>\(\)|new StoryVar\[0\])\);', s)
        if m:
            flush()
            out.append('  ' * indent + '(display: %s)' % expr(m.group(1)))
            continue
        m = re.match(r'this\.Vars\._SetupImage = "(\w+)";', s)
        if m:
            line += ' SETUP_IMAGE=%s ' % m.group(1)
            continue
        m = re.match(r'ViewItemObtain\.SetupPassagename = (.*);', s)
        if m:
            flush()
            out.append('  ' * indent + 'SETUP_NEXT=' + expr(m.group(1)))
            continue
        m = re.match(r'PassageTracker\.instance\.CheckProgress\("(.*?)", "(.*?)"\);', s)
        if m:
            flush()
            out.append('  ' * indent + 'NEXT_ROUND(%s -> %s)' % (m.group(1), m.group(2)))
            continue
        # anything else: statement / control flow
        flush()
        out.append('  ' * indent + expr(s))
    flush()
    text = '\n'.join(out)
    if not HTML:
        text = re.sub(r' ?<br>\s*<br>', '\n\n', text)
    text = re.sub(r'\*\*\s*\*\*', '', text)
    text = re.sub(r'\n{3,}', '\n\n', text)
    return text, frags


def dump(name, passages, methods):
    p = passages[name]
    res = ['===== %s %s(%s)' % (name, ('[' + ' '.join(p['tags']) + '] ') if p['tags'] else '', p['main'])]
    body, frags = render(methods[p['main']])
    res.append(body)
    done = set()
    while frags:
        f = frags.pop(0)
        if f in done or f not in methods:
            continue
        done.add(f)
        b, more = render(methods[f])
        res.append('  --- #%s' % f)
        res.append('\n'.join('    ' + x for x in b.split('\n')))
        frags.extend(more)
    return '\n'.join(res)


def targets(name, passages, methods):
    p = passages[name]
    names = set()
    todo = [p['main']]
    seen = set()
    while todo:
        mth = todo.pop()
        if mth in seen or mth not in methods:
            continue
        seen.add(mth)
        b = methods[mth]
        names.update(re.findall(r'base\.link\("(?:[^"\\]|\\.)*", "((?:[^"\\]|\\.)*)"', b))
        names.update(re.findall(r'base\.abort\((?:goToPassage: )?"((?:[^"\\]|\\.)*)"', b))
        names.update(re.findall(r'base\.passage\("((?:[^"\\]|\\.)*)"', b))
        names.update(re.findall(r'SetupPassagename = .*', b) and re.findall(r'"(\w+)"', ' '.join(re.findall(r'SetupPassagename = (.*);', b))) or [])
        names.update(re.findall(r'CheckProgress\("[^"]*", "([^"]*)"', b))
        # string literals that match passage names (e.g. $nextpsg = either("X", "Y"), OnShowBidding("X", ...))
        for lit in re.findall(r'"([^"]*)"', b):
            if lit in passages:
                names.add(lit)
        todo.extend(re.findall(r'this\.(passage\d+_Fragment_\d+)', b))
    return [n for n in names if n in passages]


def main():
    source = sys.argv[1]
    passages, methods = parse(load(source))
    args = sys.argv[2:]
    global HTML
    if '--html' in args:
        HTML = True
        args.remove('--html')
    if args and args[0] == '--graph':
        start = args[1:]
        order = []
        seen = set(start)
        q = deque(start)
        while q:
            n = q.popleft()
            order.append(n)
            for t in sorted(targets(n, passages, methods)):
                if t not in seen:
                    seen.add(t)
                    q.append(t)
        for n in order:
            print(n, '->', ', '.join(sorted(targets(n, passages, methods))))
        return
    names = args or list(passages)
    for n in names:
        print(dump(n, passages, methods))
        print()


if __name__ == '__main__':
    sys.stdout.reconfigure(encoding='utf-8')
    main()
