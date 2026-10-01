# Architecture and evolution

## One trading runtime

The cBot owns market data, deterministic XIV calculations, order decisions, broker position state, risk exits, and chart rendering. cTrader owns ticks, closed bars, pending orders, positions, and broker-side stops. One instance filters orders and positions by its symbol and unique label. The operator owns account permissions, parameters, deployment, and evidence.

```
Closed bars → XIV state (ATR, pivots, FVG, score) → approval
approval → market or pending limit order → broker position basket
ticks → software basket loss close; broker → stop/target fills
```

The score does not use a trained model. Its multiplexer is deterministic entropy across three EMA horizons. A future ML model may supply a **versioned optional input** only after causal feature parity, out-of-sample validation, and an independent fallback are demonstrated. Keep model inference outside broker order functions.

## Stable contracts

| Boundary | Input | Output | Invariant |
| --- | --- | --- | --- |
| Market state | Closed OHLC bars, configured lengths | ATR, structure and live zones | No future bar in an order decision. |
| Decision | State, weights, gates | Long, short or wait plus two scores | Hard FVG and structure gates remain explicit. |
| Execution | Approved side and risk settings | Market fill or limit pending order | Use only owned label/symbol; pending order is protected. |
| Risk | Owned basket, equity reference, ATR | Broker stop/target, software risk close, rescue cap | Never add after a rejected close without operator review. |
| Evidence | Platform report and parameter set | Gate manifest | No validated release without report hashes and build result. |

## Next extraction sequence

1. Compile the existing class in cTrader and fix API/compiler errors with a recorded build result.
2. Extract pure numerical XIV kernels into a separate C# class in the **same cBot project**, keeping a bar-by-bar parity fixture against Pine. Do not change trade decisions during extraction.
3. Extract execution and risk only after broker backtests establish the expected pending, hedge/netting, rescue, and partial-close behavior.
4. Add a read-only telemetry export with explicit permission and bounded retention. Keep credentials out of source and logs.
5. Add ML as a separately versioned experiment with a default-off gate and a replayable feature dataset; never silently replace the XIV score.

## Failure modes to verify

Spread changes at bar open, rejected protection updates, stale limit fills, bot restart with an existing basket, hedging versus netting semantics, account margin limits, stopped bot while a pending order remains, and gaps beyond the software risk close. Broker stops provide a separate protective path, though they do not guarantee a fill price.
