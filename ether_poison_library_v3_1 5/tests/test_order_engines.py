"""Source contract checks, not a Pine runtime or broker-emulator simulation."""
from pathlib import Path
import re
import unittest
ROOT = Path(__file__).resolve().parents[1]
MAIN = (ROOT/'master/ETHER_POISON_LIBRARY_MASTER.pine').read_text()
LIMIT = (ROOT/'libraries/EP_LimitOrders_v1.pine').read_text()

class OrderEngineContracts(unittest.TestCase):
 def test_toggle_defaults_to_market_and_both_paths_are_gated(self):
  self.assertIn('input.string("Market", "Entry and rescue order type", options=["Market", "Limit"]', MAIN)
  self.assertIn('if orderMode == "Market"\n    if epMarket.entryEligible', MAIN)
  self.assertIn('if orderMode == "Limit" and epLimit.canArm', MAIN)

 def test_resting_orders_are_submitted_with_prices_before_any_touch(self):
  block=MAIN[MAIN.index('if orderMode == "Limit" and epLimit.canArm'):MAIN.index('// Shared protective brackets')]
  self.assertEqual(block.count('limit=plannedPrice'),2)
  self.assertEqual(block.count('epLimit.arm('),2)
  self.assertNotRegex(block,r'\b(?:high|low)\s*[<>]')
  self.assertEqual(block.count('epLimit.isPassive('),2)
  self.assertIn('strategy.default_entry_qty(plannedPrice)', block)
  self.assertIn('isLong ? math.floor(raw / tick) * tick : math.ceil(raw / tick) * tick', LIMIT)

 def test_live_ticket_is_frozen_and_cannot_rearm_same_bar(self):
  self.assertIn('not ticket.active and confirmed and not fillPass and (na(ticket.eventBar) or bar > ticket.eventBar)', LIMIT)
  self.assertEqual(LIMIT.count('ticket.price :='),1)
  self.assertEqual(LIMIT.count('ticket.quantity :='),1)
  self.assertNotRegex(MAIN,r'limitTicket\.(?:price|quantity)\s*:=')
  self.assertIn('ticket.serial += 1',LIMIT)
  self.assertIn('ticket.expiryBar := bar + math.max(lifetime, 1)',LIMIT)

 def test_all_ticket_fields_survive_rollback(self):
  declaration=LIMIT.split('export type Ticket\n',1)[1].split('\nexport create',1)[0]
  fields=[line for line in declaration.splitlines() if line.strip()]
  self.assertEqual(len(fields),17)
  self.assertTrue(all(re.match(r'    varip (bool|int|float|string) \w+',line) for line in fields))
  self.assertIn('varip epLimit.Ticket limitTicket',MAIN)
  self.assertIn('var int paintedLimitEvent',MAIN)
  self.assertNotIn('varip label',MAIN)

 def test_confirmed_fills_win_over_cancel_and_use_trade_ids(self):
  resolve=MAIN[MAIN.index('// Resolve fills FIRST.'):MAIN.index('// MARKET ENTRY ENGINE')]
  for kind in ('opentrades','closedtrades'):
   self.assertIn(f'strategy.{kind}.entry_id(leg) == limitTicket.id',resolve)
   self.assertIn(f'strategy.{kind}.entry_price(leg)',resolve)
   self.assertIn(f'strategy.{kind}.entry_time(leg)',resolve)
  self.assertNotRegex(resolve,r'\b(?:high|low)\s*[<>]')
  self.assertLess(MAIN.index('epLimit.finish(limitTicket, "FILLED"'),MAIN.index('strategy.cancel(limitTicket.id)'))

 def test_limit_rescue_counts_fills_and_sizes_at_requested_price(self):
  self.assertEqual(MAIN.count('limitRescueCycles += 1'),1)
  self.assertIn('if limitFillDetected\n    if limitTicket.kind == "RESCUE"\n        limitRescueCycles += 1',MAIN)
  self.assertIn('strategy.position_avg_price, plannedPrice, rescueTargetAverage, positionLong, rescueMaximumMultiple)',MAIN)
  self.assertIn('limitRescueCycles < rescueMaximumCycles',MAIN)
  self.assertIn('bar_index - limitLastRescueBar >= rescueCooldown',MAIN)

 def test_actual_cancellation_precedes_ticket_finalization(self):
  cancel=MAIN[MAIN.index('if limitTicket.active and (orderMode'):MAIN.index('float plannedBuyLimit')]
  self.assertLess(cancel.index('strategy.cancel(limitTicket.id)'),cancel.index('epLimit.finish'))
  for cause in ('Position changed','Expired','Opposite approval','FVG replaced or invalidated','Harvest'):
   self.assertIn('"'+cause+'"',LIMIT)
  self.assertIn('limitCancelOnZoneChange and limitIsFvgPrice and limitTicket.kind == "ENTRY"',MAIN)

 def test_limit_harvest_reduces_position_and_cancels_pending_first(self):
  self.assertLess(MAIN.index('strategy.cancel(limitTicket.id)'),MAIN.index('if limitHarvestDue\n'))
  harvest=MAIN.split('if limitHarvestDue\n',1)[1].split('int displayedRescueCycles',1)[0]
  self.assertIn('strategy.close(',harvest)
  self.assertNotIn('strategy.order(',harvest)
  self.assertIn('not limitFillPass',MAIN)

 def test_status_labels_distinguish_pending_from_filled(self):
  self.assertIn('BUY LIMIT PENDING',MAIN)
  self.assertIn('SELL LIMIT PENDING',MAIN)
  self.assertIn('limitTicket.eventPrice',MAIN)
  self.assertIn('limitTicket.event == "CANCELLED" or limitTicket.event == "EXPIRED"',MAIN)
  self.assertIn('while array.size(limitEventLabels) >= limitLabelCapacity',MAIN)
  self.assertIn('backtest_fill_limits_assumption=0',MAIN)

if __name__ == '__main__': unittest.main()
