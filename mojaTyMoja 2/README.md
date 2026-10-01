# mojaTyMoja — all-in-one cTrader cBot

One cBot runs the XI strategy and draws its indicators, Ichimoku navigation cloud and volume-aware Heikin Ashi overlay. No separate indicator is installed or attached.

## Install

Open `artifacts/mojaTyMoja.algo` in cTrader desktop, then add a mojaTyMoja cBot instance to the intended symbol/timeframe. Configure a unique instance key. Starting the instance enables order execution and visuals together. Stop retains existing positions.

## Build from the root source

The main file is **mojaTyMoja.cs**, alongside **mojaTyMoja.csproj** at the root of this ZIP.

- Open the supplied project in your C# IDE and build with .NET 6 / cTrader.Automate 1.0.21; or
- Create a new cBot named `mojaTyMoja` in cTrader, replace its generated template entirely with `mojaTyMoja.cs`, and build.

Use a clean project or remove previous Robot/Indicator source files from the compile list. Do not append this source to an existing cBot template. Do not edit the NuGet package's `cTrader.Automate.targets` file.

## CT0003 correction

The assembly now contains one `[Robot]` class and no `[Indicator]` classes or Indicator subclasses. The internal `XiChartRenderer` is an ordinary C# helper owned by the cBot. The supplied project explicitly compiles only the root source, with no custom assembly references. Official Spotware metadata extraction and `.algo` bundling succeed.

## Validation and display scope

Compiled against the real cTrader API, packaged using Spotware's SDK tasks, and checked with 23 deterministic kernel fixtures. Desktop import, rendering and broker execution have not been run here. This is not verification of the claimed profitability or Pine/cTrader backtest parity.

Rendering covers recent bars with bounded chart objects, rather than an independently attached full-history indicator. See `docs/translation-contract.md` for exact formula, execution and display adaptations, and remaining comparison work. See `docs/parameter-map.json` for the configuration map. Tests can be run with `dotnet run --project tests/KernelTests.csproj` in a normal .NET environment.
