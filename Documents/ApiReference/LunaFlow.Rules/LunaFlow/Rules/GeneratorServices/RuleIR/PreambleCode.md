#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices').[RuleIR](index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIR')

## RuleIR\.PreambleCode Property

Gets or sets the C\# statements to emit before the When\(\)/Then\(\) blocks \(typically
variable declarations for Match\<T\>\(\(\) =\> x, \.\.\.\)\)\. Joined from
`RulePrototype.PreambleStatements` by the parser, one statement per line,
verbatim — kept as an opaque string for the same reason as ConditionCode/ActionCode
below \(see rule\-pipeline\.md's IR section\)\.

```csharp
public string PreambleCode { get; set; }
```

#### Property Value
[System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')