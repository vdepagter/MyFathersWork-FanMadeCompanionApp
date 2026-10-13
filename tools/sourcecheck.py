"""Compares the decompiled Cradle story (.txt) with the web app CSVs to catch playtest findings that tend to recur
(see PLAYTEST_FINDINGS.md):
  1. link labels from the source that do not appear in any CSV (truncated or rewritten choices, e.g. a "South" link
     that lost "back to the town. Both players gain 2 ..."); labels built as "text " + $var are checked up to the variable,
  2. passages that call a Unity-only screen (ViewEndOfGeneration.S_OnEndOfGeneration, S_OnSetSpecialSetup, ...): the
     official app shows that text as its own page, so the web app needs a separate popup/window for it.
Usage: python tools/sourcecheck.py [TheCostofDisease|FearOfTheUnknown|ATimeOfWar] [CSV prefix, default TheCostOfDisease]"""
import html, os, re, sys

HERE = os.path.dirname(os.path.abspath(__file__))
SOURCE = os.path.join(HERE, '..', 'MyFathersWorkConsole', 'MyFathersWorkConsole', 'Processor', (sys.argv[1] if len(sys.argv) > 1 else 'TheCostofDisease') + '.txt')
LOC = os.path.join(HERE, '..', 'MyFathersWorkWebApp', 'MyFathersWorkWebApp', 'wwwroot', 'localization')
PREFIX = sys.argv[2] if len(sys.argv) > 2 else 'TheCostOfDisease'
CSVS = [PREFIX + '_Localization.csv', PREFIX + '_Gameplay_Localization.csv', 'UI_Localization.csv']

src = open(SOURCE, encoding='utf-8-sig').read()
csv = ''.join(open(os.path.join(LOC, f), encoding='utf-8-sig').read() for f in CSVS if os.path.exists(os.path.join(LOC, f)))


def norm(text):
    text = re.sub(r'<[^>]+>', ' ', html.unescape(text))
    return re.sub(r'\s+', ' ', text).strip().lower()


csv_text = norm(csv)
names = {m.group(2): m.group(1) for m in re.finditer(r'Passages\["([^"]+)"\] = new StoryPassage\(.*?this\.(passage\d+)_Main', src, re.S)}

missing, hooks, seen = [], [], set()
passage, last_string = None, ''

for line in src.split('\n'):
    m = re.match(r'private IEnumerable<StoryOutput> (passage\d+)_', line)
    if m: passage = names.get(m.group(1), m.group(1))

    m = re.search(r'string (?:arg|text)\w* = (.*);', line)
    if m: last_string = m.group(1)

    m = re.search(r'\b(s_On\w+)\(', line)
    if m: hooks.append((passage, m.group(1), last_string))

    m = re.search(r'base\.link\("((?:[^"\\]|\\.)*)"(\s*\+)?', line)
    if not m: continue
    label = m.group(1).replace('\\"', '"')
    if len(label) < 4 or label.lower().startswith(('click', 'once you')): continue
    key = norm(label)
    if key in csv_text or (passage, key) in seen: continue
    seen.add((passage, key))
    missing.append((passage, label + (' + $var' if m.group(2) else '')))

print(f'== Link labels not found in {", ".join(CSVS)} ({len(missing)})')
for passage, label in missing: print(f'  {passage}: {label[:110]}')
print(f'\n== Unity-only screens - need their own popup/window in the web app ({len(hooks)})')
for passage, hook, text in hooks: print(f'  {passage}: {hook} - {text[:90]}')
