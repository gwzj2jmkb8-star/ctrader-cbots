from pathlib import Path
import importlib.util,re,json,hashlib,unittest
ROOT=Path(__file__).resolve().parents[1]
def load(name):
 spec=importlib.util.spec_from_file_location(name,ROOT/'tools'/f'{name}.py');m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m
build=load('build');audit=load('audit')
contracts=load('check_imports')
OLD=(ROOT/'originals/ATTACHED_MAIN.pine').read_text()
NEW=(ROOT/'master/ETHER_POISON_LIBRARY_MASTER.pine').read_text()
def order_block(source):
 start=source.index('if longApproved and strategy.position_size <= 0')
 end=source.index('\n\n',source.index('strategy.order("ENERGY_HARVEST"',start))
 return source[start:end]
def exports(source):
 return set(re.findall(r'^export (?:(?:enum|type) )?(\w+)',source,re.M))
class Preservation(unittest.TestCase):
 def test_shared_dependency_contract(self):
  self.assertEqual(contracts.check_project(),[])
 def test_rejects_reported_nocturne_types_split(self):
  config=json.loads((ROOT/'imports.json').read_text())
  fixture={'Nocturne.pine':'import ClassicPotatoChamberXII/EP_Types/3 as epTypes',
           'Kinetic.pine':'import ClassicPotatoChamberXII/EP_Types/4 as epTypes'}
  errors=contracts.check_sources(fixture,config)
  self.assertEqual(len(errors),1)
  self.assertIn('Nocturne.pine',errors[0])
 def test_all_original_bytes(self):
  manifest=json.loads((ROOT/'SOURCE_MANIFEST.json').read_text());self.assertEqual(len(manifest),23)
  for item in manifest:self.assertEqual(hashlib.sha256((ROOT/item['file']).read_bytes()).hexdigest(),item['sha256'])
 def test_market_order_commands_preserved(self):
  # Routing may change; each original broker command must remain verbatim.
  for line in order_block(OLD).splitlines():
   if re.search(r'strategy\.(entry|order|exit)\(',line):
    self.assertIn(line.strip(),NEW)
  market=(ROOT/'libraries/EP_MarketOrders_v1.pine').read_text()
  self.assertIn('approved and (isLong ? positionSize <= 0.0 : positionSize >= 0.0)',market)
  self.assertIn('window and quantity > 0.0',market)
  self.assertIn('if orderMode == "Market" and exhaustion',NEW)
  self.assertIn('if orderMode == "Market" and epMarket.rescueEligible(rescueWindow, rescueQuantity)',NEW)
 def test_market_rescue_state_and_sizing_preserved(self):
  start=OLD.index('var int rescueCycles = 0')
  end=OLD.index('if rescueWindow and rescueQuantity > 0.0',start)
  self.assertIn(OLD[start:end].strip(),NEW)
 def test_pre_order_revision_preserved(self):
  baseline=(ROOT/'docs/baselines/V3_1_BEFORE_ORDER_MODES.pine').read_text()
  self.assertEqual(order_block(OLD),order_block(baseline))
 def test_all_attached_inputs_and_defaults_preserved(self):
  lines=[s for s in OLD.splitlines() if re.search(r'= input\.\w+\(',s)]
  self.assertGreater(len(lines),100)
  for line in lines:self.assertIn(line,NEW)
 def test_scores_preserved(self):
  for name in ['totalWeight','longPoints','shortPoints','longScore','shortScore','scoreReady']:
   line=re.search(rf'^(?:float|bool) {name} = .*$',OLD,re.M).group();self.assertIn(line,NEW)
  for name,gate in [('longApproved','candleLongGate'),('shortApproved','candleShortGate')]:
   old=re.search(rf'^bool {name} = .*$',OLD,re.M).group()
   new=re.search(rf'^bool {name} = .*$',NEW,re.M).group().replace(f' and {gate}','')
   self.assertEqual(old,new)
  self.assertIn('input.bool(false, "Require HA direction',NEW)
 def test_disabled_shapes_preserved(self):
  old=[l for l in OLD.splitlines() if l.lstrip().startswith('//') and 'plotshape(' in l]
  self.assertEqual(len(old),10)
  for line in old:self.assertIn(line,NEW)
 def test_original_state_names_preserved(self):
  pattern=r'^(?:var(?:ip)? )?(?:float|int|bool|string|color|line|box|table|epTypes\.CloudState|array<[^>]+>) (\w+)\s*='
  names=set(re.findall(pattern,OLD,re.M));new=set(re.findall(pattern,NEW,re.M))
  for tuple_names in re.findall(r'^\[([^\]]+)\] =',NEW,re.M):new.update(n.strip() for n in tuple_names.split(','))
  self.assertEqual(names-new,set())
 def test_published_api_union_retained(self):
  for name,newfile,originals in [
   ('Math','EP_Math_v3.pine',['EP_Math.pine','EP_Math_v2.pine']),
   ('Types','EP_Types_v3.pine',['EP_Types.pine','EP_Types_v2.pine']),
   ('Engines','EP_Engines_v3.pine',['EP_Engines.pine','EP_Engines_v2.pine']),
   ('Nocturne','EP_Nocturne_v2.pine',['EP_Nocturne_v1.pine'])]:
   available=exports((ROOT/'libraries'/newfile).read_text())
   for original in originals:self.assertFalse(exports((ROOT/'originals'/original).read_text())-available,name)
 def test_every_library_is_imported_and_used(self):
  imports=build.IMPORT.findall(NEW);self.assertEqual(len(imports),13)
  stripped=audit.masked(NEW)
  for owner,name,ver,alias in imports:
   self.assertRegex(stripped,rf'\b{alias}\.\w+')
   self.assertIn((name,ver),build.CATALOG)
 def test_dependency_rebuild(self):
  out,_=build.build();self.assertEqual(out,(ROOT/'ETHER_POISON_LIBRARY_PREVIEW.pine').read_text())
  for name,ver in build.CATALOG:self.assertEqual(out.count(f'// BEGIN {name}/{ver}'),1)
 def test_compilation_risk_scan(self):
  result=audit.audit(ROOT/'ETHER_POISON_LIBRARY_PREVIEW.pine');self.assertEqual(result['errors'],[])
  self.assertLessEqual(result['conservative_plot_count'],64)
 def test_no_strategy_orders_in_any_library(self):
  for path in (ROOT/'libraries').glob('*.pine'):
   code=audit.masked(path.read_text())
   self.assertNotRegex(code,r'\bstrategy\.')
   self.assertNotRegex(code,r'\binput\.')
 def test_ledger_reads_all_new_closures(self):
  self.assertIn('for tradeIndex = firstNewClosed to strategy.closedtrades - 1',NEW)
  self.assertIn('strategy.closedtrades.exit_time(tradeIndex)',NEW)
  self.assertIn('strategy.opentrades.entry_price(tradeIndex)',NEW)
  source=(ROOT/'libraries/EP_ExecutionLedger_v1.pine').read_text()
  self.assertIn('xloc=xloc.bar_time',source)
 def test_original_visuals_retained(self):
  for line in OLD.splitlines():
   if re.search(r'\b(?:plot|fill|bgcolor|table\.cell|line\.new|box\.new)\(',line) and not line.lstrip().startswith('//'):
    if re.match(r'(?:bullZoneTop|bullZoneBottom|bearZoneTop|bearZoneBottom) = plot',line):
     title=re.search(r'"([^"]+)"',line).group(1)
     self.assertIn('"'+title+'"',NEW)
     continue
    if line.startswith(('fill(bullZoneTop,','fill(bearZoneTop,')):
     self.assertIn(line.split(', title=')[1].rstrip(')'),NEW)
     continue
    # The rescue display now selects the active engine counter.
    self.assertIn(line.replace("str.tostring(rescueCycles)","str.tostring(displayedRescueCycles)"),NEW)
if __name__=='__main__':unittest.main()
