#### [LunaFlow\.Engine](../../../index.md 'index')
### [LunaFlow\.Engine](../index.md 'LunaFlow\.Engine').[RuleEngine](index.md 'LunaFlow\.Engine\.RuleEngine')

## RuleEngine\.Retract\(object\) Method

Retracts a fact from the current rule session\.

```csharp
public void Retract(object fact);
```
#### Parameters

<a name='LunaFlow.Engine.RuleEngine.Retract(object).fact'></a>

`fact` [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object')

The fact object to retract\.

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
Thrown if the rule engine has not been initialized\.