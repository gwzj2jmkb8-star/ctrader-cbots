// mojaTyMoja: one cBot entry point, built-in chart renderer, no custom assembly references.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using cAlgo.API;
using cAlgo.API.Internals;
using FunctionalMixtureXI;
namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None, AddIndicators = false)]
    public class mojaTyMoja : Robot
    {
        [Parameter("Source", Group = "Core", DefaultValue = PriceInput.Close)]
        public PriceInput Source { get; set; }
        [Parameter("Smoothing Period", Group = "Core", DefaultValue = 200)]
        public int SmoothingPeriod { get; set; }
        [Parameter("Smoothing Method", Group = "Core", DefaultValue = SmoothingKind.ALMA)]
        public SmoothingKind SmoothingMethod { get; set; }
        [Parameter("ALMA Offset Factor", Group = "Core", DefaultValue = .94)]
        public double AlmaOffset { get; set; }
        [Parameter("ALMA Sigma Value", Group = "Core", DefaultValue = 121.0)]
        public double AlmaSigma { get; set; }
        [Parameter("FILTER EMA Period", Group = "Core", DefaultValue = 2)]
        public int FilterPeriod { get; set; }
        [Parameter("ATR Period Offset", Group = "Envelope", DefaultValue = 37)]
        public int AtrPeriod { get; set; }
        [Parameter("ATR Multiplier Offset", Group = "Envelope", DefaultValue = .6)]
        public double AtrMultiplier { get; set; }
        [Parameter("Volatility Calculation", Group = "Envelope", DefaultValue = VolatilityKind.Stdev)]
        public VolatilityKind VolMethod { get; set; }
        [Parameter("Volatility Period", Group = "Envelope", DefaultValue = 1)]
        public int VolPeriod { get; set; }
        [Parameter("Volatility EMA Period", Group = "Envelope", DefaultValue = 200)]
        public int VolEmaPeriod { get; set; }
        [Parameter("Lower Multiplier", Group = "Envelope", DefaultValue = 0.0)]
        public double LowerMultiplier { get; set; }
        [Parameter("Upper Multiplier", Group = "Envelope", DefaultValue = 0.0)]
        public double UpperMultiplier { get; set; }
        [Parameter("Percentile Lookback", Group = "Envelope", DefaultValue = 200)]
        public int RankPeriod { get; set; }
        [Parameter("Decision Threshold (unused in source)", Group = "Legacy", DefaultValue = .01)]
        public double DecisionThresholdUnused { get; set; }
        [Parameter("Wow", Group = "Trend", DefaultValue = 7)]
        public int Wow { get; set; }
        [Parameter("MA 1", Group = "Trend", DefaultValue = 34)]
        public int Ma1 { get; set; }
        [Parameter("MA 2", Group = "Trend", DefaultValue = 144)]
        public int Ma2 { get; set; }
        [Parameter("MA 3", Group = "Trend", DefaultValue = 377)]
        public int Ma3 { get; set; }
        [Parameter("Conditional profit exit (account currency)", Group = "Execution", DefaultValue = 400.0)]
        public double ProfitThreshold { get; set; }
        [Parameter("ADX Length", Group = "DI and MACD visuals", DefaultValue = 8)]
        public int DiPeriod { get; set; }
        [Parameter("ADX Smoothing", Group = "DI and MACD visuals", DefaultValue = 200)]
        public int AdxPeriod { get; set; }
        [Parameter("MACD Source", Group = "DI and MACD visuals", DefaultValue = PriceInput.Close)]
        public PriceInput MacdSource { get; set; }
        [Parameter("MACD Fast Length", Group = "DI and MACD visuals", DefaultValue = 4)]
        public int MacdFast { get; set; }
        [Parameter("MACD Slow Length", Group = "DI and MACD visuals", DefaultValue = 28)]
        public int MacdSlow { get; set; }
        [Parameter("MACD Signal Length", Group = "DI and MACD visuals", DefaultValue = 14)]
        public int MacdSignal { get; set; }
        [Parameter("Enable Pip Collector MTF Filter", Group = "Pip Collector", DefaultValue = true)]
        public bool EnablePc { get; set; }
        [Parameter("Require Flip Signal for trend entry", Group = "Pip Collector", DefaultValue = true)]
        public bool RequireFlip { get; set; }
        [Parameter("PC Source", Group = "Pip Collector", DefaultValue = PriceInput.Close)]
        public PriceInput PcSource { get; set; }
        [Parameter("PC Center EMA Length", Group = "Pip Collector", DefaultValue = 2)]
        public int PcCenterPeriod { get; set; }
        [Parameter("PC Lower Distance (ticks, source mintick)", Group = "Pip Collector", DefaultValue = 2)]
        public int PcLowerTicks { get; set; }
        [Parameter("PC Upper Distance (ticks, source mintick)", Group = "Pip Collector", DefaultValue = 2)]
        public int PcUpperTicks { get; set; }
        [Parameter("Wave Smoothing Period", Group = "Cloud wave", DefaultValue = 200)]
        public int WavePeriod { get; set; }
        [Parameter("Wave Cloud Usage", Group = "Cloud wave", DefaultValue = .90)]
        public double WaveAmplitude { get; set; }
        [Parameter("Envelope Bands", Group = "Visuals", DefaultValue = true)]
        public bool VisualEnvelope { get; set; }
        [Parameter("Heikin Ashi Visual", Group = "Visuals", DefaultValue = true)]
        public bool VisualHeikinAshi { get; set; }
        [Parameter("Colored Wave In Cloud", Group = "Visuals", DefaultValue = true)]
        public bool VisualWave { get; set; }
        [Parameter("Cloud Advantage Background", Group = "Visuals", DefaultValue = true)]
        public bool VisualAdvantage { get; set; }
        [Parameter("Show confirmed breakout labels", Group = "Visuals", DefaultValue = true)]
        public bool VisualSignalLabels { get; set; }
        [Parameter("Show DI/MACD signal labels", Group = "Visuals", DefaultValue = true)]
        public bool VisualDiMacdLabels { get; set; }
        [Parameter("Show Candle Colors", Group = "Visuals", DefaultValue = true)]
        public bool VisualCandleColors { get; set; }
        [Parameter("PC Background on full sync", Group = "Pip Collector", DefaultValue = true)]
        public bool VisualPcBackground { get; set; }
        [Parameter("PC Flip Markers", Group = "Pip Collector", DefaultValue = true)]
        public bool VisualPcColumns { get; set; }
        [Parameter("Show Measurements", Group = "Measurements", DefaultValue = true)]
        public bool VisualMeasures { get; set; }
        [Parameter("Show Level Lines", Group = "Measurements", DefaultValue = true)]
        public bool VisualLevelLines { get; set; }
        [Parameter("Show Cloud Box", Group = "Measurements", DefaultValue = true)]
        public bool VisualCloudBox { get; set; }
        [Parameter("Show Price Distance", Group = "Measurements", DefaultValue = true)]
        public bool VisualDistance { get; set; }
        [Parameter("Show Cloud Width", Group = "Measurements", DefaultValue = true)]
        public bool VisualWidth { get; set; }
        [Parameter("Show Price Delta Line", Group = "Measurements", DefaultValue = true)]
        public bool VisualDelta { get; set; }
        [Parameter("Show Trade Metrics", Group = "Measurements", DefaultValue = true)]
        public bool VisualTradeMetrics { get; set; }
        [Parameter("Measurement History Bars", Group = "Measurements", DefaultValue = 50)]
        public int VisualMeasureHistory { get; set; }
        [Parameter("Max Drawings", Group = "Measurements", DefaultValue = 450)]
        public int VisualMaxDrawings { get; set; }
        [Parameter("Label Position: AutoHighLow/Above/Below", Group = "Visuals", DefaultValue = "AutoHighLow")]
        public string VisualLabelPosition { get; set; }
        [Parameter("Theme: Aurora/Neon/Classic", Group = "Palette", DefaultValue = "Aurora")]
        public string VisualTheme { get; set; }
        [Parameter("Use Custom Palette", Group = "Palette", DefaultValue = true)]
        public bool VisualCustomPalette { get; set; }
        [Parameter("Show Ichimoku (navigation only)", Group = "Ichimoku navigation", DefaultValue = true)]
        public bool VisualIchimoku { get; set; }
        [Parameter("Tenkan Period", Group = "Ichimoku navigation", DefaultValue = 9)]
        public int VisualTenkan { get; set; }
        [Parameter("Kijun Period", Group = "Ichimoku navigation", DefaultValue = 26)]
        public int VisualKijun { get; set; }
        [Parameter("Senkou B Period", Group = "Ichimoku navigation", DefaultValue = 52)]
        public int VisualSenkouB { get; set; }
        [Parameter("Cloud Displacement (bars)", Group = "Ichimoku navigation", DefaultValue = 26)]
        public int VisualDisplacement { get; set; }
        [Parameter("Volume Candle Evolution", Group = "HA volume navigation", DefaultValue = true)]
        public bool VisualVolumeCandles { get; set; }
        [Parameter("Relative Tick Volume Length", Group = "HA volume navigation", DefaultValue = 20)]
        public int VisualVolumePeriod { get; set; }
        [Parameter("Relative Volume Visual Cap", Group = "HA volume navigation", DefaultValue = 3.0)]
        public double VisualVolumeCap { get; set; }
        [Parameter("HA Drawing History", Group = "HA volume navigation", DefaultValue = 80)]
        public int VisualCandleHistory { get; set; }
        [Parameter("Bull ARGB", Group = "Palette", DefaultValue = "#008080")]
        public string VisualBull { get; set; }
        [Parameter("Bear ARGB", Group = "Palette", DefaultValue = "#800000")]
        public string VisualBear { get; set; }
        [Parameter("Neutral ARGB", Group = "Palette", DefaultValue = "#80000000")]
        public string VisualNeutral { get; set; }
        [Parameter("Text ARGB", Group = "Palette", DefaultValue = "#FFFFFFFF")]
        public string VisualText { get; set; }
        [Parameter("LongSignal ARGB", Group = "Palette", DefaultValue = "#40008080")]
        public string VisualLongSignal { get; set; }
        [Parameter("ShortSignal ARGB", Group = "Palette", DefaultValue = "#40800000")]
        public string VisualShortSignal { get; set; }
        [Parameter("WaveUp ARGB", Group = "Palette", DefaultValue = "#8C008080")]
        public string VisualWaveUp { get; set; }
        [Parameter("WaveDown ARGB", Group = "Palette", DefaultValue = "#8C800000")]
        public string VisualWaveDown { get; set; }
        [Parameter("CandleUp ARGB", Group = "Palette", DefaultValue = "#CC000000")]
        public string VisualCandleUp { get; set; }
        [Parameter("CandleDown ARGB", Group = "Palette", DefaultValue = "#66800000")]
        public string VisualCandleDown { get; set; }
        [Parameter("MaUp ARGB", Group = "Palette", DefaultValue = "#40008080")]
        public string VisualMaUp { get; set; }
        [Parameter("MaDown ARGB", Group = "Palette", DefaultValue = "#40800000")]
        public string VisualMaDown { get; set; }
        [Parameter("PcLong ARGB", Group = "Palette", DefaultValue = "#B3008080")]
        public string VisualPcLong { get; set; }
        [Parameter("PcShort ARGB", Group = "Palette", DefaultValue = "#B3800000")]
        public string VisualPcShort { get; set; }
        [Parameter("Basis ARGB", Group = "Palette", DefaultValue = "#80FFFF00")]
        public string VisualBasis { get; set; }
        [Parameter("Filter ARGB", Group = "Palette", DefaultValue = "#8000FFFF")]
        public string VisualFilter { get; set; }
        [Parameter("PcBgLong ARGB", Group = "Palette", DefaultValue = "#80808000")]
        public string VisualPcBgLong { get; set; }
        [Parameter("PcBgShort ARGB", Group = "Palette", DefaultValue = "#80800000")]
        public string VisualPcBgShort { get; set; }
        [Parameter("AdvantageLong ARGB", Group = "Palette", DefaultValue = "#80008080")]
        public string VisualAdvantageLong { get; set; }
        [Parameter("AdvantageShort ARGB", Group = "Palette", DefaultValue = "#80800000")]
        public string VisualAdvantageShort { get; set; }
        [Parameter("DistanceColor ARGB", Group = "Palette", DefaultValue = "#80800080")]
        public string VisualDistanceColor { get; set; }
        [Parameter("WidthColor ARGB", Group = "Palette", DefaultValue = "#80FF00FF")]
        public string VisualWidthColor { get; set; }
        [Parameter("DeltaUp ARGB", Group = "Palette", DefaultValue = "#80008080")]
        public string VisualDeltaUp { get; set; }
        [Parameter("DeltaDown ARGB", Group = "Palette", DefaultValue = "#80800000")]
        public string VisualDeltaDown { get; set; }
        [Parameter("LevelColor ARGB", Group = "Palette", DefaultValue = "#80C0C0C0")]
        public string VisualLevelColor { get; set; }
        [Parameter("BoxBorder ARGB", Group = "Palette", DefaultValue = "#F2000080")]
        public string VisualBoxBorder { get; set; }
        [Parameter("BoxFill ARGB", Group = "Palette", DefaultValue = "#EB808000")]
        public string VisualBoxFill { get; set; }
        [Parameter("MeasureText ARGB", Group = "Palette", DefaultValue = "#80FFFFFF")]
        public string VisualMeasureText { get; set; }
        [Parameter("TradeProfit ARGB", Group = "Palette", DefaultValue = "#80008080")]
        public string VisualTradeProfit { get; set; }
        [Parameter("TradeLoss ARGB", Group = "Palette", DefaultValue = "#80800000")]
        public string VisualTradeLoss { get; set; }
        [Parameter("PC Timeframe 1", Group = "Pip Collector", DefaultValue = "Minute5")]
        public TimeFrame PcTf1 { get; set; }
        [Parameter("PC Timeframe 2", Group = "Pip Collector", DefaultValue = "Minute15")]
        public TimeFrame PcTf2 { get; set; }
        [Parameter("PC Timeframe 3", Group = "Pip Collector", DefaultValue = "Minute45")]
        public TimeFrame PcTf3 { get; set; }
        [Parameter("Equity allocation % (not loss risk)", Group = "Execution", DefaultValue = 90.0, MinValue = .01)]
        public double EquityPercent { get; set; }
        [Parameter("Enable Trading", Group = "Execution", DefaultValue = true)]
        public bool EnableTrading { get; set; }
        [Parameter("Evaluation", Group = "Execution", DefaultValue = EvaluationKind.EveryTick)]
        public EvaluationKind Evaluation { get; set; }
        [Parameter("Instance Key (unique per chart)", Group = "Execution", DefaultValue = "XI-1")]
        public string InstanceKey { get; set; }
        [Parameter("Warm-up Bars", Group = "Diagnostics", DefaultValue = 3000, MinValue = 500)]
        public int WarmupBars { get; set; }
        [Parameter("Export Trace to cBot Log", Group = "Diagnostics", DefaultValue = false)]
        public bool Trace { get; set; }
        [Parameter("Legacy Up Candle ARGB", Group = "Palette", DefaultValue = "#80008080")]
        public string LegacyCandleUp { get; set; }
        [Parameter("Legacy Down Candle ARGB", Group = "Palette", DefaultValue = "#80800000")]
        public string LegacyCandleDown { get; set; }
        private CoreConfig GetCore() => new CoreConfig
        {
            Source = this.Source,
            SmoothingPeriod = this.SmoothingPeriod,
            SmoothingMethod = this.SmoothingMethod,
            AlmaOffset = this.AlmaOffset,
            AlmaSigma = this.AlmaSigma,
            FilterPeriod = this.FilterPeriod,
            AtrPeriod = this.AtrPeriod,
            AtrMultiplier = this.AtrMultiplier,
            VolMethod = this.VolMethod,
            VolPeriod = this.VolPeriod,
            VolEmaPeriod = this.VolEmaPeriod,
            LowerMultiplier = this.LowerMultiplier,
            UpperMultiplier = this.UpperMultiplier,
            RankPeriod = this.RankPeriod,
            DecisionThresholdUnused = this.DecisionThresholdUnused,
            Wow = this.Wow,
            Ma1 = this.Ma1,
            Ma2 = this.Ma2,
            Ma3 = this.Ma3,
            ProfitThreshold = this.ProfitThreshold,
            DiPeriod = this.DiPeriod,
            AdxPeriod = this.AdxPeriod,
            MacdSource = this.MacdSource,
            MacdFast = this.MacdFast,
            MacdSlow = this.MacdSlow,
            MacdSignal = this.MacdSignal,
            EnablePc = this.EnablePc,
            RequireFlip = this.RequireFlip,
            PcSource = this.PcSource,
            PcCenterPeriod = this.PcCenterPeriod,
            PcLowerTicks = this.PcLowerTicks,
            PcUpperTicks = this.PcUpperTicks,
            WavePeriod = this.WavePeriod,
            WaveAmplitude = this.WaveAmplitude,
            PcTimeframe1 = PcTf1.Name, PcTimeframe2 = PcTf2.Name, PcTimeframe3 = PcTf3.Name
        };
        private VisualConfig GetVisual() => new VisualConfig
        {
            LegacyCandleUp = LegacyCandleUp, LegacyCandleDown = LegacyCandleDown,
            Envelope = this.VisualEnvelope,
            HeikinAshi = this.VisualHeikinAshi,
            Wave = this.VisualWave,
            Advantage = this.VisualAdvantage,
            SignalLabels = this.VisualSignalLabels,
            DiMacdLabels = this.VisualDiMacdLabels,
            CandleColors = this.VisualCandleColors,
            PcBackground = this.VisualPcBackground,
            PcColumns = this.VisualPcColumns,
            Measures = this.VisualMeasures,
            LevelLines = this.VisualLevelLines,
            CloudBox = this.VisualCloudBox,
            Distance = this.VisualDistance,
            Width = this.VisualWidth,
            Delta = this.VisualDelta,
            TradeMetrics = this.VisualTradeMetrics,
            MeasureHistory = this.VisualMeasureHistory,
            MaxDrawings = this.VisualMaxDrawings,
            LabelPosition = this.VisualLabelPosition,
            Theme = this.VisualTheme,
            CustomPalette = this.VisualCustomPalette,
            Ichimoku = this.VisualIchimoku,
            Tenkan = this.VisualTenkan,
            Kijun = this.VisualKijun,
            SenkouB = this.VisualSenkouB,
            Displacement = this.VisualDisplacement,
            VolumeCandles = this.VisualVolumeCandles,
            VolumePeriod = this.VisualVolumePeriod,
            VolumeCap = this.VisualVolumeCap,
            CandleHistory = this.VisualCandleHistory,
            Bull = this.VisualBull,
            Bear = this.VisualBear,
            Neutral = this.VisualNeutral,
            Text = this.VisualText,
            LongSignal = this.VisualLongSignal,
            ShortSignal = this.VisualShortSignal,
            WaveUp = this.VisualWaveUp,
            WaveDown = this.VisualWaveDown,
            CandleUp = this.VisualCandleUp,
            CandleDown = this.VisualCandleDown,
            MaUp = this.VisualMaUp,
            MaDown = this.VisualMaDown,
            PcLong = this.VisualPcLong,
            PcShort = this.VisualPcShort,
            Basis = this.VisualBasis,
            Filter = this.VisualFilter,
            PcBgLong = this.VisualPcBgLong,
            PcBgShort = this.VisualPcBgShort,
            AdvantageLong = this.VisualAdvantageLong,
            AdvantageShort = this.VisualAdvantageShort,
            DistanceColor = this.VisualDistanceColor,
            WidthColor = this.VisualWidthColor,
            DeltaUp = this.VisualDeltaUp,
            DeltaDown = this.VisualDeltaDown,
            LevelColor = this.VisualLevelColor,
            BoxBorder = this.VisualBoxBorder,
            BoxFill = this.VisualBoxFill,
            MeasureText = this.VisualMeasureText,
            TradeProfit = this.VisualTradeProfit,
            TradeLoss = this.VisualTradeLoss,
        };
        private CoreConfig _config;
        private VisualConfig _visual;
        private XiKernel _kernel;
        private XiMarketData _market;
        private XiChartRenderer _renderer;
        private string _owner;
        private bool _faulted, _inBatch, _fillRequested;
        private int _seenCount;
        private long _tick;
        private Batch _pending;
        private readonly Queue<string> _executionDrawings = new Queue<string>();
        private sealed class Batch
        {
            public List<Intent> Intents;
            public long[] PositionIds;
            public int InitialSide;
            public DateTime SignalTime;
        }
        private Position[] Owned() => Positions.Where(p => p.SymbolName == SymbolName &&
            (p.Label == _owner + ":Long" || p.Label == _owner + ":Short")).ToArray();
        private static int Side(Position p) => p.TradeType == TradeType.Buy ? 1 : -1;
        protected override void OnStart()
        {
            try
            {
                _config = GetCore(); _config.Validate(); _visual = GetVisual(); _visual.Validate();
                if (string.IsNullOrWhiteSpace(InstanceKey) || !double.IsFinite(EquityPercent) || EquityPercent <= 0)
                    throw new ArgumentException("Set a nonempty unique instance key and positive allocation.");
                _owner = "FMXI:" + InstanceKey + ":" + SymbolName + ":" + TimeFrame.Name;
                // These periods are explicit in the supplied setup; generic fixed-duration bars also work.
                for (int tries = 0; tries < 100 && Bars.Count < WarmupBars; tries++)
                    if (Bars.LoadMoreHistory() == 0) break;
                _market = new XiMarketData(Bars, tf => MarketData.GetBars(tf, SymbolName), _config, true);
                _kernel = new XiKernel(_config);
                for (int i = 0; i < Bars.Count; i++)
                    _kernel.Update(i, XiMarketData.Read(Bars, i), _market.At(i, Server.Time, i < Bars.Count - 1), Symbol.TickSize);
                _seenCount = Bars.Count;
                if (Owned().Length > 1) throw new InvalidOperationException("More than one owned position: reconcile before running XI (pyramiding=0).");
                Positions.Opened += OnOpened; Positions.Closed += OnClosed;
                _renderer = new XiChartRenderer(Chart, Bars, Symbol, _config, _visual, _owner);
                _renderer.Render(_kernel, _market);
                Print("XI initialized: {0} {1}; allocation {2}% notional, no fixed SL/TP in source. Evaluation={3}.",
                    SymbolName, TimeFrame.Name, EquityPercent, Evaluation);
                Print("MTF realtime uses developing bars; historical sampling uses completed bars. Feed/session differences require parity testing.");
                Print("Default volPeriod=1 produces zero stdev; rank=100 after valid values. PC gates trend branch only. DI/MACD and Ichimoku are display-only.");
                if (Trace) Print("XI_HEADER,bar_open_utc,evaluation_utc,tick,position_side,xi_close,xi_basis,xi_upper,xi_lower,xi_filter,xi_rank,xi_hull,xi_hull2,xi_pc_long_sync,xi_pc_short_sync,xi_pc_long_flip,xi_pc_short_flip,xi_core_long,xi_core_short,rules,xi_daily,xi_previous_daily,xi_trend_long,xi_trend_short");
                Panel();
            }
            catch (Exception ex) { Print("START FAILED: {0}", ex.Message); Stop(); }
        }
        protected override void OnTick()
        {
            if (_kernel == null || _faulted || Bars.Count == 0) return;
            _tick++;
            if (Bars.OpenTimes[0] != _kernel.Bars[0].Time)
            {
                Fault("Chart history changed during execution; restart to rebuild state safely."); return;
            }
            bool newBar = Bars.Count > _seenCount;
            // Re-evaluate the last closed bar using historical sampling only in explicitly selected close mode.
            if (newBar && Evaluation == EvaluationKind.ClosedBars && _kernel.Bars.Count > 0)
            {
                int prior = _kernel.Bars.Count - 1;
                _kernel.Update(prior, XiMarketData.Read(Bars, prior), _market.At(prior, Server.Time, true), Symbol.TickSize);
            }
            while (_kernel.Bars.Count < Bars.Count)
            {
                int i = _kernel.Bars.Count;
                _kernel.Update(i, XiMarketData.Read(Bars, i), _market.At(i, Server.Time, i < Bars.Count - 1), Symbol.TickSize);
            }
            int current = Bars.Count - 1;
            Snapshot s = _kernel.Update(current, XiMarketData.Read(Bars, current), _market.At(current, Server.Time, false), Symbol.TickSize);
            _seenCount = Bars.Count;
            try { _renderer?.Render(_kernel, _market); }
            catch (Exception ex) { Fault("Chart rendering failed: " + ex.Message); return; }
            // Fill prior tick's market intents on the next available quote, then recalculate with current holdings.
            if (_pending != null) { var batch = _pending; _pending = null; Execute(batch); }
            if (_faulted) return;
            if (Evaluation == EvaluationKind.EveryTick) Evaluate(s);
            else if (newBar && current > 0)
            {
                Evaluate(_kernel.Values[current - 1]);
                // Historical default: order from the closed candle is eligible on this first new-bar tick.
                if (_pending != null) { var batch = _pending; _pending = null; Execute(batch); }
            }
            // Fill recalculation is coalesced once after a complete reversal batch, not recursive broker calls.
            if (_fillRequested)
            {
                _fillRequested = false;
                Evaluate(Evaluation == EvaluationKind.ClosedBars && current > 0 ? _kernel.Values[current - 1] : s);
            }
            Panel();
        }
        private void Evaluate(Snapshot s)
        {
            Position[] owned = Owned();
            if (owned.Length > 1) { Fault("Pyramiding=0 violated by multiple owned positions."); return; }
            int side = owned.Length == 0 ? 0 : Side(owned[0]);
            double profit = owned.Sum(p => p.NetProfit);
            List<Intent> intents = XiDecisions.Evaluate(s, side, profit, ProfitThreshold);
            if (Trace)
            {
                string f(double x) => double.IsFinite(x) ? x.ToString("R", CultureInfo.InvariantCulture) : "NaN";
                Print("XI_TRACE,{0:o},{1:o},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22}",
                    s.Time, Server.Time, _tick, side, f(s.Close), f(s.Basis), f(s.Upper), f(s.Lower), f(s.Filter),
                    f(s.Rank), f(s.Hull), f(s.Hull2), s.PcLong, s.PcShort, s.PcLongFlip, s.PcShortFlip,
                    s.LongBreakout, s.ShortBreakout, string.Join("|", intents.Select(x => x.Rule)),
                    f(s.DailyPrice), f(s.PreviousDailyPrice), s.LongTrend, s.ShortTrend);
            }
            if (!EnableTrading || intents.Count == 0) return;
            intents = XiDecisions.Prepare(intents, side);
            if (intents.Count == 0) return;
            _pending = new Batch { Intents = intents, PositionIds = owned.Select(p => (long)p.Id).ToArray(), InitialSide = side, SignalTime = s.Time };
        }
        private void Execute(Batch batch)
        {
            if (!EnableTrading || _faulted || _inBatch) return;
            if (Account.AccountType == AccountType.Netted && Positions.Any(p => p.SymbolName == SymbolName &&
                p.Label != _owner + ":Long" && p.Label != _owner + ":Short"))
            { Fault("Foreign position exists on a netted symbol; refusing to merge ownership."); return; }
            _inBatch = true;
            try
            {
                foreach (Intent intent in batch.Intents)
                {
                    if (intent.Kind == IntentKind.Close)
                    {
                        foreach (Position p in Owned().Where(p => Side(p) == intent.Side && batch.PositionIds.Contains((long)p.Id)).ToArray())
                        {
                            var result = ClosePosition(p);
                            if (!result.IsSuccessful) { Fault("Close rejected: " + result.Error); return; }
                            Print("XI_FILL CLOSE {0} rule={1} decision={2:o}", p.Id, intent.Rule, batch.SignalTime);
                        }
                    }
                    else
                    {
                        if (Owned().Any(p => Side(p) == intent.Side)) continue;
                        foreach (Position p in Owned().Where(p => Side(p) != intent.Side).ToArray())
                        {
                            var close = ClosePosition(p);
                            if (!close.IsSuccessful) { Fault("Reversal close rejected: " + close.Error); return; }
                        }
                        var type = intent.Side > 0 ? TradeType.Buy : TradeType.Sell;
                        double price = intent.Side > 0 ? Symbol.Ask : Symbol.Bid;
                        // Percent-of-equity is not margin percentage or stop-loss risk.
                        double quoteBudget = AssetConverter.Convert(Account.Equity * EquityPercent / 100, Account.Asset, Symbol.QuoteAsset);
                        double requested = quoteBudget / price;
                        double volume = Symbol.NormalizeVolumeInUnits(requested, RoundingMode.Down);
                        if (!double.IsFinite(volume) || volume < Symbol.VolumeInUnitsMin || volume > Symbol.VolumeInUnitsMax)
                        { Fault("Notional allocation outside symbol volume range. No silent resizing: " + volume); return; }
                        if (Symbol.GetEstimatedMargin(type, volume) > Account.FreeMargin)
                        { Fault("Insufficient free margin for declared allocation. No silent resizing."); return; }
                        var order = ExecuteMarketOrder(type, SymbolName, volume, _owner + (intent.Side > 0 ? ":Long" : ":Short"));
                        if (!order.IsSuccessful) { Fault("Entry rejected: " + order.Error); return; }
                        Print("XI_FILL ENTRY {0} rule={1} volume={2} price={3} decision={4:o}",
                            order.Position.Id, intent.Rule, volume, order.Position.EntryPrice, batch.SignalTime);
                    }
                }
            }
            finally { _inBatch = false; }
        }
        private void OnOpened(PositionOpenedEventArgs e)
        {
            if (!IsOwned(e.Position)) return;
            _fillRequested = true;
            ExecutionLabel(e.Position, "FILL " + (Side(e.Position) > 0 ? "BUY" : "SELL"), e.Position.EntryPrice);
        }
        private void OnClosed(PositionClosedEventArgs e)
        {
            if (!IsOwned(e.Position)) return;
            _fillRequested = true;
            ExecutionLabel(e.Position, "EXIT " + e.Position.NetProfit.ToString("0.00", CultureInfo.InvariantCulture),
                Side(e.Position) > 0 ? Symbol.Bid : Symbol.Ask);
            Print("XI_CLOSED,{0},{1},{2}", e.Position.Id, e.Reason, e.Position.NetProfit);
        }
        private bool IsOwned(Position p) => p.SymbolName == SymbolName && (p.Label == _owner + ":Long" || p.Label == _owner + ":Short");
        private void ExecutionLabel(Position p, string text, double price)
        {
            string id = _owner + ":exec:" + Guid.NewGuid().ToString("N");
            Chart.DrawText(id, text + " #" + p.Id, Server.Time, price, Side(p) > 0 ? Color.Teal : Color.Maroon);
            _executionDrawings.Enqueue(id);
            while (_executionDrawings.Count > 100) Chart.RemoveObject(_executionDrawings.Dequeue());
        }
        private void Fault(string reason)
        {
            _faulted = true; _pending = null;
            Print("XI EXECUTION FAULT (restart after resolving): {0}", reason); Panel();
        }
        private void Panel()
        {
            if (_visual == null || !_visual.TradeMetrics) return;
            Position[] owned = Owned();
            string state = _faulted ? "FAULTED" : !EnableTrading ? "OBSERVE" : owned.Length == 0 ? "FLAT" : Side(owned[0]) > 0 ? "LONG" : "SHORT";
            Chart.DrawStaticText(_owner + ":position", $"FM XI · {state}\n{TimeFrame.Name} · {Evaluation} · allocation {EquityPercent:0.##}%\n" +
                $"Open P/L {owned.Sum(x => x.NetProfit):0.00} {Account.Asset.Name} · units {owned.Sum(x => x.VolumeInUnits):0.##}\n" +
                $"Conditional profit exit > {ProfitThreshold:0.##}; source has no fixed SL/TP\nIchimoku / HA volume: navigation only",
                VerticalAlignment.Bottom, HorizontalAlignment.Left, owned.Sum(x => x.NetProfit) >= 0 ? Color.Teal : Color.Maroon);
        }
        protected override void OnStop()
        {
            Positions.Opened -= OnOpened; Positions.Closed -= OnClosed;
            _pending = null;
            _renderer?.Dispose();
            _renderer = null;
            foreach (string name in _executionDrawings) Chart.RemoveObject(name);
            _executionDrawings.Clear();
            if (_owner != null) Chart.RemoveObject(_owner + ":position");
            Print("XI stopped. Existing positions are retained; the source defines no stop-time liquidation.");
        }
    }
}

