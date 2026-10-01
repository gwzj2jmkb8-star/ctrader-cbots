# XI translation contract and remaining parity work

## Source authority

Baseline: the user's inline Functional_Mixture_XI Pine v6 on 2026-09-30, not an older XIV/XV file. No imported libraries are present. Markdown bold/HTML spacing is presentation, not Pine code. The execution reference repairs indentation of the local variables in `f_percentrank` and omits visual-only code; its hash identifies that transcription, not the full pasted message. Exact instrument, TradingView feed, chart/session timezone, test interval, trade export and property overrides were not provided.

## Trading rules

| Rule | Source condition | C# mapping |
| --- | --- | --- |
| CORE_LONG | Close crosses above upper band, above filter EMA, rank ≥ 50 | XiKernel.LongBreakout → XiDecisions |
| CORE_SHORT | Close crosses below lower band, below filter EMA, rank ≥ 50 | XiKernel.ShortBreakout → XiDecisions |
| CORE_CLOSE_LONG | Positive position and cross below basis or short breakout | LongExit |
| CORE_CLOSE_SHORT | Negative position and cross above basis or long breakout | ShortExit |
| PROFIT_CLOSE_LONG | Daily A < B, lagged Hull > current Hull, open profit > 400 | XiDecisions profit branch |
| PROFIT_CLOSE_SHORT | Daily A > B, current Hull > lagged Hull, open profit > 400 | XiDecisions profit branch |
| TREND_PC_LONG | Daily A > B, Hull rising versus two bars earlier, PC long permission | LongTrend |
| TREND_PC_SHORT | Daily A < B, Hull falling versus two bars earlier, PC short permission | ShortTrend |

Order intentions retain this sequence: core long, core short, source-position closes, conditional profit closes, trend long, trend short. Same-ID entry calls within an evaluation are resolved by keeping the last eligible call. Same-direction entries are suppressed using the pre-fill position snapshot for pyramiding=0. This execution adapter interpretation requires matching Pine trades for confirmation in conflicting-order cases.

The PC filter gates only the final trend entries. DMI/MACD drive display state and diagnostic labels. WMA1/2/3 are visual. The Decision Threshold input is unused in the original and remains unused. Source comments mentioning future TP/SL toggles do not implement them and have not been turned into extra execution modules.

## Formula details

- ALMA uses the unfloored offset center. Gaussian weights use a common exponent shift to avoid numerical underflow while preserving normalized weights.
- EMA seeds at the first finite value; RMA uses an SMA seed. WMA gives newest observations the greatest weights.
- Source `f_mad` averages absolute deviations from each bar's own rolling mean; it is not silently replaced by deviations from one fixed window mean.
- Percentile counts current and historical equal values with `>=`. With stdev length 1, volatility is zero and rank becomes 100 after valid values. Both volatility multipliers are zero by default, making ATR offset the envelope width contributor.
- Hull n2 is n1 delayed two bars for dense OHLC input. Positive half-integer lengths use Pine-style rounding, not C# banker's rounding. The source's 1e13 comparison scale is retained.
- PC lengths labeled pips in Pine actually multiply `syminfo.mintick`; C# uses TickSize.
- Historical MTF sampling chooses the latest fully available bar; realtime sampling uses the developing bar. Lower-timeframe historical requests select the last completed intrabar available at the chart close. Daily values use the broker's daily bars.

## Execution adaptations requiring comparison

