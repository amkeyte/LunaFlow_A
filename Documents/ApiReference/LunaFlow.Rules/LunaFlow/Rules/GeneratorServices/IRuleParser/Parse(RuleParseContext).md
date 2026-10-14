#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices').[IRuleParser](index.md 'LunaFlow\.Rules\.GeneratorServices\.IRuleParser')

## IRuleParser\.Parse\(RuleParseContext\) Method

Parses one or more rule groups and returns the intermediate rule data\.

```csharp
LunaFlow.Rules.GeneratorServices.RuleIRGroup Parse(LunaFlow.Rules.GeneratorServices.RuleParseContext context);
```
#### Parameters

<a name='LunaFlow.Rules.GeneratorServices.IRuleParser.Parse(LunaFlow.Rules.GeneratorServices.RuleParseContext).context'></a>

`context` [RuleParseContext](../RuleParseContext/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleParseContext')

The context describing what to parse\.

#### Returns
[RuleIRGroup](../RuleIRGroup/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIRGroup')  
A parsed group of intermediate rule data\.