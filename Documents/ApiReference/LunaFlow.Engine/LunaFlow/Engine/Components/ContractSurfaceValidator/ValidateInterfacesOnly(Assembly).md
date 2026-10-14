#### [LunaFlow\.Engine](../../../../index.md 'index')
### [LunaFlow\.Engine\.Components](../index.md 'LunaFlow\.Engine\.Components').[ContractSurfaceValidator](index.md 'LunaFlow\.Engine\.Components\.ContractSurfaceValidator')

## ContractSurfaceValidator\.ValidateInterfacesOnly\(Assembly\) Method

Throws [System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException'), naming every offending type, if
[contractSurface](ValidateInterfacesOnly(Assembly).md#LunaFlow.Engine.Components.ContractSurfaceValidator.ValidateInterfacesOnly(System.Reflection.Assembly).contractSurface 'LunaFlow\.Engine\.Components\.ContractSurfaceValidator\.ValidateInterfacesOnly\(System\.Reflection\.Assembly\)\.contractSurface') publicly exports anything that isn't an interface\.
Scope is deliberately narrow: one assembly, one pass, no allowlist — a real need for a
non\-interface member in the contract surface is a deliberate, reviewed loosening of this
check with its own ticket, not a default this method should anticipate\.

```csharp
public static void ValidateInterfacesOnly(System.Reflection.Assembly contractSurface);
```
#### Parameters

<a name='LunaFlow.Engine.Components.ContractSurfaceValidator.ValidateInterfacesOnly(System.Reflection.Assembly).contractSurface'></a>

`contractSurface` [System\.Reflection\.Assembly](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly 'System\.Reflection\.Assembly')