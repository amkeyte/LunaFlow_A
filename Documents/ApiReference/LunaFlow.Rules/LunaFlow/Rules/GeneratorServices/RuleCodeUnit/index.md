#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices')

## RuleCodeUnit Class

Represents the output of a code generation step: a C\# file containing generated rule classes\.

```csharp
public sealed class RuleCodeUnit
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; RuleCodeUnit

| Properties | |
| :--- | :--- |
| [Classes](Classes.md 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit\.Classes') | Gets the list of rule classes to include in the file\. |
| [FileName](FileName.md 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit\.FileName') | Gets or sets the name of the file to emit\. |
| [Namespace](Namespace.md 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit\.Namespace') | Gets or sets the target namespace of the emitted code\. This default is only a fallback for constructing a [RuleCodeUnit](index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit') directly \(e\.g\. in a test\); [IRuleIRGenerator](../IRuleIRGenerator/index.md 'LunaFlow\.Rules\.GeneratorServices\.IRuleIRGenerator') implementations must always set this explicitly to `$"LunaFlow.Generated.{group.GroupName}"` — see the "D4 — generated namespace convention" decision in rule\-pipeline\.md\. |
| [RegistryMethodBody](RegistryMethodBody.md 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit\.RegistryMethodBody') | Gets or sets the registry method body for exposing all rules\. This is injected into a static group class for use at runtime\. |
| [RequiredUsings](RequiredUsings.md 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit\.RequiredUsings') | Gets or sets the usings the emitted file needs\. [IRuleIRGenerator](../IRuleIRGenerator/index.md 'LunaFlow\.Rules\.GeneratorServices\.IRuleIRGenerator') implementations must populate this as the union/dedup of every [RuleIR](../RuleIR/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIR')'s [RequiredUsings](../RuleIR/RequiredUsings.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIR\.RequiredUsings') in the group, plus whatever the generated shape itself always needs \(e\.g\. `LunaFlow.Rules`, `NRules.Fluent.Dsl`\) — this is where that union lands; the DTO had no slot for it until this field was added alongside the Phase 2 implementation \(ticket 0008\), the same kind of gap D1 closed on `RuleIR` itself\. |
