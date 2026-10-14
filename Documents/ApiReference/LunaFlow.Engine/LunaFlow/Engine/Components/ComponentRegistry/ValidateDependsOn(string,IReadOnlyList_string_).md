#### [LunaFlow\.Engine](../../../../index.md 'index')
### [LunaFlow\.Engine\.Components](../index.md 'LunaFlow\.Engine\.Components').[ComponentRegistry](index.md 'LunaFlow\.Engine\.Components\.ComponentRegistry')

## ComponentRegistry\.ValidateDependsOn\(string, IReadOnlyList\<string\>\) Method

Confirms every name in [dependsOnRoms](ValidateDependsOn(string,IReadOnlyList_string_).md#LunaFlow.Engine.Components.ComponentRegistry.ValidateDependsOn(string,System.Collections.Generic.IReadOnlyList_string_).dependsOnRoms 'LunaFlow\.Engine\.Components\.ComponentRegistry\.ValidateDependsOn\(string, System\.Collections\.Generic\.IReadOnlyList\<string\>\)\.dependsOnRoms') matches a component already
registered in this registry — the enforcement `IRuleBook.DependsOnRoms` has been
missing since it was declared \(ticket 0047\)\. Kept separate from [Validate\(\)](Validate().md 'LunaFlow\.Engine\.Components\.ComponentRegistry\.Validate\(\)')
rather than folded into it: [IHasDependencies](../IHasDependencies/index.md 'LunaFlow\.Engine\.Components\.IHasDependencies') stays ROM\-agnostic
\(Provides/Requires only\), so the registry has no structural way to know which of its
registered components carry a named\-dependency list like a Rulebook's — the caller
\(the composition root, which can see both tiers\) supplies it explicitly instead\.

```csharp
public void ValidateDependsOn(string dependentName, System.Collections.Generic.IReadOnlyList<string> dependsOnRoms);
```
#### Parameters

<a name='LunaFlow.Engine.Components.ComponentRegistry.ValidateDependsOn(string,System.Collections.Generic.IReadOnlyList_string_).dependentName'></a>

`dependentName` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

<a name='LunaFlow.Engine.Components.ComponentRegistry.ValidateDependsOn(string,System.Collections.Generic.IReadOnlyList_string_).dependsOnRoms'></a>

`dependsOnRoms` [System\.Collections\.Generic\.IReadOnlyList&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1 'System\.Collections\.Generic\.IReadOnlyList\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlylist-1 'System\.Collections\.Generic\.IReadOnlyList\`1')

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
If [dependsOnRoms](ValidateDependsOn(string,IReadOnlyList_string_).md#LunaFlow.Engine.Components.ComponentRegistry.ValidateDependsOn(string,System.Collections.Generic.IReadOnlyList_string_).dependsOnRoms 'LunaFlow\.Engine\.Components\.ComponentRegistry\.ValidateDependsOn\(string, System\.Collections\.Generic\.IReadOnlyList\<string\>\)\.dependsOnRoms') names a component that isn't registered\.