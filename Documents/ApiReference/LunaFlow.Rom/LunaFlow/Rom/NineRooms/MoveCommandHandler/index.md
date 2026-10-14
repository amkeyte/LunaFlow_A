#### [LunaFlow\.Rom](../../../../index.md 'index')
### [LunaFlow\.Rom\.NineRooms](../index.md 'LunaFlow\.Rom\.NineRooms')

## MoveCommandHandler Class

Deprecated — see [NineRoomsContent](../NineRoomsContent/index.md 'LunaFlow\.Rom\.NineRooms\.NineRoomsContent')'s Remarks\. Handles the `move`
command: moves the player one step in a cardinal direction if the grid allows it\.

```csharp
public sealed class MoveCommandHandler : LunaFlow.Engine.ICommandHandler
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; MoveCommandHandler

Implements [LunaFlow\.Engine\.ICommandHandler](https://learn.microsoft.com/en-us/dotnet/api/lunaflow.engine.icommandhandler 'LunaFlow\.Engine\.ICommandHandler')

| Methods | |
| :--- | :--- |
| [Handle\(ICommand\)](Handle(ICommand).md 'LunaFlow\.Rom\.NineRooms\.MoveCommandHandler\.Handle\(LunaFlow\.Engine\.ICommand\)') | Moves the player per [command](Handle(ICommand).md#LunaFlow.Rom.NineRooms.MoveCommandHandler.Handle(LunaFlow.Engine.ICommand).command 'LunaFlow\.Rom\.NineRooms\.MoveCommandHandler\.Handle\(LunaFlow\.Engine\.ICommand\)\.command')'s direction argument, or announces the move is invalid\. |
