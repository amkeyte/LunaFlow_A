#### [LunaFlow\.Rules](../../../../index.md 'index')
### [LunaFlow\.Rules\.GeneratorServices](../index.md 'LunaFlow\.Rules\.GeneratorServices')

## RulePrototype Class

Represents a structured rule definition intended for NRules code generation\.
Contains all elements needed to synthesize a complete Rule\-derived class\.

```csharp
public sealed class RulePrototype
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; RulePrototype

| Properties | |
| :--- | :--- |
| [Description](Description.md 'LunaFlow\.Rules\.GeneratorServices\.RulePrototype\.Description') | Optional rule description\. Not emitted unless present\. |
| [Name](Name.md 'LunaFlow\.Rules\.GeneratorServices\.RulePrototype\.Name') | Name of the rule\. Used as the generated class name and rule metadata\. |
| [PreambleStatements](PreambleStatements.md 'LunaFlow\.Rules\.GeneratorServices\.RulePrototype\.PreambleStatements') | Lines of C\# to insert before When\(\)/Then\(\) blocks\. Commonly used for variable declarations\. |
| [RequiredUsings](RequiredUsings.md 'LunaFlow\.Rules\.GeneratorServices\.RulePrototype\.RequiredUsings') | Usings or fully\-qualified namespaces required in the generated file\. |
| [ThenStatements](ThenStatements.md 'LunaFlow\.Rules\.GeneratorServices\.RulePrototype\.ThenStatements') | DSL statements to insert within the Then\(\) block\. Typically Do, Update, Insert, Retract expressions\. |
| [WhenStatements](WhenStatements.md 'LunaFlow\.Rules\.GeneratorServices\.RulePrototype\.WhenStatements') | DSL statements to insert within the When\(\) block\. Typically Match, Let, or Filter expressions\. |