1. Pine's broker emulator and cTrader broker execution are distinct. Source expressions/order intentions are translated; tick-for-tick fill equivalence is not established.
2. Reversals explicitly close owned opposite positions then open the requested side. Fill-triggered recalculation is coalesced after the reversal batch, rather than recursively submitting an order from each close/open callback. Conflicting same-evaluation entries and repeated fill-induced entries need a reference trade trace.
3. EveryTick mode queues market intentions until the next quote. ClosedBars mode executes the completed-bar intention on the first tick of the next bar. Historical Pine `calc_on_order_fills` may use emulator OHLC values that differ from the corresponding real tick path.
4. Percent-of-equity sizing uses account equity, currency conversion into quote currency, price and downward volume normalization. It is not 90% stop-loss risk or a 90% margin target. This assumes conventional linear symbol units; inverse/nonstandard contracts need an instrument-specific conversion review.
5. Profit exits use owned-position NetProfit in account currency, including broker costs. Pine commission/swap assumptions and currency must match for a meaningful comparison. Account-wide equity can differ from isolated strategy equity when other trading is present.
6. Broker feed/session boundaries and bid/ask spreads can change crossings. Fixed-duration day sampling assumes 24-hour daily bars; irregular daily/DST boundaries need feed-specific validation. cTrader tick volume is not guaranteed equal to TradingView exchange volume.
7. A failed order, invalid volume or insufficient margin faults execution until restart rather than repeatedly sending the same rejected operation. Stopping the cBot retains open positions. Unsent intentions are not persisted; restarting reconstructs calculation history and identifies owned positions by label. Use distinct instance keys. Netted-account foreign positions on the same symbol are not merged.
8. Source selection exposes Close/Open/High/Low/HL2/HLC3/OHLC4. TradingView external-indicator source inputs require an additional explicit data adapter.
9. The single Robot owns a plain internal chart renderer that reads the same kernel snapshots used by execution. The renderer is not an Indicator subclass and has no algo attribute. No custom assembly dependency or separate indicator installation is required. Drawing failure faults the app before further order submission; open positions remain. Desktop rendering is the intended visual experience; a cloud instance is not claimed to stream these custom visuals onto a remote chart.

## Validation state and next comparison

| Gate | Result |
| --- | --- |
| Real Spotware API compilation, single cBot assembly | Pass |
| Spotware metadata extraction and .algo packaging | Pass |
| 23 deterministic math, state and decision fixtures | Pass |
| cTrader import / visual runtime | Not run |
| TradingView reference compilation | Not run |
| Same-data Pine versus C# numerical and signal comparison | Not run |
| cTrader historical backtest / broker execution | Not run |

Next: run the supplied baseline on the actual instrument for 15m, 45m and 1h with the same exact settings, dates and costs. Export TradingView data/trades and cTrader traces. Compare warm-up and OHLC first, numerical features second, gates/intent sequence third, fills last. Do not optimize away a mismatch. No profitability figure is inferred from the kernel tests.

## Official implementation sources reviewed 2026-09-30

- https://help.ctrader.com/ctrader-algo/documentation/cbots/cbot-bar-events/
- https://help.ctrader.com/ctrader-algo/documentation/visual-studio-ides/
- https://help.ctrader.com/ctrader-algo/references/MarketData/IAssetConverter/
- https://help.ctrader.com/ctrader-algo/guides/ui-operations/chart-objects/
- https://www.nuget.org/packages/cTrader.Automate/1.0.21
- https://www.tradingview.com/pine-script-docs/concepts/strategies/
- https://www.tradingview.com/pine-script-docs/concepts/other-timeframes-and-data/

Method signatures and drawing APIs were also checked against the XML reference in the official 1.0.21 package, then compiled against its actual API assembly.


## Root-file / CT0003 revision

`mojaTyMoja.cs` and `mojaTyMoja.csproj` are at the package root. The entire application compiles into `mojaTyMoja.dll`, with exactly one public Robot entry point: `cAlgo.Robots.mojaTyMoja`. There is no Indicator entry point. The project explicitly includes only the root source, avoiding accidental inclusion of old algo classes and test sources.

The original combined Robot + Indicator assembly reproduced `Assembly must contain single algo type`. After converting the chart indicator into an internal drawing helper, official Spotware 1.0.21 metadata extraction and bundling pass. The error location in `cTrader.Automate.targets` points to the metadata task, not a strategy expression that needs editing.

Core calculations and MTF selection retain the original source implementation. Chart output now uses cBot-owned trend lines, filled triangle segments, rectangles, candles and labels. The default plot window covers the recent 180 bars plus Ichimoku projection, with up to 6,000 plot objects; candle/annotation objects retain their separate configurable limit. It is not a full-history native indicator plot. Clouds and colors are chart-object approximations of the Pine visuals. Ichimoku and relative tick-volume Heikin Ashi remain display-only. Stop removes owned chart objects. Import and visual lifecycle still require confirmation in desktop cTrader.
