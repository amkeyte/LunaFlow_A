#### [LunaFlow\.Engine](../../../index.md 'index')
### [LunaFlow\.Engine](../index.md 'LunaFlow\.Engine').[RuleEngine](index.md 'LunaFlow\.Engine\.RuleEngine')

## RuleEngine\.IsInitialized Property

True once rules have been loaded and a session compiled \([LoadRulesFromAssembly\(Assembly\)](LoadRulesFromAssembly(Assembly).md 'LunaFlow\.Engine\.RuleEngine\.LoadRulesFromAssembly\(System\.Reflection\.Assembly\)')
or [LoadRulesFromTypes\(IEnumerable&lt;Type&gt;\)](LoadRulesFromTypes(IEnumerable_Type_).md 'LunaFlow\.Engine\.RuleEngine\.LoadRulesFromTypes\(System\.Collections\.Generic\.IEnumerable\<System\.Type\>\)')\)\. Facts/session\-mutating methods still throw when this
is false — this is for callers that need to check first rather than catch\.

```csharp
public bool IsInitialized { get; }
```

#### Property Value
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')