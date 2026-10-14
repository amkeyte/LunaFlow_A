#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices')

## RulePipeline Class

Coordinates the full rule generation pipeline, from parsing to code generation to output\.

```csharp
public sealed class RulePipeline
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; RulePipeline

| Constructors | |
| :--- | :--- |
| [RulePipeline\(IRuleParser, IRuleIRGenerator, IRuleCompiler\)](RulePipeline(IRuleParser,IRuleIRGenerator,IRuleCompiler).md 'LunaFlow\.Rules\.GeneratorServices\.RulePipeline\.RulePipeline\(LunaFlow\.Rules\.GeneratorServices\.IRuleParser, LunaFlow\.Rules\.GeneratorServices\.IRuleIRGenerator, LunaFlow\.Rules\.GeneratorServices\.IRuleCompiler\)') | Initializes a new instance of the [RulePipeline](index.md 'LunaFlow\.Rules\.GeneratorServices\.RulePipeline') class\. |

| Methods | |
| :--- | :--- |
| [Compile\(RuleParseContext\)](Compile(RuleParseContext).md 'LunaFlow\.Rules\.GeneratorServices\.RulePipeline\.Compile\(LunaFlow\.Rules\.GeneratorServices\.RuleParseContext\)') | Executes the pipeline on the given context, performing parse, generation, and compilation steps\. |
