#### [LunaFlow\.Engine](../../../../index.md 'index')
### [LunaFlow\.Engine\.Components](../index.md 'LunaFlow\.Engine\.Components').[ComponentRegistry](index.md 'LunaFlow\.Engine\.Components\.ComponentRegistry')

## ComponentRegistry\.Validate\(\) Method

Validates the complete component graph: no duplicate capability providers,
all requirements satisfied, tier\-ordering constraints met, and no circular dependencies\.

```csharp
public void Validate();
```

#### Exceptions

[System\.InvalidOperationException](https://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception 'System\.InvalidOperationException')  
If validation fails \(duplicate provider, missing requirement, tier violation, or cycle\)\.