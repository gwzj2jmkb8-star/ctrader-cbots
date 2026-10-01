#!/usr/bin/env python3
"""Fail closed until a platform build and four broker backtests are recorded."""
import csv
import hashlib
import json
import sys
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CASES = {"baseline", "limit", "out_of_sample", "stress"}


def checksum(path):
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def contained_file(raw_path, parent):
    path = (ROOT / raw_path).resolve()
    if not path.is_file() or not path.is_relative_to(parent.resolve()):
        raise ValueError(f"Expected a file inside {parent.relative_to(ROOT)}: {raw_path}")
    return path


def number(row, name, integer=False):
    value = row.get(name, "")
    try:
        result = int(value) if integer else float(value)
    except (ValueError, TypeError):
        raise ValueError(f"{row['case']}: invalid {name}: {value!r}")
    if result < 0 or result != result or result == float("inf"):
        raise ValueError(f"{row['case']}: invalid {name}: {value!r}")
    return result


def validate():
    build = json.loads((ROOT / "ops/build_result.json").read_text(encoding="utf-8"))
    if build.get("status") != "passed":
        raise ValueError("Build has not been recorded as passed in ops/build_result.json")
    algo = contained_file(build.get("algo_path", ""), ROOT / "evidence/reports")

    with (ROOT / "ops/backtest_matrix.csv").open(newline="", encoding="utf-8") as stream:
        rows = list(csv.DictReader(stream))
    if len(rows) != len(CASES) or {r.get("case") for r in rows} != CASES:
        raise ValueError("Exactly one row per required case is needed")

    reports = {}
    for row in rows:
        case = row["case"]
        if row.get(None) is not None:
            raise ValueError(f"{case}: extra CSV columns")
        if row["status"] != "passed":
            raise ValueError(f"{case}: status is not passed")
        for field in ("symbol", "timeframe", "from_utc", "to_utc", "entry_mode", "report_path"):
            if not row.get(field):
                raise ValueError(f"{case}: missing {field}")
        if case == "limit" and row["entry_mode"] != "Limit":
            raise ValueError("limit: entry mode must be Limit")
        if case != "limit" and row["entry_mode"] != "Market":
            raise ValueError(f"{case}: entry mode must be Market")
        start = datetime.fromisoformat(row["from_utc"].replace("Z", "+00:00"))
        end = datetime.fromisoformat(row["to_utc"].replace("Z", "+00:00"))
        if start.tzinfo is None or end.tzinfo is None or start >= end:
            raise ValueError(f"{case}: invalid UTC date range")
        if number(row, "trade_count", True) == 0:
            raise ValueError(f"{case}: trade count is zero")
        number(row, "max_drawdown_pct")
        if number(row, "rejected_operations", True) != 0:
            raise ValueError(f"{case}: investigate rejected broker operations")
        if case == "stress" and number(row, "risk_close_count", True) == 0:
            raise ValueError("stress: no basket risk close was observed")
        if case != "stress":
            number(row, "risk_close_count", True)
        report = contained_file(row["report_path"], ROOT / "evidence/reports")
        reports[case] = {"sha256": checksum(report), "path": str(report.relative_to(ROOT)), "record": row}

    manifest = {
        "generated_utc": datetime.now(timezone.utc).isoformat(),
        "status": "evidence-complete; human release review required",
        "source_sha256": checksum(ROOT / "src/EtherPoisonXIVcBot.cs"),
        "project_sha256": checksum(ROOT / "EtherPoisonXIVBusiness.csproj"),
        "algo_sha256": checksum(algo),
        "algo_path": str(algo.relative_to(ROOT)),
        "build": build,
        "reports": reports,
    }
    return manifest


if __name__ == "__main__":
    try:
        result = validate()
    except (ValueError, OSError, json.JSONDecodeError) as exc:
        print(f"RELEASE BLOCKED: {exc}", file=sys.stderr)
        sys.exit(1)
    destination = ROOT / "ops/release-manifest.json"
    destination.write_text(json.dumps(result, indent=2, sort_keys=True) + "\n", encoding="utf-8")
    print(f"Evidence gate passed. Review {destination} before any release.")
