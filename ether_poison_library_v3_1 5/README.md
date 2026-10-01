# ETHER POISON — Library Edition v3.3 — Volume Candles / FVG Clouds

**CE10123 repair:** shared EP_Types imports now use `/4`, matching the reported
Kinetic enum. See `docs/FIX_CE10123_TYPES4.md` for required library republishing
and main import updates. Remote dependent version numbers remain unverified.

The attached `Pasted text(20260925-174208).txt` is the authoritative main snippet.
The working entry point is **`master/ETHER_POISON_LIBRARY_MASTER.pine`**. It uses
13 real library imports and their exported APIs. The standalone preview is an
optional local inspection artifact, not the canonical editing workflow.

## Preservation contract

- Every original main input declaration and default remains unchanged.
- Original Market broker commands, eligibility, sizing and rescue state are retained.
- A selectable Limit path adds pending entry/rescue lifecycle and fill-based counters.
- The complete pre-change main is retained in `docs/baselines/`.
- All original named state values remain available in the main, including
  intermediate cloud values returned by the extracted functions.
- Existing non-FVG plots, fills, lines and boxes are retained; FVG geometry now
  has selectable original/mitigation modes and a visual screen.
- The rescue table displays the selected engine counter.
- All ten commented-out shape statements remain commented out.
- The previous Math/Types/Engines/Nocturne public export names remain available.
- All 22 earlier attachments plus this new main are retained byte-for-byte in
  `originals/`; their hashes are recorded in `SOURCE_MANIFEST.json`.

Signal formulas/defaults remain intact with the new candle gate off, its default.
Execution equivalence after extraction still needs TradingView comparison;
source-preservation tests are not a substitute for runtime validation.

## Active libraries

| Library | Role in the main or engine dependencies |
| --- | --- |
| EP_Math | Existing numerical API, plus explicit historical denominator/saturation variants |
| EP_Types | Existing enums/packets, plus the RescueState from the original shard |
| EP_Engines | Existing facade/API and multiplexer; delegates shared engines to shard libraries |
| EP_KineticCore | Kinetic magnitude, flow, toxicity and conviction; current and historical formulas |
| EP_SpatialGates | Current gap detection; legacy standing-structure and permission diagnostics |
| EP_RescueEngine | Current bounded sizing through Engines; original recovery and profitable-harvest APIs |
| EP_SpaceTimeGrid | Current classical/rapid/volatility/SuperBoll/wave calculations plus original alternatives |
| EP_Nocturne | Existing atmosphere helpers and promoted palette/cockpit/geometry APIs |
| EP_Candles | Retained HA/RVOL API, candle anatomy, volume width and recycled candle drawings |
| EP_ExecutionLedger | Caller-owned fill records, bounded BUY/SELL labels, partial exits and rescue entries |
| EP_MarketOrders | Original market entry/rescue eligibility; main-owned order dispatch |
| EP_LimitOrders | Passive limit prices, persistent ticket, arming, cancellation and fill lifecycle |
| EP_CloudScreen | Independent FVG mitigation, lifecycle, screen reasons and visual opacity |

`docs/REUSE_MAP.md` explains how every supplied source contributes.
`docs/API_CATALOG.md` lists actual exported signatures and types.
`docs/SOURCE_INPUTS.json` retains the input declarations of every historical source,
including controls that were declared but never used in the original prototypes.

## New requested functionality

**Market / Limit toggle:** Select the engine in **08B | Market / Limit engines**.
Limit mode submits resting primary/rescue orders upfront, freezes price and
quantity, and confirms fills from actual emulator trade records. Pending lines,
PLACED/FILLED/CANCELLED/EXPIRED labels and a state board expose its lifecycle.
FVG midpoint/edge, ATR and tick pricing are available. Existing protective exits
stay active. Read `docs/ORDER_ENGINES.md` for settings, timing and verification.

**Transaction labels:** The main snapshots `strategy.opentrades` and all newly
available `strategy.closedtrades` records. The ledger library renders them at
recorded times/prices. Labels include side, quantity, entry ID and exit/P&L
information. Rescue entries and partial closures are separate trade legs.
A reversing order can therefore produce an exit label plus an entry label.
These are TradingView broker-emulator records, not live broker confirmations.

