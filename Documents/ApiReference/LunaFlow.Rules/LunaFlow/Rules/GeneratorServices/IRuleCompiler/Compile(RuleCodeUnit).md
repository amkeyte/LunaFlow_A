#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices').[IRuleCompiler](index.md 'LunaFlow\.Rules\.GeneratorServices\.IRuleCompiler')

## IRuleCompiler\.Compile\(RuleCodeUnit\) Method

Compiles a generated code unit into a loadable assembly\. Throws on compilation
failure \(diagnostics included in the exception message\) — there is no partial\-success
return value in the MVP\.

```csharp
System.Reflection.Assembly Compile(LunaFlow.Rules.GeneratorServices.RuleCodeUnit codeUnit);
```
#### Parameters

<a name='LunaFlow.Rules.GeneratorServices.IRuleCompiler.Compile(LunaFlow.Rules.GeneratorServices.RuleCodeUnit).codeUnit'></a>

`codeUnit` [RuleCodeUnit](../RuleCodeUnit/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit')

The generated code unit to compile\.

#### Returns
[System\.Reflection\.Assembly](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly 'System\.Reflection\.Assembly')  
The compiled assembly, loadable via the same `RuleRepository.Load(x =>
            x.From(assembly)...)` path `LunaFlow.Engine.RuleEngine` already uses\.