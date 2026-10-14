#### [LunaFlow\.Rom](../../../../index.md 'index')
### [LunaFlow\.Rom\.NineRooms](../index.md 'LunaFlow\.Rom\.NineRooms')

## NineRoomsContent Class

Deprecated — see Remarks\. The Nine Rooms example ROM content: the 3x3 room grid and the
movement rule it plays by\.

```csharp
public static class NineRoomsContent
```

Inheritance [System\.Object](https://learn.microsoft.com/en-us/dotnet/api/system.object 'System\.Object') &#129106; NineRoomsContent

### Remarks
> **Warning:**  
> Deprecated. Nine Rooms is disposable Phase 1.5 test content, kept only for engine-fixture
> regression coverage — not scheduled for removal, but not current ROM/Rulebook design
> guidance either. See the `LunaFlow.Rom.D20System` namespace for the current
> System/Scenario-split pattern, and `Documents/NineRooms/nine-rooms.md` for why this
> module stays flat and disposable.

| Fields | |
| :--- | :--- |
| [Rooms](Rooms.md 'LunaFlow\.Rom\.NineRooms\.NineRoomsContent\.Rooms') | The nine rooms, keyed by [Id](../Room/Id.md 'LunaFlow\.Rom\.NineRooms\.Room\.Id')\. |
| [StartRoomId](StartRoomId.md 'LunaFlow\.Rom\.NineRooms\.NineRoomsContent\.StartRoomId') | The room a new player starts in \(the center of the grid\)\. |

| Methods | |
| :--- | :--- |
| [TryMove\(int, string, int\)](TryMove(int,string,int).md 'LunaFlow\.Rom\.NineRooms\.NineRoomsContent\.TryMove\(int, string, int\)') | Resolves moving from [fromId](TryMove(int,string,int).md#LunaFlow.Rom.NineRooms.NineRoomsContent.TryMove(int,string,int).fromId 'LunaFlow\.Rom\.NineRooms\.NineRoomsContent\.TryMove\(int, string, int\)\.fromId') in [direction](TryMove(int,string,int).md#LunaFlow.Rom.NineRooms.NineRoomsContent.TryMove(int,string,int).direction 'LunaFlow\.Rom\.NineRooms\.NineRoomsContent\.TryMove\(int, string, int\)\.direction') \("north"/"south"/"east"/"west"\)\. |
| [Validate\(\)](Validate().md 'LunaFlow\.Rom\.NineRooms\.NineRoomsContent\.Validate\(\)') | Throws if any win line isn't reachable given the current rooms' Challenge/Answer content\. |
