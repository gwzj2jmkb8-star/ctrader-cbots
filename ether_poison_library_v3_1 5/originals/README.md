# ETHER Spatial Energy Matrix

This workspace has two equivalent Pine v6 delivery modes.

- `master/ETHER_POISON_IMPORTED.pine` is the canonical modular strategy. Publish
  the v2 libraries in this strict order: `EP_Math_v2.pine`, `EP_Types_v2.pine`,
  then `EP_Engines_v2.pine`. The engine library imports Math/2 and Types/2.
  The master imports all three `/2` versions. Change version suffixes only if
  TradingView assigns different published versions.
  Publish `EP_Nocturne_v1.pine` afterward; the master imports
  `ClassicPotatoChamberXII/EP_Nocturne/1` for density-aware rendering.
- `ETHER_POISON_STANDALONE.pine` contains the required library logic internally
  and needs no imports. Generate it with `./build_standalone.sh` after changing
  the imported master.

The standalone file is generated output. Make strategy changes in the imported
master and reusable calculations in the libraries, then rebuild standalone.

The visual field includes two forward clouds, a bounded projected price/time
grid, a forward wave vector, active FVG projection boxes, position geometry,
bounded BOS/CHoCH/iFVG event tags, and a four-column market-state board.

The extended projection layer adds Fibonacci depth rails, a structure-anchored
Gann fan, 9/17/26/33/42/65 time-theory rails, persistent FVG-to-iFVG zone
recoloring, score agreement/conflict/edge telemetry, and Compact/Full dashboard
density. These are render-only derivatives and do not alter order decisions.

The cloud suite also includes a two-length Volatility Cloud whose fast and slow
spans are contoured by ATR-scaled directional energy. Every visible plot family
has editable colors; cloud/FVG families expose plot shape and width, while
projected line drawings expose color, solid/dashed/dotted style, and width.

The v2 Multiplexer Cloud measures binary return entropy over fast, medium, and
slow horizons. Each lane receives an explicit inverse-entropy density multiplied
by `temp_atten`; the cloud center and direction are density-weighted, while its
width combines entropy, directional magnitude, ATR contour, and attenuation.
Its score weight defaults to zero, preserving pre-v2 decisions until explicitly
enabled by weight.

The master orchestrator can now optionally require Multiplexer direction and
quality before approving an entry. This gate defaults off, so existing trade
behavior remains unchanged. Its entropy, confidence, and attenuation limits are
explicit inputs. A bounded forward entropy veil, three density projection rails,
dominant-lane transition tags, and expanded board telemetry expose the same
library values without adding plot slots.

The execution ledger uses bounded chart labels at their actual price levels.
Approved setups show intended entry, stop, and target; fills show average price
and quantity; completed exits show realized profit, percentage return, account
equity, net profit, and closed-trade count. Setup and filled-trade labels have
independent visibility controls and dedicated palette colors.

Nocturne rendering is isolated in `EP_Nocturne_v1.pine`. Entropy, confidence,
and `temp_atten` control atmospheric opacity; direction controls hue. Candle
tinting and event halos remain independently switchable and consume a bounded
number of visual calls.

Run `./audit.sh` after `./build_standalone.sh`. The audit rejects known local
plot-scope problems and enforces a conservative 64-call visual ceiling.

Local checks can verify source structure and plot-call budget. TradingView is
authoritative for compilation, optimized-token count, order behavior, and chart
rendering.