// Functional Mixture XI: deterministic, broker-independent calculations.
namespace FunctionalMixtureXI
{
    public enum PriceInput { Close, Open, High, Low, HL2, HLC3, OHLC4 }
    public enum SmoothingKind { ALMA, EMA }
    public enum VolatilityKind { Stdev, Mad }
    public enum EvaluationKind { EveryTick, ClosedBars }
    public sealed class CoreConfig
    {
        public PriceInput Source { get; set; } = PriceInput.Close;
        public int SmoothingPeriod { get; set; } = 200;
        public SmoothingKind SmoothingMethod { get; set; } = SmoothingKind.ALMA;
        public double AlmaOffset { get; set; } = .94;
        public double AlmaSigma { get; set; } = 121;
        public int FilterPeriod { get; set; } = 2;
        public int AtrPeriod { get; set; } = 37;
        public double AtrMultiplier { get; set; } = .6;
        public VolatilityKind VolMethod { get; set; } = VolatilityKind.Stdev;
        public int VolPeriod { get; set; } = 1;
        public int VolEmaPeriod { get; set; } = 200;
        public double LowerMultiplier { get; set; } = 0;
        public double UpperMultiplier { get; set; } = 0;
        public int RankPeriod { get; set; } = 200;
        public double DecisionThresholdUnused { get; set; } = .01;
        public int Wow { get; set; } = 7;
        public int Ma1 { get; set; } = 34;
        public int Ma2 { get; set; } = 144;
        public int Ma3 { get; set; } = 377;
        public double ProfitThreshold { get; set; } = 400;
        public int DiPeriod { get; set; } = 8;
        public int AdxPeriod { get; set; } = 200;
        public PriceInput MacdSource { get; set; } = PriceInput.Close;
        public int MacdFast { get; set; } = 4;
        public int MacdSlow { get; set; } = 28;
        public int MacdSignal { get; set; } = 14;
        public bool EnablePc { get; set; } = true;
        public bool RequireFlip { get; set; } = true;
        public PriceInput PcSource { get; set; } = PriceInput.Close;
        public string PcTimeframe1 { get; set; } = "Minute5";
        public string PcTimeframe2 { get; set; } = "Minute15";
        public string PcTimeframe3 { get; set; } = "Minute45";
        public int PcCenterPeriod { get; set; } = 2;
        public int PcLowerTicks { get; set; } = 2;
        public int PcUpperTicks { get; set; } = 2;
        public int WavePeriod { get; set; } = 200;
        public double WaveAmplitude { get; set; } = .90;
        public void Validate()
        {
            var lengths = new[] { SmoothingPeriod, FilterPeriod, AtrPeriod, VolPeriod, VolEmaPeriod,
                RankPeriod, Wow, Ma1, Ma2, Ma3, DiPeriod, AdxPeriod, MacdFast, MacdSlow,
                MacdSignal, PcCenterPeriod, PcLowerTicks, PcUpperTicks, WavePeriod };
            if (lengths.Any(x => x < 1)) throw new ArgumentException("All calculation lengths must be positive.");
            if (!double.IsFinite(AlmaSigma) || AlmaSigma <= 0 || !double.IsFinite(AlmaOffset))
                throw new ArgumentException("ALMA parameters must be finite; sigma must be positive.");
            if (AtrMultiplier < 0 || LowerMultiplier < 0 || UpperMultiplier < 0 || ProfitThreshold < 1 ||
                WaveAmplitude < .1 || WaveAmplitude > 1) throw new ArgumentException("Invalid distance, threshold or wave parameter.");
        }
    }
    public readonly struct Candle
    {
        public readonly DateTime Time;
        public readonly double Open, High, Low, Close, Volume;
        public Candle(DateTime time, double o, double h, double l, double c, double v = 0)
        { Time = time; Open = o; High = h; Low = l; Close = c; Volume = v; }
        public double Source(PriceInput p) => p switch {
            PriceInput.Open => Open, PriceInput.High => High, PriceInput.Low => Low,
            PriceInput.HL2 => (High + Low) / 2, PriceInput.HLC3 => (High + Low + Close) / 3,
            PriceInput.OHLC4 => (Open + High + Low + Close) / 4, _ => Close };
    }
    public readonly struct Context
    {
        public readonly double Daily, PreviousDaily;
        public readonly bool PcLong, PcShort;
        public Context(double daily, double previousDaily, bool pcLong, bool pcShort)
        { Daily = daily; PreviousDaily = previousDaily; PcLong = pcLong; PcShort = pcShort; }
    }
    public sealed class Snapshot
    {
        public int Index, Trade;
        public DateTime Time;
        public double Close, Basis, Filter, Upper, Lower, AtrOffset, Rank, Volatility;
        public double DailyPrice, PreviousDailyPrice;
        public double Wma1, Wma2, Wma3, Hull, Hull2, DiPlus, DiMinus, Adx, Macd, MacdSignal;
        public double HaOpen, HaHigh, HaLow, HaClose, HaStrength, Wave, WaveClamped, WaveSlope;
        public double PcCenter, PcUpper, PcLower, LastLong, LastShort;
        public bool DailyUp, DailyDown, PcLong, PcShort, PcLongFlip, PcShortFlip;
        public bool LongBreakout, ShortBreakout, LongExit, ShortExit, LongTrend, ShortTrend;
        public bool LongColor, ShortColor, LongLabel, ShortLabel, Ready;
    }
    public static class MathSeries
    {
        public static bool Finite(double x) => double.IsFinite(x);
        public static double Get(IReadOnlyList<double> x, int i) => i < 0 || i >= x.Count ? double.NaN : x[i];
        public static void Put(List<double> x, int i, double v)
        { while (x.Count <= i) x.Add(double.NaN); x[i] = v; }
        public static int PineRound(double v) => (int)Math.Floor(v + .5);
        public static double Sma(IReadOnlyList<double> x, int i, int length)
        {
            double total = 0; int valid = 0;
            for (int j = i; j >= 0 && valid < length; --j)
                if (Finite(x[j])) { total += x[j]; valid++; }
            return valid == length ? total / length : double.NaN;
        }
        public static double Wma(IReadOnlyList<double> x, int i, int length)
        {
            double total = 0, weightSum = 0; int weight = length;
            for (int j = i; j >= 0 && weight > 0; --j)
                if (Finite(x[j])) { total += weight * x[j]; weightSum += weight; weight--; }
            return weight == 0 ? total / weightSum : double.NaN;
        }
        public static double Alma(IReadOnlyList<double> x, int i, int length, double offset, double sigma)
        {
            if (i < length - 1) return double.NaN;
            double m = offset * (length - 1), s = length / sigma, sum = 0, weights = 0;
            // Shift Gaussian exponents to avoid underflow for narrow kernels.
            double minSquare = double.PositiveInfinity;
            for (int k = 0; k < length; k++) minSquare = Math.Min(minSquare, (k - m) * (k - m));
            for (int k = 0; k < length; k++)
            {
                double value = x[i - length + 1 + k]; if (!Finite(value)) return double.NaN;
                double w = Math.Exp(-((k - m) * (k - m) - minSquare) / (2 * s * s));
                sum += value * w; weights += w;
            }
            return sum / weights;
        }
        public static double Ema(double current, double previous, int length) =>
            !Finite(current) ? previous : !Finite(previous) ? current : (2.0 / (length + 1)) * current + (1 - 2.0 / (length + 1)) * previous;
        public static double Rma(IReadOnlyList<double> x, int i, int length, double previous) =>
            !Finite(previous) ? Sma(x, i, length) : !Finite(x[i]) ? previous : (x[i] + (length - 1) * previous) / length;
        public static double Stdev(IReadOnlyList<double> x, int i, int length)
        {
            double mean = Sma(x, i, length); if (!Finite(mean)) return double.NaN;
            double sum = 0; int valid = 0;
            for (int j = i; j >= 0 && valid < length; --j)
                if (Finite(x[j])) { sum += Math.Pow(x[j] - mean, 2); valid++; }
            return Math.Sqrt(sum / length); // Pine ta.stdev default biased=true.
        }
        public static double Rank(IReadOnlyList<double> x, int i, int length)
        {
            if (!Finite(x[i])) return double.NaN;
            int count = 0, valid = 0;
            for (int j = i; j >= Math.Max(0, i - length + 1); --j)
                if (Finite(x[j])) { valid++; if (x[i] >= x[j]) count++; }
            return valid == 0 ? double.NaN : 100.0 * count / valid;
        }
        public static bool CrossUp(double a, double b, double prevA, double prevB) =>
            Finite(a) && Finite(b) && Finite(prevA) && Finite(prevB) && a > b && prevA <= prevB;
        public static bool CrossDown(double a, double b, double prevA, double prevB) => CrossUp(b, a, prevB, prevA);
    }
    public static class TemporalIndex
    {
        public static int Select(int count, Func<int, DateTime> timeAt, DateTime at, TimeSpan period, bool closedOnly)
        {
            int lo = 0, hi = count - 1, result = -1;
            while (lo <= hi)
            {
                int mid = (lo + hi) / 2;
                DateTime available = timeAt(mid) + (closedOnly ? period : TimeSpan.Zero);
                if (available <= at) { result = mid; lo = mid + 1; } else hi = mid - 1;
            }
            return result;
        }
    }
    public sealed class XiKernel
    {
        public readonly CoreConfig Config;
        public readonly List<Candle> Bars = new List<Candle>();
        public readonly List<Snapshot> Values = new List<Snapshot>();
        private readonly Dictionary<string, List<double>> _series = new Dictionary<string, List<double>>();
        private List<double> S(string key)
        { if (!_series.TryGetValue(key, out var x)) _series[key] = x = new List<double>(); return x; }
        private double Put(string key, int i, double x) { MathSeries.Put(S(key), i, x); return x; }
        private double Prev(string key, int i) => MathSeries.Get(S(key), i - 1);
        public XiKernel(CoreConfig config) { Config = config; config.Validate(); }
        public Snapshot Update(int i, Candle b, Context context, double tickSize)
        {
            if (i < 0 || i > Bars.Count || i < Bars.Count - 1) throw new ArgumentException("Update newest bar or append sequentially.");
            if (i == Bars.Count) { Bars.Add(b); Values.Add(new Snapshot()); } else Bars[i] = b;
            var c = Config; var p = i > 0 ? Values[i - 1] : null;
            var s = new Snapshot { Index = i, Time = b.Time, Close = b.Close };
            double src = Put("src", i, b.Source(c.Source)), close = Put("close", i, b.Close);
            s.Basis = c.SmoothingMethod == SmoothingKind.EMA
                ? MathSeries.Ema(src, Prev("basis", i), c.SmoothingPeriod)
                : MathSeries.Alma(S("src"), i, c.SmoothingPeriod, c.AlmaOffset, c.AlmaSigma);
            Put("basis", i, s.Basis);
            s.Filter = Put("filter", i, MathSeries.Ema(src, Prev("filter", i), c.FilterPeriod));
            Put("madDistance", i, Math.Abs(src - MathSeries.Sma(S("src"), i, c.VolPeriod)));
            double rawVol = c.VolMethod == VolatilityKind.Stdev ? MathSeries.Stdev(S("src"), i, c.VolPeriod)
                : MathSeries.Sma(S("madDistance"), i, c.VolPeriod);
            s.Volatility = Put("vol", i, MathSeries.Ema(rawVol, Prev("vol", i), c.VolEmaPeriod));
            s.Rank = MathSeries.Rank(S("vol"), i, c.RankPeriod);
            double tr = i == 0 ? b.High - b.Low : Math.Max(b.High - b.Low,
                Math.Max(Math.Abs(b.High - Bars[i - 1].Close), Math.Abs(b.Low - Bars[i - 1].Close)));
            Put("tr", i, tr);
            double atr = Put("atr", i, MathSeries.Rma(S("tr"), i, c.AtrPeriod, Prev("atr", i)));
            s.AtrOffset = atr * c.AtrMultiplier;
            s.Upper = s.Basis + s.Volatility * c.UpperMultiplier + s.AtrOffset;
            s.Lower = s.Basis - s.Volatility * c.LowerMultiplier - s.AtrOffset;
            s.LongBreakout = p != null && MathSeries.CrossUp(close, s.Upper, p.Close, p.Upper) && close > s.Filter && s.Rank >= 50;
            s.ShortBreakout = p != null && MathSeries.CrossDown(close, s.Lower, p.Close, p.Lower) && close < s.Filter && s.Rank >= 50;
            s.LongExit = p != null && (MathSeries.CrossDown(close, s.Basis, p.Close, p.Basis) || s.ShortBreakout);
            s.ShortExit = p != null && (MathSeries.CrossUp(close, s.Basis, p.Close, p.Basis) || s.LongBreakout);
            s.Wma1 = MathSeries.Wma(S("close"), i, c.Ma1);
            s.Wma2 = MathSeries.Wma(S("close"), i, c.Ma2);
            s.Wma3 = MathSeries.Wma(S("close"), i, c.Ma3);
            Put("diff", i, 2 * MathSeries.Wma(S("close"), i, MathSeries.PineRound(c.Wow / 2.0)) - MathSeries.Wma(S("close"), i, c.Wow));
            s.Hull = Put("hull", i, MathSeries.Wma(S("diff"), i, MathSeries.PineRound(Math.Sqrt(c.Wow))));
            s.Hull2 = MathSeries.Get(S("hull"), i - 2);
            // Preserve the source's large comparison scale; it is not a price/volume multiplier.
            const double scale = 10000000000000.0;
            s.DailyPrice = context.Daily; s.PreviousDailyPrice = context.PreviousDaily;
            s.DailyUp = context.Daily * scale > context.PreviousDaily * scale;
            s.DailyDown = context.Daily * scale < context.PreviousDaily * scale;
            s.PcLong = context.PcLong; s.PcShort = context.PcShort;
            double stamp = (b.Time - new DateTime(1970, 1, 1)).TotalMilliseconds;
            s.LastLong = context.PcLong ? stamp : p?.LastLong ?? 0;
            s.LastShort = context.PcShort ? stamp : p?.LastShort ?? 0;
            s.PcLongFlip = p != null && MathSeries.CrossUp(s.LastLong, s.LastShort, p.LastLong, p.LastShort);
            s.PcShortFlip = p != null && MathSeries.CrossUp(s.LastShort, s.LastLong, p.LastShort, p.LastLong);
            bool longOk = !c.EnablePc || context.PcLong && (!c.RequireFlip || s.PcLongFlip);
            bool shortOk = !c.EnablePc || context.PcShort && (!c.RequireFlip || s.PcShortFlip);
            s.LongTrend = s.DailyUp && s.Hull * scale > s.Hull2 * scale && longOk;
            s.ShortTrend = s.DailyDown && s.Hull * scale < s.Hull2 * scale && shortOk;
            double pcSrc = b.Source(c.PcSource);
            s.PcCenter = Put("pcCenter", i, MathSeries.Ema(pcSrc, Prev("pcCenter", i), c.PcCenterPeriod));
            s.PcUpper = s.PcCenter + c.PcUpperTicks * tickSize;
            s.PcLower = s.PcCenter - c.PcLowerTicks * tickSize;
            // DMI is a DISPLAY state in XI, not an entry gate.
            double up = i == 0 ? double.NaN : b.High - Bars[i - 1].High;
            double down = i == 0 ? double.NaN : Bars[i - 1].Low - b.Low;
            Put("plusDM", i, i == 0 ? double.NaN : up > down && up > 0 ? up : 0);
            Put("minusDM", i, i == 0 ? double.NaN : down > up && down > 0 ? down : 0);
            Put("dmiTr", i, i == 0 ? double.NaN : tr);
            double trRma = Put("diTrRma", i, MathSeries.Rma(S("dmiTr"), i, c.DiPeriod, Prev("diTrRma", i)));
            double plus = Put("plusRma", i, MathSeries.Rma(S("plusDM"), i, c.DiPeriod, Prev("plusRma", i)));
            double minus = Put("minusRma", i, MathSeries.Rma(S("minusDM"), i, c.DiPeriod, Prev("minusRma", i)));
            s.DiPlus = trRma == 0 ? p?.DiPlus ?? double.NaN : 100 * plus / trRma;
            s.DiMinus = trRma == 0 ? p?.DiMinus ?? double.NaN : 100 * minus / trRma;
            double diSum = s.DiPlus + s.DiMinus;
            Put("dx", i, 100 * Math.Abs(s.DiPlus - s.DiMinus) / (diSum == 0 ? 1 : diSum));
            s.Adx = Put("adx", i, MathSeries.Rma(S("dx"), i, c.AdxPeriod, Prev("adx", i)));
            double ms = b.Source(c.MacdSource);
            double fast = Put("mf", i, MathSeries.Ema(ms, Prev("mf", i), c.MacdFast));
            double slow = Put("ms", i, MathSeries.Ema(ms, Prev("ms", i), c.MacdSlow));
            s.Macd = fast - slow;
            s.MacdSignal = Put("mSignal", i, MathSeries.Ema(s.Macd, Prev("mSignal", i), c.MacdSignal));
            s.LongColor = s.DiPlus > s.DiMinus && s.Macd > s.MacdSignal;
            s.ShortColor = s.DiMinus > s.DiPlus && s.MacdSignal > s.Macd;
            s.Trade = s.LongColor ? 1 : s.ShortColor ? -1 : p?.Trade ?? 0;
            s.LongLabel = p != null && p.Trade != 1 && s.Trade == 1;
            s.ShortLabel = p != null && p.Trade != -1 && s.Trade == -1;
            s.HaClose = (b.Open + b.High + b.Low + b.Close) / 4;
            s.HaOpen = p == null ? (b.Open + b.Close) / 2 : (p.HaOpen + p.HaClose) / 2;
            s.HaHigh = Math.Max(b.High, Math.Max(s.HaOpen, s.HaClose));
            s.HaLow = Math.Min(b.Low, Math.Min(s.HaOpen, s.HaClose));
            double span = Math.Max(s.Upper - s.Lower, tickSize);
            s.HaStrength = Math.Min(1, Math.Abs(s.HaClose - s.HaOpen) / span);
            double half = Math.Max(span / 2, tickSize);
            double waveRaw = (b.Close - s.Basis) / half;
            double wave = Put("wave", i, MathSeries.Ema(waveRaw, Prev("wave", i), c.WavePeriod));
            s.WaveClamped = Math.Max(-1, Math.Min(1, wave));
            s.Wave = s.Basis + s.WaveClamped * half * c.WaveAmplitude;
            s.WaveSlope = p != null && double.IsFinite(p.Wave) ? s.Wave - p.Wave : 0;
            s.Ready = p != null && double.IsFinite(s.Upper) && double.IsFinite(p.Upper);
            Values[i] = s;
            return s;
        }
    }
    public enum IntentKind { Entry, Close }
    public readonly struct Intent
    {
        public readonly IntentKind Kind;
        public readonly int Side;
        public readonly string Rule;
        public Intent(IntentKind kind, int side, string rule) { Kind = kind; Side = side; Rule = rule; }
    }
    public static class XiDecisions
    {
        public static List<Intent> Prepare(List<Intent> intents, int positionSide)
        {
            var eligible = intents.Where(x => x.Kind != IntentKind.Entry || x.Side != positionSide).ToList();
            var last = new Dictionary<int, int>();
            for (int i = 0; i < eligible.Count; i++) if (eligible[i].Kind == IntentKind.Entry) last[eligible[i].Side] = i;
            return eligible.Where((x, i) => x.Kind != IntentKind.Entry || last[x.Side] == i).ToList();
        }
        // Order calls exactly follow the supplied source order. Snapshot position is pre-fill.
        public static List<Intent> Evaluate(Snapshot s, int positionSide, double openProfit, double threshold)
        {
            var r = new List<Intent>();
            if (s.LongBreakout) r.Add(new Intent(IntentKind.Entry, 1, "CORE_LONG"));
            if (s.ShortBreakout) r.Add(new Intent(IntentKind.Entry, -1, "CORE_SHORT"));
            if (positionSide > 0 && s.LongExit) r.Add(new Intent(IntentKind.Close, 1, "CORE_CLOSE_LONG"));
            if (positionSide < 0 && s.ShortExit) r.Add(new Intent(IntentKind.Close, -1, "CORE_CLOSE_SHORT"));
            const double scale = 10000000000000.0;
            if (s.DailyDown && s.Hull2 * scale > s.Hull * scale && openProfit > threshold)
                r.Add(new Intent(IntentKind.Close, 1, "PROFIT_CLOSE_LONG"));
            if (s.DailyUp && s.Hull * scale > s.Hull2 * scale && openProfit > threshold)
                r.Add(new Intent(IntentKind.Close, -1, "PROFIT_CLOSE_SHORT"));
            if (s.LongTrend) r.Add(new Intent(IntentKind.Entry, 1, "TREND_PC_LONG"));
            if (s.ShortTrend) r.Add(new Intent(IntentKind.Entry, -1, "TREND_PC_SHORT"));
            return r;
        }
    }
}

