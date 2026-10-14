#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices').[RuleCodeUnit](index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit')

## RuleCodeUnit\.Namespace Property

Gets or sets the target namespace of the emitted code\. This default is only a
fallback for constructing a [RuleCodeUnit](index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit') directly \(e\.g\. in a test\);
[IRuleIRGenerator](../IRuleIRGenerator/index.md 'LunaFlow\.Rules\.GeneratorServices\.IRuleIRGenerator') implementations must always set this explicitly to
`$"LunaFlow.Generated.{group.GroupName}"` — see the "D4 — generated namespace
convention" decision in rule\-pipeline\.md\.

```csharp
public string Namespace { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')