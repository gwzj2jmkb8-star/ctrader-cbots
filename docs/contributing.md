# Contributing

## Scope

This repository is used for research, strategy development, and selected operational cTrader execution workflows. Contributions should remain organized and traceable.

## Folder conventions

- Place business and operations assets under `src/business/`
- Place cTrader runtime code under `src/ctrader/`
- Place Pine Script strategy code under `src/pinescript/`
- Keep archive or historical experiments under `src/business/legacy/`

## Before committing

- ensure the file is in the correct domain folder
- keep naming consistent and descriptive
- preserve the original strategy intent when porting between Pine Script and C#
- add or update documentation when behavior changes materially

## Documentation expectations

Any strategy or runtime change with user-visible behavior should include:

- purpose and expected market regime
- inputs and configuration assumptions
- risk considerations
- validation notes or evidence references

## Review guidance

Review should focus on:

- strategy logic correctness
- operational safety
- reproducibility
- evidence quality
- maintainability and readability
