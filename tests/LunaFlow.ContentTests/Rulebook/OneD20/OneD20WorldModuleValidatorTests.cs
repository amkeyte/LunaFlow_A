using LunaFlow.Rulebook.OneD20;

namespace LunaFlow.ContentTests.Rulebook.OneD20
{
    /// <summary>
    /// Coverage for <see cref="OneD20WorldModuleValidator"/> — the Phase 2.8 World Module
    /// extraction's load-time validation pass, demonstrated manually against a broken module during
    /// the 0060 build (per its handoff response) but never pinned as an automated regression. Each
    /// fail-fast check is exercised in isolation via <see cref="TestWorldModule"/>, a mutable
    /// <see cref="IOneD20WorldModule"/> seeded from <see cref="DefaultOneD20WorldModule"/>'s real
    /// content with exactly one property broken per test, so a future edit to the validator's check
    /// order or wording shows up as a specific failing test rather than a silent behavior change.
    /// </summary>
    [TestClass]
    [TestCategory("Rom_OneD20")]
    [TestCategory("Phase2.8")]
    public class OneD20WorldModuleValidatorTests
    {
        [TestMethod]
        public void Validate_DefaultOneD20WorldModule_DoesNotThrow()
        {
            OneD20WorldModuleValidator.Validate(new DefaultOneD20WorldModule());
        }

        [TestMethod]
        public void Validate_HollowKeepWorldModule_DoesNotThrow()
        {
            OneD20WorldModuleValidator.Validate(new HollowKeepWorldModule());
        }

        [TestMethod]
        public void Validate_RoomsMissingAFixedGridKey_ThrowsNamingRoomCount()
        {
            var module = TestWorldModule.FromDefault();
            var rooms = new Dictionary<int, OneD20RoomData>(module.Rooms);
            rooms.Remove(9);
            module.Rooms = rooms;

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => OneD20WorldModuleValidator.Validate(module));
            Assert.IsTrue(ex.Message.Contains("Rooms must have exactly the 9 keys"));
        }

