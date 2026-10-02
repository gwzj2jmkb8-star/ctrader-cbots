# Architecture

This repository is organized by responsibility, not by file type alone. The design separates:

- business domains and operational units
- cTrader runtime and C# implementations
- Pine Script strategy and indicator code
- supporting infrastructure and automation

## Core principles

1. Strategy code remains traceable to its source and purpose.
2. Business logic and trading execution boundaries are explicit.
3. Operational evidence is treated as part of the deliverable.
4. Documentation lives with the domain it describes.

## Layout

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

## Recommended boundaries

### `src/business/`
Contains operational business units, release logic, and domain-specific runtime packages. These assets are often the most reviewable and evidence-driven parts of the project.

### `src/ctrader/`
Contains cTrader-specific runtime files, helper classes, and platform configuration.

### `src/pinescript/`
Contains Pine Script strategies and indicators, grouped by role and usage.

### `legacy/`
Experimental or archive-grade code remains available for traceability but is isolated from active execution paths.

## Governance

- Keep business logic separated from indicator research code.
- Keep platform runtime code separate from Pine Script prototypes.
- Maintain a clear upgrade path from prototype to validated deployment.
- Treat docs and release evidence as required deliverables, not optional notes.
