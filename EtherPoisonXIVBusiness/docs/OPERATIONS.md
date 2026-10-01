# Operating the first business unit

## Owner workflow

The owner controls source changes, configurations, accounts, evidence, release decisions, and customer commitments. Each release gets a version tag, source checksum, cTrader build result, backtest report checksums, selected symbol/timeframe, parameter snapshot, and rollback artifact. No account credentials belong in this package.

## Release gates

1. **Build:** record the local cTrader/.NET build in `ops/build_result.json`, including the `.algo` checksum after a successful compile.
2. **Strategy parity:** compare signal timestamps and scores on closed bars against Pine after warmup. Explain differences from feeds, EMA seeds, or pivot ties.
3. **Execution:** complete the four cases in `ops/backtest_matrix.csv`. Record actual report files, trades, drawdown, rejected operations, risk closes, and observations. The gate checks evidence completeness, not profitability.
4. **Demo soak:** run on the intended broker symbol and timeframe, observe restarts, stale pending orders, partial closes, and the basket loss close. Keep a deployment journal.
5. **Release decision:** review exposure, fill quality, broker constraints, and regression from the previous artifact. Promote only the exact reviewed `.algo` and configuration.

## During operation

- Check that the running version, symbol, timeframe, account type, and instance label match the release record.
- Watch rejected orders/protection updates, pending lifespan, open basket volume, spread, risk close events, and terminal uptime.
- If the bot stops unexpectedly, inspect owned pending orders and broker stops before restarting. Existing baskets intentionally lock rescue until flat.
- Preserve logs and the relevant configuration for investigation. Do not infer behavior from a net-profit total alone.

## Recovery

Pause new instances, cancel owned pending orders after reviewing the market, inspect open positions and protection, and use broker controls for any urgent close. Roll back to the last verified `.algo` and parameters only after confirming the old artifact is compatible with current broker behavior. Write an incident record with cause, exposure, actions, and the re-entry condition.

## Business development

Select one initial operating model and record it before distribution: own-account research, licensed cBot, managed service, or an analytics subscription. Each has different support, data, licensing, and regulatory obligations. The current package implements own-account/demo research operations; it does not implement billing, customer onboarding, pooled money, account delegation, or an investment promise.
