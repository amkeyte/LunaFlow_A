namespace LunaFlow.Rom.D20SystemInterop
{
    /// <summary>Contract interface for cross-ROM fact: the player's current location.</summary>
    public interface IPlayerLocation
    {
        /// <summary>The room the player currently occupies.</summary>
        int RoomId { get; }
    }
}
