# Volume candles, documented HA feed and FVG cloud screening — v3.3

The imported master remains the entry point. All previous libraries remain in use;
EP_Candles gains anatomy and reusable volume-candle drawings, and EP_CloudScreen
adds independent visual cloud state. Existing entry, rescue, exit, harvest and
limit-ticket logic is unchanged from the archived v3.2 main. The optional existing
HA execution gate still uses its original local recurrence; changing the new
**HA display source** only changes the display.

## Start here

Use a standard time-based candlestick chart. The defaults show TradingView HA
geometry, volume-scaled recent bodies, and mitigation clouds. Hide the chart's
own candle series if it obscures the overlay. Add a fresh strategy instance to
apply the declaration's updated visual layering settings.

| Settings group | Controls and defaults |
| --- | --- |
| 08: HA and relative volume | Existing HA + relative-volume layer; RVOL lengths 8 / 21 / 55 |
| 08A: Candle feed and volume width | TradingView HA; chart session; variable width on; 60 recent candles |
| 08A: Width response | Width at RVOL 1 = 0.45 of a bar slot; exponent = 0.5 |
| 08A: HA wick tolerance | 1 tick; strong body threshold uses the existing candle setting |
| 07D: FVG cloud screening | Mitigation cloud; All live; original envelope and midpoint on |
| 07D: Numeric screen thresholds | Zero by default; maximum distance 0 means disabled |
| 07D: Context | Inverted clouds allowed; structure-at-birth filter off |
| 07D: Transparency | 72 for untouched gaps, fading toward 95 as gaps fill |

Inputs newly added by this revision have no minval/maxval restrictions. Renderer
capacities and color/width domains are normalized internally to platform limits.

## HA and volume feed

The display follows TradingView's documented pattern:

```pine
string haTicker = ticker.heikinashi(baseTicker)
[haO, haH, haL, haC] = request.security(
    haTicker, timeframe.period, [open, high, low, close],
    gaps=barmerge.gaps_on, lookahead=barmerge.lookahead_off
)
```

A second tuple request gets standard OHLCV for price-candle display and relative
volume. Chart-session and regular-session choices share the same underlying
base ticker. Gaps remain unavailable outside the selected session. The legacy
local HA recurrence remains available as a display comparison option.

HA is synthetic geometry, not a tradable OHLC feed. Display changes never replace
order prices. The existing strategy still inherits its host chart's OHLC for
signals and execution calculations, so this upgrade does not make a synthetic
host chart equivalent to a standard-chart backtest. The candle board identifies
whether the host is standard or synthetic.

Body fraction and upper/lower wicks describe HA shape. STRONG UP requires an up
body above the body-fraction threshold with an absent/small lower wick; STRONG
DOWN mirrors that with the upper wick. These are descriptive shape classes.

TradingView native volume candles vary in thickness with volume. This overlay
implements a documented, adjustable RVOL width rule:

`width = clamp(widthAtOne × RVOL^exponent, 0.06, 0.92)`

The baseline uses prior bars only. Missing volume/baseline stays N/A with neutral
styling; zero current volume is valid. The overlay does not reproduce native
TradingView candle spacing or its proprietary width calculation.

Volume-width mode draws up to the latest retained candles within the last N host
bars (60 by default, 1–100 effective capacity); session gaps stay blank. Turn **Volume-scaled candle bodies** off for full-history
uniform-width plotcandle rendering. Off and plain Heikin Ashi modes continue to
work. Volume-width objects preserve candle open/high/low/close; only horizontal
width changes. HA has synthetic OHLC, while Price + relative volume uses the
requested market OHLC.

The existing **Divide recent bodies by volume proxy** control now splits variable
bodies into teal/rose sections. Their proportions use close location within the
market high-low range. Direction remains visible in the wick and body outline.
This is an OHLC allocation proxy, not measured buy/sell aggressor volume or a
footprint. Disabling the split restores one direction-colored body. The board
also shows the feed's volume type, such as base, quote, tick or n/a.

## FVG spans and screening

The screen tracks the latest confirmed bullish and bearish zones independently,
matching the original strategy's two active zone slots. It is a chart-local
screen, not a multi-symbol scanner or a new trade-approval engine.

| Field | Meaning |
| --- | --- |
| Outer envelope | Original gap boundaries, retained as faint context |
| Filled spans | Remaining unmitigated price interval; shrink monotonically |
| Midpoint | Original gap's 50% level, independent of later penetration |
| Remaining | Unmitigated interval / original interval, as a percentage |
| Birth width | Original interval normalized by ATR when the zone was created |
| Birth RVOL | Original chart-volume RVOL when the zone was created |
| Distance | Current price's nearest distance to the original interval, divided by current ATR |
| Age / touch bars | Bars since birth / number of distinct overlapping bars after formation |
| Birth structure | Original BOS/CHoCH gate status at formation |
| Current role | Bullish demand/bearish supply; inverted resistance/support |

