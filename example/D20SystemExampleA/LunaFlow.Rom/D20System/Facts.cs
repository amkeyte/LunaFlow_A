using LunaFlow.Cartridge;

namespace LunaFlow.Rom.D20System
{
    /// <summary>Distinguishes a room's obstacle: a skill check against a DC, or a fight against a monster.</summary>
    public enum RoomChallengeType
    {
        /// <summary>Resolved by <c>check</c>: a <c>d20 + ability modifier</c> roll against the room's DC.</summary>
        SkillCheck,

        /// <summary>Resolved by <c>fight</c>: attack-roll combat against the room's monster.</summary>
        Fight
    }

    /// <summary>Rulebook data: combines spatial (row, col) and challenge (skill, DC) per room.</summary>
    public sealed record Room(
        int Id,
        int Row,
        int Col,
        RoomChallengeType ChallengeType,
        string? Skill,
        string? SkillAbility,
        int? DC,
        string DescriptionKey,
        string? SuccessKey,
        string? FailureKey,
        string? PromptKey);

    /// <summary>A room's challenge data (without spatial information, which GridMap owns).</summary>
    public sealed record RoomChallenge(
        int Id,
        RoomChallengeType ChallengeType,
        string? Skill,
        string? SkillAbility,
        int? DC,
        string? SuccessKey,
        string? FailureKey,
        string? PromptKey);

    /// <summary>A combatant's fixed, pre-baked stat block used by attack/damage resolution.</summary>
    public sealed record CombatantStats(
        int Defense,
        int AttackBonus,
        string DamageDie,
        int DamageBonus,
        int MaxHp);

    /// <summary>The pregenerated Strong-hero PC. Singleton, seeded once at session start.</summary>
    public sealed record PlayerCharacter(CombatantStats Stats);

    /// <summary>The single monster template shared by all three combat rooms. Singleton.</summary>
    public sealed record MonsterTemplate(CombatantStats Stats);

    /// <summary>Fact marking that a room's challenge has been resolved successfully.</summary>
    public sealed record RoomSolved(int RoomId) : IChallengeSolved;

    /// <summary>
    /// An in-progress fight in a specific room. Both HP values are seeded fresh from
    /// PlayerCharacter/MonsterTemplate's MaxHp whenever a new round is created.
    /// </summary>
    public sealed record CombatRound(int RoomId, int MonsterHp, int PlayerHp, int RoundsElapsed) : ICombatStatus;

    /// <summary>
    /// Fact marking that a combat room's fight ended without a win — either the 3-round timeout,
    /// or the PC's HP reaching 0 during that fight. Permanent; no PC-HP-based session-ending state.
    /// </summary>
    public sealed record RoomFailed(int RoomId);
}
