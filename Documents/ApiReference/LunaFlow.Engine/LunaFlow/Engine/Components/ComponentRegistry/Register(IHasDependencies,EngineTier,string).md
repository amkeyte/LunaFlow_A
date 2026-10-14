#### [LunaFlow\.Engine](../../../../index.md 'index')
### [LunaFlow\.Engine\.Components](../index.md 'LunaFlow\.Engine\.Components').[ComponentRegistry](index.md 'LunaFlow\.Engine\.Components\.ComponentRegistry')

## ComponentRegistry\.Register\(IHasDependencies, EngineTier, string\) Method

Registers a component \(ROM or Rulebook\) with its tier and an identifying name\.
A component's registered name is its identity within this registry and must be unique\.

```csharp
public void Register(LunaFlow.Engine.Components.IHasDependencies component, LunaFlow.Engine.Components.EngineTier tier, string name);
```
#### Parameters

<a name='LunaFlow.Engine.Components.ComponentRegistry.Register(LunaFlow.Engine.Components.IHasDependencies,LunaFlow.Engine.Components.EngineTier,string).component'></a>

`component` [IHasDependencies](../IHasDependencies/index.md 'LunaFlow\.Engine\.Components\.IHasDependencies')

<a name='LunaFlow.Engine.Components.ComponentRegistry.Register(LunaFlow.Engine.Components.IHasDependencies,LunaFlow.Engine.Components.EngineTier,string).tier'></a>

`tier` [EngineTier](../EngineTier/index.md 'LunaFlow\.Engine\.Components\.EngineTier')

<a name='LunaFlow.Engine.Components.ComponentRegistry.Register(LunaFlow.Engine.Components.IHasDependencies,LunaFlow.Engine.Components.EngineTier,string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

#### Exceptions

[System\.ArgumentNullException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentnullexception 'System\.ArgumentNullException')  
If component is null\.

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
If name is null/empty/whitespace, or if a component with this name is already registered\.