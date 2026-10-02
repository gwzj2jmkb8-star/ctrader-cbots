
    
  
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using cAlgo.API;
using cAlgo.API.Indicators;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace cAlgo.Robots;

/// <summary>
/// widownia06 - rebuild of widownia05a.
///
/// SECTIONS
///   1. Enums and parameters
///   2. Lifecycle (OnStart / OnTimer / OnBar / OnStop)
///   3. Model autodiscovery, flat "*.*" catalog, tree view
///   4. Model runtimes (ONNX in-process, LightGBM text in-process, sidecar bridge for joblib/keras/h5/saved_model/torch)
///   5. Market kernels (Ichimoku, FVG, BOS/CHoCH, Donchian, Keltner, Elder, PSAR, polynomial regression)
///   6. Component fusion (Ensemble / Chronos / Hermes / Adaptive hedge weights, macro group mapping)
///   7. Gate, sizing, execution
///   7b. Position trajectory, rescue, positive finalization
///   8. Rendering
///   9. Persistence (replay, checkpoints, snapshots)
///  10. Data types
///
/// Rescue policy:
///   * Ordinary strategy-driven finalization is positive-only when PositiveFinalizeOnly is enabled.
///   * Strong candle, breakout, pullback, structural reconstruction and a signal reversal can finalize winners.
///   * Losing positions can be resized upward on supportive trajectory or reduced on hostile trajectory.
///   * Adds are bounded by RescueMaxAdds and RescueMaxSizeMultiple.
///   * RescueDisasterPips remains the emergency exception. A trading strategy cannot guarantee all positions
///     close profitably because gaps, stop-outs, slippage, broker rejection and margin events can still realize loss.
/// </summary>
[Robot(AccessRights = AccessRights.FullAccess, AddIndicators = true)]
public class widownia06 : Robot
{
    #region 1. Enums and parameters

    public enum ScalingMode { Linear, Log10 }
    public enum StrategyMode { Ensemble, Chronos, Hermes, Adaptive }
    public enum ConfidenceGateMode { Strict, SoftBypass }
    public enum OrderSizingMode { FixedUnits, EquityRiskPercent, VolatilityAdjusted }
    public enum MacroMappingMode { UnifiedBaseline, ChronosTemporal, DeepHermesFusion, ReinforcementAdaptive }
    public enum PreferredRuntime { Auto, Onnx, Keras, H5, SavedModel, Safetensors, Joblib, TreeText, TorchPolicy }

    private enum AssetRole
    {
        SklearnModel, OnnxModel, KerasModel, H5Weights, SavedModelGraph, SavedModelVariables,
        SafetensorsShard, PoolShard, TreeText, TreeBinary, PolicyTorch, Dataset, Report
    }

    private enum Grp { Temporal, Reversion, Structure, Model }

    // ---- General
    [Parameter("Setup Name", Group = "General", DefaultValue = "widownia-runtime")]
    public string SetupName { get; set; }

    [Parameter("Enable Trading", Group = "General", DefaultValue = true)]
    public bool EnableTrading { get; set; }

    [Parameter("Models Root Path", Group = "General", DefaultValue = "models/itam_bundle/")]
    public string ModelsRootPath { get; set; }

    [Parameter("Preferred Runtime", Group = "General", DefaultValue = PreferredRuntime.Auto)]
    public PreferredRuntime RuntimePreference { get; set; }

    [Parameter("Rediscover Sec", Group = "General", DefaultValue = 30, MinValue = 5)]
    public int RediscoverSeconds { get; set; }

    [Parameter("Compute SHA256", Group = "General", DefaultValue = true)]
    public bool ComputeSha256 { get; set; }

    [Parameter("Joblib Priority", Group = "General", DefaultValue = 2.0, MinValue = 1.0, MaxValue = 5.0, Step = 0.1)]
    public double JoblibPriority { get; set; }

    // ---- Strategy
    [Parameter("Strategy", Group = "Strategy", DefaultValue = StrategyMode.Ensemble)]
    public StrategyMode Strategy { get; set; }

    [Parameter("Macro Mapping", Group = "Strategy", DefaultValue = MacroMappingMode.UnifiedBaseline)]
    public MacroMappingMode MacroMapping { get; set; }

    [Parameter("Adaptive Hedge Learning", Group = "Strategy", DefaultValue = true)]
    public bool AdaptiveReinforcement { get; set; }

    [Parameter("Hedge Learning Rate", Group = "Strategy", DefaultValue = 0.15, MinValue = 0.01, MaxValue = 1.0, Step = 0.01)]
    public double AdaptiveLearningRate { get; set; }

    [Parameter("Fast MA", Group = "Strategy", DefaultValue = 14, MinValue = 2)]
    public int FastMaPeriod { get; set; }

