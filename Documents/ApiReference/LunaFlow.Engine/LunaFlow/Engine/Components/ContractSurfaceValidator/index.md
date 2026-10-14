#### [LunaFlow\.Engine](../../../../index.md 'index')
### [LunaFlow\.Engine\.Components](../index.md 'LunaFlow\.Engine\.Components')

## ContractSurfaceValidator Class

Bootstrap\-time shape check for a Cartridge's contract\-surface assembly, per
`cross-rom-capability-spec.md` §5\.4: `cartridge.md`'s Structure section states the
contract surface holds interfaces only — "the pinout, not the chip" — but nothing enforced it
until a bare concrete record \(`EquippedWeapon`\) shipped directly in
`LunaFlow.Cartridge` and was caught only in review \(ticket 0071\)\. This turns that manual
bar into an automated, bootstrap\-enforced one, same "hard fail before play starts, never a
silent degradation" discipline the existing compatibility probe \(§5\.1–§5\.3\) already holds, over
a different question: not whether a declared capability's pairing binds, but whether everything
exported from the contract surface is actually an interface\.

```csharp
public static class ContractSurfaceValidator
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; ContractSurfaceValidator

| Methods | |
| :--- | :--- |
| [ValidateInterfacesOnly\(Assembly\)](ValidateInterfacesOnly(Assembly).md 'LunaFlow\.Engine\.Components\.ContractSurfaceValidator\.ValidateInterfacesOnly\(System\.Reflection\.Assembly\)') | Throws [System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException'), naming every offending type, if [contractSurface](ValidateInterfacesOnly(Assembly).md#LunaFlow.Engine.Components.ContractSurfaceValidator.ValidateInterfacesOnly(System.Reflection.Assembly).contractSurface 'LunaFlow\.Engine\.Components\.ContractSurfaceValidator\.ValidateInterfacesOnly\(System\.Reflection\.Assembly\)\.contractSurface') publicly exports anything that isn't an interface\. Scope is deliberately narrow: one assembly, one pass, no allowlist — a real need for a non\-interface member in the contract surface is a deliberate, reviewed loosening of this check with its own ticket, not a default this method should anticipate\. |
