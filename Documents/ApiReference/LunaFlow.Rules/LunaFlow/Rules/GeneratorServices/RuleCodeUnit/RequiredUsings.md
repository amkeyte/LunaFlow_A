#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices').[RuleCodeUnit](index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit')

## RuleCodeUnit\.RequiredUsings Property

Gets or sets the usings the emitted file needs\. [IRuleIRGenerator](../IRuleIRGenerator/index.md 'LunaFlow\.Rules\.GeneratorServices\.IRuleIRGenerator')
implementations must populate this as the union/dedup of every [RuleIR](../RuleIR/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIR')'s
[RequiredUsings](../RuleIR/RequiredUsings.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIR\.RequiredUsings') in the group, plus whatever the generated shape
itself always needs \(e\.g\. `LunaFlow.Rules`, `NRules.Fluent.Dsl`\) — this is
where that union lands; the DTO had no slot for it until this field was added
alongside the Phase 2 implementation \(ticket 0008\), the same kind of gap D1 closed on
`RuleIR` itself\.

```csharp
public System.Collections.Generic.List<string> RequiredUsings { get; set; }
```

#### Property Value
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')