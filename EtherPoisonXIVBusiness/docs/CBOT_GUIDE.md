# ETHER POISON XIV — cTrader cBot

`EtherPoisonXIVcBot.cs` ports the **Spatial Energy Matrix v2** execution path from the attached `ETHER_POISON_STANDALONE.pine`. The attached `README.md` designates `ETHER_POISON_IMPORTED.pine` as its modular master and the standalone as generated output; the supplied imported copy includes a stray `Thanks for now ;)` line, so the standalone is the reference for executable decisions. Numerical formulas also follow `EP_Math_v2.pine` and `EP_Engines_v2.pine`.

## Install

1. In cTrader Algo on Windows or Mac, create a C# cBot named `EtherPoisonXIVcBot`.
2. Replace its generated source with the contents of `EtherPoisonXIVcBot.cs`, then build it in cTrader.
3. Add an instance to the intended symbol and timeframe. Start in a **demo backtest** with historical bars, commission and spread configured for that symbol.
4. Use a unique **Instance label** for each running instance on the same symbol. Orders and positions are filtered by this label and symbol.

There is no external machine learning service or network dependency. The three horizon entropy multiplexer is a deterministic analytical feature, **not a trained model**. Its gate is off and its score weight is zero by default, matching the source.

## Risk and exit controls

| Input | Behavior |
| --- | --- |
| `Sizing = EquityRiskPercent`, `Entry risk % equity = 1` | cTrader estimates units for the effective ATR stop against 1% of current account equity. `FixedLots` selects the explicit lots input. |
| `Close basket loss % = 2` | On each tick, close every position owned by this instance when their combined **net unrealized P/L** reaches minus 2% of the equity snapshot taken at entry. Set to `0` to disable. After a risk exit, new entries pause for the configured number of bars. |
| `Stop (ATR)`, `Target (ATR)` and `SL/TP adjustment %` | Effective stop/target distance is ATR multiplier times adjustment percentage divided by 100. The bot attaches protection on entry and updates owned positions against the basket average on each closed bar. |
| `Maximum basket stop risk % = 2` | Approximate total stop exposure budget before a rescue add; set to `0` to remove this additional rescue constraint. Rescue is off by default. |
| `Refresh / wakeup (seconds) = 60` | cTrader's timer wakes the running instance every 60 seconds, rechecks the basket loss guard and processes any newly closed bars that the normal bar callback has not handled. Each bar is processed at most once. |

These risk estimates use the broker symbol's volume and risk calculations. Spread, gaps, slippage, conversion, commissions and rejected close requests can make realized loss larger than the configured threshold. The percent loss close is a software action while the bot runs; the attached stop remains at the broker.

## Chart indicators