namespace FunctionalMixtureXI
{
    // Historical lookahead_off sampling is deliberately separate from developing realtime MTF bars.
    public sealed class XiMarketData
    {
        private readonly Bars _chart, _daily;
        private readonly CoreConfig _config;
        private readonly Feed[] _feeds;
        public TimeSpan ChartPeriod { get; }
        public sealed class Feed
        {
            public readonly Bars Bars;
            public readonly TimeSpan Period;
            public readonly List<double> Ema = new List<double>();
            private readonly CoreConfig _config;
            private DateTime _first;
            public Feed(Bars bars, CoreConfig config) { Bars = bars; _config = config; Period = Duration(bars.TimeFrame); }
            public void Ensure(int index)
            {
                if (Bars.Count == 0) return;
                if (_first != Bars.OpenTimes[0]) { Ema.Clear(); _first = Bars.OpenTimes[0]; }
                int start = Math.Max(0, Ema.Count - 1);
                for (int k = start; k <= index; k++)
                    MathSeries.Put(Ema, k, MathSeries.Ema(Read(Bars, k).Source(_config.PcSource),
                        MathSeries.Get(Ema, k - 1), _config.PcCenterPeriod));
            }
            public int Direction(DateTime at, bool historical)
            {
                int k = Select(Bars, at, Period, historical);
                if (k < 0) return 0;
                Ensure(k);
                double value = Read(Bars, k).Source(_config.PcSource), ema = Ema[k];
                return value > ema ? 1 : value < ema ? -1 : 0;
            }
        }
        public XiMarketData(Bars chart, Func<TimeFrame, Bars> getBars, CoreConfig config, bool loadHistory)
        {
            _chart = chart; _config = config; ChartPeriod = Duration(chart.TimeFrame);
            _daily = getBars(TimeFrame.Daily);
            var keys = new[] { config.PcTimeframe1, config.PcTimeframe2, config.PcTimeframe3 };
            _feeds = keys.Select(x => new Feed(getBars(TimeFrame.Parse(x)), config)).ToArray();
            if (loadHistory && chart.Count > 0)
            {
                DateTime start = chart.OpenTimes[0].AddDays(-10);
                LoadTo(_daily, start);
                foreach (var f in _feeds) LoadTo(f.Bars, start);
            }
        }
        public static void LoadTo(Bars bars, DateTime start)
        {
            for (int attempt = 0; attempt < 100 && bars.Count > 0 && bars.OpenTimes[0] > start; attempt++)
                if (bars.LoadMoreHistory() == 0) break;
        }
        public static Candle Read(Bars b, int i) => new Candle(b.OpenTimes[i], b.OpenPrices[i], b.HighPrices[i],
            b.LowPrices[i], b.ClosePrices[i], b.TickVolumes[i]);
        public Context At(int chartIndex, DateTime evaluationTime, bool historical)
        {
            DateTime at = historical ? _chart.OpenTimes[chartIndex] + ChartPeriod : evaluationTime;
            // Same-timeframe security equals the chart context even during close evaluation.
            int[] directions = _feeds.Select(f => f.Direction(at, historical)).ToArray();
            int d = Select(_daily, at, TimeSpan.FromDays(1), historical);
            double a = d >= 0 ? _daily.ClosePrices[d] : double.NaN;
            double b = d > 0 ? _daily.ClosePrices[d - 1] : double.NaN;
            return new Context(a, b, directions.All(x => x == 1), directions.All(x => x == -1));
        }
        public static int Select(Bars b, DateTime at, TimeSpan period, bool closedOnly)
        {
            return TemporalIndex.Select(b.Count, i => b.OpenTimes[i], at, period, closedOnly);
        }
        public static TimeSpan Duration(TimeFrame tf)
        {
            string n = tf.Name;
            if (n == "Minute") return TimeSpan.FromMinutes(1);
            if (n == "Hour") return TimeSpan.FromHours(1);
            if (n == "Daily" || n == "Day") return TimeSpan.FromDays(1);
            foreach (string prefix in new[] { "Minute", "Hour", "Day" })
                if (n.StartsWith(prefix) && int.TryParse(n.Substring(prefix.Length), out int x) && x > 0)
                    return prefix == "Minute" ? TimeSpan.FromMinutes(x) : prefix == "Hour" ? TimeSpan.FromHours(x) : TimeSpan.FromDays(x);
            throw new ArgumentException("XI supports fixed-duration time bars; unsupported timeframe: " + n);
        }
    }
    public sealed class VisualConfig
    {
        public bool Envelope { get; set; } = true;
        public bool HeikinAshi { get; set; } = true;
        public bool Wave { get; set; } = true;
        public bool Advantage { get; set; } = true;
        public bool SignalLabels { get; set; } = true;
        public bool DiMacdLabels { get; set; } = true;
        public bool CandleColors { get; set; } = true;
        public bool PcBackground { get; set; } = true;
        public bool PcColumns { get; set; } = true;
        public bool Measures { get; set; } = true;
        public bool LevelLines { get; set; } = true;
        public bool CloudBox { get; set; } = true;
        public bool Distance { get; set; } = true;
        public bool Width { get; set; } = true;
        public bool Delta { get; set; } = true;
        public bool TradeMetrics { get; set; } = true;
        public int MeasureHistory { get; set; } = 50;
        public int MaxDrawings { get; set; } = 450;
        public string LabelPosition { get; set; } = "AutoHighLow";
        public string Theme { get; set; } = "Aurora";
        public bool CustomPalette { get; set; } = true;
        public string Bull { get; set; } = "#008080";
        public string Bear { get; set; } = "#800000";
        public string Neutral { get; set; } = "#80000000";
        public string Text { get; set; } = "#FFFFFFFF";
        public string LongSignal { get; set; } = "#40008080";
        public string ShortSignal { get; set; } = "#40800000";
        public string WaveUp { get; set; } = "#8C008080";
        public string WaveDown { get; set; } = "#8C800000";
        public string CandleUp { get; set; } = "#CC000000";
        public string CandleDown { get; set; } = "#66800000";
        public string MaUp { get; set; } = "#40008080";
        public string MaDown { get; set; } = "#40800000";
        public string PcLong { get; set; } = "#B3008080";
        public string PcShort { get; set; } = "#B3800000";
        public string Basis { get; set; } = "#80FFFF00";
        public string Filter { get; set; } = "#8000FFFF";
        public string PcBgLong { get; set; } = "#80808000";
        public string PcBgShort { get; set; } = "#80800000";
        public string AdvantageLong { get; set; } = "#80008080";
        public string AdvantageShort { get; set; } = "#80800000";
        public string DistanceColor { get; set; } = "#80800080";
        public string WidthColor { get; set; } = "#80FF00FF";
        public string DeltaUp { get; set; } = "#80008080";
        public string DeltaDown { get; set; } = "#80800000";
        public string LevelColor { get; set; } = "#80C0C0C0";
        public string BoxBorder { get; set; } = "#F2000080";
        public string BoxFill { get; set; } = "#EB808000";
        public string MeasureText { get; set; } = "#80FFFFFF";
        public string TradeProfit { get; set; } = "#80008080";
        public string TradeLoss { get; set; } = "#80800000";
        public string LegacyCandleUp { get; set; } = "#80008080";
        public string LegacyCandleDown { get; set; } = "#80800000";
        public bool Ichimoku { get; set; } = true;
        public int Tenkan { get; set; } = 9;
        public int Kijun { get; set; } = 26;
        public int SenkouB { get; set; } = 52;
        public int Displacement { get; set; } = 26;
        public bool VolumeCandles { get; set; } = true;
        public int VolumePeriod { get; set; } = 20;
        public double VolumeCap { get; set; } = 3;
        public int CandleHistory { get; set; } = 80;
        public void Validate()
        {
            if (Tenkan < 1 || Kijun < 1 || SenkouB < 1 || Displacement < 0 || VolumePeriod < 1 ||
                VolumeCap <= 0 || CandleHistory < 1 || MaxDrawings < 10 || MeasureHistory < 1)
                throw new ArgumentException("Invalid navigation/drawing settings.");
        }
    }
}