        [TestMethod]
        public void Validate_FewerThanSixChallenges_ThrowsNamingChallengePoolFloor()
        {
            var module = TestWorldModule.FromDefault();
            module.Challenges = module.Challenges.Take(5).ToDictionary(kv => kv.Key, kv => kv.Value);

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => OneD20WorldModuleValidator.Validate(module));
            Assert.IsTrue(ex.Message.Contains("Challenges pool must have at least 6"));
        }

        [TestMethod]
        public void Validate_FewerThanThreeMonsters_ThrowsNamingMonsterRosterFloor()
        {
            var module = TestWorldModule.FromDefault();
            module.Monsters = module.Monsters.Take(2).ToDictionary(kv => kv.Key, kv => kv.Value);

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => OneD20WorldModuleValidator.Validate(module));
            Assert.IsTrue(ex.Message.Contains("Monsters roster must have at least 3"));
        }

        [TestMethod]
        public void Validate_MoreSkillCheckRoomsThanChallenges_ThrowsNamingPoolVsSlotFit()
        {
            // Flip a Fight room to SkillCheck: 7 SkillCheck rooms against the unmodified 9-entry
            // Challenges pool would still fit, so trim to exactly 6 (the floor from the check above)
            // to force skillCheckCount(7) > Challenges.Count(6).
            var module = TestWorldModule.FromDefault();
            var rooms = new Dictionary<int, OneD20RoomData>(module.Rooms);
            var flipped = rooms[1] with { ChallengeType = OneD20ChallengeType.SkillCheck };
            rooms[1] = flipped;
            module.Rooms = rooms;
            module.Challenges = module.Challenges.Take(6).ToDictionary(kv => kv.Key, kv => kv.Value);

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => OneD20WorldModuleValidator.Validate(module));
            Assert.IsTrue(ex.Message.Contains("SkillCheck room(s) but only"));
        }

        [TestMethod]
        public void Validate_MoreFightRoomsThanMonsters_ThrowsNamingRosterVsSlotFit()
        {
            // Flip a SkillCheck room to Fight: 4 Fight rooms against the roster trimmed to the
            // 3-entry floor forces fightCount(4) > Monsters.Count(3).
            var module = TestWorldModule.FromDefault();
            var rooms = new Dictionary<int, OneD20RoomData>(module.Rooms);
            var flipped = rooms[2] with { ChallengeType = OneD20ChallengeType.Fight };
            rooms[2] = flipped;
            module.Rooms = rooms;
            module.Monsters = module.Monsters.Take(3).ToDictionary(kv => kv.Key, kv => kv.Value);

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => OneD20WorldModuleValidator.Validate(module));
            Assert.IsTrue(ex.Message.Contains("Fight room(s) but only"));
        }

        [TestMethod]
        public void Validate_RoomDescriptionKeyMissingFromFlavorText_ThrowsNamingTheRoom()
        {
            var module = TestWorldModule.FromDefault();
            var flavorText = new Dictionary<string, string>(module.FlavorText);
            flavorText.Remove("room-9-desc");
            module.FlavorText = flavorText;

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => OneD20WorldModuleValidator.Validate(module));
            Assert.IsTrue(ex.Message.Contains("Room 9's DescriptionKey"));
        }

        [TestMethod]
        public void Validate_ChallengePromptKeyMissingFromFlavorText_ThrowsNamingTheChallenge()
        {
            var module = TestWorldModule.FromDefault();
            var flavorText = new Dictionary<string, string>(module.FlavorText);
            flavorText.Remove("search-prompt");
            module.FlavorText = flavorText;

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => OneD20WorldModuleValidator.Validate(module));
            Assert.IsTrue(ex.Message.Contains("Challenge 'search's PromptKey"));
        }

        [TestMethod]
        public void Validate_MonsterDescriptionKeyMissingFromFlavorText_ThrowsNamingTheMonster()
        {
            var module = TestWorldModule.FromDefault();
            var flavorText = new Dictionary<string, string>(module.FlavorText);
            flavorText.Remove("monster-duelist-desc");
            module.FlavorText = flavorText;

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => OneD20WorldModuleValidator.Validate(module));
            Assert.IsTrue(ex.Message.Contains("Monster 'duelist's DescriptionKey"));
        }

        [TestMethod]
        public void Validate_PlayerAbilityScoresMissingARequiredAbility_ThrowsNamingIt()
        {
            var module = TestWorldModule.FromDefault();
            var scores = new Dictionary<string, int>(module.PlayerAbilityScores);
            scores.Remove("Con");
            module.PlayerAbilityScores = scores;

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => OneD20WorldModuleValidator.Validate(module));
            Assert.IsTrue(ex.Message.Contains("missing required ability 'Con'"));
        }

        [TestMethod]
        public void Validate_FixedVocabularyFlavorKeyMissing_ThrowsNamingIt()
        {
            // Per ticket 0061: these 11 keys are read as hardcoded literals by shared handler code
            // (FightCommandHandler/FleeCommandHandler/InventoryCommandHandler/Program.cs), unlike
            // every other FlavorText key which is always reached through a template's *Key field —
            // so this is the one failure mode that can't be produced via a template/room lookup miss.
            var module = TestWorldModule.FromDefault();
            var flavorText = new Dictionary<string, string>(module.FlavorText);
            flavorText.Remove("win-announce");
            module.FlavorText = flavorText;

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => OneD20WorldModuleValidator.Validate(module));
            Assert.IsTrue(ex.Message.Contains("missing the fixed-vocabulary key 'win-announce'"));
        }

        // ---- Item 11 (phase-2-12-lua-worldmodule-stress-test-spec.md): Enrage pairing/range check ----
        // Mutates one monster ("duelist") in an otherwise-untouched, fully-valid TestWorldModule —
        // deliberately NOT a minimal single-monster module (see
        // example/Phase2_12Spike/Program.cs's SingleMonsterProbeWorldModule doc comment for the
        // mistake that shape caused: under-populated modules trip the roster/pool-size floor checks
        // first, so a "reject" assertion passes for the wrong reason). TestWorldModule.FromDefault()
        // already has 9 rooms, 9 challenges, 5 monsters — every other check stays satisfied.

        [TestMethod]
        public void Validate_MonsterEnrageThresholdSetWithoutDefenseBonus_ThrowsNamingMonsterAndEnrage()
        {
            var module = TestWorldModule.FromDefault();
            var monsters = new Dictionary<string, OneD20MonsterTemplate>(module.Monsters);
            monsters["duelist"] = monsters["duelist"] with { EnrageThresholdFraction = 0.5, EnrageDefenseBonus = null };
            module.Monsters = monsters;

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => OneD20WorldModuleValidator.Validate(module));
            Assert.IsTrue(ex.Message.Contains("Enrage"));
            Assert.IsTrue(ex.Message.Contains("'duelist'"));
        }

        [TestMethod]
        public void Validate_MonsterEnrageDefenseBonusSetWithoutThreshold_ThrowsNamingMonsterAndEnrage()
        {
            var module = TestWorldModule.FromDefault();
            var monsters = new Dictionary<string, OneD20MonsterTemplate>(module.Monsters);
            monsters["duelist"] = monsters["duelist"] with { EnrageThresholdFraction = null, EnrageDefenseBonus = 3 };
            module.Monsters = monsters;

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => OneD20WorldModuleValidator.Validate(module));
            Assert.IsTrue(ex.Message.Contains("Enrage"));
            Assert.IsTrue(ex.Message.Contains("'duelist'"));
        }

        [TestMethod]
        public void Validate_MonsterEnrageThresholdAboveOne_ThrowsNamingMonsterAndEnrage()
        {
            var module = TestWorldModule.FromDefault();
            var monsters = new Dictionary<string, OneD20MonsterTemplate>(module.Monsters);
            monsters["duelist"] = monsters["duelist"] with { EnrageThresholdFraction = 1.5, EnrageDefenseBonus = 3 };
            module.Monsters = monsters;

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => OneD20WorldModuleValidator.Validate(module));
            Assert.IsTrue(ex.Message.Contains("Enrage"));
            Assert.IsTrue(ex.Message.Contains("'duelist'"));
        }

        [TestMethod]
        public void Validate_MonsterEnrageThresholdAtZero_ThrowsNamingMonsterAndEnrage()
        {
            // Exclusive lower bound: (0, 1] rejects exactly 0.
            var module = TestWorldModule.FromDefault();
            var monsters = new Dictionary<string, OneD20MonsterTemplate>(module.Monsters);
            monsters["duelist"] = monsters["duelist"] with { EnrageThresholdFraction = 0.0, EnrageDefenseBonus = 3 };
            module.Monsters = monsters;

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => OneD20WorldModuleValidator.Validate(module));
            Assert.IsTrue(ex.Message.Contains("Enrage"));
            Assert.IsTrue(ex.Message.Contains("'duelist'"));
        }

        [TestMethod]
        public void Validate_MonsterEnrageThresholdAtOne_DoesNotThrow()
        {
            // Inclusive upper bound: (0, 1] accepts exactly 1.
            var module = TestWorldModule.FromDefault();
            var monsters = new Dictionary<string, OneD20MonsterTemplate>(module.Monsters);
            monsters["duelist"] = monsters["duelist"] with { EnrageThresholdFraction = 1.0, EnrageDefenseBonus = 3 };
            module.Monsters = monsters;

            OneD20WorldModuleValidator.Validate(module);
        }

        [TestMethod]
        public void HollowKeepWorldModule_RoomTopology_MatchesDefaultsGridPositionsAndSlotKinds()
        {
            // Ticket 0059/0060: a reskin, not a relayout or rebalance — grid position and
            // Fight/SkillCheck slot assignment must be byte-for-byte identical to the default module,
            // verified manually via diff during the build but not pinned as a regression until now.
            var defaultModule = new DefaultOneD20WorldModule();
            var hollowKeep = new HollowKeepWorldModule();

            foreach (var roomId in defaultModule.Rooms.Keys)
            {
                var expected = defaultModule.Rooms[roomId];
                var actual = hollowKeep.Rooms[roomId];
                Assert.AreEqual(expected.Row, actual.Row, $"Room {roomId}'s Row must match between World Modules");
                Assert.AreEqual(expected.Col, actual.Col, $"Room {roomId}'s Col must match between World Modules");
                Assert.AreEqual(expected.ChallengeType, actual.ChallengeType,
                    $"Room {roomId}'s slot kind (Fight/SkillCheck) must match between World Modules");
            }
            Assert.AreEqual(defaultModule.StartRoomId, hollowKeep.StartRoomId);
        }

        [TestMethod]
        public void HollowKeepWorldModule_ChallengeMechanics_MatchDefaultsSkillAbilityAndDcPerPoolEntry()
        {
            // Same "reskin, not rebalance" bar for the challenge pool — Skill/SkillAbility/DC values
            // unchanged, only Key and flavor-text keys are this module's own (per the class's own doc
            // comment). Zips pool entries by ordinal position since the keys themselves differ
            // ("search" vs "hk-search").
            var defaultChallenges = new DefaultOneD20WorldModule().Challenges.Values.ToList();
            var hollowKeepChallenges = new HollowKeepWorldModule().Challenges.Values.ToList();

            Assert.AreEqual(defaultChallenges.Count, hollowKeepChallenges.Count);
            foreach (var (expected, actual) in defaultChallenges.Zip(hollowKeepChallenges))
            {
                Assert.AreEqual(expected.Skill, actual.Skill);
                Assert.AreEqual(expected.SkillAbility, actual.SkillAbility);
                Assert.AreEqual(expected.DC, actual.DC);
            }
        }

        [TestMethod]
        public void HollowKeepWorldModule_PlayerAbilityScores_SameKeysAndValuesAsDefault()
        {
            var defaultModule = new DefaultOneD20WorldModule();
            var hollowKeep = new HollowKeepWorldModule();

            CollectionAssert.AreEquivalent(
                defaultModule.PlayerAbilityScores.Keys.ToList(), hollowKeep.PlayerAbilityScores.Keys.ToList());
            foreach (var ability in defaultModule.PlayerAbilityScores.Keys)
            {
                Assert.AreEqual(defaultModule.PlayerAbilityScores[ability], hollowKeep.PlayerAbilityScores[ability],
                    $"Ability '{ability}' must be unchanged between World Modules");
            }
        }

        /// <summary>
        /// A settable-property <see cref="IOneD20WorldModule"/> double, seeded from a real
        /// <see cref="DefaultOneD20WorldModule"/>'s content so every test above breaks exactly one
        /// property and leaves the rest exercising the validator's real, currently-shipped data.
        /// </summary>
        private sealed class TestWorldModule : IOneD20WorldModule
        {
            public IReadOnlyDictionary<int, OneD20RoomData> Rooms { get; set; } = null!;
            public int StartRoomId { get; set; }
            public IReadOnlyDictionary<string, OneD20ChallengeTemplate> Challenges { get; set; } = null!;
            public IReadOnlyDictionary<string, OneD20MonsterTemplate> Monsters { get; set; } = null!;
            public IReadOnlyDictionary<string, string> ItemIdByMonsterId { get; set; } = null!;
            public OneD20CombatantStats PlayerCharacter { get; set; } = null!;
            public IReadOnlyDictionary<string, int> PlayerAbilityScores { get; set; } = null!;
            public IReadOnlyDictionary<string, string> FlavorText { get; set; } = null!;

            public static TestWorldModule FromDefault()
            {
                var d = new DefaultOneD20WorldModule();
                return new TestWorldModule
                {
                    Rooms = new Dictionary<int, OneD20RoomData>(d.Rooms),
                    StartRoomId = d.StartRoomId,
                    Challenges = new Dictionary<string, OneD20ChallengeTemplate>(d.Challenges),
                    Monsters = new Dictionary<string, OneD20MonsterTemplate>(d.Monsters),
                    ItemIdByMonsterId = new Dictionary<string, string>(d.ItemIdByMonsterId),
                    PlayerCharacter = d.PlayerCharacter,
                    PlayerAbilityScores = new Dictionary<string, int>(d.PlayerAbilityScores),
                    FlavorText = new Dictionary<string, string>(d.FlavorText),
                };
            }
        }
    }
}
