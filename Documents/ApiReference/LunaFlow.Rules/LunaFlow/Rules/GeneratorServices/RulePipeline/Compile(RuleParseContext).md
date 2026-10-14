#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices').[RulePipeline](index.md 'LunaFlow\.Rules\.GeneratorServices\.RulePipeline')

## RulePipeline\.Compile\(RuleParseContext\) Method

Executes the pipeline on the given context, performing parse, generation, and compilation steps\.

```csharp
public System.Reflection.Assembly Compile(LunaFlow.Rules.GeneratorServices.RuleParseContext context);
```
#### Parameters

<a name='LunaFlow.Rules.GeneratorServices.RulePipeline.Compile(LunaFlow.Rules.GeneratorServices.RuleParseContext).context'></a>

`context` [RuleParseContext](../RuleParseContext/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleParseContext')

The context describing which rule groups to process\.

#### Returns
[System\.Reflection\.Assembly](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly 'System\.Reflection\.Assembly')  
The compiled assembly produced by the configured [IRuleCompiler](../IRuleCompiler/index.md 'LunaFlow\.Rules\.GeneratorServices\.IRuleCompiler')\.