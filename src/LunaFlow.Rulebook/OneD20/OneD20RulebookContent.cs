namespace LunaFlow.Rulebook.OneD20
{
    /// <summary>
    /// The 1.d20 Department-7 mission's Scenario data: the 9 rooms' specific skill/DC/flavor-key
    /// assignments, the pregenerated PC's sheet, and the monster's stat block — Rulebook-owned data
    /// types only (see <see cref="OneD20RoomData"/>/<see cref="OneD20CombatantStats"/>), never a ROM
    /// type. Translated into
    /// GridMap's/D20System's own fact shapes by <c>Program.cs</c> (the composition root, exempt from
    /// the Rulebook/ROM boundary). Pure data; no logic, no fact-store access.
    /// </summary>
    public static class OneD20RulebookContent
    {
        /// <summary>The nine rooms, keyed by <see cref="OneD20RoomData.Id"/>. Rooms 1, 5, 8 are Fight; the rest are SkillCheck.</summary>
        public static readonly IReadOnlyDictionary<int, OneD20RoomData> Rooms = new Dictionary<int, OneD20RoomData>
        {
            [1] = new OneD20RoomData(1, 0, 0, OneD20ChallengeType.Fight, null, null, null, "room-1-desc", null, null, null),
            [2] = new OneD20RoomData(2, 0, 1, OneD20ChallengeType.SkillCheck, "Search", "Int", 10, "room-2-desc", "check-2-success", "check-2-failure", "room-2-prompt"),
            [3] = new OneD20RoomData(3, 0, 2, OneD20ChallengeType.SkillCheck, "Climb", "Str", 12, "room-3-desc", "check-3-success", "check-3-failure", "room-3-prompt"),
            [4] = new OneD20RoomData(4, 1, 0, OneD20ChallengeType.SkillCheck, "Listen", "Wis", 8, "room-4-desc", "check-4-success", "check-4-failure", "room-4-prompt"),
            [5] = new OneD20RoomData(5, 1, 1, OneD20ChallengeType.Fight, null, null, null, "room-5-desc", null, null, null),
            [6] = new OneD20RoomData(6, 1, 2, OneD20ChallengeType.SkillCheck, "Swim", "Str", 10, "room-6-desc", "check-6-success", "check-6-failure", "room-6-prompt"),
            [7] = new OneD20RoomData(7, 2, 0, OneD20ChallengeType.SkillCheck, "Disable Device", "Int", 12, "room-7-desc", "check-7-success", "check-7-failure", "room-7-prompt"),
            [8] = new OneD20RoomData(8, 2, 1, OneD20ChallengeType.Fight, null, null, null, "room-8-desc", null, null, null),
            [9] = new OneD20RoomData(9, 2, 2, OneD20ChallengeType.SkillCheck, "Jump", "Str", 8, "room-9-desc", "check-9-success", "check-9-failure", "room-9-prompt"),
        };

        /// <summary>The room a new player starts in (the center of the grid — also a Fight room).</summary>
        public const int StartRoomId = 5;

        /// <summary>The pregenerated level-1 Strong-hero PC's stats.</summary>
        public static readonly OneD20CombatantStats PlayerCharacter =
            new(Defense: 11, AttackBonus: 4, DamageDie: "1d8", DamageBonus: 3, MaxHp: 11);

        /// <summary>The seedy man with a rapier — same monster in all three combat rooms.</summary>
        public static readonly OneD20CombatantStats Monster =
            new(Defense: 13, AttackBonus: 3, DamageDie: "1d6", DamageBonus: 1, MaxHp: 8);

        /// <summary>The PC's six ability scores, per the Strong-hero sheet.</summary>
        public static readonly IReadOnlyDictionary<string, int> PlayerAbilityScores = new Dictionary<string, int>
        {
            ["Str"] = 16,
            ["Dex"] = 10,
            ["Con"] = 16,
            ["Int"] = 10,
            ["Wis"] = 10,
            ["Cha"] = 10,
        };
    }
}
