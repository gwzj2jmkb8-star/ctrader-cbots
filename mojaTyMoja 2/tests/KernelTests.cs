using System;
using System.Collections.Generic;
using System.Linq;
using FunctionalMixtureXI;
class KernelTests
{
    static int passed;
    static void Test(string name, Action test) { test(); passed++; Console.WriteLine("PASS " + name); }
    static void Eq(double a, double b, double tolerance = 1e-9)
    { if (double.IsNaN(a) && double.IsNaN(b)) return; if (!double.IsFinite(a) || !double.IsFinite(b) || Math.Abs(a-b) > tolerance) throw new Exception($"Expected {b:R}, got {a:R}"); }
    static void Check(bool ok, string msg = "assertion failed") { if (!ok) throw new Exception(msg); }
    static readonly DateTime Start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    static Candle B(int i, double close, double volume = 10) => new Candle(Start.AddMinutes(i*15), close-.2, close+.4, close-.5, close, volume);
    static string Rules(IEnumerable<Intent> x) => string.Join(",", x.Select(i => i.Rule));
    static CoreConfig Small() => new CoreConfig { SmoothingPeriod=3, AtrPeriod=2, Wow=3, Ma1=2, Ma2=3, Ma3=4, DiPeriod=2, AdxPeriod=2, WavePeriod=3 };
    static int Main()
    {
        try
        {
            Test("Declared XI defaults preserved", () => { var c=new CoreConfig(); Eq(c.SmoothingPeriod,200); Eq(c.AlmaOffset,.94); Eq(c.AlmaSigma,121); Eq(c.VolPeriod,1); Eq(c.UpperMultiplier,0); Eq(c.ProfitThreshold,400); Check(c.EnablePc && c.RequireFlip); });
            Test("Pine round half-away for Wow/2", () => { Eq(MathSeries.PineRound(7/2.0),4); Eq(MathSeries.PineRound(Math.Sqrt(7)),3); });
            Test("WMA gives largest weight to newest value", () => Eq(MathSeries.Wma(new double[]{1,2,3},2,3),14.0/6));
            Test("WMA missing history is NaN", () => Check(double.IsNaN(MathSeries.Wma(new double[]{1,2},1,3))));
            Test("ALMA follows unfloored offset and correct chronology", () => {
                double[] a={1,2,4,8,16}; double m=.94*4,s=5.0/121,weight=0,total=0;
                for(int k=0;k<5;k++){double w=Math.Exp(-Math.Pow(k-m,2)/(2*s*s));weight+=w;total+=w*a[k];}
                Eq(MathSeries.Alma(a,4,5,.94,121),total/weight,1e-8);
            });
            Test("EMA seeds from first finite sample", () => {Eq(MathSeries.Ema(12,double.NaN,200),12); Eq(MathSeries.Ema(15,12,2),14);});
            Test("RMA seeds using SMA rather than EMA seed", () => {var x=new double[]{2,4,6,8}; Eq(MathSeries.Rma(x,2,3,double.NaN),4); Eq(MathSeries.Rma(x,3,3,4),16.0/3);});
            Test("stdev1 zero and default percentile 100", () => {var k=new XiKernel(new CoreConfig());for(int i=0;i<250;i++){var s=k.Update(i,B(i,100+Math.Sin(i)),new Context(101,100,false,false),.01);Eq(s.Volatility,0);Eq(s.Rank,100);}});
            Test("MAD retains rolling-deviation source formula", () => {var c=Small();c.VolMethod=VolatilityKind.Mad;c.VolPeriod=2;c.VolEmaPeriod=1;var k=new XiKernel(c);Snapshot s=null;for(int i=0;i<3;i++)s=k.Update(i,B(i,1+2*i),new Context(1,1,false,false),.01);Eq(s.Volatility,1);});
            Test("crossovers include previous equality but not present equality", () => {Check(MathSeries.CrossUp(2,1,1,1));Check(!MathSeries.CrossUp(1,1,0,1));Check(!MathSeries.CrossUp(2,1,double.NaN,1));});
            Test("Hull comparison is exactly two bars delayed", () => {var k=new XiKernel(Small());for(int i=0;i<30;i++){var s=k.Update(i,B(i,100+i*i*.01),new Context(1,1,false,false),.01);if(i>8)Eq(s.Hull2,k.Values[i-2].Hull);}});
            Test("repeated ticks roll back EMA/HA/PC to previous bar", () => {
                var a=new XiKernel(Small());var b=new XiKernel(Small());
                for(int i=0;i<15;i++){a.Update(i,B(i,100+i),new Context(1,2,false,true),.01);b.Update(i,B(i,100+i),new Context(1,2,false,true),.01);}
                a.Update(15,B(15,90),new Context(2,1,true,false),.01);a.Update(15,B(15,150),new Context(2,1,true,false),.01);
                var x=a.Update(15,B(15,120),new Context(2,1,true,false),.01);var y=b.Update(15,B(15,120),new Context(2,1,true,false),.01);
                Eq(x.Filter,y.Filter);Eq(x.HaOpen,y.HaOpen);Eq(x.Wave,y.Wave);Eq(x.MacdSignal,y.MacdSignal);Eq(x.LastLong,y.LastLong);Check(x.PcLongFlip==y.PcLongFlip);
            });
            Test("PC flip uses bar timestamps and survives repeated ticks", () => {var k=new XiKernel(Small());k.Update(0,B(0,100),new Context(1,2,false,true),.01);var s=k.Update(1,B(1,101),new Context(2,1,true,false),.01);Check(s.PcLongFlip);Check(k.Update(1,B(1,102),new Context(2,1,true,false),.01).PcLongFlip);});
            Test("PC distances use mintick, not pips", () => {var k=new XiKernel(Small());var s=k.Update(0,B(0,100),new Context(1,1,false,false),.01);Eq(s.PcUpper-s.PcCenter,.02);});
            Test("core entry is independent of PC synchronization", () => {var s=new Snapshot{LongBreakout=true,PcLong=false};Check(Rules(XiDecisions.Evaluate(s,0,0,400))=="CORE_LONG");});
            Test("profit exit is conditional and strictly greater than 400", () => {var s=new Snapshot{DailyDown=true,Hull=1,Hull2=2};Check(XiDecisions.Evaluate(s,1,400,400).Count==0);Check(Rules(XiDecisions.Evaluate(s,1,400.01,400))=="PROFIT_CLOSE_LONG");s.DailyDown=false;Check(XiDecisions.Evaluate(s,1,500,400).Count==0);});
            Test("source order keeps core entries before close and trend", () => {var s=new Snapshot{LongBreakout=true,ShortExit=true,LongTrend=true};Check(Rules(XiDecisions.Evaluate(s,-1,0,400))=="CORE_LONG,CORE_CLOSE_SHORT,TREND_PC_LONG");});
            Test("pyramiding0 filters same-side entry snapshot", () => {var s=new Snapshot{LongBreakout=true,LongTrend=true};Check(XiDecisions.Prepare(XiDecisions.Evaluate(s,1,0,400),1).Count==0);});
            Test("same-ID entry updates retain last source call", () => {var s=new Snapshot{LongBreakout=true,LongTrend=true};Check(Rules(XiDecisions.Prepare(XiDecisions.Evaluate(s,0,0,400),0))=="TREND_PC_LONG");});
            Test("display-only state does not gate orders", () => {var s=new Snapshot{LongTrend=true};string before=Rules(XiDecisions.Evaluate(s,0,0,400));s.HaOpen=900;s.HaClose=1;s.Adx=0;s.Trade=-1;s.LongColor=false;s.ShortColor=true;Check(Rules(XiDecisions.Evaluate(s,0,0,400))==before);});
            Test("historical MTF cannot read unclosed higher timeframe", () => {DateTime[] times={Start,Start.AddMinutes(45),Start.AddMinutes(90)};Eq(TemporalIndex.Select(3,i=>times[i],Start.AddMinutes(44),TimeSpan.FromMinutes(45),true),-1);Eq(TemporalIndex.Select(3,i=>times[i],Start.AddMinutes(45),TimeSpan.FromMinutes(45),true),0);Eq(TemporalIndex.Select(3,i=>times[i],Start.AddMinutes(46),TimeSpan.FromMinutes(45),false),1);});
            Test("lower timeframe historical sample is last closed intrabar", () => {var times=Enumerable.Range(0,5).Select(i=>Start.AddMinutes(i*5)).ToArray();Eq(TemporalIndex.Select(times.Length,i=>times[i],Start.AddMinutes(15),TimeSpan.FromMinutes(5),true),2);});
            Test("MTF gap carries known bar and never future bar", () => {DateTime[] times={Start,Start.AddDays(3)};Eq(TemporalIndex.Select(2,i=>times[i],Start.AddDays(2),TimeSpan.FromHours(1),true),0);});
            Console.WriteLine($"{passed} tests passed. These are kernel fixtures, not Pine/cTrader backtest parity.");return 0;
        }
        catch(Exception e){Console.Error.WriteLine(e);return 1;}
    }
}
