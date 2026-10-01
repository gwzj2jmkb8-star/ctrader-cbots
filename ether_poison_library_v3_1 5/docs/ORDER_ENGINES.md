# Market / Limit order engines — v3.2

Select **08B | Market / Limit engines → Entry and rescue order type**.
The default is **Market**. The toggle controls primary entries, reversals and rescue
adds. Existing ATR stop-loss/take-profit brackets stay active in either mode.
Energy harvest remains a market reduction; it does not wait for a limit touch.
Changing an input causes TradingView to recalculate the strategy's history.

## Separate engines and reused infrastructure

`EP_MarketOrders` exports existing entry/rescue eligibility. The main dispatches
the original LONG/SHORT, RESCUE and ENERGY_HARVEST market commands, using the
original market rescue state and sizing. Its previous repeated-order and rescue
reset behavior is retained. The pre-change main is archived in
`baselines/V3_1_BEFORE_ORDER_MODES.pine`.

`EP_LimitOrders` exports a Ticket, price selection, passive-price validation,
cancellation rules and lifecycle transitions. It has no EP_Types dependency.
The main submits/cancels real Pine strategy orders and supplies actual trade
records. Neither library issues strategy orders or owns user inputs.

All ten earlier libraries remain imported and used. Limit rescue sizing reuses
`EP_Engines.rescueQuantity`, delegating to the existing RescueEngine. It uses the
planned limit price to calculate size. No signal weights or existing input
defaults have been changed. The shared EP_Types/4 contract remains consistent.

## Limit workflow

1. At an approved, confirmed setup close, choose a passive price. Approval uses
   the existing FVG, structure, score and optional candle/multiplexer gates.
2. Submit `strategy.entry(..., limit=...)` immediately, before a future touch.
   This is not a market order generated after inspecting a candle's high/low.
3. Freeze the submitted price and quantity. Keep one pending entry **or** rescue
   at a time; do not chase the price or replenish it every bar.
4. The broker emulator can fill the resting limit at its price or better.
   Match its unique order ID in open/closed trade records before marking FILLED.
   Actual fills take precedence over cancellation conditions observed afterward.
5. Cancel an unfilled order when its lifetime ends, its position context changes,
   harvest is due, or enabled opposite-approval/FVG cancellation rules fire.
   No same-bar replacement follows a fill, cancellation or expiry.

| Control | Default | Meaning |
| --- | --- | --- |
| Order type | Market | Select Market or Limit for entries and rescue adds |
| Entry price | FVG midpoint | Alternative: FVG near edge, ATR offset, tick offset |
| ATR entry offset | 0.25 ATR | Buy below close; sell above close |
| Tick entry offset | 10 ticks | Used only with tick pricing |
| Pending lifetime | 12 bars | Expiry at the expiry bar's close if still unfilled |
| Cancel on opposite setup | On | Cancel the current ticket when the opposite setup is approved |
| Cancel on FVG change | On | Applies only to FVG-priced primary entries |
| Allow limit reversal | On | Opposite-side entry reverses existing exposure upon fill |
| Rescue offset | 0.25 ATR | Separate passive price for same-side rescue adds |
| History retention | 40 labels | Bounded to 1–80; live pending label is separate |

For buys, the near edge is the bullish FVG top; for sells it is the bearish FVG
bottom. Buy prices round down to a tick; sell prices round up. If the requested
price is already marketable, unavailable or nonpositive, skip the order and show
the blocked plan in the board. ATR/tick pricing still requires the original
approved setup; it does not bypass entry gates.

Primary size is frozen from `strategy.default_entry_qty(plannedPrice)` using the
strategy's Properties. A reversal entry's transaction quantity also closes the
old position; the displayed target quantity describes the newly opened side.
Rescue quantities are additions and count toward the rescue cycle limit only
when the trade actually fills. Expired/cancelled rescue tickets do not consume a
cycle. Limit rescue cooldown starts at the fill bar and has independent state.

In Limit mode, harvest cancels pending orders first, then uses `strategy.close`
on open legs to reduce exposure. It executes at most once per bar and skips
fill-triggered recalculations. The original Market harvest remains unchanged.

## Labels and fills

- A dashed line and BUY LIMIT PENDING / SELL LIMIT PENDING label show the active price.
- PLACED, FILLED, CANCELLED and EXPIRED history labels identify the lifecycle.
- FILLED uses the emulator's recorded entry price/time, including a better gap fill.
- Tooltips identify the unique order ID, entry/rescue kind, quantity and reason.
- Existing transaction-ledger labels still show entries and exits independently.
- The order board shows engine, state, requested price/quantity and remaining bars.

Ticket fields explicitly use `varip` so Pine rollback cannot restore stale order
state after a fill. Drawings use ordinary `var` and are recreated from the ticket
event when rollback removes a temporary drawing.

## Timing boundaries

The declaration uses `backtest_fill_limits_assumption=0` for ordinary touch-based
limit fills. Properties can override this with a stricter verification setting.
The emulator's intrabar path, Bar Magnifier, available margin and symbol rules
still determine whether an order fills. A touch is not itself a fill record.

Cancellation takes effect when the strategy calculates. It cannot undo a fill
that occurred before that calculation. With normal settings, expiry and
opposite/FVG checks occur at bar close; position-change cancellation also runs
on fill recalculation. Existing stops and targets remain active while a reversal
or rescue limit waits. They may close the position before its pending ticket
can be cancelled; same-bar ordering must be checked in TradingView.

These labels are strategy/broker-emulator events, not external broker execution
acknowledgments. No live order routing was added.

## Publication and validation

Publish the new `EP_MarketOrders_v1.pine` and `EP_LimitOrders_v1.pine` libraries,
then set their actual assigned versions in `imports.json` and run
`python3 tools/configure_imports.py`. The configured `/1` values are local source
contracts. Keep the actual published versions of existing dependencies synchronized
as described in README.md. Publishing a corrected dependent library creates a
new version; an old Nocturne publication does not inherit the local Types/4 fix.

Local checks cover source preservation, imports, deterministic bundling, plot
budget and order-lifecycle source invariants. They do not execute Pine. TradingView
compilation and the following broker-emulator comparisons remain required:

| Case | Expected result to verify in TradingView |
| --- | --- |
| Market baseline | Same trade list as archived v3.1 under identical chart/Properties |
| Untouched long/short | Pending label appears at placement; no filled label before a recorded fill |
| Limit touch / gap through | Requested price stays fixed; FILLED matches actual trade price |
| Lifetime expiry | One cancellation at expiry close; no fill on a later touch |
| Opposite/FVG invalidation | Pending ID cancelled; disabled cancellation option leaves it pending |
| Filled rescue | One cycle increments at fill; cancelled rescue consumes no cycle |
| Stop/target while waiting | Pending ticket cancelled on position change, no stale add on a later bar |
| Reversal | Existing protection remains while waiting; opposite exposure opens at limit fill |
| Same-bar recalculations | No duplicate ticket or cycle; labels agree with Strategy Tester records |

Implementation references: [TradingView strategies](https://www.tradingview.com/pine-script-docs/concepts/strategies/)
and [UDT field persistence](https://www.tradingview.com/pine-script-docs/language/objects/).
