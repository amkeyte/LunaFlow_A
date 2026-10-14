#### [LunaFlow\.Engine](../../../index.md 'index')
### [LunaFlow\.Engine](../index.md 'LunaFlow\.Engine')

## IEngineHost Interface

The engine surface exposed to content assemblies \(ROM today, Rulebook once that tier
exists\)\. Deliberately smaller than GameEngineCore's full surface — content code should
never reach GameEngineCore itself, which stays internal\.

```csharp
public interface IEngineHost
```

| Properties | |
| :--- | :--- |
| [Rules](Rules.md 'LunaFlow\.Engine\.IEngineHost\.Rules') | The rule engine content can insert facts into and fire\. |

| Methods | |
| :--- | :--- |
| [Announce\(string\)](Announce(string).md 'LunaFlow\.Engine\.IEngineHost\.Announce\(string\)') | Posts [message](Announce(string).md#LunaFlow.Engine.IEngineHost.Announce(string).message 'LunaFlow\.Engine\.IEngineHost\.Announce\(string\)\.message') to the engine's `MessagePosted` event for the host translator to display\. |
