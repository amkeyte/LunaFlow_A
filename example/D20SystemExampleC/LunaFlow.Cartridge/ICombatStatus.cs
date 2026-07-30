namespace LunaFlow.Cartridge
{
    /// <summary>Contract interface for cross-ROM fact: the status of an active fight.</summary>
    public interface ICombatStatus
    {
        /// <summary>The combat room this fight is happening in.</summary>
        int RoomId { get; }

        /// <summary>The monster's remaining hit points this fight.</summary>
        int MonsterHp { get; }

        /// <summary>The PC's remaining hit points this fight.</summary>
        int PlayerHp { get; }
    }
}
