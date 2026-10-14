#### [LunaFlow\.Engine](../../../../index.md 'index')
### [LunaFlow\.Engine\.Components](../index.md 'LunaFlow\.Engine\.Components')

## IHasDependencies Interface

Contract for components \(ROMs, Ruleooks, World Modules, etc\.\) that declare
what capabilities they provide and require\.

```csharp
public interface IHasDependencies
```

| Properties | |
| :--- | :--- |
| [Provides](Provides.md 'LunaFlow\.Engine\.Components\.IHasDependencies\.Provides') | Capabilities this component advertises for other components to depend on\. |
| [Requires](Requires.md 'LunaFlow\.Engine\.Components\.IHasDependencies\.Requires') | Capabilities this component needs from other components\. |
