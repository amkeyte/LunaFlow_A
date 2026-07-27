using LunaFlow.Rom.D20SystemInterop;

namespace LunaFlow.Rom.GridMap
{
    /// <summary>Room metadata for the grid: position, description key.</summary>
    public sealed record GridRoomData(int Id, int Row, int Col, string DescriptionKey);

    /// <summary>Fact marking which room the player currently occupies.</summary>
    public sealed record PlayerPosition(int RoomId) : IPlayerLocation;

    /// <summary>The room the player occupied before their last successful <c>move</c>. Read by <c>flee</c>.</summary>
    public sealed record PreviousRoomId(int RoomId);

    /// <summary>Fact marking that a win line (a row, column, or diagonal of solved rooms) is complete.</summary>
    public sealed record WinMarker(IReadOnlyList<int> Line);
}
