namespace LunaFlow.Rom.D20SystemInterop
{
    /// <summary>Contract interface for cross-ROM fact: a room's challenge has been resolved successfully.</summary>
    public interface IChallengeSolved
    {
        /// <summary>The room whose challenge was solved.</summary>
        int RoomId { get; }
    }
}
