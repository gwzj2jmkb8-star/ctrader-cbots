# cTrader cBots

A professional collection of cTrader trading systems, Pine Script strategies, and supporting business/runtime components. The repository is organized around three primary concerns:

- business logic and operational units
- cTrader runtime and C# implementations
- Pine Script indicators and trading strategies

## Repository layout

```text
src/
├── business/
│   ├── EtherPoisonXIV/
│   ├── AlgoTrader/
│   ├── SupportingScripts/
│   └── legacy/
├── ctrader/
│   ├── csharp/
│   ├── indicators/
│   └── configs/
└── pinescript/
    ├── indicators/
    ├── strategies/
    └── libraries/
```

## Included systems

- EtherPoisonXIVBusiness: structured operational unit with release gates, documentation, and infrastructure definitions
- AlgoTrader: MRSB and DATR channel-based trading logic and supporting documentation
- Pine Script strategy library: multiple experimental and production-oriented strategy scripts
- Supporting scripts: utility and evaluation tooling

## Standards

This repository emphasizes:

- clear domain separation
- reproducible build and validation steps
- operational evidence and release discipline
- maintainable strategy documentation
- clean project boundaries for strategy, runtime, and business logic

## Quick orientation

- `src/business/` contains the operational business units
- `src/ctrader/` contains cTrader runtime artifacts and platform wrappers
- `src/pinescript/` contains Pine Script indicators and strategies
- `docs/` contains project-wide governance and architecture notes
- `infra/` contains deployment and environment infrastructure assets

## Notes

This repo contains both research-grade Pine Script work and operational cBot business components. Strategy performance and operational readiness should be validated before production deployment.
