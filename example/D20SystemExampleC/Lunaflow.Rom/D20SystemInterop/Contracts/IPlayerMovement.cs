namespace LunaFlow.Rom.D20SystemInterop
{
    /// <summary>Contract interface for cross-ROM service: player movement actions.</summary>
    public interface IPlayerMovement
    {
        /// <summary>
        /// Moves the player back to <c>PreviousRoomId</c>. Returns false (no-op) if none
        /// exists yet — the room-5-pre-any-move edge case.
        /// </summary>
        bool ReturnToPreviousRoom();
    }
}
