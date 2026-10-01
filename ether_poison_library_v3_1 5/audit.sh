#!/bin/sh
set -eu
project_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
python3 "$project_dir/tools/check_imports.py"
python3 "$project_dir/tools/build.py" --check
python3 "$project_dir/tools/audit.py"
python3 -m unittest discover -s "$project_dir/tests" -v
