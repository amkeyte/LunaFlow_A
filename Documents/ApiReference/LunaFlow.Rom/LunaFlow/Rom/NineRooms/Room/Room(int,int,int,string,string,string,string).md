#### [LunaFlow\.Rom](../../../../index.md 'index')
### [LunaFlow\.Rom\.NineRooms](../index.md 'LunaFlow\.Rom\.NineRooms').[Room](index.md 'LunaFlow\.Rom\.NineRooms\.Room')

## Room\(int, int, int, string, string, string, string\) Constructor

Deprecated — see [NineRoomsContent](../NineRoomsContent/index.md 'LunaFlow\.Rom\.NineRooms\.NineRoomsContent')'s Remarks\. A single room in the Nine
Rooms grid: its position, flavor text, and challenge/answer pair\.

```csharp
public Room(int Id, int Row, int Col, string Name, string Description, string? ChallengePrompt, string? Answer);
```
#### Parameters

<a name='LunaFlow.Rom.NineRooms.Room.Room(int,int,int,string,string,string,string).Id'></a>

`Id` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The room's 1\-9 identifier, row\-major\.

<a name='LunaFlow.Rom.NineRooms.Room.Room(int,int,int,string,string,string,string).Row'></a>

`Row` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

Zero\-based grid row \(0\-2\)\.

<a name='LunaFlow.Rom.NineRooms.Room.Room(int,int,int,string,string,string,string).Col'></a>

`Col` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

Zero\-based grid column \(0\-2\)\.

<a name='LunaFlow.Rom.NineRooms.Room.Room(int,int,int,string,string,string,string).Name'></a>

`Name` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The room's display name\.

<a name='LunaFlow.Rom.NineRooms.Room.Room(int,int,int,string,string,string,string).Description'></a>

`Description` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The room's flavor text, shown by `look` and on entry\.

<a name='LunaFlow.Rom.NineRooms.Room.Room(int,int,int,string,string,string,string).ChallengePrompt'></a>

`ChallengePrompt` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The riddle/question posed to the player, or null if the room has none\.

<a name='LunaFlow.Rom.NineRooms.Room.Room(int,int,int,string,string,string,string).Answer'></a>

`Answer` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

The expected answer text, or null if the room has none\.