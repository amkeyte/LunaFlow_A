#### [LunaFlow\.Engine](../../../index.md 'index')
### [LunaFlow\.Engine](../index.md 'LunaFlow\.Engine')

## RuleEngine Class

Provides a simplified wrapper around the NRules engine for dynamic rule loading and execution\.

```csharp
public class RuleEngine
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; RuleEngine

| Properties | |
| :--- | :--- |
| [IsInitialized](IsInitialized.md 'LunaFlow\.Engine\.RuleEngine\.IsInitialized') | True once rules have been loaded and a session compiled \([LoadRulesFromAssembly\(Assembly\)](LoadRulesFromAssembly(Assembly).md 'LunaFlow\.Engine\.RuleEngine\.LoadRulesFromAssembly\(System\.Reflection\.Assembly\)') or [LoadRulesFromTypes\(IEnumerable&lt;Type&gt;\)](LoadRulesFromTypes(IEnumerable_Type_).md 'LunaFlow\.Engine\.RuleEngine\.LoadRulesFromTypes\(System\.Collections\.Generic\.IEnumerable\<System\.Type\>\)')\)\. Facts/session\-mutating methods still throw when this is false — this is for callers that need to check first rather than catch\. |

| Methods | |
| :--- | :--- |
| [Clear\(\)](Clear().md 'LunaFlow\.Engine\.RuleEngine\.Clear\(\)') | Clears the rule repository and disposes the current session\. This can be used to reset the engine before reloading a new set of rules\. |
| [Fire\(\)](Fire().md 'LunaFlow\.Engine\.RuleEngine\.Fire\(\)') | Executes all active rules in the current rule session\. |
| [Insert\(object\)](Insert(object).md 'LunaFlow\.Engine\.RuleEngine\.Insert\(object\)') | Inserts a fact into the current rule session\. |
| [LoadRulesFromAssembly\(Assembly\)](LoadRulesFromAssembly(Assembly).md 'LunaFlow\.Engine\.RuleEngine\.LoadRulesFromAssembly\(System\.Reflection\.Assembly\)') | Loads rules from the specified [System\.Reflection\.Assembly](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly 'System\.Reflection\.Assembly')\. All types that derive from [NRules\.Fluent\.Dsl\.Rule](https://learn.microsoft.com/en-us/dotnet/api/nrules.fluent.dsl.rule 'NRules\.Fluent\.Dsl\.Rule') are included\. |
| [LoadRulesFromType\(Type\)](LoadRulesFromType(Type).md 'LunaFlow\.Engine\.RuleEngine\.LoadRulesFromType\(System\.Type\)') | Loads a single rule type into the engine\. |
| [LoadRulesFromTypes\(IEnumerable&lt;Type&gt;\)](LoadRulesFromTypes(IEnumerable_Type_).md 'LunaFlow\.Engine\.RuleEngine\.LoadRulesFromTypes\(System\.Collections\.Generic\.IEnumerable\<System\.Type\>\)') | Loads a set of rule types into the engine\. |
| [Query&lt;T&gt;\(\)](Query_T_().md 'LunaFlow\.Engine\.RuleEngine\.Query\<T\>\(\)') | Queries facts of the given type from the current rule session\. |
| [Retract\(object\)](Retract(object).md 'LunaFlow\.Engine\.RuleEngine\.Retract\(object\)') | Retracts a fact from the current rule session\. |
| [Update\(object\)](Update(object).md 'LunaFlow\.Engine\.RuleEngine\.Update\(object\)') | Updates a fact in the current rule session\. |
