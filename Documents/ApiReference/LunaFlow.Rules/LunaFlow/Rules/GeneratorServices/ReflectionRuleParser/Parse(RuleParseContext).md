#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices').[ReflectionRuleParser](index.md 'LunaFlow\.Rules\.GeneratorServices\.ReflectionRuleParser')

## ReflectionRuleParser\.Parse\(RuleParseContext\) Method

Parses the single rule\-group type in [context](Parse(RuleParseContext).md#LunaFlow.Rules.GeneratorServices.ReflectionRuleParser.Parse(LunaFlow.Rules.GeneratorServices.RuleParseContext).context 'LunaFlow\.Rules\.GeneratorServices\.ReflectionRuleParser\.Parse\(LunaFlow\.Rules\.GeneratorServices\.RuleParseContext\)\.context') into a [RuleIRGroup](../RuleIRGroup/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIRGroup')\.

```csharp
public LunaFlow.Rules.GeneratorServices.RuleIRGroup Parse(LunaFlow.Rules.GeneratorServices.RuleParseContext context);
```
#### Parameters

<a name='LunaFlow.Rules.GeneratorServices.ReflectionRuleParser.Parse(LunaFlow.Rules.GeneratorServices.RuleParseContext).context'></a>

`context` [RuleParseContext](../RuleParseContext/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleParseContext')

Implements [Parse\(RuleParseContext\)](../IRuleParser/Parse(RuleParseContext).md 'LunaFlow\.Rules\.GeneratorServices\.IRuleParser\.Parse\(LunaFlow\.Rules\.GeneratorServices\.RuleParseContext\)')

#### Returns
[RuleIRGroup](../RuleIRGroup/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIRGroup')

#### Exceptions

[System\.ArgumentException](https://learn.microsoft.com/en-us/dotnet/api/system.argumentexception 'System\.ArgumentException')  
[context](Parse(RuleParseContext).md#LunaFlow.Rules.GeneratorServices.ReflectionRuleParser.Parse(LunaFlow.Rules.GeneratorServices.RuleParseContext).context 'LunaFlow\.Rules\.GeneratorServices\.ReflectionRuleParser\.Parse\(LunaFlow\.Rules\.GeneratorServices\.RuleParseContext\)\.context') has zero or more than one rule\-group type, or its type is missing [RuleGroupAttribute](../RuleGroupAttribute/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleGroupAttribute')\.