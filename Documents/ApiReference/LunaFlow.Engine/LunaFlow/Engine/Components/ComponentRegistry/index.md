#### [LunaFlow\.Engine](../../../../index.md 'index')
### [LunaFlow\.Engine\.Components](../index.md 'LunaFlow\.Engine\.Components')

## ComponentRegistry Class

Registry and validator for ROM/Rulebook components\. Enforces that all declared
capabilities are unique and that all requirements can be satisfied\.

```csharp
public class ComponentRegistry
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; ComponentRegistry

| Methods | |
| :--- | :--- |
| [Register\(IHasDependencies, EngineTier, string\)](Register(IHasDependencies,EngineTier,string).md 'LunaFlow\.Engine\.Components\.ComponentRegistry\.Register\(LunaFlow\.Engine\.Components\.IHasDependencies, LunaFlow\.Engine\.Components\.EngineTier, string\)') | Registers a component \(ROM or Rulebook\) with its tier and an identifying name\. A component's registered name is its identity within this registry and must be unique\. |
| [Validate\(\)](Validate().md 'LunaFlow\.Engine\.Components\.ComponentRegistry\.Validate\(\)') | Validates the complete component graph: no duplicate capability providers, all requirements satisfied, tier\-ordering constraints met, and no circular dependencies\. |
| [ValidateDependsOn\(string, IReadOnlyList&lt;string&gt;\)](ValidateDependsOn(string,IReadOnlyList_string_).md 'LunaFlow\.Engine\.Components\.ComponentRegistry\.ValidateDependsOn\(string, System\.Collections\.Generic\.IReadOnlyList\<string\>\)') | Confirms every name in [dependsOnRoms](ValidateDependsOn(string,IReadOnlyList_string_).md#LunaFlow.Engine.Components.ComponentRegistry.ValidateDependsOn(string,System.Collections.Generic.IReadOnlyList_string_).dependsOnRoms 'LunaFlow\.Engine\.Components\.ComponentRegistry\.ValidateDependsOn\(string, System\.Collections\.Generic\.IReadOnlyList\<string\>\)\.dependsOnRoms') matches a component already registered in this registry — the enforcement `IRuleBook.DependsOnRoms` has been missing since it was declared \(ticket 0047\)\. Kept separate from [Validate\(\)](Validate().md 'LunaFlow\.Engine\.Components\.ComponentRegistry\.Validate\(\)') rather than folded into it: [IHasDependencies](../IHasDependencies/index.md 'LunaFlow\.Engine\.Components\.IHasDependencies') stays ROM\-agnostic \(Provides/Requires only\), so the registry has no structural way to know which of its registered components carry a named\-dependency list like a Rulebook's — the caller \(the composition root, which can see both tiers\) supplies it explicitly instead\. |
