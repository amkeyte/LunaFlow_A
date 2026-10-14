#### [LunaFlow\.Engine](../../../../index.md 'index')
### [LunaFlow\.Engine\.Components](../index.md 'LunaFlow\.Engine\.Components').[Capability](index.md 'LunaFlow\.Engine\.Components\.Capability')

## Capability\.ForceMemberResolution\(\) Method

Forces JIT resolution of every method in [ProviderType](ProviderType.md 'LunaFlow\.Engine\.Components\.Capability\.ProviderType')'s implementation of
[ContractType](ContractType.md 'LunaFlow\.Engine\.Components\.Capability\.ContractType'), without invoking any of them — catching a member\-signature
change in the installed contract assembly that a compile\-time check against this ROM's own
build can't see \(cross\-rom\-capability\-spec\.md §5\.3\)\. No\-op for Requires\-side capabilities
\([ProviderType](ProviderType.md 'LunaFlow\.Engine\.Components\.Capability\.ProviderType') null — nothing to force\)\.

```csharp
public void ForceMemberResolution();
```