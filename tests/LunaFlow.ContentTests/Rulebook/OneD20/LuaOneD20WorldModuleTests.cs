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
