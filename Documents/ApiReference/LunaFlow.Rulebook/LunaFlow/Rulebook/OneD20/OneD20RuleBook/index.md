#### [LunaFlow\.Rulebook](../../../../index.md 'index')
### [LunaFlow\.Rulebook\.OneD20](../index.md 'LunaFlow\.Rulebook\.OneD20')

## OneD20RuleBook Class

The 1\.d20 Rulebook's [IRuleBook](../../IRuleBook/index.md 'LunaFlow\.Rulebook\.IRuleBook') declaration — depends on the `GridMap`,
`D20System`, `Loot`, and \(per phase\-2\-11\-capability\-stress\-test\-spec\.md item 7\)
`Reputation` ROMs \(spatial, challenge/combat, loot/inventory, and reputation\-tracking
concerns, respectively\)\. Carries no logic of its own: this project's job is to be pure data
\([IOneD20WorldModule](../IOneD20WorldModule/index.md 'LunaFlow\.Rulebook\.OneD20\.IOneD20WorldModule')'s implementations plus
[OneD20SessionAssembler](../OneD20SessionAssembler/index.md 'LunaFlow\.Rulebook\.OneD20\.OneD20SessionAssembler')\) plus this declaration, never a reference to
`LunaFlow.Engine`\.

```csharp
public sealed class OneD20RuleBook : LunaFlow.Rulebook.IRuleBook, LunaFlow.Rules.IHasLunaFlowReference, LunaFlow.Engine.Components.IHasDependencies
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; OneD20RuleBook

Implements [IRuleBook](../../IRuleBook/index.md 'LunaFlow\.Rulebook\.IRuleBook'), [LunaFlow\.Rules\.IHasLunaFlowReference](https://learn.microsoft.com/en-us/dotnet/api/lunaflow.rules.ihaslunaflowreference 'LunaFlow\.Rules\.IHasLunaFlowReference'), [LunaFlow\.Engine\.Components\.IHasDependencies](https://learn.microsoft.com/en-us/dotnet/api/lunaflow.engine.components.ihasdependencies 'LunaFlow\.Engine\.Components\.IHasDependencies')

| Properties | |
| :--- | :--- |
| [DependsOnRoms](DependsOnRoms.md 'LunaFlow\.Rulebook\.OneD20\.OneD20RuleBook\.DependsOnRoms') | The name\(s\) of the ROM\(s\) this Rulebook depends on\. |
| [Provides](Provides.md 'LunaFlow\.Rulebook\.OneD20\.OneD20RuleBook\.Provides') | Capabilities this component advertises for other components to depend on\. |
| [Reference](Reference.md 'LunaFlow\.Rulebook\.OneD20\.OneD20RuleBook\.Reference') | This object's reference\. |
| [Requires](Requires.md 'LunaFlow\.Rulebook\.OneD20\.OneD20RuleBook\.Requires') | Capabilities this component needs from other components\. |
