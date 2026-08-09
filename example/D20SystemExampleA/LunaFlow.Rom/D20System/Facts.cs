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

    /// <summary>Distinguishes a SkillCheck room's resolution basis: an ability-and-skill check, or a
    /// saving throw. Null on <see cref="RoomChallenge"/> for Fight rooms; required for SkillCheck rooms.</summary>
    public enum CheckKind { Skill, Save }

    /// <summary>A room's challenge data (without spatial information, which GridMap owns).
    /// <c>MonsterId</c> is set for Fight rooms only (resolves into <see cref="MonsterTemplate"/> via
    /// the session's monster roster); null for SkillCheck rooms. <c>Kind</c> discriminates a
    /// SkillCheck room's <c>Skill</c>/<c>SkillAbility</c> fields (Skill kind) from <c>SaveType</c>
    /// (Save kind) — mutually exclusive, per <c>phase-2-10-content-coverage-spec.md</c>.</summary>
    public sealed record RoomChallenge(
        int Id,
        RoomChallengeType ChallengeType,
        CheckKind? Kind,
        string? Skill,
        string? SkillAbility,
        int? DC,
        string? SaveType,
        string? SuccessKey,
        string? FailureKey,
        string? PromptKey,
        string? MonsterId);

    /// <summary>A combatant's fixed, pre-baked stat block used by attack/damage resolution.</summary>
    public sealed record CombatantStats(
        int Defense,
        int AttackBonus,
        string DamageDie,
        int DamageBonus,
        int MaxHp);

    /// <summary>The pregenerated Strong-hero PC. Singleton, seeded once at session start.</summary>
    public sealed record PlayerCharacter(CombatantStats Stats);

    /// <summary>The PC's three saving-throw bonuses — derived once at session assembly from the fixed
    /// Strong-hero base-save table plus the matching ability modifier already in
    /// <c>PlayerAbilityScores</c> (Con for Fortitude, Dex for Reflex, Wis for Will). Attached
    /// alongside <see cref="PlayerCharacter"/>, not folded into <see cref="CombatantStats"/> —
    /// monsters don't make saves in this content.</summary>
    public sealed record SavingThrows(int Fortitude, int Reflex, int Will);

    /// <summary>One entry in the monster roster — a keyed, identified template (five entries this
    /// session's content ships), replacing the pre-roster implicit singleton. <c>InflictsPoison</c>
    /// is true for exactly one roster monster (the Scrapper) — its hit connecting refreshes
    /// <see cref="Poisoned"/>. <c>EnrageThresholdFraction</c>/<c>EnrageDefenseBonus</c>
    /// (phase-2-12-lua-worldmodule-stress-test-spec.md item 11) mirror
    /// <c>OneD20MonsterTemplate</c>'s Rulebook-owned fields unchanged — carried through by
    /// <c>Program.cs</c>'s translation, same as <c>InflictsPoison</c>. See
    /// <see cref="MonsterEnraged"/> for the mechanic these two fields drive.</summary>
    public sealed record MonsterTemplate(
        string Id, string Name, string DescriptionKey, CombatantStats Stats,
        bool InflictsPoison = false,
        double? EnrageThresholdFraction = null,
        int? EnrageDefenseBonus = null);

    /// <summary>
    /// Fact marking that the room's monster has crossed its Enrage HP threshold this fight —
    /// inserted once per <see cref="CombatRound"/>, never re-inserted while it lasts. Purely
    /// defensive (per ticket 0083's ruling): while present, the monster's effective Defense for any
    /// subsequent PC attack this fight is <c>Stats.Defense + EnrageDefenseBonus</c>; it has no effect
    /// on the monster's own attack roll (<c>FightCommandHandler</c>'s counterattack branch never
    /// reads the monster's Defense at all). Retracted whenever this room's <c>CombatRound</c> ends —
    /// win, loss, timeout, or flee — via <see cref="EnrageEffect.RetractIfPresent"/>, so a fresh
    /// fight against the same monster in a retry always starts un-enraged. Deliberately combat-scoped
    /// (unlike <see cref="Poisoned"/>, which is deliberately cross-room) — the contrast is worth
    /// keeping straight, not a copy-paste of the same lifecycle shape.
    /// </summary>
    public sealed record MonsterEnraged(int RoomId);

    /// <summary>
    /// A lingering roll penalty, inflicted by a poison-carrying monster's connecting hit and refreshed
    /// (not stacked) on subsequent hits. Applies a -2 penalty to the PC's next <see cref="ChecksRemaining"/>
    /// <c>check</c>/<c>fight</c> rolls, decrementing on each one, retracted at 0 — deliberately a roll
    /// penalty rather than damage-over-time (1.d20 has no persistent PC HP outside an active
    /// <c>CombatRound</c>; see <c>phase-2-10-content-coverage-spec.md</c>'s Mechanics section). Not
    /// scoped to a room — accumulates and applies across a room transition, unlike <c>CombatRound</c>.
    /// </summary>
    public sealed record Poisoned(int ChecksRemaining);

    /// <summary>A weapon's mechanical profile for <c>fight</c>'s PC-attack step: attack bonus, damage
    /// die, damage bonus. The PC's default (starting sword) profile lives on
    /// <see cref="PlayerCharacter"/>.<see cref="CombatantStats"/>; an equipped weapon (see
    /// <c>LunaFlow.Cartridge.EquippedWeapon</c>) substitutes one of these instead. Fixed
    /// Loot/D20System-tier data, not World-Module-authored — per-item combat math is a System-level
    /// capability, the same way the PC's own starting sword stats are fixed.</summary>
    public sealed record WeaponProfile(int AttackBonus, string DamageDie, int DamageBonus);

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

    /// <summary>
    /// The challenge-prompt rule's seam into Rulebook flavor text (ticket 0043, Amara's ruling):
    /// already-resolved text, not a key — generated <c>RulePrototype</c> code has no constructor/DI
    /// seam, so <c>Program.cs</c> resolves the flavor key once at seed time and hands the rule only
    /// the finished string, same pattern <c>NineRooms/ChallengeRules.cs</c>'s pre-split
    /// <c>Room.ChallengePrompt</c> already used. One per room, seeded at session start.
    /// </summary>
    public sealed record RoomChallengePrompt(int RoomId, string Text);
}
