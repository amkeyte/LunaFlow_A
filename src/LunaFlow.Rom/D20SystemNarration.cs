namespace LunaFlow.Rom.D20System
{
    /// <summary>
    /// TEST HELPER: Narration and announcement text construction for test assertions.
    /// This class was originally part of Phase 2.6 test infrastructure and was deleted during
    /// the Phase 2.7 architectural split. It is recreated here to support test assertions
    /// that verify exact narration wording from handlers and rules.
    /// AUTHORIZED FOR TEST USE ONLY (user directive — not production code).
    /// </summary>
    public static class D20SystemNarration
    {
        /// <summary>Formats the win announcement with the winning line (room sequence).</summary>
        public static string WinAnnouncement(IReadOnlyList<int> winningLine)
        {
            var roomList = string.Join(", ", winningLine);
            return $"Three for three — that's enough to call it. Mission complete. [Rooms: {roomList}]";
        }
    }
}