`Render indicators` draws the XIV decision context and two display-only navigation layers. `Show Heikin Ashi` overlays translucent candles (calculated from the chart's raw OHLC) with open, high, low, close and direction on the board. `Show Ichimoku` draws Tenkan, Kijun, Senkou A/B projected forward by Kijun bars, translucent green/red cloud fill, and Chikou (the current close plotted Kijun bars back). Their current calculated values and display offsets appear on the board. The cloud fill is a barwise rectangle approximation between the span boundaries. Use a standard candlestick chart for the Heikin Ashi overlay; applying it to an existing Heikin Ashi chart would smooth synthetic candles again.

Neither Heikin Ashi nor Ichimoku contributes to the trade score, approval gates, entries, risk or exits. Rapid/volatility contours, SuperBoll bands, multiplexer bounds, FVG zones, BOS/CHoCH labels and basket guides remain visible. `Draw bars` bounds the rolling objects; projected Ichimoku drawings extend Kijun bars beyond the latest candle. Both layers require an available chart, so headless Console runs still execute the raw-price strategy without drawings.

## Signal contract

| XIV Pine behavior | cBot implementation |
| --- | --- |
| `ta.atr`, kinetic saturation with `tan`, 14 bar momentum | Wilder ATR and bounded tangent kinetic calculation on closed bars |
| `ta.pivothigh/low`, BOS, structure window | Left/right confirmed pivots, close cross, per-direction age |
| Latest bullish/bearish FVG, inversion, lifetime | Stateful latest zone per side; invalidation on close cross |
| SuperBoll z score and breakout; EMA wave | Price-based score components remain; Ichimoku is calculated only for navigation |
| Entropy fast/medium/slow, temperature attenuation, confidence | Three EMA lanes, density weighted direction and optional quality gate |
| FVG and structure hard gates, score threshold, stronger side | Closed-bar order decision with matching default weights |
| ATR stop/target; rescue; exhaustion harvest | Protected orders, updated aggregate levels, bounded adds and partial closes |

Set **Orders** to `Market` for the source-like path. `Limit` is an opt-in extension: a buy is placed below the signal close and the current ask, a sell above the close and current bid, retreated by `Limit retreat (ATR)`. Pending orders carry relative stop and target protection, expire after `Limit lifespan (bars)`, and are replaced on a later qualifying signal. The execution log prints `LIMIT PENDING`, fills are then visible in cTrader's order/position history. Limit fills do **not** imply an additional score check when touched; approval happened at placement.

## Execution assumptions and differences

- Default entry sizing is **1% equity risk at the effective ATR stop**, normalized to the broker's symbol volume units. The alternative fixed size defaults to 0.01 lots. Pine's `10% of equity` is a position notional allocation, so neither mode exactly reproduces its quantity across FX, metals and CFDs.
- Signals use the last fully closed chart bar. cTrader's `OnBarClosed` runs when the next bar's first tick arrives. The bid/ask at execution and spread can move away from Pine's `process_orders_on_close=true` fill. Backtests will differ even with matching inputs.
- **Score migration:** the earlier cBot used `Cloud weight = 0.8` for unshifted Ichimoku spans. This release removes that weight and its score contribution to keep the requested Ichimoku layer informational. Signal thresholds and trade counts can change; redo the Pine/cTrader comparison and broker backtests before promotion. A saved `.cbotset` containing `Cloud weight` should be reviewed and regenerated for this version.
- The timer refreshes the running bot's market state; it cannot manufacture a missing tick or live-replace C# code. To change the strategy source, build a new `.algo` and restart/deploy the reviewed artifact. No trade occurs on a timer wakeup without a newly closed bar.
- ATR and EMA seed handling is approximated during historical warmup; exact first-bar tie behavior for confirmed Pine pivots and intrabar stop/target collisions can also vary. Compare signal timestamps after a generous warmup, especially on XAUUSD H4 if that is the selected instrument.
- cTrader hedging accounts can hold several positions. This bot uses a weighted average for its owned basket and updates stop and target on each owned position. On netting accounts, broker merging and labels may change the basket behavior; inspect backtest deals before use.
- Rescue is **disabled by default**, capped by both cycle count and add multiple. If restarted with an existing basket, rescue stays locked until flat because the previous cycle count is not reconstructible. Stops stay broker-side. Some broker minimum-stop distances may reject an update; rejection is logged and the original protection remains.
- This bot renders the indicator contours and decision events; Pine's extended projected Fibonacci/Gann/time grid and atmospheric effects are not ported. The rolling chart drawings do not change order decisions.
- In headless cTrader Console runs, chart drawing is skipped when no chart is available; signal and risk calculations continue.

## Validation pass

Backtest market and limit modes separately on the target broker's historical bid/ask feed. Inspect order timestamps, spread, position volume, risk exits, stop/target rejections, partial closes, rescue cycles, drawdown and total exposure. Reconcile a bar-by-bar export from Pine and cTrader before attributing any performance difference to the signal formula. This workspace has no cTrader Algo compiler or broker feed, so platform compilation and trade simulation are still required.

Sources: [cTrader bar events](https://help.ctrader.com/ctrader-algo/documentation/cbots/cbot-bar-events/), [Robot API](https://help.ctrader.com/ctrader-algo/references/General/Robot/), [Symbol API](https://help.ctrader.com/ctrader-algo/references/MarketData/Symbols/Symbol/).
