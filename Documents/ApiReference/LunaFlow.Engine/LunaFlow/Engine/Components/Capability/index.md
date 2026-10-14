#### [LunaFlow\.Engine](../../../../index.md 'index')
### [LunaFlow\.Engine\.Components](../index.md 'LunaFlow\.Engine\.Components')

## Capability Class

A named capability that a component can provide or require\.
The capability acts as a mediation point for peer components to depend on each other
without direct assembly references\.

```csharp
public sealed record Capability : System.IEquatable<LunaFlow.Engine.Components.Capability>
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; Capability

Implements [System\.IEquatable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.iequatable-1 'System\.IEquatable\`1')[Capability](index.md 'LunaFlow\.Engine\.Components\.Capability')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.iequatable-1 'System\.IEquatable\`1')

| Constructors | |
| :--- | :--- |
| [Capability\(string, Type\)](Capability(string,Type).md 'LunaFlow\.Engine\.Components\.Capability\.Capability\(string, System\.Type\)') | Creates a capability with the given name and contract type\. Used for Requires\-side             entries — a requirement doesn't own an implementation, so there's nothing to pair\. |

| Properties | |
| :--- | :--- |
| [ContractType](ContractType.md 'LunaFlow\.Engine\.Components\.Capability\.ContractType') | The contract type this capability represents\. |
| [Name](Name.md 'LunaFlow\.Engine\.Components\.Capability\.Name') | The unique name of this capability\. |
| [ProviderType](ProviderType.md 'LunaFlow\.Engine\.Components\.Capability\.ProviderType') | The concrete type implementing [ContractType](ContractType.md 'LunaFlow\.Engine\.Components\.Capability\.ContractType'), set only on             Provides\-side entries via [Provided&lt;TContract,TProvider&gt;\(string\)](Provided_TContract,TProvider_(string).md 'LunaFlow\.Engine\.Components\.Capability\.Provided\<TContract,TProvider\>\(string\)')\. Null for             Requires\-side entries\. |

| Methods | |
| :--- | :--- |
| [ForceMemberResolution\(\)](ForceMemberResolution().md 'LunaFlow\.Engine\.Components\.Capability\.ForceMemberResolution\(\)') | Forces JIT resolution of every method in [ProviderType](ProviderType.md 'LunaFlow\.Engine\.Components\.Capability\.ProviderType')'s implementation of [ContractType](ContractType.md 'LunaFlow\.Engine\.Components\.Capability\.ContractType'), without invoking any of them — catching a member\-signature change in the installed contract assembly that a compile\-time check against this ROM's own build can't see \(cross\-rom\-capability\-spec\.md §5\.3\)\. No\-op for Requires\-side capabilities \([ProviderType](ProviderType.md 'LunaFlow\.Engine\.Components\.Capability\.ProviderType') null — nothing to force\)\. |
| [Provided&lt;TContract,TProvider&gt;\(string\)](Provided_TContract,TProvider_(string).md 'LunaFlow\.Engine\.Components\.Capability\.Provided\<TContract,TProvider\>\(string\)') | Declares a Provides\-side capability\. `TProvider : TContract` makes a mismatched pairing a compile error, per ticket 0048's ruling — this is what replaces a runtime assignability check: a ROM can't ship a capability paired with a type that doesn't implement its contract\. |
