#### [LunaFlow\.Rom](../../../../index.md 'index')
### [LunaFlow\.Rom\.NineRooms](../index.md 'LunaFlow\.Rom\.NineRooms')

## AnswerCommandHandler Class

Deprecated — see [NineRoomsContent](../NineRoomsContent/index.md 'LunaFlow\.Rom\.NineRooms\.NineRoomsContent')'s Remarks\. Handles the `answer`
command: checks the player's answer against the current room's challenge and marks the
room solved if it matches\.

```csharp
public sealed class AnswerCommandHandler : LunaFlow.Engine.ICommandHandler
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; AnswerCommandHandler

Implements [LunaFlow\.Engine\.ICommandHandler](https://learn.microsoft.com/en-us/dotnet/api/lunaflow.engine.icommandhandler 'LunaFlow\.Engine\.ICommandHandler')

| Methods | |
| :--- | :--- |
| [Handle\(ICommand\)](Handle(ICommand).md 'LunaFlow\.Rom\.NineRooms\.AnswerCommandHandler\.Handle\(LunaFlow\.Engine\.ICommand\)') | Compares [command](Handle(ICommand).md#LunaFlow.Rom.NineRooms.AnswerCommandHandler.Handle(LunaFlow.Engine.ICommand).command 'LunaFlow\.Rom\.NineRooms\.AnswerCommandHandler\.Handle\(LunaFlow\.Engine\.ICommand\)\.command')'s arguments \(joined\) against the current room's answer\. |
