#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices').[IRuleIRGenerator](index.md 'LunaFlow\.Rules\.GeneratorServices\.IRuleIRGenerator')

## IRuleIRGenerator\.Generate\(RuleIRGroup\) Method

Converts an intermediate rule group into a fully\-formed code unit\.

```csharp
LunaFlow.Rules.GeneratorServices.RuleCodeUnit Generate(LunaFlow.Rules.GeneratorServices.RuleIRGroup ir);
```
#### Parameters

<a name='LunaFlow.Rules.GeneratorServices.IRuleIRGenerator.Generate(LunaFlow.Rules.GeneratorServices.RuleIRGroup).ir'></a>

`ir` [RuleIRGroup](../RuleIRGroup/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIRGroup')

The rule group to convert\.

#### Returns
[RuleCodeUnit](../RuleCodeUnit/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit')  
A code unit ready for compilation or emission\.