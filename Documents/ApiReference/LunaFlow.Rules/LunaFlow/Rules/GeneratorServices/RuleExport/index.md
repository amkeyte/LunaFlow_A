#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices')

## RuleExport Class

Entry point used to trigger the rule pipeline generation\.
Authors may invoke this manually to compile or emit rule outputs\.
Wires [ReflectionRuleParser](../ReflectionRuleParser/index.md 'LunaFlow\.Rules\.GeneratorServices\.ReflectionRuleParser'), [RuleCodeGenerator](../RuleCodeGenerator/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeGenerator'), and
[RoslynRuleCompiler](../RoslynRuleCompiler/index.md 'LunaFlow\.Rules\.GeneratorServices\.RoslynRuleCompiler') through [RulePipeline](../RulePipeline/index.md 'LunaFlow\.Rules\.GeneratorServices\.RulePipeline') — the compile\-time
mode of the pipeline described in rule\-pipeline\.md\.

```csharp
public static class RuleExport
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; RuleExport

| Methods | |
| :--- | :--- |
| [Compile\(Type\)](Compile(Type).md 'LunaFlow\.Rules\.GeneratorServices\.RuleExport\.Compile\(System\.Type\)') | Compiles a single `[RuleGroup]`\-attributed class into a loadable assembly\. |
| [CompileAll\(Assembly\)](CompileAll(Assembly).md 'LunaFlow\.Rules\.GeneratorServices\.RuleExport\.CompileAll\(System\.Reflection\.Assembly\)') | Compiles every `[RuleGroup]`\-attributed class found in [source](CompileAll(Assembly).md#LunaFlow.Rules.GeneratorServices.RuleExport.CompileAll(System.Reflection.Assembly).source 'LunaFlow\.Rules\.GeneratorServices\.RuleExport\.CompileAll\(System\.Reflection\.Assembly\)\.source'), one compiled assembly per group — [IRuleParser](../IRuleParser/index.md 'LunaFlow\.Rules\.GeneratorServices\.IRuleParser') produces one [RuleIRGroup](../RuleIRGroup/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIRGroup') per call, so an assembly with multiple groups can't be collapsed into a single output assembly without merging distinct GroupNames\. |