For a bullish gap, the remaining upper span follows the lowest subsequent low,
bounded by the original bottom. For a bearish gap, the remaining lower span
follows the highest subsequent high, bounded by the original top. The formation
bar does not mitigate itself. Recalculations cannot count the same touch bar
twice. Crossing a gap by a price jump can exhaust this geometric measure; it does
not prove that volume traded at every intervening price.

Lifecycle states are NONE, FRESH, PARTIAL, MITIGATED, INVERTED and EXPIRED. Close-
based inversion and lifetime rules come from the existing main. Inverted clouds
use the original interval in violet; their remaining percentage still describes
the original gap's mitigation, not a refilled inventory of orders. Cloud thickness
and opacity describe geometry, not measured liquidity, demand size or probability.

A new same-side zone replaces that side's packet. Historical plot samples remain,
with a one-bar plot break at a new zone so unrelated gaps are not bridged. The new
cloud begins displaying after its formation bar; the screen identifies it at the
confirmed formation close. These are causal evolution plots, not retrospectively
redrawn flat rectangles. **Original spans** retains the prior geometry and optional
legacy FVG boxes. The numeric screen controls apply to Mitigation cloud mode.

The display filters are:

- **All live:** unexpired unmitigated zones plus allowed inverted zones.
- **Passing screen:** only zones that pass the configured geometry/context rules.
- **All tracked:** show the two tracked zones even when mitigated or expired, for inspection.

The screen reports the first rejection reason: NO ZONE, AGE, INVERTED, MITIGATED,
REMAINING, WIDTH, RVOL, DISTANCE or STRUCTURE. A positive minimum RVOL rejects
missing volume; zero disables that filter. Inverted zones bypass the original
remaining-percentage rule because their displayed role has changed. Screen
settings never feed LONG/SHORT approvals or limit placement/cancellation.

## Applied visual and performance guidance

| TradingView reference | Application in this revision |
| --- | --- |
| [Visuals overview](https://www.tradingview.com/pine-script-docs/visuals/overview/) | Serial clouds use plots; variable-width bodies use drawing objects; explicit visual ordering |
| [Plots](https://www.tradingview.com/pine-script-docs/visuals/plots/) | Global-scope plots with conditional na and line breaks; diagnostics in Data Window |
| [Fills](https://www.tradingview.com/pine-script-docs/visuals/fills/) | Matching plot-ID pairs; fillgaps=false prevents gaps being painted across hidden zones |
| [Profiling and optimization](https://www.tradingview.com/pine-script-docs/writing/profiling-and-optimization/) | Two tuple requests, bounded recent drawings, recycled IDs/setters, last-bar table rendering |
| [Line wrapping](https://www.tradingview.com/pine-script-docs/writing/style-guide/#line-wrapping) | New long call signatures and cloud calls wrap arguments inside parentheses |
| [Non-standard chart data](https://www.tradingview.com/pine-script-docs/concepts/non-standard-charts-data/) | ticker.heikinashi plus same-timeframe request.security with explicit gaps/lookahead |
| [Volume candles](https://www.tradingview.com/support/solutions/43000724995-understanding-volume-candle-charts/) | Volume-dependent body thickness with an explicitly stated overlay normalization |

Resource estimate: **61 / 64 plot slots**, including inherited alertconditions.
The new candle pool uses at most 200 boxes and 100 wick lines. It updates existing
IDs on recalculation and recycles the oldest on new bars. New cloud visuals add
no boxes/labels; their two state packets do not scan historical arrays. Tables
render on the last bar. The full script declares 300 boxes, 500 lines and 400
labels, accommodating existing registries plus this pool.

The strategy's calculation schedule is unchanged: normally bar close, with
fill-triggered recalculations. This is not an every-tick live candle renderer.
No measured profiler results are claimed. Use Pine Editor's Profiler to compare
identical chart ranges with width on/off; inspect request, table and drawing costs.

## Publication and checks

Update/publish **EP_Candles** from `libraries/EP_Candles_v2.pine` and publish the
new **EP_CloudScreen** from `libraries/EP_CloudScreen_v1.pine`. Set actual assigned
versions in imports.json, run `python3 tools/configure_imports.py`, and use the
synchronized main. Configured Candles/2 and CloudScreen/1 are local contracts,
not verified remote publications. Existing EP_Types/4 consistency is retained.

34 local source/contract checks pass, including unchanged order code, retained
original inputs and source hashes, consistent imports, plot budget, explicit HA
feed settings, cloud state bounds and drawing reuse. These do not execute Pine.
TradingView compilation, rendering, replay, profiler timing and Strategy Tester
comparison are still required.

Check a regular-session symbol, a symbol without reliable volume, zero volume,
a fresh/partial/fully mitigated/inverted gap, zone replacement and expiry. Compare
the trade list before/after toggling every new visual control; it should remain
unchanged under identical chart, strategy and existing execution-gate settings.
