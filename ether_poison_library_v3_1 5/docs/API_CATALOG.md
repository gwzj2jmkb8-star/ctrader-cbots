# Exported API catalog

Generated from actual library files. See REUSE_MAP.md for historical adaptations.

## EP_Candles_v2

```pine
export heikinAshi(float o, float h, float l, float c) => ...

export relativeVolume(float v, simple int length) => ...

export buyShare(float h, float l, float c) => ...

export bodyFraction(float o, float h, float l, float c) => ...

export anatomy(float o, float h, float l, float c, float tick, float bodyThreshold, float wickToleranceTicks) => ...

export volumeWidth(float relativeVolume, float widthAtOne, float exponent) => ...

export type VolumeCanvas
    array<box> upperBodies
    array<box> lowerBodies
    array<line> wicks
    int lastStamp = na

export createCanvas() => ...

export drawVolume(
    VolumeCanvas canvas,
    int stamp,
    int duration,
    float o,
    float h,
    float l,
    float c,
    float widthFraction,
    float buyFraction,
    bool splitProxy,
    color directionInk,
    color buyInk,
    color sellInk,
    int alpha,
    int capacity
) => ...
```

## EP_CloudScreen_v1

```pine
export type Cloud
    bool exists = false
    bool bullish = true
    int born = na
    float top = na
    float bottom = na
    float remainingTop = na
    float remainingBottom = na
    float birthAtr = na
    float birthRvol = na
    bool structureAtBirth = false
    int touchBars = 0
    int lastTouchBar = na
    int lastObservedBar = na
    bool inverted = false
    bool expired = false

export create() => ...

export observe(
    Cloud cloud,
    bool bullish,
    int zoneBar,
    float top,
    float bottom,
    float h,
    float l,
    bool inverted,
    bool ageValid,
    int bar,
    float atr,
    float rvol,
    bool structureAligned
) => ...

export remaining(Cloud cloud) => ...

export distanceAtr(Cloud cloud, float price, float atr) => ...

export widthAtr(Cloud cloud) => ...

export stateName(Cloud cloud) => ...

export rejection(
    Cloud cloud,
    float price,
    float atr,
    float minimumRemaining,
    float minimumWidth,
    float minimumRvol,
    float maximumDistance,
    bool requireStructure,
    bool allowInverted
) => ...

export opacity(Cloud cloud, int strongest, int faintest) => ...
```

## EP_Engines_v3

```pine
export midpoint(float highValue, float lowValue) => ...

export kineticMagnitude(float source, float sourcePast, float atr, float scale) => ...

export zScore(float source, simple int length) => ...

export normalizeSigned(float value, float scale) => ...

export rescueQuantity(float currentQuantity, float currentAverage, float rescuePrice, float targetAverage, bool isLong, float maximumMultiple) => ...

export multiplexerCloud(
    float source,
    float atr,
    simple int fastLength,
    simple int mediumLength,
    simple int slowLength,
    float temperature,
    float attenuationFloor,
    float contourAtr) => ...
```

## EP_ExecutionLedger_v1

```pine
export type EntryFill
    string id
    int stamp
    int entryBar
    float price
    float quantity

export type ClosedFill
    EntryFill entry
    int index
    string exitId
    int exitStamp
    float exitPrice
    float profit
    float profitPercent

export type Ledger
    array<label> labels
    array<string> activeKeys
    int closedCursor = 0

export create() => ...

export tradeKey(EntryFill entry) => ...

export addLabel(Ledger state, int stamp, float price, string caption, string details, bool isBuy, color ink, int capacity) => ...

export observe(Ledger state, array<EntryFill> openFills, array<ClosedFill> closedFills, int closedCount, bool visible, int capacity, color buyColor, color sellColor, color profitColor, color lossColor, color flatColor, float positionAverage, float positionQuantity) => ...
```

## EP_KineticCore_v1

```pine
export magnitude(float source, float sourcePast, float atr, float scale) => ...

export f_map_toxicity(float mag, float thresholdLethal, float thresholdViral) => ...

export f_determine_flow(float src, simple int len) => ...

export f_calculate_magnitude(float src, simple int len, float scale, float tickSize) => ...

export state(float magnitudeValue, epTypes.EnergyFlow flow, float lethal, float viral) => ...

export f_get_kinetic_state(float src, float scale, float thL, float thV) => ...

export legacyMagnitude(float source, float sourcePast, float atr, float scale) => ...

export finalKineticState(float src, float scale, float lethal, float viral) => ...
```

