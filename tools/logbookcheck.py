"""Check the Log Book CSVs (wwwroot/localization/<Scenario>_LogBook.csv): every screen exists and calls SaveToUndo,
titles are unique, and a screen listed under several entries has each entry title in its _Title tag."""
import re,glob,sys
from collections import Counter,defaultdict
for sc in ['TheCostOfDisease','FearOfTheUnknown','ATimeOfWar']:
    try: lines=open(f'MyFathersWorkApp/MyFathersWorkApp/wwwroot/localization/{sc}_LogBook.csv',encoding='utf-8').read().split('\n')
    except FileNotFoundError: continue
    code=''.join(open(f,encoding='utf-8-sig').read() for f in glob.glob(f'MyFathersWorkApp/MyFathersWorkApp/Shared/Scenarios/{sc}/*.cs'))
    bodies=dict(re.findall(r'static void (\w+)\(GlobalData globalData\)\s*(\{.*?\n    \})',code,re.S))
    tags={}
    for line in open(f'MyFathersWorkApp/MyFathersWorkApp/wwwroot/localization/{sc}_Localization.csv',encoding='utf-8-sig'):
        if ';' in line: t,v=line.rstrip('\n').split(';',1); tags[t]=v
    titles=Counter(); per=defaultdict(list); n=0
    for l in lines:
        if not l or l.startswith('//') or l.startswith('Screens;'): continue
        p=l.split(';')
        if len(p)!=4: print(sc,'BAD COLS',l[:80])
        n+=1; titles[p[1]]+=1
        for m in p[0].split():
            per[m].append(p[1])
            if 'SaveToUndo' not in bodies.get(m,''): print(sc,'NO SCREEN',m)
    for t,c in titles.items():
        if c>1: print(sc,'DUP TITLE',t)
    for m,ts in per.items():
        if len(ts)>1:
            tt=tags.get(m+'_Title','')
            for t in ts:
                if t.lower() not in tt.lower(): print(sc,'MULTI not in title',m,t)
    print(sc,n,'entries',len(per),'screens')
