#### [LunaFlow\.Rulebook](../../index.md 'index')

## LunaFlow\.Rulebook Namespace

| Interfaces | |
| :--- | :--- |
| [IRuleBook](IRuleBook/index.md 'LunaFlow\.Rulebook\.IRuleBook') | Top\-level organizational unit of a specific, compiled game built on one or more ROMs\. A Rulebook declares which ROM\(s\) it depends on and implements the capability\-mediation contract for its dependencies, but never references `LunaFlow.Engine` primitives directly \(the infrastructure\-type participation in `IHasDependencies` is exempt; that's how the tier's own identity integrates with the engine's module system\)\. |
