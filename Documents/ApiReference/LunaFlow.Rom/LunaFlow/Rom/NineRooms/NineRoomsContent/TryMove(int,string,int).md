#### [LunaFlow\.Rom](../../../../index.md 'index')
### [LunaFlow\.Rom\.NineRooms](../index.md 'LunaFlow\.Rom\.NineRooms').[NineRoomsContent](index.md 'LunaFlow\.Rom\.NineRooms\.NineRoomsContent')

## NineRoomsContent\.TryMove\(int, string, int\) Method

Resolves moving from [fromId](TryMove(int,string,int).md#LunaFlow.Rom.NineRooms.NineRoomsContent.TryMove(int,string,int).fromId 'LunaFlow\.Rom\.NineRooms\.NineRoomsContent\.TryMove\(int, string, int\)\.fromId') in [direction](TryMove(int,string,int).md#LunaFlow.Rom.NineRooms.NineRoomsContent.TryMove(int,string,int).direction 'LunaFlow\.Rom\.NineRooms\.NineRoomsContent\.TryMove\(int, string, int\)\.direction') \("north"/"south"/"east"/"west"\)\.

```csharp
public static bool TryMove(int fromId, string direction, out int toId);
```
#### Parameters

<a name='LunaFlow.Rom.NineRooms.NineRoomsContent.TryMove(int,string,int).fromId'></a>

`fromId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The room id to move from\.

<a name='LunaFlow.Rom.NineRooms.NineRoomsContent.TryMove(int,string,int).direction'></a>

`direction` [System\.String](https://learn.microsoft.com/en-us/dotnet/api/system.string 'System\.String')

"north", "south", "east", or "west" \(case\-insensitive\)\.

<a name='LunaFlow.Rom.NineRooms.NineRoomsContent.TryMove(int,string,int).toId'></a>

`toId` [System\.Int32](https://learn.microsoft.com/en-us/dotnet/api/system.int32 'System\.Int32')

The destination room id if the move is valid; otherwise \-1\.

#### Returns
[System\.Boolean](https://learn.microsoft.com/en-us/dotnet/api/system.boolean 'System\.Boolean')  
True if the move stays on the grid\.