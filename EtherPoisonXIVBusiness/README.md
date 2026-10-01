# ETHER POISON XIV — operating codebase

This package is the first bounded business unit around the XIV cBot. It contains one runtime, one reproducible build definition, a provenance record, and an evidence gate. It is a **build candidate**, pending cTrader compilation and broker backtests.

## Start

1. Install a .NET SDK that supports `net7.0` and cTrader Algo on Windows or Mac.
2. In this directory, run `dotnet restore EtherPoisonXIVBusiness.csproj` and `dotnet build EtherPoisonXIVBusiness.csproj -c Release`.
3. Load the generated `.algo` in cTrader Algo or build the source there. Set a distinct `Instance label` for each instance. Begin with a demo account.
4. Run the market and limit cases described in `ops/backtest_matrix.csv`, export the reports, create `evidence/reports/` and place them there, and fill the matrix with their observed figures.
5. Run `python3 scripts/release_gate.py`. It verifies all four evidence cases, report hashes, positive trade counts, and a recorded build result, then writes `ops/release-manifest.json` only on success.

The build reference is pinned to `cTrader.Automate` **1.0.21**. Update it deliberately after rebuilding and rechecking the backtests. A cTrader build and backtest have not been run in this workspace.

The chart now offers display-only Heikin Ashi candles and a projected Ichimoku cloud. The earlier Ichimoku score weight was removed, so this source version requires fresh signal comparison and backtests before release; see `docs/CBOT_GUIDE.md`.

## Structure

| Path | Responsibility |
| --- | --- |
| `src/EtherPoisonXIVcBot.cs` | The cTrader runtime and XIV-derived numerical logic; MPL 2.0 notice retained. |
| `docs/CBOT_GUIDE.md` | Input, signal, execution, and chart behavior. |
| `docs/ARCHITECTURE.md` | Domain boundaries and next extraction points. |
| `docs/PROVENANCE.md` | Source lineage and distribution notice. |
| `docs/OPERATIONS.md` | Demo to live gates, incident response, release and rollback. |
| `ops/backtest_matrix.csv` | Required evidence rows. Fill with observed data. |
| `ops/build_result.json` | Build result recorded by the operator after a platform build. |
| `scripts/release_gate.py` | Local gate and source/report checksum manifest. |
| `ops/unit.json` | Stable operational identity for the unit. |
| `infra/terraform/` | AWS inventory, ECR, private evidence, secrets shell, logs and paused ECS runtime. |
| `deploy/` | Pinned Spotware Console image wrapper for a reviewed `.algo`. |

The package contains no credentials, account IDs, trading history, or private customer data. `*.cbotset` and raw reports are intentionally excluded from a future Git repository; a private evidence archive should hold them with access control.

## Business boundary

The runtime serves a single strategy on an explicitly selected account, symbol, and timeframe. Business operation means repeatable evidence and controlled deployment before monetization. The pricing, customer, entity, jurisdiction and distribution terms are decisions for the owner, not assumptions encoded into this bot. See `docs/OPERATIONS.md` for an initial service workflow.

## Current state

The source has been structurally checked and API calls checked against cTrader documentation. The release gate must fail with the supplied blank evidence. Compilation, fill parity with Pine, broker-specific volume/protection behavior, and out-of-sample results remain open acceptance criteria.

## Infrastructure

Read `infra/terraform/README.md` before provisioning AWS resources. Terraform starts at zero ECS tasks. It records the operational unit through AWS names, tags and an output; it does not create a legal business entity or a cTrader Store registration. The runtime image is built only after the cBot compiles and the evidence is reviewed.
