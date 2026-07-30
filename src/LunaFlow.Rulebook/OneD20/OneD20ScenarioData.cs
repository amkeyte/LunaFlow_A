namespace LunaFlow.Rulebook.OneD20
{
    /// <summary>Rulebook-owned mirror of D20System's <c>RoomChallengeType</c> — no ROM reference.</summary>
    public enum OneD20ChallengeType
    {
        /// <summary>Resolved by <c>check</c>.</summary>
        SkillCheck,

        /// <summary>Resolved by <c>fight</c>.</summary>
        Fight
    }

    /// <summary>
    /// Rulebook-owned data for one room: spatial and challenge fields together, translated by
    /// <c>Program.cs</c> into GridMap's <c>GridRoomData</c> and D20System's <c>RoomChallenge</c> —
    /// this Rulebook never references either ROM's types directly.
    /// </summary>
    public sealed record OneD20RoomData(
        int Id, int Row, int Col,
        OneD20ChallengeType ChallengeType,
        string? Skill, string? SkillAbility, int? DC,
        string DescriptionKey, string? SuccessKey, string? FailureKey, string? PromptKey);

    /// <summary>Rulebook-owned mirror of D20System's <c>CombatantStats</c> — no ROM reference.</summary>
    public sealed record OneD20CombatantStats(
        int Defense, int AttackBonus, string DamageDie, int DamageBonus, int MaxHp);
}
