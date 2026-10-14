#### [LunaFlow\.Rulebook](../../../index.md 'index')
### [LunaFlow\.Rulebook](../index.md 'LunaFlow\.Rulebook')

## IRuleBook Interface

Top\-level organizational unit of a specific, compiled game built on one or more ROMs\.
A Rulebook declares which ROM\(s\) it depends on and implements the capability\-mediation
contract for its dependencies, but never references `LunaFlow.Engine` primitives directly
\(the infrastructure\-type participation in `IHasDependencies` is exempt; that's how the
tier's own identity integrates with the engine's module system\)\.

```csharp
public interface IRuleBook : LunaFlow.Rules.IHasLunaFlowReference, LunaFlow.Engine.Components.IHasDependencies
```

Derived  
&#8627; [OneD20RuleBook](../OneD20/OneD20RuleBook/index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20RuleBook')

Implements [LunaFlow\.Rules\.IHasLunaFlowReference](https://learn.microsoft.com/en-us/dotnet/api/lunaflow.rules.ihaslunaflowreference 'LunaFlow\.Rules\.IHasLunaFlowReference'), [LunaFlow\.Engine\.Components\.IHasDependencies](https://learn.microsoft.com/en-us/dotnet/api/lunaflow.engine.components.ihasdependencies 'LunaFlow\.Engine\.Components\.IHasDependencies')

### Remarks
Smallest shape that distinguishes Rulebook\-tier content from ROM\-tier content\.
[DependsOnRoms](DependsOnRoms.md 'LunaFlow\.Rulebook\.IRuleBook\.DependsOnRoms') names the ROM\(s\) this Rulebook depends on; that dependency
is static data only, not ordered or arbitrated at runtime — those concerns stay deferred
per `non-goals.md` until a second real ROM composition needs them \(Phase 2\.7\)\.

| Properties | |
| :--- | :--- |
| [DependsOnRoms](DependsOnRoms.md 'LunaFlow\.Rulebook\.IRuleBook\.DependsOnRoms') | The name\(s\) of the ROM\(s\) this Rulebook depends on\. |
