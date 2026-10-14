#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices').[RoslynRuleCompiler](index.md 'LunaFlow\.Rules\.GeneratorServices\.RoslynRuleCompiler')

## RoslynRuleCompiler\.Compile\(RuleCodeUnit\) Method

Compiles [codeUnit](Compile(RuleCodeUnit).md#LunaFlow.Rules.GeneratorServices.RoslynRuleCompiler.Compile(LunaFlow.Rules.GeneratorServices.RuleCodeUnit).codeUnit 'LunaFlow\.Rules\.GeneratorServices\.RoslynRuleCompiler\.Compile\(LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit\)\.codeUnit')'s source in memory and loads the resulting assembly\.

```csharp
public System.Reflection.Assembly Compile(LunaFlow.Rules.GeneratorServices.RuleCodeUnit codeUnit);
```
#### Parameters

<a name='LunaFlow.Rules.GeneratorServices.RoslynRuleCompiler.Compile(LunaFlow.Rules.GeneratorServices.RuleCodeUnit).codeUnit'></a>

`codeUnit` [RuleCodeUnit](../RuleCodeUnit/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit')

Implements [Compile\(RuleCodeUnit\)](../IRuleCompiler/Compile(RuleCodeUnit).md 'LunaFlow\.Rules\.GeneratorServices\.IRuleCompiler\.Compile\(LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit\)')

#### Returns
[System\.Reflection\.Assembly](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly 'System\.Reflection\.Assembly')

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
The generated source failed to compile\.