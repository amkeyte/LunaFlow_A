namespace LunaFlow.Rulebook.OneD20
{
    /// <summary>
    /// The second, independently-authored <see cref="IOneD20WorldModule"/> proving the World Module
    /// tier is a genuine swap, not a relabel — per ticket 0059/0060 and
    /// <c>1-d20-worldmodule-hollow-keep-spec.md</c>. A reskin, not a rebalance: room count, grid
    /// positions, Fight/SkillCheck slot assignment, challenge skill/ability/DC values, and monster/PC
    /// stat blocks are all byte-for-byte identical to <see cref="DefaultOneD20WorldModule"/> — only
    /// names, descriptions, and narration differ throughout. Flavor text lives in the
    /// <c>FlavorText</c> partial (<c>HollowKeepWorldModule.FlavorText.cs</c>), same file-split
    /// convention as <see cref="DefaultOneD20WorldModule"/>.
    /// </summary>
    public sealed partial class HollowKeepWorldModule : IOneD20WorldModule
    {
        /// <summary>Same 9 fixed grid positions and slot-kinds as <see cref="DefaultOneD20WorldModule"/>
        /// — rooms 1, 5, 8 are Fight; the rest are SkillCheck.</summary>
        public IReadOnlyDictionary<int, OneD20RoomData> Rooms { get; } = new Dictionary<int, OneD20RoomData>
        {
            [1] = new OneD20RoomData(1, 0, 0, OneD20ChallengeType.Fight, "hk-room-1-desc"),
            [2] = new OneD20RoomData(2, 0, 1, OneD20ChallengeType.SkillCheck, "hk-room-2-desc"),
            [3] = new OneD20RoomData(3, 0, 2, OneD20ChallengeType.SkillCheck, "hk-room-3-desc"),
            [4] = new OneD20RoomData(4, 1, 0, OneD20ChallengeType.SkillCheck, "hk-room-4-desc"),
            [5] = new OneD20RoomData(5, 1, 1, OneD20ChallengeType.Fight, "hk-room-5-desc"),
            [6] = new OneD20RoomData(6, 1, 2, OneD20ChallengeType.SkillCheck, "hk-room-6-desc"),
            [7] = new OneD20RoomData(7, 2, 0, OneD20ChallengeType.SkillCheck, "hk-room-7-desc"),
            [8] = new OneD20RoomData(8, 2, 1, OneD20ChallengeType.Fight, "hk-room-8-desc"),
            [9] = new OneD20RoomData(9, 2, 2, OneD20ChallengeType.SkillCheck, "hk-room-9-desc"),
        };

        /// <summary>The room a new player starts in — same center-of-grid Fight room as the default.</summary>
        public int StartRoomId { get; } = 5;

        /// <summary>The pool of 9 skill-check challenges. Skill/SkillAbility/DC values are unchanged
        /// from <see cref="DefaultOneD20WorldModule"/>'s pool — only keys and flavor text are this
        /// module's own.</summary>
        public IReadOnlyDictionary<string, OneD20ChallengeTemplate> Challenges { get; } =
            new Dictionary<string, OneD20ChallengeTemplate>
            {
                ["hk-search"] = new("hk-search", "Search", "Int", 10, "hk-search-success", "hk-search-failure", "hk-search-prompt", Kind: OneD20CheckKind.Skill),
                ["hk-climb"] = new("hk-climb", "Climb", "Str", 12, "hk-climb-success", "hk-climb-failure", "hk-climb-prompt", Kind: OneD20CheckKind.Skill),
                ["hk-listen"] = new("hk-listen", "Listen", "Wis", 8, "hk-listen-success", "hk-listen-failure", "hk-listen-prompt", Kind: OneD20CheckKind.Skill),
                ["hk-swim"] = new("hk-swim", "Swim", "Str", 10, "hk-swim-success", "hk-swim-failure", "hk-swim-prompt", Kind: OneD20CheckKind.Skill),
                ["hk-disabledevice"] = new("hk-disabledevice", "Disable Device", "Int", 12, "hk-disabledevice-success", "hk-disabledevice-failure", "hk-disabledevice-prompt", Kind: OneD20CheckKind.Skill),
                ["hk-jump"] = new("hk-jump", "Jump", "Str", 8, "hk-jump-success", "hk-jump-failure", "hk-jump-prompt", Kind: OneD20CheckKind.Skill),
                ["hk-balance"] = new("hk-balance", "Balance", "Dex", 11, "hk-balance-success", "hk-balance-failure", "hk-balance-prompt", Kind: OneD20CheckKind.Skill),
                ["hk-spot"] = new("hk-spot", "Spot", "Wis", 13, "hk-spot-success", "hk-spot-failure", "hk-spot-prompt", Kind: OneD20CheckKind.Skill),
                ["hk-concentration"] = new("hk-concentration", "Concentration", "Con", 12, "hk-concentration-success", "hk-concentration-failure", "hk-concentration-prompt", Kind: OneD20CheckKind.Skill),
                // Save-kind additions (phase-2-10-content-coverage-spec.md), reskinned — same
                // SaveType/DC as DefaultOneD20WorldModule's wire/fumes, own keys and flavor.
                ["hk-trap"] = new("hk-trap", null, null, 10, "hk-trap-success", "hk-trap-failure", "hk-trap-prompt", Kind: OneD20CheckKind.Save, SaveType: "Reflex"),
                ["hk-miasma"] = new("hk-miasma", null, null, 12, "hk-miasma-success", "hk-miasma-failure", "hk-miasma-prompt", Kind: OneD20CheckKind.Save, SaveType: "Fortitude"),
            };

        /// <summary>The roster of 5 monsters. Stat blocks are unchanged, id-for-id, from
        /// <see cref="DefaultOneD20WorldModule"/>'s roster — only names and descriptions differ.</summary>
        public IReadOnlyDictionary<string, OneD20MonsterTemplate> Monsters { get; } =
            new Dictionary<string, OneD20MonsterTemplate>
            {
                ["hk-outrider"] = new("hk-outrider", "The Outrider", "hk-monster-outrider-desc",
                    new OneD20CombatantStats(Defense: 13, AttackBonus: 3, DamageDie: "1d6", DamageBonus: 1, MaxHp: 8)),
                ["hk-brute"] = new("hk-brute", "The Brute", "hk-monster-brute-desc",
                    new OneD20CombatantStats(Defense: 11, AttackBonus: 4, DamageDie: "1d4", DamageBonus: 3, MaxHp: 12)),
                ["hk-longbow"] = new("hk-longbow", "The Longbowman", "hk-monster-longbow-desc",
                    new OneD20CombatantStats(Defense: 12, AttackBonus: 5, DamageDie: "2d6", DamageBonus: 0, MaxHp: 6)),
                ["hk-ironclad"] = new("hk-ironclad", "The Ironclad Sergeant", "hk-monster-ironclad-desc",
                    new OneD20CombatantStats(Defense: 15, AttackBonus: 2, DamageDie: "1d8", DamageBonus: 2, MaxHp: 14)),
                ["hk-footpad"] = new("hk-footpad", "The Footpad", "hk-monster-footpad-desc",
                    new OneD20CombatantStats(Defense: 14, AttackBonus: 2, DamageDie: "1d4", DamageBonus: 1, MaxHp: 6),
                    InflictsPoison: true),
            };

        /// <summary>The static, deterministic monster→item drop table (1:1).</summary>
        public IReadOnlyDictionary<string, string> ItemIdByMonsterId { get; } = new Dictionary<string, string>
        {
            ["hk-outrider"] = "hk-item-blade",
            ["hk-brute"] = "hk-item-knuckledusters",
            ["hk-longbow"] = "hk-item-recurve",
            ["hk-ironclad"] = "hk-item-cuirass",
            ["hk-footpad"] = "hk-item-knife",
        };

        /// <summary>The pregenerated PC's stats — unchanged from the Strong-hero sheet, reflavored as
        /// a wandering blade-for-hire rather than a Department-7 operative.</summary>
        public OneD20CombatantStats PlayerCharacter { get; } =
            new(Defense: 11, AttackBonus: 4, DamageDie: "1d8", DamageBonus: 3, MaxHp: 11);

        /// <summary>The PC's six ability scores — unchanged from the Strong-hero sheet.</summary>
        public IReadOnlyDictionary<string, int> PlayerAbilityScores { get; } = new Dictionary<string, int>
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
