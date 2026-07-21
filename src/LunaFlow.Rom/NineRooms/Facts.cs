namespace LunaFlow.Rom.NineRooms
{
    /// <summary>
    /// Deprecated — see <see cref="NineRoomsContent"/>'s Remarks. A single room in the Nine
    /// Rooms grid: its position, flavor text, and challenge/answer pair.
    /// </summary>
    /// <param name="Id">The room's 1-9 identifier, row-major.</param>
    /// <param name="Row">Zero-based grid row (0-2).</param>
    /// <param name="Col">Zero-based grid column (0-2).</param>
    /// <param name="Name">The room's display name.</param>
    /// <param name="Description">The room's flavor text, shown by <c>look</c> and on entry.</param>
    /// <param name="ChallengePrompt">The riddle/question posed to the player, or null if the room has none.</param>
    /// <param name="Answer">The expected answer text, or null if the room has none.</param>
    public sealed record Room(
        int Id,            // 1-9, row-major
        int Row,           // 0-2
        int Col,           // 0-2
        string Name,
        string Description,
        string? ChallengePrompt,
        string? Answer);

    /// <summary>
    /// Deprecated — see <see cref="NineRoomsContent"/>'s Remarks. Fact marking which room
    /// the player currently occupies.
    /// </summary>
    /// <param name="RoomId">The occupied room's id.</param>
    public sealed record PlayerPosition(int RoomId);

    /// <summary>
    /// Deprecated — see <see cref="NineRoomsContent"/>'s Remarks. Fact marking that a
    /// room's challenge has been answered correctly.
    /// </summary>
    /// <param name="RoomId">The solved room's id.</param>
    public sealed record RoomSolved(int RoomId);

    /// <summary>
    /// Deprecated — see <see cref="NineRoomsContent"/>'s Remarks. Fact marking that a win
    /// line (a row, column, or diagonal of solved rooms) is complete.
    /// </summary>
    /// <param name="Line">The room ids making up the completed line.</param>
    public sealed record WinMarker(IReadOnlyList<int> Line);
}
