#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices').[RuleIR](index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIR')

## RuleIR\.RequiredUsings Property

Gets or sets the namespaces the generated file needs\. Copied from
`RulePrototype.RequiredUsings` by the parser; the generator unions/dedups these
across every [RuleIR](index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIR') in a group when it writes one file's using block\.

```csharp
public System.Collections.Generic.List<string> RequiredUsings { get; set; }
```

#### Property Value
[System\.Collections\.Generic\.List&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1 'System\.Collections\.Generic\.List\`1')