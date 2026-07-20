namespace LunaFlow.Rom.OneD20
{
    /// <summary>Distinguishes a room's obstacle: a skill check against a DC, or a fight against a monster.</summary>
    public enum RoomChallengeType
    {
        /// <summary>Resolved by <c>check</c>: a <c>d20 + ability modifier</c> roll against the room's DC.</summary>
        SkillCheck,

        /// <summary>Resolved by <c>fight</c>: attack-roll combat against the room's monster.</summary>
        Fight
    }

    /// <summary>A single room in the 1.d20 grid: its position, challenge type, and skill-check data if any.</summary>
    /// <param name="Id">The room's 1-9 identifier, row-major.</param>
    /// <param name="Row">Zero-based grid row (0-2).</param>
    /// <param name="Col">Zero-based grid column (0-2).</param>
    /// <param name="ChallengeType">Whether this room is a skill check or a fight.</param>
    /// <param name="Skill">The key ability's skill name (e.g. "Search"), or null for Fight rooms.</param>
    /// <param name="SkillAbility">"Str" | "Dex" | "Con" | "Int" | "Wis" | "Cha", or null for Fight rooms.</param>
    /// <param name="DC">The skill check's target number, or null for Fight rooms.</param>
    /// <param name="DescriptionKey">Key into <see cref="OneD20Flavor"/>, announced by <c>MoveCommandHandler</c>/<c>look</c>, always.</param>
    /// <param name="SuccessKey">Key into <see cref="OneD20Flavor"/> for a successful <c>check</c>, SkillCheck rooms only, else null.</param>
    /// <param name="FailureKey">Key into <see cref="OneD20Flavor"/> for a failed <c>check</c>, SkillCheck rooms only, else null.</param>
    /// <param name="PromptKey">
    /// Key into <see cref="OneD20Flavor"/>, announced by the challenge-prompt rule while
    /// unsolved/unfailed — SkillCheck rooms only. Fight rooms use the fixed <c>monster-desc</c> key
    /// instead, so this is null for them.
    /// </param>
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

    /// <summary>A combatant's fixed, pre-baked stat block used by attack/damage resolution.</summary>
    /// <param name="Defense">Target number an incoming attack roll must meet or beat.</param>
    /// <param name="AttackBonus">Modifier added to this combatant's own attack rolls.</param>
    /// <param name="DamageDie">Damage die notation, e.g. "1d8".</param>
    /// <param name="DamageBonus">Flat bonus added to a damage roll.</param>
    /// <param name="MaxHp">Starting/maximum hit points.</param>
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

    /// <summary>Fact marking which room the player currently occupies.</summary>
    /// <param name="RoomId">The occupied room's id.</param>
    public sealed record PlayerPosition(int RoomId);

    /// <summary>Fact marking that a room's challenge has been resolved successfully.</summary>
    /// <param name="RoomId">The solved room's id.</param>
    public sealed record RoomSolved(int RoomId);

    /// <summary>Fact marking that a win line (a row, column, or diagonal of solved rooms) is complete.</summary>
    /// <param name="Line">The room ids making up the completed line.</param>
    public sealed record WinMarker(IReadOnlyList<int> Line);

    /// <summary>
    /// An in-progress fight in a specific room. Both HP values are seeded fresh from
    /// <see cref="PlayerCharacter"/>/<see cref="MonsterTemplate"/>'s <c>MaxHp</c> whenever a new
    /// round is created — no HP persists across separate fights, including a retry after
    /// <c>flee</c> or a loss in a different room. Exists only while the fight is active; retracted
    /// on win, loss, the 3-round timeout, or <c>flee</c>.
    /// </summary>
    /// <param name="RoomId">The combat room this fight is happening in.</param>
    /// <param name="MonsterHp">The monster's remaining hit points this fight.</param>
    /// <param name="PlayerHp">The PC's remaining hit points this fight.</param>
    /// <param name="RoundsElapsed">How many <c>fight</c> rounds have been resolved so far.</param>
    public sealed record CombatRound(int RoomId, int MonsterHp, int PlayerHp, int RoundsElapsed);

    /// <summary>
    /// Fact marking that a combat room's fight ended without a win — either the 3-round timeout,
    /// or the PC's HP reaching 0 during that fight. Permanent; no PC-HP-based session-ending state
    /// exists — losing a fight fails the room, same as the timeout, never the whole session.
    /// </summary>
    /// <param name="RoomId">The failed room's id.</param>
    public sealed record RoomFailed(int RoomId);

    /// <summary>The room the player occupied before their last successful <c>move</c>. Read by <c>flee</c>.</summary>
    /// <param name="RoomId">The previous room's id.</param>
    public sealed record PreviousRoomId(int RoomId);
}
