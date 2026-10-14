#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices')

## ReflectionRuleParser Class

Reflects over [RuleGroupTypes](../RuleParseContext/RuleGroupTypes.md 'LunaFlow\.Rules\.GeneratorServices\.RuleParseContext\.RuleGroupTypes') — classes marked
[RuleGroupAttribute](../RuleGroupAttribute/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleGroupAttribute') — and their [RuleTemplateAttribute](../RuleTemplateAttribute/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleTemplateAttribute')\-marked
static properties, mapping each returned [RulePrototype](../RulePrototype/index.md 'LunaFlow\.Rules\.GeneratorServices\.RulePrototype') to a [RuleIR](../RuleIR/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIR')
per the D1 join rules in rule\-pipeline\.md\.

```csharp
public sealed class ReflectionRuleParser : LunaFlow.Rules.GeneratorServices.IRuleParser
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; ReflectionRuleParser

Implements [IRuleParser](../IRuleParser/index.md 'LunaFlow\.Rules\.GeneratorServices\.IRuleParser')

| Methods | |
| :--- | :--- |
| [Parse\(RuleParseContext\)](Parse(RuleParseContext).md 'LunaFlow\.Rules\.GeneratorServices\.ReflectionRuleParser\.Parse\(LunaFlow\.Rules\.GeneratorServices\.RuleParseContext\)') | Parses the single rule\-group type in [context](Parse(RuleParseContext).md#LunaFlow.Rules.GeneratorServices.ReflectionRuleParser.Parse(LunaFlow.Rules.GeneratorServices.RuleParseContext).context 'LunaFlow\.Rules\.GeneratorServices\.ReflectionRuleParser\.Parse\(LunaFlow\.Rules\.GeneratorServices\.RuleParseContext\)\.context') into a [RuleIRGroup](../RuleIRGroup/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIRGroup')\. |
