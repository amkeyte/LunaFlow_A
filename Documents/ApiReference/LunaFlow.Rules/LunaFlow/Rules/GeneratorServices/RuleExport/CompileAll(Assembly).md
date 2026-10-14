#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices').[RuleExport](index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleExport')

## RuleExport\.CompileAll\(Assembly\) Method

Compiles every `[RuleGroup]`\-attributed class found in [source](CompileAll(Assembly).md#LunaFlow.Rules.GeneratorServices.RuleExport.CompileAll(System.Reflection.Assembly).source 'LunaFlow\.Rules\.GeneratorServices\.RuleExport\.CompileAll\(System\.Reflection\.Assembly\)\.source'),
one compiled assembly per group — [IRuleParser](../IRuleParser/index.md 'LunaFlow\.Rules\.GeneratorServices\.IRuleParser') produces one
[RuleIRGroup](../RuleIRGroup/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIRGroup') per call, so an assembly with multiple groups can't be
collapsed into a single output assembly without merging distinct GroupNames\.

```csharp
public static System.Collections.Generic.IEnumerable<System.Reflection.Assembly> CompileAll(System.Reflection.Assembly source);
```
#### Parameters

<a name='LunaFlow.Rules.GeneratorServices.RuleExport.CompileAll(System.Reflection.Assembly).source'></a>

`source` [System\.Reflection\.Assembly](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly 'System\.Reflection\.Assembly')

#### Returns
[System\.Collections\.Generic\.IEnumerable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')[System\.Reflection\.Assembly](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly 'System\.Reflection\.Assembly')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1 'System\.Collections\.Generic\.IEnumerable\`1')