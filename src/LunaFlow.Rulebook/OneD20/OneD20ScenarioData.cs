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
    /// Rulebook-owned data for one room's fixed grid position and which slot-kind it is — the
    /// topology (position, fight-vs-skill-check) never varies session to session. The specific
    /// challenge/monster filling the slot is session-assigned data instead (see
    /// <c>OneD20SessionAssembler</c>), no longer carried on this record.
    /// </summary>
    public sealed record OneD20RoomData(
        int Id, int Row, int Col,
        OneD20ChallengeType ChallengeType,
        string DescriptionKey);

    /// <summary>Rulebook-owned mirror of D20System's <c>CheckKind</c> — no ROM reference.</summary>
    public enum OneD20CheckKind { Skill, Save }

    /// <summary>
    /// Rulebook-owned data for one pool skill-check challenge (9 total this session's content
    /// ships) — assignable to any of the grid's fixed skill-check positions. Keyed by
    /// <see cref="Key"/> (e.g. <c>"search"</c>), which doubles as the flavor-text key prefix
    /// (<c>{key}-prompt</c>/<c>{key}-success</c>/<c>{key}-failure</c>). <c>Kind</c>/<c>SaveType</c>
    /// mirror D20System's <c>RoomChallenge</c> extension (phase-2-10-content-coverage-spec.md): a
    /// Skill-kind entry sets <c>Skill</c>/<c>SkillAbility</c> and leaves <c>SaveType</c> null; a
    /// Save-kind entry does the reverse. Trailing optional parameters so existing Skill-kind pool
    /// entries need no positional-argument changes.
    /// </summary>
    public sealed record OneD20ChallengeTemplate(
        string Key, string? Skill, string? SkillAbility, int DC,
        string SuccessKey, string FailureKey, string PromptKey,
        OneD20CheckKind? Kind = null, string? SaveType = null);

    /// <summary>Rulebook-owned mirror of D20System's <c>CombatantStats</c> — no ROM reference.</summary>
    public sealed record OneD20CombatantStats(
        int Defense, int AttackBonus, string DamageDie, int DamageBonus, int MaxHp);

    /// <summary>
    /// Rulebook-owned data for one roster monster (5 total this session's content ships) —
    /// assignable to any of the grid's fixed fight positions. Keyed by <see cref="Id"/>, which also
    /// keys the monster→item loot table. <c>InflictsPoison</c> mirrors D20System's
    /// <c>MonsterTemplate</c> extension — true for exactly one roster monster.
    /// </summary>
    public sealed record OneD20MonsterTemplate(
        string Id, string Name, string DescriptionKey, OneD20CombatantStats Stats,
        bool InflictsPoison = false);
}
