using LunaFlow.Rulebook.OneD20;

namespace LunaFlow.ContentTests.Rulebook.OneD20
{
    /// <summary>
    /// Maintained coverage for <see cref="LuaOneD20WorldModule"/> per ticket 0066 — malformed-script
    /// and sandbox-escape rejection (both demonstrated ad hoc during the 0065 build but never pinned
    /// as automated regressions), plus a full diff of the shipped demonstration script
    /// (<c>DefaultOneD20WorldModule.lua</c>, loaded via <see cref="LuaOneD20WorldModule.FromEmbeddedResource"/>)
    /// against <see cref="DefaultOneD20WorldModule"/>'s C#-authored content, which it is a verbatim
    /// transcription of. Pure Rulebook-tier construction/marshaling — no engine, so this is plain
    /// unit-level coverage, not a fixture/handler test.
    /// </summary>
    [TestClass]
    [TestCategory("Rom_OneD20")]
    [TestCategory("Phase2.9")]
    public class LuaOneD20WorldModuleTests
    {
        [TestMethod]
        public void Construct_SyntaxError_ThrowsNamingSourceAndFailsToExecute()
        {
            const string source = "return { this is not valid lua";

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => new LuaOneD20WorldModule(source, "syntax-error"));
            Assert.IsTrue(ex.Message.Contains("syntax-error"));
            Assert.IsTrue(ex.Message.Contains("failed to execute"));
        }

