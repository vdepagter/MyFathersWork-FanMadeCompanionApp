"""Static check: derives every scenario localization tag the C# code requests (CallerMemberName conventions) and reports tags
missing from the scenario CSVs. Dynamic tags (computed indices) are listed for manual checking.
Usage: python tools/tagcheck.py [file-filter ...] [--unused]"""
import re, glob, sys

import os
ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'MyFathersWorkWebApp', 'MyFathersWorkWebApp') + '/'
files = sorted(glob.glob(ROOT + 'Shared/Scenarios/TheCostOfDisease/*.cs'))
args=[a for a in sys.argv[1:] if not a.startswith("--")]
if args: files = [f for f in files if any(a in f for a in args)]

tags = set()
for f in ['TheCostOfDisease_Localization.csv', 'TheCostOfDisease_Gameplay_Localization.csv']:
    for l in open(ROOT + 'wwwroot/localization/' + f, encoding='utf-8-sig').read().splitlines():
        if l and not l.startswith('//'): tags.add(l.split(';')[0])

allsrc = ''.join(open(f, encoding='utf-8-sig').read() for f in glob.glob(ROOT + 'Shared/Scenarios/TheCostOfDisease/*.cs'))
consts = dict(re.findall(r'const string (\w+)\s*=\s*"([^"]*)"', allsrc))

need = {}   # tag -> where
dynamic = []
used_tags = set()

def add(tag, where):
    need.setdefault(tag, where)

for f in files:
    src = open(f, encoding='utf-8-sig').read()
    # split into methods
    for m in re.finditer(r'\n    (?:public|private) static (?:void|string|bool|int|Faction|string\[\]) (\w+)\(([^)]*)\)\s*(=>[^\n]*\n|\n    \{\n(.*?)\n    \})', src, re.S):
        name, body = m.group(1), m.group(3)
        where = f.split('/')[-1] + ':' + name
        if 'new GameplayHub' in body:
            add(name + '_Title', where) if 'SetDefaultTitle' in body else None
            secs = dict(re.findall(r'const string\s+(\w+)\s*=\s*"([^"]*)"', body))
            for var in re.findall(r'AddSection\((\w+)', body):
                if var in secs and secs[var]: add(f'{name}_{secs[var]}_Title', where)
            for var in re.findall(r'\.AddDefaultContent\((\w+)', body):
                add(f'{name}_{secs[var]}_Content', where)
            for n, var in re.findall(r'\.AddNextContent\((\d+),\s*(\w+)', body):
                add(f'{name}_{secs[var]}_Content{n}', where)
            for var in re.findall(r'\.AddSpecialClickHere\((\w+)', body):
                add(f'{name}_{secs[var]}_ClickHere', where)
            continue
        if re.search(r'\.AddDefaultTitle\(', body): add(name + '_Title', where)
        if re.search(r'\.AddDefaultBaseTitle\(', body): add(name.split('_')[0] + '_Title', where)
        if re.search(r'\.AddDefaultSubtitle\(', body): add(name + '_SubTitle', where)
        if re.search(r'ActiveWindow\.AddDefaultContent\(', body): add(name + '_Content', where)
        for arg in re.findall(r'ActiveWindow\.AddNextContent(?:WithLinks)?\(([^,)]+)', body):
            arg = arg.strip()
            if arg.isdigit(): add(f'{name}_Content{arg}', where)
            else: dynamic.append((where, arg))
        for call in re.findall(r'new GameplayPopup\((.*?)\);', body, re.S):
            lit = re.findall(r'"([^"]*)"', call)
            if 'string.Empty' in call: continue
            if lit: [add(x, where) for x in lit]
            elif re.search(r'GlobalTags\.', call.split('PopUpButton')[1] if 'PopUpButton' in call else ''): continue
            else: add(name + '_Content', where)
        for call in re.findall(r'new GameplayInputPopup\(globalData,\s*"([^"]*)"(.*?)\);', body, re.S):
            add(name + '_Content', where)
            if re.search(r',\s*true\s*(,|$)', call[1].strip()) or call[1].rstrip().endswith('true'): add(call[0], where)
        for x in re.findall(r'GetScenarioLocalizedTag\("([^"]+)"\)', body): add(x, where)
        for x in re.findall(r'GetScenarioLocalizedTag\((_[A-Z0-9_]+)\)', body): add(consts.get(x, x), where)
        for x, suffix in re.findall(r'GetScenarioLocalizedTag\((_[A-Z0-9_]+) \+ "(\w+)"\)', body): add(consts.get(x, x) + suffix, where)

missing = sorted(t for t in need if t not in tags)
print('REQUIRED', len(need), 'MISSING', len(missing))
for t in missing: print('  MISSING', t, '<-', need[t])
print('DYNAMIC (check manually):')
for w, a in dynamic: print('  ', w, a)
# unused tags heuristic
if '--unused' in sys.argv:
    prefixes = set()
    unused = sorted(t for t in tags if t not in need)
    print('NOT DIRECTLY REFERENCED', len(unused))
    for t in unused: print('  ', t)
