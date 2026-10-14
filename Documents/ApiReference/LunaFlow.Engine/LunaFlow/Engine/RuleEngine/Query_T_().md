#### [LunaFlow\.Engine](../../../index.md 'index')
### [LunaFlow\.Engine](../index.md 'LunaFlow\.Engine').[RuleEngine](index.md 'LunaFlow\.Engine\.RuleEngine')

## RuleEngine\.Query\<T\>\(\) Method

Queries facts of the given type from the current rule session\.

```csharp
public System.Linq.IQueryable<T> Query<T>();
```
#### Type parameters

<a name='LunaFlow.Engine.RuleEngine.Query_T_().T'></a>

`T`

#### Returns
[System\.Linq\.IQueryable&lt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')[T](Query_T_().md#LunaFlow.Engine.RuleEngine.Query_T_().T 'LunaFlow\.Engine\.RuleEngine\.Query\<T\>\(\)\.T')[&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.linq.iqueryable-1 'System\.Linq\.IQueryable\`1')

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
Thrown if the rule engine has not been initialized\.