        [TestMethod]
        public void Construct_MissingRoomsKey_ThrowsNamingTopLevelAndRooms()
        {
            const string source = "return { startRoomId = 1 }";

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => new LuaOneD20WorldModule(source, "missing-rooms-key"));
            Assert.IsTrue(ex.Message.Contains("missing-rooms-key"));
            Assert.IsTrue(ex.Message.Contains("top level is missing required table field 'rooms'"));
        }

        [TestMethod]
        public void Construct_ChallengeDcAsString_ThrowsNamingChallengeAndField()
        {
            const string source = """
                return {
                  rooms = {},
                  challenges = {
                    search = { skill = "Search", skillAbility = "Int", dc = "ten",
                               successKey = "s", failureKey = "f", promptKey = "p" },
                  },
                  monsters = {},
                  itemIdByMonsterId = {},
                  playerCharacter = {},
                  playerAbilityScores = {},
                  flavorText = {},
                  startRoomId = 1,
                }
                """;

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => new LuaOneD20WorldModule(source, "dc-as-string"));
            Assert.IsTrue(ex.Message.Contains("dc-as-string"));
            Assert.IsTrue(ex.Message.Contains("Challenge 'search' is missing required number field 'dc'"));
        }

        [TestMethod]
        public void Construct_UnrecognizedChallengeType_ThrowsNamingRoomAndValue()
        {
            const string source = """
                return {
                  rooms = {
                    [1] = { row = 0, col = 0, challengeType = "Dance-Off", descriptionKey = "room-1-desc" },
                  },
                  challenges = {},
                  monsters = {},
                  itemIdByMonsterId = {},
                  playerCharacter = {},
                  playerAbilityScores = {},
                  flavorText = {},
                  startRoomId = 1,
                }
                """;

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => new LuaOneD20WorldModule(source, "unrecognized-challengetype"));
            Assert.IsTrue(ex.Message.Contains("unrecognized-challengetype"));
            Assert.IsTrue(ex.Message.Contains("Room 1's challengeType ('Dance-Off') is not a recognized OneD20ChallengeType"));
        }

        [TestMethod]
        public void Construct_ScriptCallsIoOpen_RejectedBecauseIoIsNotSandboxed()
        {
            const string source = """
                io.open("should-not-exist.txt", "w")
                return {}
                """;

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => new LuaOneD20WorldModule(source, "hostile-io-open"));
            Assert.IsTrue(ex.Message.Contains("hostile-io-open"));
            Assert.IsTrue(ex.Message.Contains("failed to execute"),
                "io.open must fail because 'io' is not a global under Preset_HardSandbox, not because of unrelated marshal validation");
        }

        [TestMethod]
        public void Construct_ScriptCallsOsExecute_RejectedBecauseOsIsNotSandboxed()
        {
            const string source = """
                os.execute("dir")
                return {}
                """;

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => new LuaOneD20WorldModule(source, "hostile-os-execute"));
            Assert.IsTrue(ex.Message.Contains("hostile-os-execute"));
            Assert.IsTrue(ex.Message.Contains("failed to execute"),
                "os.execute must fail because 'os' is not a global under Preset_HardSandbox, not because of unrelated marshal validation");
        }

        [TestMethod]
        public void FromEmbeddedResource_DemoScript_PassesOneD20WorldModuleValidatorUnmodified()
        {
            var luaModule = LuaOneD20WorldModule.FromEmbeddedResource();

            OneD20WorldModuleValidator.Validate(luaModule);
        }

        // ---- Phase 2.12 item 9/10/11: "Signal Static" — a second, Lua-only World Module (no C# twin) ----

        [TestMethod]
        public void FromEmbeddedResource_SignalStatic_PassesOneD20WorldModuleValidatorUnmodified()
        {
            var luaModule = LuaOneD20WorldModule.FromEmbeddedResource(
                resourceName: "LunaFlow.Rulebook.OneD20.Content.SignalStaticWorldModule.lua");

            OneD20WorldModuleValidator.Validate(luaModule);
        }

        // ---- Item 11: enrageThresholdFraction/enrageDefenseBonus marshaling ----

        [TestMethod]
        public void Construct_MonsterWithEnrageFieldsSet_MarshalsThresholdAndBonusExactly()
        {
            const string source = """
                return {
                  rooms = {},
                  challenges = {},
                  monsters = {
                    hound = { name = "Hound", descriptionKey = "x",
                               stats = { defense = 12, attackBonus = 2, damageDie = "1d6", damageBonus = 0, maxHp = 8 },
                               enrageThresholdFraction = 0.5, enrageDefenseBonus = 3 },
                  },
                  itemIdByMonsterId = {},
                  playerCharacter = { defense = 10, attackBonus = 0, damageDie = "1d4", damageBonus = 0, maxHp = 10 },
                  playerAbilityScores = {},
                  flavorText = {},
                  startRoomId = 1,
                }
                """;

            var module = new LuaOneD20WorldModule(source, "enrage-fields-set");

            var hound = module.Monsters["hound"];
            Assert.AreEqual(0.5, hound.EnrageThresholdFraction);
            Assert.AreEqual(3, hound.EnrageDefenseBonus);
        }

        [TestMethod]
        public void Construct_MonsterWithoutEnrageFields_MarshalsBothFieldsAsNull()
        {
            // Scripts predating phase-2-12-lua-worldmodule-stress-test-spec.md (including
            // DefaultOneD20WorldModule.lua) omit both fields entirely and must keep marshaling
            // unchanged — the fields default to null, not zero or some other silent substitution.
            const string source = """
                return {
                  rooms = {},
                  challenges = {},
                  monsters = {
                    hound = { name = "Hound", descriptionKey = "x",
                               stats = { defense = 12, attackBonus = 2, damageDie = "1d6", damageBonus = 0, maxHp = 8 } },
                  },
                  itemIdByMonsterId = {},
                  playerCharacter = { defense = 10, attackBonus = 0, damageDie = "1d4", damageBonus = 0, maxHp = 10 },
                  playerAbilityScores = {},
                  flavorText = {},
                  startRoomId = 1,
                }
                """;

            var module = new LuaOneD20WorldModule(source, "enrage-fields-absent");

            var hound = module.Monsters["hound"];
            Assert.IsNull(hound.EnrageThresholdFraction);
            Assert.IsNull(hound.EnrageDefenseBonus);
        }

        // ---- Item 12: the two Lua-path failure probes ----

        [TestMethod]
        public void Construct_ChallengeDcAsString_ThrowsBeforeValidatorIsEverReached()
        {
            // Marshal-failure probe: LuaOneD20WorldModule's constructor must throw before
            // OneD20WorldModuleValidator.Validate is ever reachable — proven by a flag that can only
            // ever be set on the far side of a successful construction, same technique
            // example/Phase2_12Spike/Program.cs's RunItem12Probes uses.
            const string source = """
                return {
                  rooms = {
                    [1] = { row = 0, col = 0, challengeType = "SkillCheck", descriptionKey = "x" },
                  },
                  challenges = {
                    broken = { skill = "Search", skillAbility = "Int", dc = "ten",
                               successKey = "s", failureKey = "f", promptKey = "p" },
                  },
                  monsters = {},
                  itemIdByMonsterId = {},
                  playerCharacter = { defense = 10, attackBonus = 0, damageDie = "1d4", damageBonus = 0, maxHp = 10 },
                  playerAbilityScores = { Str = 10, Dex = 10, Con = 10, Int = 10, Wis = 10, Cha = 10 },
                  flavorText = {},
                  startRoomId = 1,
                }
                """;
            var validatorReached = false;

            var ex = Assert.ThrowsException<InvalidOperationException>(() =>
            {
                var probe = new LuaOneD20WorldModule(source, "item-12-marshal-probe");
                OneD20WorldModuleValidator.Validate(probe);
                validatorReached = true;
            });

            Assert.IsTrue(ex.Message.Contains("item-12-marshal-probe"), "Message must name the script");
            Assert.IsTrue(ex.Message.Contains("'dc'"), "Message must name the offending field");
            Assert.IsTrue(ex.Message.Contains("'broken'"), "Message must name the offending entry");
            Assert.IsFalse(validatorReached, "OneD20WorldModuleValidator must never be reached — the constructor must throw first");
        }

        [TestMethod]
        public void Construct_FourChallengesAgainstSixSlotFloor_ConstructsCleanly_ThenRejectedByValidator()
        {
            // Validator-failure probe: marshal-clean (every field the right type, 9 valid rooms) but
            // only 4 challenges against the grid's 6-slot requirement. Must construct successfully,
            // then fail at OneD20WorldModuleValidator's existing pool-size check specifically — proves
            // the two-layer fail-fast split (marshal-shape here, content-validity in the validator)
            // for the failure mode that lands in the second layer, complementing the marshal-failure
            // probe above which lands in the first.
            var roomsBlock = string.Join(",\n", Enumerable.Range(1, 9)
                .Select(i => $"[{i}] = {{ row = 0, col = 0, challengeType = \"SkillCheck\", descriptionKey = \"x\" }}"));
            var source = $$"""
                return {
                  rooms = {
                    {{roomsBlock}}
                  },
                  challenges = {
                    c1 = { skill = "Search", skillAbility = "Int", dc = 8, successKey = "s", failureKey = "f", promptKey = "p" },
                    c2 = { skill = "Search", skillAbility = "Int", dc = 8, successKey = "s", failureKey = "f", promptKey = "p" },
                    c3 = { skill = "Search", skillAbility = "Int", dc = 8, successKey = "s", failureKey = "f", promptKey = "p" },
                    c4 = { skill = "Search", skillAbility = "Int", dc = 8, successKey = "s", failureKey = "f", promptKey = "p" },
                  },
                  monsters = {},
                  itemIdByMonsterId = {},
                  playerCharacter = { defense = 10, attackBonus = 0, damageDie = "1d4", damageBonus = 0, maxHp = 10 },
                  playerAbilityScores = { Str = 10, Dex = 10, Con = 10, Int = 10, Wis = 10, Cha = 10 },
                  flavorText = {},
                  startRoomId = 1,
                }
                """;

            var probe = new LuaOneD20WorldModule(source, "item-12-validator-probe"); // must not throw

            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => OneD20WorldModuleValidator.Validate(probe));
            Assert.IsTrue(ex.Message.Contains("Challenges"));
            Assert.IsTrue(ex.Message.Contains("4"));
        }

        [TestMethod]
        public void FromEmbeddedResource_DemoScript_StartRoomIdMatchesDefault()
        {
            var luaModule = LuaOneD20WorldModule.FromEmbeddedResource();
            var defaultModule = new DefaultOneD20WorldModule();

            Assert.AreEqual(defaultModule.StartRoomId, luaModule.StartRoomId);
        }

        [TestMethod]
        public void FromEmbeddedResource_DemoScript_RoomsMatchDefaultExactly()
        {
            var luaModule = LuaOneD20WorldModule.FromEmbeddedResource();
            var defaultModule = new DefaultOneD20WorldModule();

            CollectionAssert.AreEquivalent(defaultModule.Rooms.Keys.ToList(), luaModule.Rooms.Keys.ToList());
            foreach (var id in defaultModule.Rooms.Keys)
                Assert.AreEqual(defaultModule.Rooms[id], luaModule.Rooms[id],
                    $"Room {id} must match exactly between the Lua and C#-authored World Modules");
        }

        [TestMethod]
        public void FromEmbeddedResource_DemoScript_ChallengesMatchDefaultExactly()
        {
            var luaModule = LuaOneD20WorldModule.FromEmbeddedResource();
            var defaultModule = new DefaultOneD20WorldModule();

            CollectionAssert.AreEquivalent(defaultModule.Challenges.Keys.ToList(), luaModule.Challenges.Keys.ToList());
            foreach (var key in defaultModule.Challenges.Keys)
                Assert.AreEqual(defaultModule.Challenges[key], luaModule.Challenges[key],
                    $"Challenge '{key}' must match exactly between the Lua and C#-authored World Modules");
        }

        [TestMethod]
        public void FromEmbeddedResource_DemoScript_MonstersMatchDefaultExactly()
        {
            var luaModule = LuaOneD20WorldModule.FromEmbeddedResource();
            var defaultModule = new DefaultOneD20WorldModule();

            CollectionAssert.AreEquivalent(defaultModule.Monsters.Keys.ToList(), luaModule.Monsters.Keys.ToList());
            foreach (var id in defaultModule.Monsters.Keys)
                Assert.AreEqual(defaultModule.Monsters[id], luaModule.Monsters[id],
                    $"Monster '{id}' must match exactly between the Lua and C#-authored World Modules");
        }

        [TestMethod]
        public void FromEmbeddedResource_DemoScript_ItemIdByMonsterIdMatchesDefaultExactly()
        {
            var luaModule = LuaOneD20WorldModule.FromEmbeddedResource();
            var defaultModule = new DefaultOneD20WorldModule();

            CollectionAssert.AreEquivalent(
                defaultModule.ItemIdByMonsterId.Keys.ToList(), luaModule.ItemIdByMonsterId.Keys.ToList());
            foreach (var monsterId in defaultModule.ItemIdByMonsterId.Keys)
                Assert.AreEqual(defaultModule.ItemIdByMonsterId[monsterId], luaModule.ItemIdByMonsterId[monsterId],
                    $"Loot drop for '{monsterId}' must match exactly between the Lua and C#-authored World Modules");
        }

        [TestMethod]
        public void FromEmbeddedResource_DemoScript_PlayerCharacterMatchesDefaultExactly()
        {
            var luaModule = LuaOneD20WorldModule.FromEmbeddedResource();
            var defaultModule = new DefaultOneD20WorldModule();

            Assert.AreEqual(defaultModule.PlayerCharacter, luaModule.PlayerCharacter);
        }

        [TestMethod]
        public void FromEmbeddedResource_DemoScript_PlayerAbilityScoresMatchDefaultExactly()
        {
            var luaModule = LuaOneD20WorldModule.FromEmbeddedResource();
            var defaultModule = new DefaultOneD20WorldModule();

            CollectionAssert.AreEquivalent(
                defaultModule.PlayerAbilityScores.Keys.ToList(), luaModule.PlayerAbilityScores.Keys.ToList());
            foreach (var ability in defaultModule.PlayerAbilityScores.Keys)
                Assert.AreEqual(defaultModule.PlayerAbilityScores[ability], luaModule.PlayerAbilityScores[ability],
                    $"Ability '{ability}' must match exactly between the Lua and C#-authored World Modules");
        }

        [TestMethod]
        public void FromEmbeddedResource_DemoScript_FlavorTextMatchesDefaultExactly()
        {
            var luaModule = LuaOneD20WorldModule.FromEmbeddedResource();
            var defaultModule = new DefaultOneD20WorldModule();

            CollectionAssert.AreEquivalent(
                defaultModule.FlavorText.Keys.ToList(), luaModule.FlavorText.Keys.ToList());
            foreach (var key in defaultModule.FlavorText.Keys)
                Assert.AreEqual(defaultModule.FlavorText[key], luaModule.FlavorText[key],
                    $"Flavor text '{key}' must match byte-for-byte between the Lua and C#-authored World Modules");
        }
    }
}