## EP_LimitOrders_v1

```pine
export type Ticket
    varip bool active = false
    varip string id = ""
    varip string kind = ""
    varip bool isLong = true
    varip float price = na
    varip float quantity = na
    varip float positionAtArm = 0.0
    varip int zoneAtArm = na
    varip int bornBar = na
    varip int expiryBar = na
    varip int serial = 0
    varip int eventSerial = 0
    varip int eventBar = na
    varip int eventTime = na
    varip string event = "IDLE"
    varip string reason = ""
    varip float eventPrice = na

export create() => ...

export entryPrice(bool isLong, string basis, float closePrice, float atr, float offsetAtr, int offsetTicks, float zoneTop, float zoneBottom, float tick) => ...

export isPassive(bool isLong, float price, float currentPrice, float tick) => ...

export cancellationReason(Ticket ticket, int bar, float positionSize, bool oppositeApproval, bool cancelOpposite, bool zoneValid, bool cancelZone, bool harvesting) => ...

export canArm(Ticket ticket, int bar, bool confirmed, bool fillPass) => ...

export arm(Ticket ticket, bool isLong, string kind, float price, float quantity, float positionSize, int zoneBar, int bar, int stamp, int lifetime) => ...

export finish(Ticket ticket, string status, string reason, float price, int bar, int stamp) => ...
```

## EP_MarketOrders_v1

```pine
export entryEligible(bool approved, bool isLong, float positionSize) => ...

export rescueEligible(bool window, float quantity) => ...
```

## EP_Math_v3

```pine
export clamp(float value, float low, float high) => ...

export safeDiv(float numerator, float denominator, float epsilon = 1e-6) => ...

export norm(float value, float scale) => ...

export robust(float value, float center, float spread, float clampN) => ...

export energyFlux(float value, float center, float spread, float clampN) => ...

export kinetic(float value, float scale) => ...

export signedKinetic(float value, float scale) => ...

export weightedMean(float numerator, float denominator) => ...

export blend2(float a, float weightA, float b, float weightB) => ...

export blend3(float a, float weightA, float b, float weightB, float c, float weightC) => ...

export rangeLocation(float value, float low, float high) => ...

export distanceAtr(float value, float reference, float atr) => ...

export qtyFromEquityPct(float equity, float percent, float price, float pointValue) => ...

export binaryEntropy(float probability) => ...

export tempAtten(float entropy, float temperature, float floorValue) => ...

export agreement(float a, float b, float c) => ...

export confidence(float direction, float agreementValue, float inverseEntropy) => ...

export legacySafeDiv(float numerator, float denominator, float epsilon = 1e-6) => ...

export legacyKineticSaturation(float value, float scale) => ...
```

## EP_Nocturne_v2

```pine
export clampAlpha(float value) => ...

export densityAlpha(float confidence, float entropy, float tempAtten, int lightAlpha, int darkAlpha) => ...

export directionalColor(color bull, color bear, color neutral, float direction, float deadband, int alpha) => ...

export spectralColor(color base, float density, int faintAlpha, int strongAlpha) => ...

export stateGlyph(float direction, float entropy, float confidence) => ...

export f_get_toxicity_color(epTypes.Toxicity tox, color lethal, color viral, color dormant, color stable) => ...

export f_render_nocturne(float spanA, float spanB, float rapidA, float rapidB, epTypes.Toxicity tox, color lethal, color viral, color dormant, color stable, color shadow, color ghost) => ...

export f_render_cockpit(table cockpit, epTypes.KineticState state, epTypes.Permission perm, color textColor, color bullColor, color bearColor, color dormant, color stable) => ...
```

## EP_RescueEngine_v1

```pine
export f_calc_rescue_qty(float current_qty, float current_avg, float rescue_price, float target_avg, bool isLong) => ...

export quantity(float currentQuantity, float currentAverage, float rescuePrice, float targetAverage, bool isLong, float maximumMultiple) => ...

export adverseDistance(float entryPrice, float price, bool isLong) => ...

export f_check_rescue_window(float entryPrice, bool isLong, float minimumDistance) => ...

export f_execute_rescue(epTypes.RescueState state, float entryPrice, float currentQty, bool isLong, float minimumDistance, float targetTicks, int maximumCycles) => ...

export f_check_harvest(float entryPrice, float currentQty, bool isLong, float kineticMagnitude, float harvestPercent) => ...

export f_handle_rescue(float entry, float qty, bool isLong, float minimumDistance, float targetTicks, int maximumCycles, int cycles) => ...
```