The ledger is passive: no library can submit, cancel, or change a strategy order.
Its ordinary-var state and drawings share rollback. Old labels are removed at the
retention limit. Overlapping fills can have overlapping labels; tooltips identify
legs. Same-time/order-ID re-entries and fill recalculation need replay verification.

**HA + volume candles / FVG cloud screening:** The default display now requests
TradingView's Heikin Ashi feed with explicit gaps and no lookahead. RVOL scales
recent candle widths; anatomy classifies strong bodies and absent opposite wicks.
Mitigation clouds retain original envelopes/midpoints and shrink as price enters
the gap. A separate screen exposes state, remaining %, birth ATR width/RVOL,
distance, age, touch bars and structure context. The trade engines remain intact.

Start with `docs/VOLUME_FVG_CLOUDS.md` for settings, precise formulas, display
limits, the five requested TradingView visual/performance references, publication
steps and replay checks. Volume-width mode retains 60 candles by default; turn
it off for full-history uniform candles. Body subdivisions remain an OHLC volume
allocation proxy, not bid/ask delta. Use a standard chart for strategy comparisons.

**Knowledge board:** Historical toxicity, flow, permission, conviction, wave
energy, profitable-harvest proposal, SMA SuperBoll, percent-price rapid ranges,
and historical cloud separation are available alongside current calculations.
These are diagnostics, not extra trading votes. Teal/maroon palette controls are
explicit inputs. Full historical return tuples remain available to later modules.

## Publication and version synchronization

The import versions in `imports.json` are a **local source contract**, not proof
that matching publications exist on TradingView. No libraries were published or
remote source bodies verified here. In particular, the attached master imported
Math/3 and Types/3 while its supplied engine source imported /2: this package
uses one consistent dependency contract throughout.

Publish or update libraries under your account in this order:

1. EP_Math and EP_Types; the independent new EP_MarketOrders and EP_LimitOrders
   can also be published at this stage. EP_CloudScreen is independent too.
2. EP_KineticCore, EP_SpatialGates, EP_RescueEngine, EP_SpaceTimeGrid,
   EP_Nocturne, EP_Candles, EP_ExecutionLedger, after resolving their imports.
3. EP_Engines, which depends on Math, Types, KineticCore, RescueEngine and SpaceTimeGrid.
4. Save the imported main after all dependencies resolve.

After TradingView assigns each actual publication version, edit that library's
version (and the owner if needed) in `imports.json`, then run:

```sh
python3 tools/configure_imports.py
```

This synchronizes all downstream imports locally. Use the synchronized sources
for the next publication. Do not assume a `_v3.pine` filename receives `/3` when
published. In particular, an existing publication version is immutable.

To produce an optional standalone from those exact sources and run local checks:

```sh
sh build_preview.sh
sh audit.sh
```

The imported master remains the source of truth. The bundler reads actual
libraries, resolves dependencies, preserves string/comment content and rejects
unmapped imports or cycles. It is not a Pine compiler.

For this visual upgrade, publish the updated EP_Candles and new EP_CloudScreen
using actual assigned versions. Other library source bodies are unchanged.

## Validation and next step

All 34 local source/API and order-contract checks pass; the conservative visual estimate is
61 of 64 plot slots, including the four inherited alertcondition declarations.
TradingView compilation, optimized token counts, rendering and broker-emulator
behavior remain **unverified**. See `validation.log` and `STATIC_AUDIT.json`.

Compile the libraries in dependency order, then the main. For a quick check before
publication, paste `ETHER_POISON_LIBRARY_PREVIEW.pine` into Pine Editor. Compare
Strategy Tester records and labels for a long, short, rescue, partial harvest,
reversal and several closures on one bar. With the candle gate off, toggling
visual options should leave the trade list unchanged.

Market mode preserves the prior fill-recalculation behavior, including repeated
orders and rescue resets. Limit mode has separate persistent state and counts
rescue cycles on confirmed fills. Use `docs/ORDER_ENGINES.md` for the pending-order
acceptance cases and compare both modes with Strategy Tester.

Inherited alertcondition declarations are kept as source knowledge; for strategy
transactions use an **Order fills only** alert. A message can use these built-in
placeholders without changing order code:

```json
{"id":"{{strategy.order.id}}","action":"{{strategy.order.action}}","quantity":"{{strategy.order.contracts}}","price":"{{strategy.order.price}}","ticker":"{{ticker}}","position":"{{strategy.market_position}}"}
```

No external webhook, broker connection, or live trading integration is configured.
