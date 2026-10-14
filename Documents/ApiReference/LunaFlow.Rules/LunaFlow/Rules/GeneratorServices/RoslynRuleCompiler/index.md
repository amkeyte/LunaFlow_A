#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices')

## RoslynRuleCompiler Class

In\-memory Roslyn compiler per the D2 decision in rule\-pipeline\.md: assembles a
[RuleCodeUnit](../RuleCodeUnit/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit')'s usings/namespace/classes/registry into one source file,
compiles it against the currently loaded assemblies, and loads the result directly from
memory — no path on disk, so a generated rule can load and fire in the same run\.

```csharp
public sealed class RoslynRuleCompiler : LunaFlow.Rules.GeneratorServices.IRuleCompiler
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; RoslynRuleCompiler

Implements [IRuleCompiler](../IRuleCompiler/index.md 'LunaFlow\.Rules\.GeneratorServices\.IRuleCompiler')

| Methods | |
| :--- | :--- |
| [Compile\(RuleCodeUnit\)](Compile(RuleCodeUnit).md 'LunaFlow\.Rules\.GeneratorServices\.RoslynRuleCompiler\.Compile\(LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit\)') | Compiles [codeUnit](Compile(RuleCodeUnit).md#LunaFlow.Rules.GeneratorServices.RoslynRuleCompiler.Compile(LunaFlow.Rules.GeneratorServices.RuleCodeUnit).codeUnit 'LunaFlow\.Rules\.GeneratorServices\.RoslynRuleCompiler\.Compile\(LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit\)\.codeUnit')'s source in memory and loads the resulting assembly\. |
