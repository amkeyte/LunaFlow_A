#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices')

## RuleCodeGenerator Class

Turns a parsed [RuleIRGroup](../RuleIRGroup/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIRGroup') into the C\# source of one [RuleCodeUnit](../RuleCodeUnit/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit') —
one [ARule](../../ARule/index.md 'LunaFlow\.Rules\.ARule')\-derived class per [RuleIR](../RuleIR/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIR'), plus a group registry class\.
Shape follows rule\-pipeline\.md's "Output conventions" section exactly \(namespace per D4,
Reference per D3, \.Not\<T\>\(\) injection per \[StandingRule\]\)\.

```csharp
public sealed class RuleCodeGenerator : LunaFlow.Rules.GeneratorServices.IRuleIRGenerator
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; RuleCodeGenerator

Implements [IRuleIRGenerator](../IRuleIRGenerator/index.md 'LunaFlow\.Rules\.GeneratorServices\.IRuleIRGenerator')

| Methods | |
| :--- | :--- |
| [Generate\(RuleIRGroup\)](Generate(RuleIRGroup).md 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeGenerator\.Generate\(LunaFlow\.Rules\.GeneratorServices\.RuleIRGroup\)') | Generates the [RuleCodeUnit](../RuleCodeUnit/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeUnit') for [ir](Generate(RuleIRGroup).md#LunaFlow.Rules.GeneratorServices.RuleCodeGenerator.Generate(LunaFlow.Rules.GeneratorServices.RuleIRGroup).ir 'LunaFlow\.Rules\.GeneratorServices\.RuleCodeGenerator\.Generate\(LunaFlow\.Rules\.GeneratorServices\.RuleIRGroup\)\.ir'): one rule class per [RuleIR](../RuleIR/index.md 'LunaFlow\.Rules\.GeneratorServices\.RuleIR'), plus the group's registry class\. |
