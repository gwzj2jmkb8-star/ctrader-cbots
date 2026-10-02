# Trading strategy overview

This repository combines several strategy families and ledger-style operational workflows. The codebase includes both research-oriented Pine Script prototypes and more structured business-facing execution units.

## Strategy domains

### MRSB / DATR
The AlgoTrader components center around Multi-Range Support Breakout and Dynamic ATR-based logic. These are designed to blend regime detection with breakout or channel-based execution signals.

### XIV / operational strategy
The EtherPoisonXIV-related assets emphasize stronger operational discipline: evidence recording, reproducible build validation, structured docs, and deployment controls.

### Pine Script research library
The Pine Script set contains indicators and strategies used for signal generation, regime interpretation, and experimental trade logic.

## Operational discipline

Strategy code should be reviewed in the context of:

- timeframe and symbol assumptions
- execution environment
- market regime applicability
- evidence of prior backtesting or validation
- clarity of operational ownership

## Recommended workflow

1. prototype in Pine Script or a relevant sandbox
2. document the design assumptions
3. validate against the target market and setup
4. move validated logic into a controlled runtime or business package
5. retain historical experiments in the archive layer