namespace FunctionalMixtureXI
{
    internal sealed class XiChartRenderer : IDisposable
    {
        private readonly Chart Chart;
        private readonly Bars Bars;
        private readonly cAlgo.API.Internals.Symbol Symbol;
        private readonly List<PlotLine> _plots = new List<PlotLine>();
        private readonly Queue<string> _plotNames = new Queue<string>();
        private readonly HashSet<string> _plotKnown = new HashSet<string>();
        private readonly int _history = 180;
        private int _lastRendered = -1;
        private readonly PlotLine Basis, Filter, Upper, Lower, Ma1, Ma2, Ma3, Wave, WaveBasis,
            PcCenter, PcUpper, PcLower, Tenkan, Kijun, SpanA, SpanB, Chikou;
        private CoreConfig _core;
        private VisualConfig _v;
        private XiKernel _kernel;
        private XiMarketData _market;
        private readonly Queue<string> _names = new Queue<string>();
        private readonly HashSet<string> _known = new HashSet<string>();
        private string _prefix;

        private Color Bull => Color.FromHex(_v.CustomPalette ? _v.Bull : "#000000");
        private Color Bear => Color.FromHex(_v.CustomPalette ? _v.Bear : "#FFFFFF");
        private Color Neutral => Color.FromHex(_v.CustomPalette ? _v.Neutral : _v.Theme == "Classic" ? "#808080" : "#68A0FF");
        private Color C(string hex) => Color.FromHex(hex);
        private Color Alpha(Color color, int opacity) => Color.FromArgb(Math.Max(0, Math.Min(255, opacity)), color);
        public XiChartRenderer(Chart chart, Bars bars, cAlgo.API.Internals.Symbol symbol,
            CoreConfig config, VisualConfig visual, string owner)
        {
            Chart = chart; Bars = bars; Symbol = symbol; _core = config; _v = visual;
            _core.Validate(); _v.Validate();
            if (!_v.CustomPalette)
            {
                _v.CandleUp = _v.LegacyCandleUp; _v.CandleDown = _v.LegacyCandleDown;
                _v.MaUp = "#FF000000"; _v.MaDown = "#FFFFFFFF";
                _v.Basis = "#FFFFFF00"; _v.Filter = "#FF808000";
                _v.WaveUp = "#FF000000"; _v.WaveDown = "#FFFFFFFF";
                _v.PcLong = "#FF800000"; _v.PcShort = "#FF008080";
                _v.PcBgLong = "#FF008080"; _v.PcBgShort = "#FF800000";
                _v.AdvantageLong = "#FF000000"; _v.AdvantageShort = "#FFFFFFFF";
                _v.LongSignal = "#FF008080"; _v.ShortSignal = "#FF800000";
            }
            _prefix = owner + ":built-in:";
            Basis=Plot("basis",Color.Yellow,2); Filter=Plot("filter",Color.Aqua);
            Upper=Plot("upper",Color.Teal,2); Lower=Plot("lower",Color.Maroon,2);
            Ma1=Plot("wma1",Color.Teal,4); Ma2=Plot("wma2",Color.Teal); Ma3=Plot("wma3",Color.Maroon);
            Wave=Plot("wave",Color.Teal,2); WaveBasis=Plot("waveBase",Color.Transparent);
            PcCenter=Plot("pcCenter",Color.Gray); PcUpper=Plot("pcUpper",Color.Gray); PcLower=Plot("pcLower",Color.Gray);
            Tenkan=Plot("tenkan",Color.DeepSkyBlue); Kijun=Plot("kijun",Color.DarkOrange);
            SpanA=Plot("senkouA",Color.Teal); SpanB=Plot("senkouB",Color.Maroon); Chikou=Plot("chikou",Color.Silver);
        }
        private PlotLine Plot(string name, Color color, int thickness=1)
        { var p=new PlotLine(name,color,thickness);_plots.Add(p);return p; }
        public void Render(XiKernel kernel, XiMarketData market)
        {
            _kernel=kernel; _market=market;
            int last=kernel.Values.Count-1;
            if(last<0)return;
            // Full geometry once; subsequent ticks update the newest/just-closed bars only.
            int first=_lastRendered<0 ? Math.Max(0,last-_history-_v.Displacement) : Math.Max(0,_lastRendered-1);
            for(int i=first;i<=last;i++) RenderBar(i,kernel.Values[i]);
            _lastRendered=last;
        }
        private void RenderBar(int index, Snapshot s)
        {
            Basis[index] = s.Basis; Filter[index] = s.Filter;
            Upper[index] = _v.Envelope ? s.Upper : double.NaN;
            Lower[index] = _v.Envelope ? s.Lower : double.NaN;
            Ma1[index] = s.Wma1; Ma2[index] = s.Wma2; Ma3[index] = s.Wma3;
            Wave[index] = _v.Wave ? s.Wave : double.NaN;
            WaveBasis[index] = _v.Wave ? s.Basis : double.NaN;
            PcCenter[index] = s.PcCenter; PcUpper[index] = s.PcUpper; PcLower[index] = s.PcLower;
            SetLineAppearance(Basis.LineOutput, index, 1, Alpha(C(_v.Basis), 179));
            SetLineAppearance(Filter.LineOutput, index, 1, Alpha(C(_v.Filter), 179));
            SetLineAppearance(Ma1.LineOutput, index, 1, C(s.DailyUp ? _v.MaUp : _v.MaDown));
            Color trend = C(s.Wma2 > s.Wma3 ? _v.MaUp : _v.MaDown);
            SetLineAppearance(Ma2.LineOutput, index, 1, trend); SetLineAppearance(Ma3.LineOutput, index, 1, trend);
            SetLineAppearance(Upper.LineOutput, index, 1, Alpha(s.Close >= s.Basis ? Bull : Neutral, 166));
            SetLineAppearance(Lower.LineOutput, index, 1, Alpha(s.Close < s.Basis ? Bear : Neutral, 166));
            SetLineAppearance(Wave.LineOutput, index, 1, Alpha(C(s.WaveClamped >= 0 ? _v.WaveUp : _v.WaveDown), 179));
            if (_v.CandleColors)
                Chart.SetBarColor(index, s.LongColor ? C(_v.CandleUp) : s.ShortColor ? C(_v.CandleDown) : Neutral);
            else Chart.ResetBarColor(index);
            double tenkan = Midpoint(index, _v.Tenkan), kijun = Midpoint(index, _v.Kijun);
            Tenkan[index] = _v.Ichimoku ? tenkan : double.NaN;
            Kijun[index] = _v.Ichimoku ? kijun : double.NaN;
            SpanA[index + _v.Displacement] = _v.Ichimoku ? (tenkan + kijun) / 2 : double.NaN;
            SpanB[index + _v.Displacement] = _v.Ichimoku ? Midpoint(index, _v.SenkouB) : double.NaN;
            if (index >= _v.Displacement) Chikou[index - _v.Displacement] = _v.Ichimoku ? s.Close : double.NaN;
            if (index >= Bars.Count - _v.CandleHistory) DrawBar(index, s);
            DrawPlots(index);
            DrawPlots(index + _v.Displacement);
            if (index >= _v.Displacement) DrawPlots(index - _v.Displacement);
            if (index == Bars.Count - 1)
            {
                // Refresh the preceding bar so confirmed-only labels appear when a new bar opens.
                if (index > 0) DrawBar(index - 1, _kernel.Values[index - 1]);
                DrawMeasures(index, s, tenkan, kijun);
            }
        }
        private double Midpoint(int i, int length)
        {
            if (i < length - 1) return double.NaN;
            double high = double.NegativeInfinity, low = double.PositiveInfinity;
            for (int j = i - length + 1; j <= i; j++) { high = Math.Max(high, Bars.HighPrices[j]); low = Math.Min(low, Bars.LowPrices[j]); }
            return (high + low) / 2;
        }
        private string Name(string key)
        {
            string n = _prefix + key;
            if (_known.Add(n)) _names.Enqueue(n);
            while (_names.Count > _v.MaxDrawings) { string old = _names.Dequeue(); _known.Remove(old); Chart.RemoveObject(old); }
            return n;
        }
        private void Remove(string key) => Chart.RemoveObject(_prefix + key);
        private double RelativeVolume(int i)
        {
            if (i < _v.VolumePeriod - 1) return double.NaN;
            double sum = 0;
            for (int j = i - _v.VolumePeriod + 1; j <= i; j++) sum += Bars.TickVolumes[j];
            return sum > 0 ? Bars.TickVolumes[i] / (sum / _v.VolumePeriod) : 0;
        }
        private void DrawBar(int i, Snapshot s)
        {
            var time = Bars.OpenTimes[i]; var period = _market.ChartPeriod;
            var end = time + period;
            string id = time.Ticks.ToString(CultureInfo.InvariantCulture);
            double rvol = RelativeVolume(i);
            if (_v.HeikinAshi)
            {
                double fraction = _v.VolumeCandles && double.IsFinite(rvol) ? Math.Clamp(rvol / _v.VolumeCap, 0, 1) : .65;
                double halfWidth = _v.VolumeCandles ? .12 + .30 * fraction : .32;
                var center = time + TimeSpan.FromTicks(period.Ticks / 2);
                var left = center - TimeSpan.FromTicks((long)(period.Ticks * halfWidth));
                var right = center + TimeSpan.FromTicks((long)(period.Ticks * halfWidth));
                double strength = double.IsFinite(s.HaStrength) ? s.HaStrength : 0;
                Color hue = s.HaClose >= s.HaOpen ? Bull : Bear;
                Color color = Alpha(hue, (int)(77 + 127 * strength));
                var body = Chart.DrawRectangle(Name(id + ":ha"), left, Math.Max(s.HaOpen, s.HaClose), right,
                    Math.Min(s.HaOpen, s.HaClose), color); body.IsFilled = true; body.IsInteractive = false;
                Chart.DrawTrendLine(Name(id + ":wick"), center, s.HaLow, center, s.HaHigh, Alpha(hue, 115));
                if (_v.VolumeCandles && double.IsFinite(rvol))
                {
                    double level = Math.Min(s.HaOpen, s.HaClose) + Math.Abs(s.HaClose - s.HaOpen) * fraction;
                    Chart.DrawTrendLine(Name(id + ":volume"), left, level, right, level, C(_v.Text));
                }
            }
            if (_v.PcBackground && (s.PcLong || s.PcShort) && double.IsFinite(s.Upper))
            {
                var bg = Chart.DrawRectangle(Name(id + ":pcBg"), time, s.Upper, end, s.Lower,
                    Alpha(C(s.PcLong ? _v.PcBgLong : _v.PcBgShort), 25)); bg.IsFilled = true;
            }
            else Remove(id + ":pcBg");
            bool advL = s.Close <= s.Upper && s.Close >= s.Basis && s.WaveClamped >= 0;
            bool advS = s.Close >= s.Lower && s.Close < s.Basis && s.WaveClamped < 0;
            if (_v.Advantage && (advL || advS))
            {
                var bg = Chart.DrawRectangle(Name(id + ":adv"), time, s.Upper, end, s.Lower,
                    Alpha(C(advL ? _v.AdvantageLong : _v.AdvantageShort), 18)); bg.IsFilled = true;
            }
            else Remove(id + ":adv");
            double offset = double.IsFinite(s.AtrOffset) ? s.AtrOffset : Math.Max(Symbol.TickSize, Bars.HighPrices[i] - Bars.LowPrices[i]);
            if (_v.PcColumns && (s.PcLongFlip || s.PcShortFlip))
                Chart.DrawIcon(Name(id + ":pc"), s.PcLongFlip ? ChartIconType.UpTriangle : ChartIconType.DownTriangle,
                    i, s.PcLongFlip ? Bars.LowPrices[i] - offset * .5 : Bars.HighPrices[i] + offset * .5,
                    C(s.PcLongFlip ? _v.PcLong : _v.PcShort));
            else Remove(id + ":pc");
            if (_v.DiMacdLabels && (s.LongLabel || s.ShortLabel))
                Chart.DrawText(Name(id + ":di"), s.LongLabel ? "DI BUY" : "DI SELL", i,
                    s.LongLabel ? Bars.LowPrices[i] - offset : Bars.HighPrices[i] + offset,
                    C(s.LongLabel ? _v.LongSignal : _v.ShortSignal));
            else Remove(id + ":di");
            // Pine barstate.isconfirmed labels: never paint the developing bar as confirmed.
            if (_v.SignalLabels && i < Bars.Count - 1 && (s.LongBreakout || s.ShortBreakout))
            {
                bool above = s.LongBreakout ? _v.LabelPosition == "Above" : _v.LabelPosition != "Below";
                Chart.DrawText(Name(id + ":breakout"), s.LongBreakout ? "BREAKOUT BUY" : "BREAKOUT SELL", i,
                    above ? Bars.HighPrices[i] + offset : Bars.LowPrices[i] - offset,
                    C(s.LongBreakout ? _v.LongSignal : _v.ShortSignal));
            }
        }
        private void DrawMeasures(int i, Snapshot s, double tenkan, double kijun)
        {
            if (_v.Measures && double.IsFinite(s.Upper))
            {
                int x1 = Math.Max(0, i - _v.MeasureHistory);
                if (_v.LevelLines)
                {
                    Chart.DrawTrendLine(Name("levelU"), x1, s.Upper, i, s.Upper, C(_v.LevelColor), 1, LineStyle.Lines);
                    Chart.DrawTrendLine(Name("levelL"), x1, s.Lower, i, s.Lower, C(_v.LevelColor), 1, LineStyle.Lines);
                    Chart.DrawTrendLine(Name("levelM"), x1, (s.Upper + s.Lower) / 2, i, (s.Upper + s.Lower) / 2, C(_v.LevelColor), 1, LineStyle.Dots);
                }
                if (_v.CloudBox)
                {
                    var box = Chart.DrawRectangle(Name("measureBox"), x1, s.Upper, i, s.Lower, Alpha(C(_v.BoxFill), 18));
                    box.IsFilled = true;
                    Chart.DrawRectangle(Name("measureBorder"), x1, s.Upper, i, s.Lower, C(_v.BoxBorder));
                }
                if (_v.Delta) Chart.DrawTrendLine(Name("delta"), i, s.Basis, i, s.Close, C(s.Close >= s.Basis ? _v.DeltaUp : _v.DeltaDown), 2);
            }
            string f(double v) => double.IsFinite(v) ? v.ToString("0.#####", CultureInfo.InvariantCulture) : "warming";
            string metrics = "XI navigation · indicators only\n" +
                $"Basis {f(s.Basis)} | ATR offset {f(s.AtrOffset)} | Vol rank {f(s.Rank)}\n" +
                $"DI+ {f(s.DiPlus)} DI− {f(s.DiMinus)} ADX {f(s.Adx)} | MACD {f(s.Macd)} / {f(s.MacdSignal)}\n" +
                $"Daily {(s.DailyUp ? "UP" : s.DailyDown ? "DOWN" : "FLAT")} | HMA {f(s.Hull)} / lag2 {f(s.Hull2)}\n" +
                $"PC {(s.PcLong ? "LONG SYNC" : s.PcShort ? "SHORT SYNC" : "MIXED")} | Flip {(s.PcLongFlip ? "LONG" : s.PcShortFlip ? "SHORT" : "—")}\n" +
                $"Ichimoku NAV: Tenkan {f(tenkan)} Kijun {f(kijun)} | RVOL {f(RelativeVolume(i))} (tick volume)";
            if (_v.Measures && _v.Distance) metrics += $"\nDistance {f(s.Close - s.Basis)} ({f(s.Basis == 0 ? double.NaN : 100 * (s.Close - s.Basis) / s.Basis)}%)";
            if (_v.Measures && _v.Width) metrics += $" | Cloud width {f(s.Upper - s.Lower)}";
            Chart.DrawStaticText(Name("panel"), metrics, VerticalAlignment.Top, HorizontalAlignment.Left, C(_v.Text));
        }
        private sealed class PlotLine
        {
            public readonly string Name;
            public readonly Color Color;
            public readonly int Thickness;
            private readonly Dictionary<int,double> _values=new Dictionary<int,double>();
            private readonly Dictionary<int,Color> _colors=new Dictionary<int,Color>();
            public PlotLine(string name,Color color,int thickness) {Name=name;Color=color;Thickness=thickness;}
            public PlotLine LineOutput=>this;
            public double this[int i] {get=>_values.TryGetValue(i,out double v)?v:double.NaN;set=>_values[i]=value;}
            public Color At(int i)=>_colors.TryGetValue(i,out var c)?c:Color;
            public void SetColor(int i,Color color)=>_colors[i]=color;
            public void Prune(int before)
            {
                foreach(int i in _values.Keys.Where(k=>k<before).ToArray())_values.Remove(i);
                foreach(int i in _colors.Keys.Where(k=>k<before).ToArray())_colors.Remove(i);
            }
        }
        private void SetLineAppearance(PlotLine plot,int index,int count,Color color)=>plot.SetColor(index,color);
        private string PlotName(string key)
        {
            string n=_prefix+"plot:"+key;
            if(_plotKnown.Add(n))_plotNames.Enqueue(n);
            // Geometry has a separate fixed bound; MaxDrawings remains the candle/annotation budget.
            while(_plotNames.Count>6000){string old=_plotNames.Dequeue();_plotKnown.Remove(old);Chart.RemoveObject(old);}
            return n;
        }
        private void DrawPlots(int i)
        {
            int last=_kernel.Values.Count-1;
            if(i<1 || i<last-_history)return;
            foreach(var p in _plots)
            {
                if(double.IsFinite(p[i-1])&&double.IsFinite(p[i])&&p.Color!=Color.Transparent)
                    Chart.DrawTrendLine(PlotName(p.Name+":"+i),i-1,p[i-1],i,p[i],p.At(i),p.Thickness);
                else Chart.RemoveObject(_prefix+"plot:"+p.Name+":"+i);
            }
            Snapshot s=_kernel.Values[Math.Min(i,last)];
            Fill("envelope",Upper,Lower,i,Alpha(s.Close>=s.Basis?Bull:Bear,23));
            Fill("ma",Ma2,Ma3,i,Alpha(C(s.Wma2>s.Wma3?_v.MaUp:_v.MaDown),38));
            Fill("wave",Wave,WaveBasis,i,Alpha(s.WaveClamped>=0?Bull:Bear,56));
            Fill("ichi",SpanA,SpanB,i,Alpha(SpanA[i]>=SpanB[i]?Color.Teal:Color.Maroon,46));
            int before=last-_history-_v.Displacement-3;
            if(i==last)foreach(var p in _plots)p.Prune(before);
        }
        private void Fill(string key,PlotLine a,PlotLine b,int i,Color color)
        {
            if(!double.IsFinite(a[i-1])||!double.IsFinite(a[i])||!double.IsFinite(b[i-1])||!double.IsFinite(b[i]))return;
            // Two triangles form a filled segment between the exact series points.
            var t1=Chart.DrawTriangle(PlotName(key+":a:"+i),i-1,a[i-1],i,a[i],i,b[i],color);
            var t2=Chart.DrawTriangle(PlotName(key+":b:"+i),i-1,a[i-1],i,b[i],i-1,b[i-1],color);
            t1.IsFilled=true;t2.IsFilled=true;t1.IsInteractive=false;t2.IsInteractive=false;
        }
        public void Dispose()
        {
            foreach(string name in _known)Chart.RemoveObject(name);
            foreach(string name in _plotKnown)Chart.RemoveObject(name);
            if(_v.CandleColors)Chart.ResetBarColors();
            _known.Clear();_plotKnown.Clear();_names.Clear();_plotNames.Clear();
        }
    }
}
