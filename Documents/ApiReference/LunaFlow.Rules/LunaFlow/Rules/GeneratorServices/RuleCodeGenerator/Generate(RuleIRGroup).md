#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices').[RuleCodeGenerator](index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeGenerator')

## RuleCodeGenerator\.Generate\(RuleIRGroup\) Method

Generates the [RuleCodeUnit](../RuleCodeUnit/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit') for [ir](Generate(RuleIRGroup).md#LunaFlow.Rules.GeneratorServices.RuleCodeGenerator.Generate(LunaFlow.Rules.GeneratorServices.RuleIRGroup).ir 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeGenerator\.Generate\(LunaFlow\.Rules\.GeneratorServices\.RuleIRGroup\)\.ir'): one rule class per [RuleIR](../RuleIR/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIR'), plus the group's registry class\.

```csharp
public LunaFlow.Rules.GeneratorServices.RuleCodeUnit Generate(LunaFlow.Rules.GeneratorServices.RuleIRGroup ir);
```
#### Parameters

<a name='LunaFlow.Rules.GeneratorServices.RuleCodeGenerator.Generate(LunaFlow.Rules.GeneratorServices.RuleIRGroup).ir'></a>

`ir` [RuleIRGroup](../RuleIRGroup/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIRGroup')

Implements [Generate\(RuleIRGroup\)](../IRuleIRGenerator/Generate(RuleIRGroup).md 'LunaFlow\.Rules\.GeneratorServices\.IRuleIRGenerator\.Generate\(LunaFlow\.Rules\.GeneratorServices\.RuleIRGroup\)')

#### Returns
[RuleCodeUnit](../RuleCodeUnit/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit')