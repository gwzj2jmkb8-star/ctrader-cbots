#!/usr/bin/env python3
"""Check local publication contracts; cannot inspect published TradingView code."""
from pathlib import Path
import json
import re

ROOT = Path(__file__).resolve().parents[1]
IMPORT = re.compile(r'^import (\w+)/(\w+)/(\d+) as (\w+)\s*$', re.M)

def check_sources(sources, config):
    expected = {item['name']: str(item['version']) for item in config['libraries']}
    errors = []
    for path, source in sources.items():
        for owner, name, version, alias in IMPORT.findall(source):
            wanted = expected.get(name)
            if owner != config['owner'] or version != wanted:
                errors.append(f'{path}: {owner}/{name}/{version} must match '
                              f"{config['owner']}/{name}/{wanted}; published dependents need a new version too")
    return errors

def check_project():
    config = json.loads((ROOT/'imports.json').read_text())
    paths = [*(ROOT/'libraries').glob('*.pine'), *(ROOT/'master').glob('*.pine')]
    return check_sources({str(p.relative_to(ROOT)): p.read_text() for p in paths}, config)

if __name__ == '__main__':
    errors = check_project()
    for error in errors:
        print(error)
    print('FAIL: local dependency mismatch' if errors else 'PASS: all local imports match the shared version contract')
    raise SystemExit(bool(errors))
