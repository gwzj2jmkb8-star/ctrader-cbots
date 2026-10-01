#!/usr/bin/env python3
"""Targeted static checks. This does not parse/type-check all of Pine."""
from pathlib import Path
import re, sys, json
ROOT=Path(__file__).resolve().parents[1]
MASK=re.compile(r'//[^\n]*|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'')
def masked(text):
    return MASK.sub(lambda m: ''.join('\n' if c=='\n' else ' ' for c in m.group()),text)
def arguments(text):
    result=[]; depth=0; start=0
    for i,c in enumerate(text):
        if c in '([': depth+=1
        elif c in ')]': depth-=1
        elif c==',' and depth==0: result.append(text[start:i].strip());start=i+1
    result.append(text[start:].strip());return result

def audit(path):
    source=Path(path).read_text(); code=masked(source); errors=[]; warnings=[]; counts={}; calls=[]
    if len(re.findall(r'^strategy\(',code,re.M))!=1: errors.append('Expected one strategy declaration')
    for pattern,desc in [(r'^\s*plot\s+\w+\s*=', 'Unsupported plot type declaration'),(r'^import ', 'Standalone has unresolved imports'),(r'\bmath\.clamp\(', 'Unsupported math.clamp'),(r'\bthen\b','Unexpected then token'),(r'^export ', 'Standalone has export')]:
        if re.search(pattern,code,re.M): errors.append(desc)
    if re.search(r'\balertcondition\(',code): warnings.append('Inherited alertcondition declarations retained; use strategy order-fill alerts for fills.')
    # Balanced delimiters after masking strings/comments.
    stack=[]
    for c in code:
        if c in '([': stack.append(c)
        elif c in ')]':
            if not stack or stack.pop()!=('(' if c==')' else '['): errors.append('Unbalanced delimiter');break
    if stack: errors.append('Unclosed delimiter')
    static_colors=set(re.findall(r'^color (\w+) = (?:input\.color\(|color\.rgb\()',code,re.M))
    def static_color(expr):
        return expr in static_colors or expr=='na' or bool(re.fullmatch(r'color\.\w+',expr))
    for m in re.finditer(r'\b(plot|plotshape|plotchar|plotarrow|plotcandle|plotbar|bgcolor|barcolor|fill|alertcondition)\(',code):
        kind=m.group(1); start=m.end();depth=1;j=start
        while j<len(code) and depth:
            if code[j]=='(':depth+=1
            elif code[j]==')':depth-=1
            j+=1
        args=arguments(code[start:j-1]); kwargs={}
        for a in args:
            if '=' in a and re.match(r'^\w+\s*=',a):
                k,v=a.split('=',1);kwargs[k.strip()]=v.strip()
        line=code.count('\n',0,m.start())+1
        prefix=code[code.rfind('\n',0,m.start())+1:m.start()]
        if prefix.startswith((' ','\t')): errors.append(f'Visual call in local/indented scope at {line}')
        if kind=='plot': cost=1+int('color' in kwargs and not static_color(kwargs['color']))
        elif kind in ('plotshape','plotchar'):
            color=kwargs.get('color',args[4] if len(args)>4 else 'na')
            cost=1+int(not static_color(color))+int('textcolor' in kwargs and not static_color(kwargs['textcolor']))
        elif kind=='fill': cost=1 # Count even static fills conservatively.
        elif kind=='plotcandle':cost=7 # Maximum, including all color series.
        elif kind=='plotbar':cost=5
        elif kind=='plotarrow':cost=3
        else:cost=1
        counts[kind]=counts.get(kind,0)+cost;calls.append({'line':line,'call':kind,'upper_plot_count':cost})
    if sum(counts.values())>64:errors.append('Estimated plot count exceeds 64')
    names=re.findall(r'^(?:var(?:ip)? )?(?:(?:const|simple|series) )?(?:float|int|bool|string|color|line|label|box|table|array<[^>]+>|EP_Types_\w+) (\w+)\s*=',code,re.M)
    duplicate=sorted({n for n in names if names.count(n)>1})
    if duplicate: errors.append('Duplicate global declarations: '+', '.join(duplicate))
    if 'Thanks for now' in source:errors.append('Stray prose remains')
    return {'file':Path(path).name,'status':'FAIL' if errors else 'PASS','kind':'static-only; not TradingView compilation','conservative_plot_count':sum(counts.values()),'by_call':counts,'errors':errors,'warnings':warnings,'visual_calls':calls}
if __name__=='__main__':
    path=Path(sys.argv[1]) if len(sys.argv)>1 else ROOT/'ETHER_POISON_LIBRARY_PREVIEW.pine'
    result=audit(path)
    if len(sys.argv)>2 and sys.argv[2]=='--json': print(json.dumps(result,indent=2))
    else:
        print(f"{result['status']}: static structural audit")
        print(f"Conservative plot count: {result['conservative_plot_count']} / 64; {result['by_call']}")
        for error in result['errors']:print(error)
        print('TradingView compilation and broker-emulator runtime checks remain required.')
    raise SystemExit(bool(result['errors']))
