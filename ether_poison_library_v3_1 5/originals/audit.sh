#!/bin/sh
set -eu

project_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
target=${1:-"$project_dir/ETHER_POISON_STANDALONE.pine"}

test -f "$target"

if grep -nE '\bthen\b|ta\.vwap$|^[[:space:]]+plot\(' "$target"; then
    echo "ERROR: known Pine syntax or local-scope plot risk found" >&2
    exit 1
fi

counts=$(awk '
BEGIN { plotLike = 0; fills = 0 }
/(^|[^[:alnum:]_])plot\(/ { plotLike++ }
/(^|[^[:alnum:]_])plotshape\(/ { plotLike++ }
/(^|[^[:alnum:]_])bgcolor\(/ { plotLike++ }
/(^|[^[:alnum:]_])barcolor\(/ { plotLike++ }
/(^|[^[:alnum:]_])fill\(/ { fills++ }
END { print plotLike, fills, plotLike + fills }
' "$target")

set -- $counts
echo "plot-like calls: $1"
echo "fill calls: $2"
echo "conservative visual total: $3 / 64"
echo "source bytes: $(wc -c < "$target" | tr -d ' ')"
echo "source lines: $(wc -l < "$target" | tr -d ' ')"

if [ "$3" -gt 64 ]; then
    echo "ERROR: visual-call budget exceeded" >&2
    exit 1
fi

echo "PASS: local structural audit"
echo "NOTE: TradingView compilation is authoritative for optimized tokens and runtime behavior."
