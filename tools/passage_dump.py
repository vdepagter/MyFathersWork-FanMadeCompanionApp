"""Dump console-version passages (MyFathersWorkConsole/.../Processor/Scenario/ScenarioPart_*.cs) in a compact, readable form.
OPT("X", MethodX) link labels lost by the console conversion are restored from the decompiled source (LINK(original text -> Target)).
Usage: python tools/passage_dump.py GloomyHunterIntro EvilConsequences   (method names without the "Method" prefix)
Set MFW_SOURCE=FearOfTheUnknown.txt / ATimeOfWar.txt when working on the other scenarios."""
import re,glob,sys
import os
D=os.path.join(os.path.dirname(os.path.abspath(__file__)),'..','MyFathersWorkConsole','MyFathersWorkConsole','Processor')+'/'
SOURCE=os.environ.get('MFW_SOURCE','TheCostofDisease.txt')
src="".join(open(f,encoding='utf-8-sig').read() for f in sorted(glob.glob(D+'Scenario/ScenarioPart_*.cs')))
txt=open(D+SOURCE,encoding='utf-8-sig').read()
links={}
for t,g in re.findall(r'base\.link\("((?:[^"\\]|\\.)*)", "((?:[^"\\]|\\.)*)"',txt):
    links.setdefault(re.sub(r'[^a-zA-Z0-9_]','',g),set()).add(t)
parts=re.split(r'\n    private static void (Method\w+)\(\)\n',src)
m={parts[i]:parts[i+1] for i in range(1,len(parts),2)}
def fix(mo):
    lab,tgt=mo.group(1),mo.group(2)
    if 'Method'+lab==tgt: return 'LINK(%s -> %s)'%(' || '.join(sorted(links.get(lab,{'??'}))),lab)
    return 'OPT("%s", %s)'%(lab,tgt)
for n in sys.argv[1:]:
    b=m['Method'+n]
    b=b.replace('        Console.Clear();\n        ScenarioOptionsManager optionsManager = new ScenarioOptionsManager();\n','')
    b=re.sub(r'\n\s*Console.WriteLine\(\);\n\s*Console.WriteLine\(\);',' <BR2>',b)
    b=re.sub(r'\n\s*Console.WriteLine\(\);',' <BR>',b)
    b=re.sub(r'Console.Write\(\s*',' W(',b)
    b=b.replace('StaticGameState.','$').replace('optionsManager.AddOption','OPT')
    b=re.sub(r'OPT\("(\w+)", (\w+)\)',fix,b)
    b=b.replace('        optionsManager.PresentOptions();\n','')
    print('=====',n); print(b)
