# Reuse map: knowledge retained as callable infrastructure

## Existing API union

Every exported name in supplied EP_Math/EP_Math_v2, EP_Types/EP_Types_v2,
EP_Engines/EP_Engines_v2 and EP_Nocturne_v1 is retained in its corresponding
library. Earlier files are versions of the same APIs, so importing both versions
into the main would create duplicate and potentially incompatible types.
The merged public contract preserves the names and distinguishes variant math
under explicit additional names.

| Source or family | Reusable home | Integration |
| --- | --- | --- |
| New attached main | master/ETHER_POISON_LIBRARY_MASTER.pine | Authoritative base; exact inputs, orders and drawings retained |
| EP_Math.pine + EP_Math_v2.pine | EP_Math | All exports retained; original formulas retained |
| EP_Types.pine + EP_Types_v2.pine | EP_Types | All enums, type fields and helpers retained |
| EP_Engines.pine + EP_Engines_v2.pine | EP_Engines | All exports retained; shared math delegated to shard libraries |
| EP_Nocturne_v1.pine | EP_Nocturne | Density/alpha/direction/spectral/glyph APIs retained |
| 01_KINETIC_CORE.pine | EP_KineticCore | f_calculate_magnitude, f_determine_flow, f_map_toxicity and state exports |
| 02_SPATIAL_GATES.pine | EP_SpatialGates | f_detect_fvg, f_get_spatial_permission, legacyStructure and legacyPermission |
| 03_RESCUE_ENGINE.pine | EP_RescueEngine + EP_Types.RescueState | Recovery proposal, quantity equation, adverse window and profitable harvest |
| 04_SPACE_TIME_GRID.pine | EP_SpaceTimeGrid | Both original and current cloud, wave and reversion conventions exported |
| jade.pine | Same four shard libraries + EP_Nocturne | Its concatenated algorithms are promoted once, rather than duplicated |
| NOCTURNE_VISUALS.pine | EP_Nocturne | f_get_toxicity_color, f_render_nocturne, f_render_cockpit; caller-controlled palette/table |
| ETHER_POISON_MASTER.pine | KineticCore, SpatialGates, RescueEngine, SpaceTimeGrid, Nocturne | Embedded routines exposed as reusable APIs; old order preset remains documented in originals |
| ETHER_POISON_FINAL.pine | Same shards + EP_Math legacy variants | Embedded equations preserved; invalid local-scope plot placement not copied |
| ETHER_POISON_FINAL_MERGED.pine | Same shard exports | Duplicate historical composition preserved in originals, not competing live orders |
| ETHER_POISON_FINAL_FIXED.pine | Same shard exports | Historical rescue and cloud conventions retained; no incompatible order merge |
| ETHER_POISON_IMPORTED.pine | New attached main supersedes it | Its advanced matrix/cloud/Nocturne infrastructure remains in the main |
| ETHER_POISON_STANDALONE.pine | EP_ExecutionLedger + generated preview workflow | Fill-labelled experience extended into a real library; averages/latest-exit-only logic replaced in observer only |
| README.md | README + this map | Modular workflow retained and expanded |
| build_standalone.sh | tools/build.py + build_preview.sh | Actual library bodies bundled; hard-coded copied formulas removed |
| audit.sh | tools/audit.py + tests + audit.sh | Source/API preservation, resource estimates and dependency checks |

## Historical distinctions retained explicitly

- Math `safeDiv` preserves the sign of a tiny negative denominator;
  embedded `f_safeDiv` did not. `legacySafeDiv` retains that difference.
- Math `kinetic` uses absolute scale; embedded `f_kinetic_sat` does not.
  `legacyKineticSaturation` retains it.
- Early Engines kinetic uses an epsilon ATR floor, while the original kinetic
  shard and Final use mintick. `legacyMagnitude`, `f_calculate_magnitude` and
  `finalKineticState` distinguish those conventions.
- The original spatial shard uses strict gap-size `>` and standing level
  conditions. The attached main uses `>=`, crossover events, age gates and
  inversion state. `f_detect_fvg`/`legacyStructure` preserve the former;
  `detectFvg` and the main's existing state machine preserve the latter.
- The original Final helper takes three spatial settings; the shard helper
  takes six booleans. They are exposed as `legacyPermission` and
  `f_get_spatial_permission`, respectively, avoiding an ambiguous contract.
- Original rescue functions mutated hidden state. Reusable exports now require
  caller-owned `RescueState` or a cycle count, explicitly returned/updated.
  They calculate quantities; they do not place orders. The attached main's own
  Market rescue state and bounded-size path remain unchanged. Limit rescue
  reuses the bounded sizing function at its planned fill price.
- Legacy harvest requires profit and magnitude below 0.3. Current main harvest
  uses a crossing below 0.22. Both survive; legacy output is diagnostic only.
- Original rapid clouds expand by percent of price; current rapid clouds use
  ATR and kinetic energy. Original SuperBoll uses SMA; current main uses EMA.
  `legacyGrid`, `rapid` and `reversion` preserve these distinctions.
- Historical clouds use `[displacement]`; forward display uses plot offset.
  `historicalCloud` exposes the former without replacing the main's offset.
- Nocturne's old renderer selected local colors but did not return/draw the
  geometry. The new export returns all geometry/color values for caller rendering.
  Its cockpit receives the caller's table so it cannot overwrite the main board.

## Knowledge that was descriptive, not implemented

The original EP_Engines comment block proposes a five-dimensional oscillator,
volume consumption within FVGs, cloud angle/trend/fill state, Supertrend and
regression components. These are preserved verbatim in originals and the source
index. They are not claimed to be working engines. The old `wave_sensitivity`
input was declared but unused; it is retained in SOURCE_INPUTS.json rather than
being silently assigned a new mathematical meaning.

Future extensions should add exported state packets for cloud geometry/mitigation,
then feed explicitly weighted signals into the existing score. Their initial
weights should remain zero until enabled. First obtain TradingView compilation
and a fixed trade-list baseline for this library edition.

## Evidence boundary

All main input declarations, original named state, original Market commands, active visual
statements and disabled shapes are checked against the exact new attachment.
The API test checks the union of earlier library export names. It does not assert
that an incomplete prototype was already a working library or that TradingView
published versions match the local files. Runtime equivalence and optimized
compiler limits must be checked in TradingView.

Official references: [Libraries](https://www.tradingview.com/pine-script-docs/concepts/libraries/),
[Type system](https://www.tradingview.com/pine-script-docs/language/type-system/),
[Strategies](https://www.tradingview.com/pine-script-docs/concepts/strategies/),
[Limitations](https://www.tradingview.com/pine-script-docs/writing/limitations/).

## v3.2 order extension

EP_MarketOrders retains original eligibility. EP_LimitOrders adds an independent
resting-ticket lifecycle while reusing Engines/RescueEngine sizing. All ten
previous library imports remain active; two order libraries are added. The
original main and pre-extension revision remain available. See ORDER_ENGINES.md.

## v3.3 visual extension

All twelve prior libraries remain active. EP_Candles preserves every earlier
export and adds geometry/recycled width drawings. EP_CloudScreen is the thirteenth
import, with independent mitigation and screening state. Existing scoring and
Market/Limit code are unchanged. The v3.2 main and earlier Candle source are
archived in baselines. See VOLUME_FVG_CLOUDS.md for interpretation and references.
