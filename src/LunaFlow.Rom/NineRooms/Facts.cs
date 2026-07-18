namespace LunaFlow.Rom.NineRooms
{
    public sealed record Room(
        int Id,            // 1-9, row-major
        int Row,           // 0-2
        int Col,           // 0-2
        string Name,
        string Description,
        string? ChallengePrompt,
        string? Answer);

    public sealed record PlayerPosition(int RoomId);

    public sealed record RoomSolved(int RoomId);

    public sealed record WinMarker(IReadOnlyList<int> Line);
}
