# cTrader runtime

This area contains the platform-specific runtime and implementation assets for cTrader-based systems.

## Purpose

The `src/ctrader/` namespace separates the execution engine from the strategy research code so that the runtime can be documented and reviewed independently.

## Recommended structure

```text
src/ctrader/
├── README.md
├── csharp/
├── indicators/
├── configs/
└── instance-settings/
```
