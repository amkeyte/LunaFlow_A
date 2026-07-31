namespace LunaFlow.Rulebook.OneD20
{
    /// <summary>
    /// The 1.d20 Department-7 mission's Scenario data: the 9 rooms' fixed grid positions, the pool
    /// of 9 skill-check challenges and roster of 5 monsters session content draws from, the
    /// monster→item loot table, the pregenerated PC's sheet, and ability scores — Rulebook-owned
    /// data types only, never a ROM type. Which specific challenge/monster fills a given room is
    /// session-assigned by <see cref="OneD20SessionAssembler"/>, not fixed here. Translated into
    /// GridMap's/D20System's/Loot's own fact shapes by <c>Program.cs</c> (the composition root,
    /// exempt from the Rulebook/ROM boundary). Pure data; no logic, no fact-store access.
    /// </summary>
    public static class OneD20RulebookContent
    {
        /// <summary>The nine rooms' fixed grid position and slot-kind, keyed by <see cref="OneD20RoomData.Id"/>.
        /// Rooms 1, 5, 8 are Fight; the rest are SkillCheck — this topology never varies per session.</summary>
        public static readonly IReadOnlyDictionary<int, OneD20RoomData> Rooms = new Dictionary<int, OneD20RoomData>
        {
            [1] = new OneD20RoomData(1, 0, 0, OneD20ChallengeType.Fight, "room-1-desc"),
            [2] = new OneD20RoomData(2, 0, 1, OneD20ChallengeType.SkillCheck, "room-2-desc"),
            [3] = new OneD20RoomData(3, 0, 2, OneD20ChallengeType.SkillCheck, "room-3-desc"),
            [4] = new OneD20RoomData(4, 1, 0, OneD20ChallengeType.SkillCheck, "room-4-desc"),
            [5] = new OneD20RoomData(5, 1, 1, OneD20ChallengeType.Fight, "room-5-desc"),
            [6] = new OneD20RoomData(6, 1, 2, OneD20ChallengeType.SkillCheck, "room-6-desc"),
            [7] = new OneD20RoomData(7, 2, 0, OneD20ChallengeType.SkillCheck, "room-7-desc"),
            [8] = new OneD20RoomData(8, 2, 1, OneD20ChallengeType.Fight, "room-8-desc"),
            [9] = new OneD20RoomData(9, 2, 2, OneD20ChallengeType.SkillCheck, "room-9-desc"),
        };

        /// <summary>The room a new player starts in (the center of the grid — also a Fight room).</summary>
        public const int StartRoomId = 5;

        /// <summary>
        /// The pool of 9 skill-check challenges, keyed by <see cref="OneD20ChallengeTemplate.Key"/>.
        /// Session-assembly assigns 6 of these to the grid's 6 fixed skill-check positions.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, OneD20ChallengeTemplate> Challenges =
            new Dictionary<string, OneD20ChallengeTemplate>
            {
                ["search"] = new("search", "Search", "Int", 10, "search-success", "search-failure", "search-prompt"),
                ["climb"] = new("climb", "Climb", "Str", 12, "climb-success", "climb-failure", "climb-prompt"),
                ["listen"] = new("listen", "Listen", "Wis", 8, "listen-success", "listen-failure", "listen-prompt"),
                ["swim"] = new("swim", "Swim", "Str", 10, "swim-success", "swim-failure", "swim-prompt"),
                ["disabledevice"] = new("disabledevice", "Disable Device", "Int", 12, "disabledevice-success", "disabledevice-failure", "disabledevice-prompt"),
                ["jump"] = new("jump", "Jump", "Str", 8, "jump-success", "jump-failure", "jump-prompt"),
                ["balance"] = new("balance", "Balance", "Dex", 11, "balance-success", "balance-failure", "balance-prompt"),
                ["spot"] = new("spot", "Spot", "Wis", 13, "spot-success", "spot-failure", "spot-prompt"),
                ["concentration"] = new("concentration", "Concentration", "Con", 12, "concentration-success", "concentration-failure", "concentration-prompt"),
            };

        /// <summary>
        /// The roster of 5 monsters, keyed by <see cref="OneD20MonsterTemplate.Id"/>. Session-assembly
        /// assigns 3 of these to the grid's 3 fixed fight positions. Stats per
        /// loot-rom-content-expansion-spec.md's roster table, all checked against the PC (attack +4,
        /// Defense 11) to land in a plausible-either-way band.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, OneD20MonsterTemplate> Monsters =
            new Dictionary<string, OneD20MonsterTemplate>
            {
                ["duelist"] = new("duelist", "The Duelist", "monster-duelist-desc",
                    new OneD20CombatantStats(Defense: 13, AttackBonus: 3, DamageDie: "1d6", DamageBonus: 1, MaxHp: 8)),
                ["brawler"] = new("brawler", "The Brawler", "monster-brawler-desc",
                    new OneD20CombatantStats(Defense: 11, AttackBonus: 4, DamageDie: "1d4", DamageBonus: 3, MaxHp: 12)),
                ["marksman"] = new("marksman", "The Marksman", "monster-marksman-desc",
                    new OneD20CombatantStats(Defense: 12, AttackBonus: 5, DamageDie: "2d6", DamageBonus: 0, MaxHp: 6)),
                ["enforcer"] = new("enforcer", "The Enforcer", "monster-enforcer-desc",
                    new OneD20CombatantStats(Defense: 15, AttackBonus: 2, DamageDie: "1d8", DamageBonus: 2, MaxHp: 14)),
                ["scrapper"] = new("scrapper", "The Scrapper", "monster-scrapper-desc",
                    new OneD20CombatantStats(Defense: 14, AttackBonus: 2, DamageDie: "1d4", DamageBonus: 1, MaxHp: 6)),
            };

        /// <summary>
        /// The static, deterministic monster→item drop table (1:1) — same monster always drops the
        /// same item, per loot-rom-content-expansion-spec.md. Translated into Loot's
        /// <c>MonsterLootTable</c> by <c>Program.cs</c>.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> ItemIdByMonsterId = new Dictionary<string, string>
        {
            ["duelist"] = "rapier",
            ["brawler"] = "knuckles",
            ["marksman"] = "pistol",
            ["enforcer"] = "vest",
            ["scrapper"] = "knife",
        };

        /// <summary>The pregenerated level-1 Strong-hero PC's stats.</summary>
        public static readonly OneD20CombatantStats PlayerCharacter =
            new(Defense: 11, AttackBonus: 4, DamageDie: "1d8", DamageBonus: 3, MaxHp: 11);

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
