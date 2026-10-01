// MPL-2.0; port of ClassicPotatoChamberXII ETHER Spatial Energy Matrix v2.
// Source: ETHER_POISON_STANDALONE.pine and EP_Math/Types/Engines v2.
using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;

namespace cAlgo.Robots
{
    public enum EtherOrderEngine { Market, Limit }
    public enum EtherSizing { FixedLots, EquityRiskPercent }

    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class EtherPoisonXIVcBot : Robot
    {
        [Parameter("Instance label", DefaultValue = "ETHER-XIV")]
        public string InstanceLabel { get; set; }
        [Parameter("Orders", DefaultValue = EtherOrderEngine.Market)]
        public EtherOrderEngine OrdersMode { get; set; }
        [Parameter("Entry lots", DefaultValue = 0.01, MinValue = 0.0)]
        public double EntryLots { get; set; }
        [Parameter("Sizing", DefaultValue = EtherSizing.EquityRiskPercent)]
        public EtherSizing SizingMode { get; set; }
        [Parameter("Entry risk % equity", DefaultValue = 1.0, MinValue = 0.01, MaxValue = 100)]
        public double EntryRiskPercent { get; set; }
        [Parameter("Close basket loss %", DefaultValue = 2.0, MinValue = 0.0, MaxValue = 100)]
        public double RiskClosePercent { get; set; }
        [Parameter("Maximum basket stop risk %", DefaultValue = 2.0, MinValue = 0.0, MaxValue = 100)]
        public double MaxBasketRiskPercent { get; set; }
        [Parameter("Risk exit pause (bars)", DefaultValue = 3, MinValue = 0)]
        public int RiskExitPauseBars { get; set; }
        [Parameter("Refresh / wakeup (seconds)", DefaultValue = 60, MinValue = 5)]
        public int RefreshSeconds { get; set; }
        [Parameter("Limit retreat (ATR)", DefaultValue = 0.25, MinValue = 0.0)]
        public double LimitRetreatAtr { get; set; }
        [Parameter("Limit lifespan (bars)", DefaultValue = 3, MinValue = 1)]
        public int LimitLifespan { get; set; }

        [Parameter("ATR length", DefaultValue = 14, MinValue = 2)] public int AtrLength { get; set; }
        [Parameter("Momentum horizon", DefaultValue = 14, MinValue = 2)] public int MomentumLength { get; set; }
        [Parameter("Kinetic saturation", DefaultValue = 2.5, MinValue = 0.1)] public double KineticScale { get; set; }
        [Parameter("Pivot left", DefaultValue = 5, MinValue = 1)] public int PivotLeft { get; set; }
        [Parameter("Pivot right", DefaultValue = 5, MinValue = 1)] public int PivotRight { get; set; }
        [Parameter("Minimum FVG (ATR)", DefaultValue = 0.15, MinValue = 0)] public double MinimumFvgAtr { get; set; }
        [Parameter("Zone lifetime", DefaultValue = 80, MinValue = 1)] public int ZoneLifetime { get; set; }
        [Parameter("Structure window", DefaultValue = 12, MinValue = 1)] public int StructureWindow { get; set; }
        [Parameter("CHoCH displacement", DefaultValue = 0.25, MinValue = 0)] public double ChochAtr { get; set; }
        [Parameter("Tenkan", DefaultValue = 9, MinValue = 1)] public int TenkanLength { get; set; }
        [Parameter("Kijun", DefaultValue = 26, MinValue = 1)] public int KijunLength { get; set; }
        [Parameter("Span B", DefaultValue = 52, MinValue = 2)] public int SpanBLength { get; set; }
        [Parameter("SuperBoll length", DefaultValue = 34, MinValue = 5)] public int SuperBollLength { get; set; }
        [Parameter("SuperBoll deviation", DefaultValue = 2.0, MinValue = 0.1)] public double SuperBollDeviation { get; set; }
        [Parameter("Time wave length", DefaultValue = 21, MinValue = 3)] public int TimeRailLength { get; set; }
        [Parameter("Rapid cloud length", DefaultValue = 9, MinValue = 2)] public int RapidLength { get; set; }
        [Parameter("Rapid width (ATR)", DefaultValue = 0.65, MinValue = 0)] public double RapidExpansion { get; set; }
        [Parameter("Volatility fast", DefaultValue = 13, MinValue = 2)] public int VolatilityFastLength { get; set; }
        [Parameter("Volatility slow", DefaultValue = 34, MinValue = 3)] public int VolatilitySlowLength { get; set; }
        [Parameter("Volatility contour", DefaultValue = 0.35, MinValue = 0)] public double VolatilityCloudAtr { get; set; }
        [Parameter("Render indicators", DefaultValue = true)] public bool RenderIndicators { get; set; }
        [Parameter("Draw bars", DefaultValue = 100, MinValue = 20, MaxValue = 250)] public int DrawBars { get; set; }

        [Parameter("Entry threshold", DefaultValue = 0.62, MinValue = 0, MaxValue = 1)] public double EntryThreshold { get; set; }
        [Parameter("Energy weight", DefaultValue = 1.0, MinValue = 0)] public double WeightEnergy { get; set; }
        [Parameter("FVG weight", DefaultValue = 1.0, MinValue = 0)] public double WeightFvg { get; set; }
        [Parameter("Structure weight", DefaultValue = 1.0, MinValue = 0)] public double WeightStructure { get; set; }
        [Parameter("Reversion weight", DefaultValue = 0.6, MinValue = 0)] public double WeightReversion { get; set; }
        [Parameter("Cloud weight", DefaultValue = 0.8, MinValue = 0)] public double WeightCloud { get; set; }
        [Parameter("Breakout weight", DefaultValue = 0.8, MinValue = 0)] public double WeightBreakout { get; set; }
        [Parameter("Wave weight", DefaultValue = 0.6, MinValue = 0)] public double WeightWave { get; set; }
        [Parameter("Multiplexer weight", DefaultValue = 0.0, MinValue = 0)] public double WeightMultiplexer { get; set; }

        [Parameter("Multiplexer enabled", DefaultValue = true)] public bool MultiplexerEnabled { get; set; }
        [Parameter("Multiplexer gate", DefaultValue = false)] public bool MultiplexerGate { get; set; }
        [Parameter("Entropy fast", DefaultValue = 8, MinValue = 2)] public int EntropyFast { get; set; }
        [Parameter("Entropy medium", DefaultValue = 21, MinValue = 3)] public int EntropyMedium { get; set; }
        [Parameter("Entropy slow", DefaultValue = 55, MinValue = 4)] public int EntropySlow { get; set; }
        [Parameter("Entropy temperature", DefaultValue = 1.35, MinValue = 0)] public double EntropyTemperature { get; set; }
        [Parameter("Attenuation floor", DefaultValue = 0.18, MinValue = 0, MaxValue = 1)] public double AttenuationFloor { get; set; }
        [Parameter("Maximum entropy", DefaultValue = 0.72, MinValue = 0, MaxValue = 1)] public double MaximumEntropy { get; set; }
        [Parameter("Minimum confidence", DefaultValue = 0.35, MinValue = 0, MaxValue = 1)] public double MinimumConfidence { get; set; }
        [Parameter("Minimum attenuation", DefaultValue = 0.20, MinValue = 0, MaxValue = 1)] public double MinimumAttenuation { get; set; }

        [Parameter("Stop (ATR)", DefaultValue = 1.8, MinValue = 0.1)] public double StopAtr { get; set; }
        [Parameter("Target (ATR)", DefaultValue = 3.0, MinValue = 0.1)] public double TargetAtr { get; set; }
        [Parameter("SL adjustment %", DefaultValue = 100.0, MinValue = 1)] public double StopAdjustmentPercent { get; set; }
        [Parameter("TP adjustment %", DefaultValue = 100.0, MinValue = 1)] public double TargetAdjustmentPercent { get; set; }
        [Parameter("Rescue enabled", DefaultValue = false)] public bool RescueEnabled { get; set; }
        [Parameter("Rescue adverse ATR", DefaultValue = 1.25, MinValue = 0.1)] public double RescueAdverseAtr { get; set; }
        [Parameter("Rescue target ticks", DefaultValue = 8.0, MinValue = 1)] public double RescueTargetTicks { get; set; }
        [Parameter("Maximum add / basket", DefaultValue = 0.5, MinValue = 0)] public double RescueMaximumMultiple { get; set; }
        [Parameter("Rescue cycles", DefaultValue = 2, MinValue = 0)] public int RescueMaximumCycles { get; set; }
        [Parameter("Rescue cooldown", DefaultValue = 8, MinValue = 1)] public int RescueCooldown { get; set; }
        [Parameter("Harvest percent", DefaultValue = 40.0, MinValue = 1, MaxValue = 100)] public double HarvestPercent { get; set; }

        private readonly List<double> _close = new List<double>();
        private readonly List<double> _high = new List<double>();
        private readonly List<double> _low = new List<double>();
        private readonly List<double> _atr = new List<double>();
        private readonly List<double> _basis = new List<double>();
        private readonly List<double> _wave = new List<double>();
        private readonly List<double> _tenkan = new List<double>(), _kijun = new List<double>();
        private readonly List<double> _spanA = new List<double>(), _spanB = new List<double>();
        private readonly List<double> _rapid = new List<double>(), _volFast = new List<double>(), _volSlow = new List<double>();
        private readonly List<double> _muxCenterFast = new List<double>(), _muxCenterMedium = new List<double>(), _muxCenterSlow = new List<double>();
        private readonly List<double> _muxUpper = new List<double>(), _muxLower = new List<double>();
        private readonly List<double> _entropyFast = new List<double>();
        private readonly List<double> _entropyMedium = new List<double>();
        private readonly List<double> _entropySlow = new List<double>();
        private readonly List<double> _directionFast = new List<double>();
        private readonly List<double> _directionMedium = new List<double>();
        private readonly List<double> _directionSlow = new List<double>();
        private double _pivotHigh = double.NaN, _pivotLow = double.NaN;
        private double _bullTop, _bullBottom, _bearTop, _bearBottom;
        private int _bullBar = -1, _bearBar = -1, _bullStructureBar = -100000, _bearStructureBar = -100000;
        private int _structureDirection, _rescueCycles, _lastRescueBar = -100000, _limitPlacedBar = -1;
        private bool _bullInverted, _bearInverted, _rescueRestartLock, _hadPosition;
        private int _riskExitBar = -100000;
        private double _riskReferenceEquity;
        private double _previousKinetic;
        private DateTime _lastBarTime = DateTime.MinValue;
        private string Label { get { return InstanceLabel + ":" + SymbolName; } }
        private Position[] Owned { get { return Positions.Where(p => p.SymbolName == SymbolName && p.Label == Label).ToArray(); } }
        private PendingOrder[] Queued { get { return PendingOrders.Where(p => p.SymbolName == SymbolName && p.Label == Label).ToArray(); } }
        private double EffectiveStopAtr { get { return StopAtr * StopAdjustmentPercent / 100.0; } }
        private double EffectiveTargetAtr { get { return TargetAtr * TargetAdjustmentPercent / 100.0; } }

        protected override void OnStart()
        {
            if (string.IsNullOrWhiteSpace(InstanceLabel) || EntropyMedium <= EntropyFast || EntropySlow <= EntropyMedium)
            {
                Print("Invalid instance label or entropy horizon order."); Stop(); return;
            }
            // OnStart includes a forming bar. Warm state through the preceding closed bar.
            for (int i = 0; i < Bars.Count - 1; i++) Process(i, false);
            _rescueRestartLock = Owned.Length > 0;
            _hadPosition = _rescueRestartLock;
            _riskReferenceEquity = Account.Equity;
            if (Queued.Length > 0) _limitPlacedBar = _close.Count - 1;
            Timer.Start(TimeSpan.FromSeconds(RefreshSeconds));
            Print("XIV ready on {0}; warmed {1} bars. Existing basket: {2}", SymbolName, _close.Count, _hadPosition);
        }

        protected override void OnBarClosed()
        {
            // OnBarClosed Bars excludes the new live bar.
            int i = Bars.Count - 1;
            if (i < 0 || Bars.OpenTimes[i] <= _lastBarTime) return;
            Process(i, true);
        }

        protected override void OnTick()
        {
            CheckBasketRisk();
        }

        protected override void OnTimer()
        {
            CheckBasketRisk();
            // OnTimer sees the live bar; only catch up through the preceding closed bar.
            // A bar already handled by OnBarClosed is never processed again.
            int lastClosed = Bars.Count - 2;
            int refreshed = 0;
            for (int i = _close.Count; i <= lastClosed; i++)
            {
                if (Bars.OpenTimes[i] <= _lastBarTime) continue;
                Process(i, true);
                refreshed++;
            }
            if (refreshed > 0) Print("XIV wakeup: refreshed {0} closed bar(s)", refreshed);
            CheckBasketRisk();
        }

        protected override void OnStop()
        {
            Timer.Stop();
        }

        private void CheckBasketRisk()
        {
            if (RiskClosePercent <= 0 || _riskExitBar >= _close.Count - 1) return;
            Position[] basket = Owned;
            if (basket.Length == 0) return;
            double reference = _riskReferenceEquity > 0 ? _riskReferenceEquity : Account.Equity;
            double loss = basket.Sum(p => p.NetProfit);
            if (loss > -reference * RiskClosePercent / 100.0) return;
            CancelQueued();
            bool allClosed = true;
            foreach (Position p in basket)
            {
                TradeResult result = ClosePosition(p);
                if (!result.IsSuccessful) { allClosed = false; Print("Risk close rejected: {0}", result.Error); }
            }
            if (allClosed) { _riskExitBar = _close.Count - 1; Print("RISK CLOSE loss={0:F2}, threshold={1:F2}", loss, reference * RiskClosePercent / 100.0); }
        }

        private static double Clamp(double x, double lo, double hi) { return Math.Max(lo, Math.Min(hi, x)); }
        private static double Ema(double x, double prior, int length) { return double.IsNaN(prior) ? x : prior + (2.0 / (length + 1.0)) * (x - prior); }
        private static double Last(List<double> xs) { return xs.Count == 0 ? double.NaN : xs[xs.Count - 1]; }
        private static double Entropy(double probability)
        {
            double p = Clamp(probability, 1e-6, 1 - 1e-6);
            return -(p * Math.Log(p) + (1 - p) * Math.Log(1 - p)) / Math.Log(2);
        }
        private static double Kinetic(double change, double atr, double scale)
        {
            return Clamp(Math.Tan(Clamp(Math.Abs(change) / Math.Max(atr, 1e-6) / scale, 0, 1) * Math.PI / 4), 0, 1);
        }
        private static double WindowExtreme(List<double> values, int end, int length, bool highest)
        {
            double result = highest ? double.NegativeInfinity : double.PositiveInfinity;
            for (int j = Math.Max(0, end - length + 1); j <= end; j++)
                result = highest ? Math.Max(result, values[j]) : Math.Min(result, values[j]);
            return result;
        }
        private bool Pivot(List<double> values, int i, bool high)
        {
            int center = i - PivotRight;
            if (center < PivotLeft) return false;
            double v = values[center];
            for (int j = center - PivotLeft; j <= i; j++)
                if (j != center && (high ? values[j] >= v : values[j] <= v)) return false;
            return true;
        }
        private static double PopulationStdev(List<double> values, int end, int length)
        {
            if (end + 1 < length) return double.NaN;
            double mean = 0, sum = 0;
            for (int j = end - length + 1; j <= end; j++) mean += values[j] / length;
            for (int j = end - length + 1; j <= end; j++) sum += (values[j] - mean) * (values[j] - mean) / length;
            return Math.Sqrt(sum);
        }

        private void Process(int sourceIndex, bool trade)
        {
            double close = Bars.ClosePrices[sourceIndex], high = Bars.HighPrices[sourceIndex], low = Bars.LowPrices[sourceIndex];
            int i = _close.Count;
            _lastBarTime = Bars.OpenTimes[sourceIndex];
            double prevClose = i == 0 ? close : _close[i - 1];
            double prevHighPivot = _pivotHigh, prevLowPivot = _pivotLow;
            double previousBasis = Last(_basis), previousWave = Last(_wave);
            double priorAtr = Last(_atr);
            _close.Add(close); _high.Add(high); _low.Add(low);
            double trueRange = Math.Max(high - low, Math.Max(Math.Abs(high - prevClose), Math.Abs(low - prevClose)));
            // Pine ta.atr uses RMA of true range, seeded by its first full SMA.
            double atr = i < AtrLength - 1 ? double.NaN : i == AtrLength - 1
                ? Enumerable.Range(0, AtrLength).Sum(j => j == 0 ? _high[j] - _low[j]
                    : Math.Max(_high[j] - _low[j], Math.Max(Math.Abs(_high[j] - _close[j - 1]), Math.Abs(_low[j] - _close[j - 1])))) / AtrLength
                : (priorAtr * (AtrLength - 1) + trueRange) / AtrLength;
            _atr.Add(atr);
            double past = i >= MomentumLength ? _close[i - MomentumLength] : close;
            double kinetic = double.IsNaN(atr) ? 0 : Kinetic(close - past, atr, KineticScale);
            double signedEnergy = close >= past ? kinetic : -kinetic;

            if (Pivot(_high, i, true)) _pivotHigh = _high[i - PivotRight];
            if (Pivot(_low, i, false)) _pivotLow = _low[i - PivotRight];
            bool bosBull = !double.IsNaN(_pivotHigh) && close > _pivotHigh &&
                (i == 0 || prevClose <= (double.IsNaN(prevHighPivot) ? _pivotHigh : prevHighPivot));
            bool bosBear = !double.IsNaN(_pivotLow) && close < _pivotLow &&
                (i == 0 || prevClose >= (double.IsNaN(prevLowPivot) ? _pivotLow : prevLowPivot));
            bool chochBull = bosBull && _structureDirection < 0 && close - _pivotHigh >= atr * ChochAtr;
            bool chochBear = bosBear && _structureDirection > 0 && _pivotLow - close >= atr * ChochAtr;
            if (bosBull) { _bullStructureBar = i; _structureDirection = 1; }
            if (bosBear) { _bearStructureBar = i; _structureDirection = -1; }
            bool bullGate = i - _bullStructureBar <= StructureWindow;
            bool bearGate = i - _bearStructureBar <= StructureWindow;

            if (!double.IsNaN(atr) && i >= 2 && low > _high[i - 2] && low - _high[i - 2] >= atr * MinimumFvgAtr)
            { _bullTop = low; _bullBottom = _high[i - 2]; _bullBar = i; _bullInverted = false; }
            if (!double.IsNaN(atr) && i >= 2 && high < _low[i - 2] && _low[i - 2] - high >= atr * MinimumFvgAtr)
            { _bearTop = _low[i - 2]; _bearBottom = high; _bearBar = i; _bearInverted = false; }
            if (_bullBar >= 0 && i - _bullBar <= ZoneLifetime && !_bullInverted && close < _bullBottom && prevClose >= _bullBottom) _bullInverted = true;
            if (_bearBar >= 0 && i - _bearBar <= ZoneLifetime && !_bearInverted && close > _bearTop && prevClose <= _bearTop) _bearInverted = true;
            bool bullFvg = _bullBar >= 0 && i - _bullBar <= ZoneLifetime && !_bullInverted && low > _bullBottom;
            bool bearFvg = _bearBar >= 0 && i - _bearBar <= ZoneLifetime && !_bearInverted && high < _bearTop;

            double tenkan = (WindowExtreme(_high, i, TenkanLength, true) + WindowExtreme(_low, i, TenkanLength, false)) / 2;
            double kijun = (WindowExtreme(_high, i, KijunLength, true) + WindowExtreme(_low, i, KijunLength, false)) / 2;
            double spanA = (tenkan + kijun) / 2;
            double spanB = (WindowExtreme(_high, i, SpanBLength, true) + WindowExtreme(_low, i, SpanBLength, false)) / 2;
            _tenkan.Add(tenkan); _kijun.Add(kijun); _spanA.Add(spanA); _spanB.Add(spanB);
            double hlc3 = (high + low + close) / 3.0;
            _rapid.Add(Ema(hlc3, Last(_rapid), RapidLength));
            _volFast.Add(Ema(hlc3, Last(_volFast), VolatilityFastLength));
            _volSlow.Add(Ema(hlc3, Last(_volSlow), VolatilitySlowLength));
            double basis = Ema(close, previousBasis, SuperBollLength);
            _basis.Add(basis);
            double deviation = PopulationStdev(_close, i, SuperBollLength);
            bool breakoutLong = i > 0 && !double.IsNaN(deviation) && close > basis + deviation * SuperBollDeviation;
            bool breakoutShort = i > 0 && !double.IsNaN(deviation) && close < basis - deviation * SuperBollDeviation;
            if (breakoutLong)
            {
                double priorDev = PopulationStdev(_close, i - 1, SuperBollLength);
                breakoutLong = !double.IsNaN(priorDev) && prevClose <= previousBasis + priorDev * SuperBollDeviation;
            }
            if (breakoutShort)
            {
                double priorDev = PopulationStdev(_close, i - 1, SuperBollLength);
                breakoutShort = !double.IsNaN(priorDev) && prevClose >= previousBasis - priorDev * SuperBollDeviation;
            }
            double mean = _close.Count >= SuperBollLength ? _close.Skip(_close.Count - SuperBollLength).Average() : double.NaN;
            double z = !double.IsNaN(deviation) && deviation > 0 ? (close - mean) / deviation : 0;
            double wave = Ema(close - prevClose, previousWave, TimeRailLength);
            _wave.Add(wave);

            double muxLong = 0, muxShort = 0, muxDirection = 0, muxConfidence = 0, muxAttenuation = 0, muxEntropy = 1;
            bool muxQuality = false, muxUp = false, muxDown = false;
            if (!double.IsNaN(atr))
            {
                double norm = Clamp((close - prevClose) / Math.Max(Math.Abs(atr), 1e-6), -1, 1);
                double localEntropy = Entropy(0.5 + norm * 0.5);
                int[] horizons = { EntropyFast, EntropyMedium, EntropySlow };
                List<double>[] entropies = { _entropyFast, _entropyMedium, _entropySlow };
                List<double>[] directions = { _directionFast, _directionMedium, _directionSlow };
                for (int lane = 0; lane < 3; lane++)
                {
                    entropies[lane].Add(Ema(localEntropy, Last(entropies[lane]), horizons[lane]));
                    directions[lane].Add(Ema(close - prevClose, Last(directions[lane]), horizons[lane]));
                }
                double e0 = Last(_entropyFast), e1 = Last(_entropyMedium), e2 = Last(_entropySlow);
                double composite = (e0 + e1 + e2) / 3;
                double attenuation = Clamp(Math.Exp(-EntropyTemperature * composite), AttenuationFloor, 1);
                muxEntropy = composite; muxAttenuation = attenuation;
                double d0 = Clamp(Last(_directionFast) / Math.Max(Math.Abs(atr), 1e-6), -1, 1);
                double d1 = Clamp(Last(_directionMedium) / Math.Max(Math.Abs(atr), 1e-6), -1, 1);
                double d2 = Clamp(Last(_directionSlow) / Math.Max(Math.Abs(atr), 1e-6), -1, 1);
                double w0 = (1 - e0) * attenuation, w1 = (1 - e1) * attenuation, w2 = (1 - e2) * attenuation;
                double direction = w0 + w1 + w2 > 0 ? (d0 * w0 + d1 * w1 + d2 * w2) / (w0 + w1 + w2) : 0;
                double agreement = Clamp(1 - (Math.Abs(d0 - d1) + Math.Abs(d1 - d2) + Math.Abs(d0 - d2)) / 3, 0, 1);
                double confidence = Clamp(Math.Abs(direction) * 0.45 + agreement * 0.30 + (1 - composite) * 0.25, 0, 1);
                muxDirection = direction; muxConfidence = confidence;
                _muxCenterFast.Add(Ema(close, Last(_muxCenterFast), EntropyFast));
                _muxCenterMedium.Add(Ema(close, Last(_muxCenterMedium), EntropyMedium));
                _muxCenterSlow.Add(Ema(close, Last(_muxCenterSlow), EntropySlow));
                double muxCenter = w0 + w1 + w2 > 0 ?
                    (Last(_muxCenterFast) * w0 + Last(_muxCenterMedium) * w1 + Last(_muxCenterSlow) * w2) / (w0 + w1 + w2)
                    : Last(_muxCenterMedium);
                double muxWidthAtr = (0.35 + composite * 0.65) * (0.50 + Math.Abs(direction) * 0.50) * attenuation;
                double muxShift = direction * atr * muxWidthAtr * 0.35;
                _muxUpper.Add(muxCenter + muxShift + atr * muxWidthAtr);
                _muxLower.Add(muxCenter + muxShift - atr * muxWidthAtr);
                muxUp = direction > 0; muxDown = direction < 0;
                muxQuality = MultiplexerEnabled && composite <= MaximumEntropy && confidence >= MinimumConfidence && attenuation >= MinimumAttenuation;
                if (MultiplexerEnabled) { muxLong = muxUp ? confidence * attenuation : 0; muxShort = muxDown ? confidence * attenuation : 0; }
            }

            double total = WeightEnergy + WeightFvg + WeightStructure + WeightReversion + WeightCloud + WeightBreakout + WeightWave + WeightMultiplexer;
            double longPoints = Math.Max(signedEnergy, 0) * WeightEnergy + (bullFvg ? 1 : 0) * WeightFvg + (bullGate ? 1 : 0) * WeightStructure
                + (z < -1 ? Math.Min(Math.Abs(z) / 3, 1) : 0) * WeightReversion + (close > Math.Max(spanA, spanB) ? 1 : 0) * WeightCloud
                + (breakoutLong ? 1 : 0) * WeightBreakout + (wave > 0 && wave > previousWave ? 1 : 0) * WeightWave + muxLong * WeightMultiplexer;
            double shortPoints = Math.Max(-signedEnergy, 0) * WeightEnergy + (bearFvg ? 1 : 0) * WeightFvg + (bearGate ? 1 : 0) * WeightStructure
                + (z > 1 ? Math.Min(Math.Abs(z) / 3, 1) : 0) * WeightReversion + (close < Math.Min(spanA, spanB) ? 1 : 0) * WeightCloud
                + (breakoutShort ? 1 : 0) * WeightBreakout + (wave < 0 && wave < previousWave ? 1 : 0) * WeightWave + muxShort * WeightMultiplexer;
            bool longSignal = total > 0 && bullFvg && bullGate && (!MultiplexerGate || muxQuality && muxUp) && longPoints / total >= EntryThreshold && longPoints > shortPoints;
            bool shortSignal = total > 0 && bearFvg && bearGate && (!MultiplexerGate || muxQuality && muxDown) && shortPoints / total >= EntryThreshold && shortPoints > longPoints;
            if (RenderIndicators && Chart != null && !double.IsNaN(atr) && (trade || sourceIndex >= Bars.Count - DrawBars - 1))
            {
                Render(sourceIndex, i, atr, kinetic, signedEnergy, spanA, spanB, basis, deviation,
                    muxDirection, muxConfidence, muxAttenuation, muxEntropy, longSignal, shortSignal,
                    total > 0 ? longPoints / total : 0, total > 0 ? shortPoints / total : 0);
                string eventName = Label + ":event:" + sourceIndex;
                if (bosBull || bosBear)
                    Chart.DrawText(eventName, chochBull ? "CHoCH ↑" : chochBear ? "CHoCH ↓" : bosBull ? "BOS ↑" : "BOS ↓",
                        sourceIndex, bosBull ? low - atr * 0.25 : high + atr * 0.25, bosBull ? Color.Green : Color.Red);
                string signalName = Label + ":signal:" + sourceIndex;
                if (longSignal || shortSignal)
                    Chart.DrawText(signalName, longSignal ? "BUY APPROVED" : "SELL APPROVED", sourceIndex,
                        longSignal ? low - atr * 0.5 : high + atr * 0.5, longSignal ? Color.Green : Color.Red);
                if (sourceIndex >= DrawBars)
                {
                    Chart.RemoveObject(Label + ":event:" + (sourceIndex - DrawBars));
                    Chart.RemoveObject(Label + ":signal:" + (sourceIndex - DrawBars));
                }
            }
            if (trade && !double.IsNaN(atr)) Execute(i, atr, kinetic, longSignal, shortSignal, close, longPoints / Math.Max(total, 1e-9), shortPoints / Math.Max(total, 1e-9));
            _previousKinetic = kinetic;
        }

        private void Segment(string family, int bar, double before, double now, Color color)
        {
            if (bar < 1 || double.IsNaN(before) || double.IsNaN(now)) return;
            string name = Label + ":" + family + ":" + bar;
            Chart.DrawTrendLine(name, bar - 1, before, bar, now, color);
            if (bar >= DrawBars) Chart.RemoveObject(Label + ":" + family + ":" + (bar - DrawBars));
        }

        private void Render(int bar, int i, double atr, double kinetic, double signedEnergy,
            double spanA, double spanB, double basis, double deviation,
            double muxDirection, double muxConfidence, double muxAttenuation, double muxEntropy,
            bool longSignal, bool shortSignal, double longScore, double shortScore)
        {
            if (i < 1) return;
            Segment("tenkan", bar, _tenkan[i - 1], _tenkan[i], Color.Green);
            Segment("kijun", bar, _kijun[i - 1], _kijun[i], Color.Yellow);
            Segment("ichiA", bar, _spanA[i - 1], spanA, Color.Green);
            Segment("ichiB", bar, _spanB[i - 1], spanB, Color.Red);
            double rapidWidth = atr * RapidExpansion * (0.5 + kinetic);
            double pastAtr = _atr[i - 1], pastKinetic = i > MomentumLength && !double.IsNaN(pastAtr)
                ? Kinetic(_close[i - 1] - _close[i - 1 - MomentumLength], pastAtr, KineticScale) : 0;
            double pastWidth = pastAtr * RapidExpansion * (0.5 + pastKinetic);
            Segment("rapidA", bar, _rapid[i - 1] + pastWidth, _rapid[i] + rapidWidth, Color.Blue);
            Segment("rapidB", bar, _rapid[i - 1] - pastWidth, _rapid[i] - rapidWidth, Color.Blue);
            double pastSigned = i > MomentumLength && !double.IsNaN(pastAtr)
                ? (_close[i - 1] >= _close[i - 1 - MomentumLength] ? pastKinetic : -pastKinetic) : 0;
            Segment("volA", bar, _volFast[i - 1] + pastAtr * VolatilityCloudAtr * pastSigned,
                _volFast[i] + atr * VolatilityCloudAtr * signedEnergy, Color.Blue);
            Segment("volB", bar, _volSlow[i - 1] - pastAtr * VolatilityCloudAtr * pastSigned,
                _volSlow[i] - atr * VolatilityCloudAtr * signedEnergy, Color.Red);
            double priorDev = PopulationStdev(_close, i - 1, SuperBollLength);
            Segment("sbBasis", bar, _basis[i - 1], basis, Color.Yellow);
            Segment("sbUpper", bar, _basis[i - 1] + priorDev * SuperBollDeviation,
                basis + deviation * SuperBollDeviation, Color.Red);
            Segment("sbLower", bar, _basis[i - 1] - priorDev * SuperBollDeviation,
                basis - deviation * SuperBollDeviation, Color.Green);
            if (_muxUpper.Count > 1)
            {
                Segment("muxA", bar, _muxUpper[_muxUpper.Count - 2], Last(_muxUpper), Color.Blue);
                Segment("muxB", bar, _muxLower[_muxLower.Count - 2], Last(_muxLower), Color.Red);
            }
            if (_bullBar >= 0 && i - _bullBar <= ZoneLifetime && !_bullInverted)
                Chart.DrawRectangle(Label + ":bullFVG", Math.Max(_bullBar, bar - DrawBars), _bullTop, bar + 1, _bullBottom, Color.Green);
            else Chart.RemoveObject(Label + ":bullFVG");
            if (_bearBar >= 0 && i - _bearBar <= ZoneLifetime && !_bearInverted)
                Chart.DrawRectangle(Label + ":bearFVG", Math.Max(_bearBar, bar - DrawBars), _bearTop, bar + 1, _bearBottom, Color.Red);
            else Chart.RemoveObject(Label + ":bearFVG");
            string status = string.Format("ETHER XIV | {0} | L {1:P0} S {2:P0} | K {3:F2} | Wave {4:F3}\nMux H {5:F2} Q {6:F2} A {7:F2} D {8:F2}\nRisk {9:F2}% | SL {10:F2} ATR | TP {11:F2} ATR | {12}",
                longSignal ? "BUY" : shortSignal ? "SELL" : "WAIT", longScore, shortScore, kinetic, Last(_wave),
                muxEntropy, muxConfidence, muxAttenuation, muxDirection, RiskClosePercent,
                EffectiveStopAtr, EffectiveTargetAtr, OrdersMode);
            Chart.DrawStaticText(Label + ":board", status, VerticalAlignment.Top, HorizontalAlignment.Right, Color.White);
            Position[] basket = Owned;
            if (basket.Length > 0)
            {
                double units = basket.Sum(p => p.VolumeInUnits);
                double average = basket.Sum(p => p.EntryPrice * p.VolumeInUnits) / units;
                TradeType side = basket[0].TradeType;
                double sl = average + (side == TradeType.Buy ? -1 : 1) * atr * EffectiveStopAtr;
                double tp = average + (side == TradeType.Buy ? 1 : -1) * atr * EffectiveTargetAtr;
                Chart.DrawTrendLine(Label + ":entry", Math.Max(0, bar - 12), average, bar + 2, average, Color.Yellow);
                Chart.DrawTrendLine(Label + ":sl", Math.Max(0, bar - 12), sl, bar + 2, sl, Color.Red);
                Chart.DrawTrendLine(Label + ":tp", Math.Max(0, bar - 12), tp, bar + 2, tp, Color.Green);
            }
            else
            {
                Chart.RemoveObject(Label + ":entry"); Chart.RemoveObject(Label + ":sl"); Chart.RemoveObject(Label + ":tp");
            }
        }

        private void Execute(int bar, double atr, double kinetic, bool longSignal, bool shortSignal, double close, double longScore, double shortScore)
        {
            if (Queued.Length > 0 && _limitPlacedBar >= 0 && bar - _limitPlacedBar >= LimitLifespan) CancelQueued();
            if (bar - _riskExitBar <= RiskExitPauseBars) return;
            Position[] basket = Owned;
            if (basket.Length == 0 && _hadPosition) { _rescueCycles = 0; _lastRescueBar = -100000; _rescueRestartLock = false; }
            _hadPosition = basket.Length > 0;
            // Keep a valid same-direction limit order until fill or expiry.
            PendingOrder[] waiting = Queued;
            if (waiting.Length > 0 && basket.Length == 0 &&
                ((longSignal && waiting.All(p => p.TradeType == TradeType.Buy)) ||
                 (shortSignal && waiting.All(p => p.TradeType == TradeType.Sell)))) return;
            // Pine's basket is net. cTrader hedging accounts need explicit opposite flattening.
            if (longSignal && !basket.Any(p => p.TradeType == TradeType.Buy)) Enter(TradeType.Buy, bar, atr, close, longScore);
            else if (shortSignal && !basket.Any(p => p.TradeType == TradeType.Sell)) Enter(TradeType.Sell, bar, atr, close, shortScore);
            basket = Owned;
            if (basket.Length == 0) return;
            TradeType side = basket[0].TradeType;
            if (basket.Any(p => p.TradeType != side)) { Print("Mixed basket; flatten manually before restarting."); return; }
            double volume = basket.Sum(p => p.VolumeInUnits);
            double average = basket.Sum(p => p.EntryPrice * p.VolumeInUnits) / volume;
            double adverse = side == TradeType.Buy ? average - close : close - average;
            if (RescueEnabled && !_rescueRestartLock && adverse >= atr * RescueAdverseAtr && _rescueCycles < RescueMaximumCycles && bar - _lastRescueBar >= RescueCooldown)
            {
                double targetAverage = close + (side == TradeType.Buy ? 1 : -1) * RescueTargetTicks * Symbol.TickSize;
                double denominator = side == TradeType.Buy ? targetAverage - close : close - targetAverage;
                double numerator = side == TradeType.Buy ? volume * (average - targetAverage) : volume * (targetAverage - average);
                double requested = denominator > 0 && numerator > 0 ? Math.Min(numerator / denominator, volume * RescueMaximumMultiple) : 0;
                // Bound the additional stop exposure, accounting for the whole owned basket.
                double stopPips = atr * EffectiveStopAtr / Symbol.PipSize;
                if (MaxBasketRiskPercent > 0)
                {
                    double used = basket.Sum(p => Symbol.AmountRisked(p.VolumeInUnits,
                        Math.Abs(p.EntryPrice - (average + (side == TradeType.Buy ? -1 : 1) * atr * EffectiveStopAtr)) / Symbol.PipSize));
                    double remaining = Math.Max(0, _riskReferenceEquity * MaxBasketRiskPercent / 100.0 - used);
                    requested = Math.Min(requested, remaining > 0 ? Symbol.VolumeForFixedRisk(remaining, stopPips, RoundingMode.Down) : 0);
                }
                double add = Normalize(requested);
                if (add >= Symbol.VolumeInUnitsMin)
                {
                    TradeResult result = ExecuteMarketOrder(side, SymbolName, add, Label, stopPips, atr * EffectiveTargetAtr / Symbol.PipSize, "XIV rescue");
                    if (result.IsSuccessful) { _rescueCycles++; _lastRescueBar = bar; Print("RESCUE {0}/{1}: {2} units", _rescueCycles, RescueMaximumCycles, add); }
                    else Print("Rescue rejected: {0}", result.Error);
                }
            }
            basket = Owned;
            if (basket.Length == 0) return;
            volume = basket.Sum(p => p.VolumeInUnits);
            average = basket.Sum(p => p.EntryPrice * p.VolumeInUnits) / volume;
            double stop = average + (side == TradeType.Buy ? -1 : 1) * atr * EffectiveStopAtr;
            double target = average + (side == TradeType.Buy ? 1 : -1) * atr * EffectiveTargetAtr;
            // Same ATR and aggregate average are reapplied each closed bar, as in strategy.exit.
            foreach (Position p in basket)
            {
                double exitBidAsk = side == TradeType.Buy ? Symbol.Bid : Symbol.Ask;
                if ((side == TradeType.Buy && stop < exitBidAsk && target > exitBidAsk) || (side == TradeType.Sell && stop > exitBidAsk && target < exitBidAsk))
                {
                    TradeResult edit = ModifyPosition(p, Math.Round(stop, Symbol.Digits), Math.Round(target, Symbol.Digits), ProtectionType.Absolute);
                    if (!edit.IsSuccessful) Print("Protection update rejected: {0}", edit.Error);
                }
            }
            if (kinetic < 0.22 && _previousKinetic >= 0.22)
            {
                double left = Normalize(volume * HarvestPercent / 100.0);
                foreach (Position p in basket.OrderByDescending(p => p.VolumeInUnits))
                {
                    if (left < Symbol.VolumeInUnitsMin) break;
                    double part = Math.Min(p.VolumeInUnits, left);
                    double remainder = p.VolumeInUnits - part;
                    if (remainder > 0 && remainder < Symbol.VolumeInUnitsMin) part = p.VolumeInUnits;
                    TradeResult result = part >= p.VolumeInUnits ? ClosePosition(p) : ClosePosition(p, part);
                    if (!result.IsSuccessful) { Print("Harvest rejected: {0}", result.Error); break; }
                    left -= part;
                }
                Print("ENERGY HARVEST at {0}", close);
            }
        }

        private double Normalize(double volume)
        {
            if (double.IsNaN(volume) || double.IsInfinity(volume) || volume < Symbol.VolumeInUnitsMin) return 0;
            return Symbol.NormalizeVolumeInUnits(Math.Min(volume, Symbol.VolumeInUnitsMax), RoundingMode.Down);
        }
        private void CancelQueued()
        {
            foreach (PendingOrder order in Queued)
            {
                TradeResult result = CancelPendingOrder(order);
                if (!result.IsSuccessful) Print("Cancel rejected: {0}", result.Error);
            }
            _limitPlacedBar = -1;
        }
        private void Enter(TradeType side, int bar, double atr, double close, double score)
        {
            if (Owned.Any(p => p.TradeType != side) && Queued.Length > 0) CancelQueued();
            if (Owned.Any(p => p.TradeType != side))
            {
                CancelQueued();
                foreach (Position p in Owned)
                    if (!ClosePosition(p).IsSuccessful) { Print("Opposite basket close failed; entry blocked"); return; }
                _rescueCycles = 0; _lastRescueBar = -100000; _rescueRestartLock = false;
            }
            double slPips = atr * EffectiveStopAtr / Symbol.PipSize, tpPips = atr * EffectiveTargetAtr / Symbol.PipSize;
            double volume = Normalize(SizingMode == EtherSizing.EquityRiskPercent
                ? Symbol.VolumeForProportionalRisk(ProportionalAmountType.Equity, EntryRiskPercent, slPips, RoundingMode.Down)
                : Symbol.QuantityToVolumeInUnits(EntryLots));
            if (volume < Symbol.VolumeInUnitsMin) { Print("Entry size below symbol minimum"); return; }
            CancelQueued();
            _riskReferenceEquity = Account.Equity;
            TradeResult result;
            if (OrdersMode == EtherOrderEngine.Market)
                result = ExecuteMarketOrder(side, SymbolName, volume, Label, slPips, tpPips, "XIV market");
            else
            {
                double price = side == TradeType.Buy ? Math.Min(close - LimitRetreatAtr * atr, Symbol.Ask - Symbol.TickSize)
                    : Math.Max(close + LimitRetreatAtr * atr, Symbol.Bid + Symbol.TickSize);
                price = Math.Round(price, Symbol.Digits);
                result = PlaceLimitOrder(side, SymbolName, volume, price, Label, slPips, tpPips, ProtectionType.Relative);
                if (result.IsSuccessful) _limitPlacedBar = bar;
            }
            if (!result.IsSuccessful) Print("{0} entry rejected: {1}", OrdersMode, result.Error);
            else Print("{0} {1} score={2:F3} signalClose={3} volume={4} {5}", OrdersMode, side, score, close, volume,
                OrdersMode == EtherOrderEngine.Limit ? "LIMIT PENDING" : "MARKET FILLED");
        }
    }
}
