#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices').[RulePipeline](index.md 'LunaFlow\.Rules\.GeneratorServices\.RulePipeline')

## RulePipeline\(IRuleParser, IRuleIRGenerator, IRuleCompiler\) Constructor

Initializes a new instance of the [RulePipeline](index.md 'LunaFlow\.Rules\.GeneratorServices\.RulePipeline') class\.

```csharp
public RulePipeline(LunaFlow.Rules.GeneratorServices.IRuleParser parser, LunaFlow.Rules.GeneratorServices.IRuleIRGenerator generator, LunaFlow.Rules.GeneratorServices.IRuleCompiler compiler);
```
#### Parameters

<a name='LunaFlow.Rules.GeneratorServices.RulePipeline.RulePipeline(LunaFlow.Rules.GeneratorServices.IRuleParser,LunaFlow.Rules.GeneratorServices.IRuleIRGenerator,LunaFlow.Rules.GeneratorServices.IRuleCompiler).parser'></a>

`parser` [IRuleParser](../IRuleParser/index.md 'LunaFlow\.Rules\.GeneratorServices\.IRuleParser')

The parser to extract rule definitions\.

<a name='LunaFlow.Rules.GeneratorServices.RulePipeline.RulePipeline(LunaFlow.Rules.GeneratorServices.IRuleParser,LunaFlow.Rules.GeneratorServices.IRuleIRGenerator,LunaFlow.Rules.GeneratorServices.IRuleCompiler).generator'></a>

`generator` [IRuleIRGenerator](../IRuleIRGenerator/index.md 'LunaFlow\.Rules\.GeneratorServices\.IRuleIRGenerator')

The IR generator to transform parsed rules\.

<a name='LunaFlow.Rules.GeneratorServices.RulePipeline.RulePipeline(LunaFlow.Rules.GeneratorServices.IRuleParser,LunaFlow.Rules.GeneratorServices.IRuleIRGenerator,LunaFlow.Rules.GeneratorServices.IRuleCompiler).compiler'></a>

`compiler` [IRuleCompiler](../IRuleCompiler/index.md 'LunaFlow\.Rules\.GeneratorServices\.IRuleCompiler')

The compiler to emit or build rule output\.