## EP_SpaceTimeGrid_v1

```pine
export midpoint(float h, float l) => ...

export classical(float h, float l, simple int tenkanLength, simple int kijunLength, simple int spanBLength) => ...

export rapid(float source, float atr, float kinetic, simple int length, float expansion) => ...

export volatility(float source, float atr, float signedEnergy, simple int fastLength, simple int slowLength, float contourAtr) => ...

export reversion(float source, simple int length, float deviationMultiple) => ...

export wave(float source, simple int length) => ...

export legacyGrid(simple int spanALength, simple int spanBLength, float rapidPercent, simple int bollLength, float bollMultiple, simple int railLength) => ...

export historicalCloud(float spanA, float spanB, simple int displacement) => ...
```

## EP_SpatialGates_v1

```pine
export detectFvg(float h, float l, float highTwoBarsAgo, float lowTwoBarsAgo, float atr, float minimumAtr) => ...

export f_detect_fvg(float minimumAtr) => ...

export f_get_spatial_permission(bool bullFvg, bool bearFvg, bool bBull, bool bBear, bool cBull, bool cBear) => ...

export legacyStructure(simple int lookback, float chochAtr) => ...

export legacyPermission(float minimumAtr, simple int lookback, float chochAtr) => ...
```

## EP_Types_v3

```pine
export enum Direction
    LONG = "LONG"
    SHORT = "SHORT"
    WAIT = "WAIT"

export enum EnergyState
    EXPANSION = "EXPANSION"
    IMPULSE = "IMPULSE"
    BALANCED = "BALANCED"
    COMPRESSION = "COMPRESSION"

export enum GateState
    OPEN = "OPEN"
    PARTIAL = "PARTIAL"
    BLOCKED = "BLOCKED"

export enum FvgState
    NONE = "NONE"
    FRESH = "FRESH"
    PARTIAL = "PARTIAL"
    INVERTED = "INVERTED"
    FILLED = "FILLED"

export enum EntropyState
    ORDERED = "ORDERED"
    MIXED = "MIXED"
    DIFFUSE = "DIFFUSE"
    CHAOTIC = "CHAOTIC"

export enum CloudLane
    FAST = "FAST"
    BALANCED = "BALANCED"
    SLOW = "SLOW"

export enum Toxicity
    LETHAL = "LETHAL"
    VIRAL = "VIRAL"
    DORMANT = "DORMANT"
    STABLE = "STABLE"

export enum EnergyFlow
    ASCENDING = "ASCENDING"
    DESCENDING = "DESCENDING"
    VORTEX = "VORTEX"
    STATIC = "STATIC"

export enum Permission
    OFFENSIVE = "OFFENSIVE"
    DEFENSIVE = "DEFENSIVE"
    WAIT = "WAIT"

export type KineticState
    float magnitude = 0.0
    Toxicity toxicity = Toxicity.DORMANT
    EnergyFlow flow = EnergyFlow.STATIC
    float conviction = 0.0

export type OutcomeState
    Permission permission = Permission.WAIT
    float score = 0.0
    string reason = "NONE"
    string signature = "NONE"

export type EngineState
    Direction direction = Direction.WAIT
    EnergyState energy = EnergyState.BALANCED
    GateState gate = GateState.BLOCKED
    float longScore = 0.0
    float shortScore = 0.0
    float conviction = 0.0
    string reason = "WAIT"

export type CloudState
    float spanA = na
    float spanB = na
    float center = na
    float widthAtr = 0.0
    float direction = 0.0
    float entropyFast = 1.0
    float entropyMedium = 1.0
    float entropySlow = 1.0
    float entropyComposite = 1.0
    float tempAtten = 0.0
    float densityFast = 0.0
    float densityMedium = 0.0
    float densitySlow = 0.0
    float agreement = 0.0
    float confidence = 0.0
    EntropyState entropyState = EntropyState.CHAOTIC
    CloudLane dominantLane = CloudLane.BALANCED
    bool valid = false

export directionValue(Direction direction) => ...

export permissionDirection(Permission permission) => ...

export entropyName(EntropyState state) => ...

export laneName(CloudLane lane) => ...

export type RescueState
    int cycles = 0
    float lastRescuePrice = na
    bool isRecovering = false
```
