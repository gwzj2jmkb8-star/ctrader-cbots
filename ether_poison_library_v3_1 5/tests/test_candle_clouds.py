"""Source contracts for the display upgrade; no Pine execution is implied."""
from pathlib import Path
import re,unittest
R=Path(__file__).resolve().parents[1]
M=(R/'master/ETHER_POISON_LIBRARY_MASTER.pine').read_text()
BASE=(R/'docs/baselines/V3_2_BEFORE_CLOUDS.pine').read_text()
C=(R/'libraries/EP_Candles_v2.pine').read_text()
F=(R/'libraries/EP_CloudScreen_v1.pine').read_text()
def compact(s):return re.sub(r'\s+',' ',s).strip()
def orders(s):return s[s.index('// ── Separate order engines:'):s.index('// Snapshot adapter only;')]
class CandleCloudContracts(unittest.TestCase):
 def test_orders_unchanged_by_display_upgrade(self):
  self.assertEqual(orders(M),orders(BASE))
  for name in ('candleLongGate','candleShortGate','longApproved','shortApproved'):
   self.assertEqual(re.search(r'^bool '+name+r' = .*$',M,re.M).group(),re.search(r'^bool '+name+r' = .*$',BASE,re.M).group())
 def test_documented_feed_has_no_future_lookahead(self):
  self.assertIn('ticker.heikinashi(candleBaseTicker)',M)
  self.assertEqual(M.count('request.security('),2)
  self.assertEqual(M.count('lookahead=barmerge.lookahead_off'),2)
  self.assertEqual(M.count('gaps=barmerge.gaps_on'),2)
  self.assertIn('session.regular : syminfo.session',M)
 def test_volume_pool_reuses_drawings_and_bounds_history(self):
  self.assertIn('box.set_lefttop(upperBody',C)
  self.assertIn('upperBody := array.shift(canvas.upperBodies)',C)
  self.assertIn('canvas.lastStamp == stamp',C)
  self.assertIn('math.min(capacity, 100)',C)
  self.assertIn('bar_index >= last_bar_index - math.max(1, math.min(volumeWidthBars, 100)) + 1',M)
 def test_prior_candle_exports_remain_available(self):
  old=(R/'docs/baselines/EP_Candles_v1.pine').read_text()
  names=lambda x:set(re.findall(r'^export (?:type )?(\w+)',x,re.M))
  self.assertFalse(names(old)-names(C))
 def test_cloud_mitigation_is_clamped_monotonic_and_skips_birth(self):
  self.assertIn('bar > cloud.born and ageValid and not cloud.inverted',F)
  self.assertIn('math.max(bottom, math.min(cloud.remainingTop, l))',F)
  self.assertIn('math.min(top, math.max(cloud.remainingBottom, h))',F)
  self.assertIn('cloud.lastTouchBar != bar',F)
  self.assertIn('if not cloud.exists or cloud.born != zoneBar',F)
 def test_cloud_gaps_and_last_bar_screen(self):
  self.assertEqual(M.count('fillgaps=false'),4)
  self.assertIn('bar_index > bullScreenCloud.born',M)
  self.assertIn('bar_index > bearScreenCloud.born',M)
  self.assertIn('if barstate.islast\n    if showFvgScreen',M)
  self.assertIn('if barstate.isconfirmed\n    epCloud.observe(',M)
 def test_filter_can_explain_missing_volume_and_terminal_states(self):
  for reason in ('NO ZONE','AGE','INVERTED','MITIGATED','REMAINING','WIDTH','RVOL','DISTANCE','STRUCTURE'):
   self.assertIn('"'+reason+'"',F)
  self.assertIn('minimumRvol > 0.0 and (na(cloud.birthRvol)',F)
 def test_reserved_text_not_declared(self):
  for source in (M,C,F):self.assertNotRegex(source,r'\b(?:string|float|int|bool)\s+text\s*=')
if __name__=='__main__':unittest.main()
