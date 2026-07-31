using LunaFlow.Rulebook.OneD20;

namespace LunaFlow.EngineTests.Rulebook.OneD20
{
    /// <summary>
    /// Coverage for <see cref="OneD20SessionAssembler"/>: the session-start content-assembly step
    /// that draws 6-of-9 skill-check challenges and 3-of-5 roster monsters and assigns them to the
    /// grid's fixed room-type slots. Pure Rulebook-tier data transformation — no engine, no facts,
    /// no <c>ICommandHandler</c> — so this is plain unit-level coverage, not a fixture/handler test.
    /// </summary>
    [TestClass]
    [TestCategory("Rom_OneD20")]
    [TestCategory("Phase2.7.1")]
    public class OneD20SessionAssemblerTests
    {
        private static readonly int[] FightRoomIds = { 1, 5, 8 };
        private static readonly int[] SkillCheckRoomIds = { 2, 3, 4, 6, 7, 9 };

        [TestMethod]
        public void Assemble_SameSeed_ProducesReproducibleAssignment()
        {
            var a = new OneD20SessionAssembler(new Random(42)).Assemble();
            var b = new OneD20SessionAssembler(new Random(42)).Assemble();

            foreach (var roomId in SkillCheckRoomIds)
                Assert.AreEqual(a.ChallengeKeyByRoom[roomId], b.ChallengeKeyByRoom[roomId],
                    $"Room {roomId}'s challenge assignment must be reproducible for the same seed");

            foreach (var roomId in FightRoomIds)
            {
                Assert.AreEqual(a.MonsterIdByRoom[roomId], b.MonsterIdByRoom[roomId],
                    $"Room {roomId}'s monster assignment must be reproducible for the same seed");
                Assert.AreEqual(a.ItemIdByRoom[roomId], b.ItemIdByRoom[roomId],
                    $"Room {roomId}'s derived loot must be reproducible for the same seed");
            }
        }

        [TestMethod]
        public void Assemble_AssignsExactlySixOfNineChallengeKeys_AllValidAndUnique()
        {
            var assignment = new OneD20SessionAssembler(new Random(1)).Assemble();

            Assert.AreEqual(6, assignment.ChallengeKeyByRoom.Count);
            Assert.AreEqual(6, assignment.ChallengeKeyByRoom.Values.Distinct().Count(),
                "No pool challenge should be assigned to more than one room");
            Assert.IsTrue(assignment.ChallengeKeyByRoom.Values.All(k => OneD20RulebookContent.Challenges.ContainsKey(k)),
                "Every assigned key must be a real entry in the 9-challenge pool");
        }

        [TestMethod]
        public void Assemble_AssignsExactlyThreeOfFiveMonsterIds_AllValidAndUnique()
        {
            var assignment = new OneD20SessionAssembler(new Random(1)).Assemble();

            Assert.AreEqual(3, assignment.MonsterIdByRoom.Count);
            Assert.AreEqual(3, assignment.MonsterIdByRoom.Values.Distinct().Count(),
                "No roster monster should be assigned to more than one room");
            Assert.IsTrue(assignment.MonsterIdByRoom.Values.All(id => OneD20RulebookContent.Monsters.ContainsKey(id)),
                "Every assigned id must be a real entry in the 5-monster roster");
        }

        [TestMethod]
        public void Assemble_NeverAssignsOutsideFixedGridTopology_AcrossManySeeds()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                var assignment = new OneD20SessionAssembler(new Random(seed)).Assemble();

                CollectionAssert.AreEquivalent(SkillCheckRoomIds, assignment.ChallengeKeyByRoom.Keys.ToList(),
                    $"Seed {seed}: challenge assignment must land only on the fixed skill-check positions");
                CollectionAssert.AreEquivalent(FightRoomIds, assignment.MonsterIdByRoom.Keys.ToList(),
                    $"Seed {seed}: monster assignment must land only on the fixed fight positions");
            }
        }

        [TestMethod]
        public void Assemble_ItemIdByRoom_DerivedFromEachRoomsAssignedMonstersLootTableEntry()
        {
            var assignment = new OneD20SessionAssembler(new Random(7)).Assemble();

            Assert.AreEqual(assignment.MonsterIdByRoom.Count, assignment.ItemIdByRoom.Count);
            foreach (var (roomId, monsterId) in assignment.MonsterIdByRoom)
            {
                Assert.AreEqual(OneD20RulebookContent.ItemIdByMonsterId[monsterId], assignment.ItemIdByRoom[roomId],
                    $"Room {roomId}'s loot must match its assigned monster's drop-table entry");
            }
        }

        [TestMethod]
        public void Assemble_DefaultConstructor_StillProducesTheCorrectShape()
        {
            var assignment = new OneD20SessionAssembler().Assemble();

            Assert.AreEqual(6, assignment.ChallengeKeyByRoom.Count);
            Assert.AreEqual(3, assignment.MonsterIdByRoom.Count);
            Assert.AreEqual(3, assignment.ItemIdByRoom.Count);
        }
    }
}
