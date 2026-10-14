#### [LunaFlow\.Engine](../../../index.md 'index')
### [LunaFlow\.Engine](../index.md 'LunaFlow\.Engine').[RuleEngine](index.md 'LunaFlow\.Engine\.RuleEngine')

## RuleEngine\.Insert\(object\) Method

Inserts a fact into the current rule session\.

```csharp
public void Insert(object fact);
```
#### Parameters

<a name='LunaFlow.Engine.RuleEngine.Insert(object).fact'></a>

`fact` [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object')

The fact object to insert\.

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
Thrown if the rule engine has not been initialized\.