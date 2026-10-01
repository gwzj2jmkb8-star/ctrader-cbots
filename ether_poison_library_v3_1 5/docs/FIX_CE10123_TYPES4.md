# CE10123: Nocturne receives Toxicity from EP_Types/4

The reported error identifies the actual argument as a Toxicity enum from
ClassicPotatoChamberXII/EP_Types/4. The previously supplied Nocturne source imported
EP_Types/3. Different definitions of the enum are incompatible even if their
member names match. The expected type's `not imported` wording does not mean
that changing a local alias or casting to int repairs the library contract.

The fix in every type-consuming local source is:

```pine
import ClassicPotatoChamberXII/EP_Types/4 as epTypes
```

Keep the typed export signature and the existing call unchanged:

```pine
export f_render_nocturne(float spanA, float spanB, float rapidA, float rapidB, epTypes.Toxicity tox, color lethal, color viral, color dormant, color stable, color shadow, color ghost) =>
    // Existing implementation remains unchanged.
```

Apply this in TradingView:

1. Open EP_Nocturne's source and set its EP_Types import to /4. Save and publish
   a new version of EP_Nocturne. Keep the typed functions, including
   f_render_cockpit, intact.
2. In the master, replace the EP_Nocturne import suffix with the actual newly
   published version. The next Nocturne version is not known here; do not assume
   it is /3 or /4. EP_Types/4 and EP_Nocturne's version are independent numbers.
3. Use EP_Types/4 in the master and all libraries that exchange its types:
   KineticCore, SpatialGates, RescueEngine, Engines and Nocturne. Libraries
   already published against /4 need no change solely for this error.
4. If any other library still imports /3, update and republish it, then update
   its consumers to the newly assigned version. Engines depends on KineticCore
   and RescueEngine, so update Engines after those dependencies if needed.

Existing published library versions retain their original dependencies. Editing
only the master or a local library source cannot change a previously published
Nocturne version. No need to republish EP_Types itself merely for this fix.

The local imports.json now records Types/4 based on the compiler diagnostic.
All other publication numbers remain the previous local contract, not a verified
remote release manifest. Record actual newly published versions in imports.json,
then run tools/configure_imports.py, build_preview.sh and audit.sh. The source
snapshot file EP_Types_v3.pine has not been renamed or modified: its filename
records the local source revision, not a verified download of published /4.

This repair changes import bindings and dependency checks only. No rendering
formula, enum value, signal, order, quantity, or trading default was changed.
Local checks pass; TradingView compilation remains to be confirmed.

Official references:
- https://www.tradingview.com/pine-script-docs/language/enums/
- https://www.tradingview.com/pine-script-docs/concepts/libraries/
