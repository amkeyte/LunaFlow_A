#### [LunaFlow\.Engine](../../../../index.md 'index')
### [LunaFlow\.Engine\.Components](../index.md 'LunaFlow\.Engine\.Components').[Capability](index.md 'LunaFlow\.Engine\.Components\.Capability')

## Capability\.Provided\<TContract,TProvider\>\(string\) Method

Declares a Provides\-side capability\. `TProvider : TContract` makes a mismatched
pairing a compile error, per ticket 0048's ruling — this is what replaces a runtime
assignability check: a ROM can't ship a capability paired with a type that doesn't
implement its contract\.

```csharp
public static LunaFlow.Engine.Components.Capability Provided<TContract,TProvider>(string name)
    where TProvider : TContract;
```
#### Type parameters

<a name='LunaFlow.Engine.Components.Capability.Provided_TContract,TProvider_(string).TContract'></a>

`TContract`

<a name='LunaFlow.Engine.Components.Capability.Provided_TContract,TProvider_(string).TProvider'></a>

`TProvider`
#### Parameters

<a name='LunaFlow.Engine.Components.Capability.Provided_TContract,TProvider_(string).name'></a>

`name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

#### Returns
[Capability](index.md 'LunaFlow\.Engine\.Components\.Capability')