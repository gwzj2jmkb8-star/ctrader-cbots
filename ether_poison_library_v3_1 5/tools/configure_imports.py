#!/usr/bin/env python3
"""Apply imports.json's ACTUAL owner/version assignments to every source.
Does not publish, authenticate or access TradingView.
"""
from pathlib import Path
import json,re
ROOT=Path(__file__).resolve().parents[1]
config=json.loads((ROOT/'imports.json').read_text());versions={i['name']:str(i['version']) for i in config['libraries']}
owner=config['owner']
if not re.fullmatch(r'\w+',owner):raise SystemExit('Invalid owner identifier')
for path in [*(ROOT/'libraries').glob('*.pine'),*(ROOT/'master').glob('*.pine')]:
    def replace(m):
        name=m.group(2)
        if name not in versions:raise ValueError('Unknown library '+name)
        return f'import {owner}/{name}/{versions[name]} as {m.group(4)}'
    text=re.sub(r'^import (\w+)/(\w+)/(\d+) as (\w+)$',replace,path.read_text(),flags=re.M)
    path.write_text(text)
print('Synchronized local imports; no libraries were published.')
