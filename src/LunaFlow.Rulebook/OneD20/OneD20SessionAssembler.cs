namespace LunaFlow.Rulebook.OneD20
{
    /// <summary>
    /// One session's randomized assignment of pool challenges and roster monsters to the grid's
    /// fixed skill-check/fight positions, plus the loot each fight room derives from its monster
    /// assignment. Room *type*/position (which ids are fight vs. skill-check) never varies — only
    /// which specific challenge/monster fills each slot, per session.
    /// </summary>
    public sealed record OneD20SessionAssignment(
        IReadOnlyDictionary<int, string> ChallengeKeyByRoom,
        IReadOnlyDictionary<int, string> MonsterIdByRoom,
        IReadOnlyDictionary<int, string> ItemIdByRoom);

    /// <summary>
    /// Session-start content assembly: chooses 6 of 9 skill-check challenges and 3 of 5 monsters,
    /// assigns them to the grid's fixed room-type slots, and derives each fight room's loot from its
    /// monster assignment — same session, same source of truth. Pure Rulebook-tier data
    /// transformation (no <c>ICommandHandler</c>, no <c>LunaFlowEngine.Host</c>, no fact-store
    /// access) — <c>Program.cs</c> calls <see cref="Assemble"/> once and passes the result to ROM
    /// constructors, per the authored-content boundary in <c>engine-core.md</c>.
    /// </summary>
    public sealed class OneD20SessionAssembler
    {
        private readonly Random _random;

        /// <summary>Constructs the assembler with an injectable RNG — defaults to a real
        /// <see cref="Random"/>, swappable for a seeded one in tests, same seam as ticket 0019's
        /// <c>D20ResolutionMechanic</c>.</summary>
        public OneD20SessionAssembler(Random? random = null)
        {
            _random = random ?? new Random();
        }

        /// <summary>Produces one session's randomized room→challenge/room→monster/room→item assignment.</summary>
        public OneD20SessionAssignment Assemble()
        {
            var skillCheckRoomIds = OneD20RulebookContent.Rooms.Values
                .Where(r => r.ChallengeType == OneD20ChallengeType.SkillCheck)
                .Select(r => r.Id)
                .ToList();
            var fightRoomIds = OneD20RulebookContent.Rooms.Values
                .Where(r => r.ChallengeType == OneD20ChallengeType.Fight)
                .Select(r => r.Id)
                .ToList();

            var challengeKeys = Shuffle(OneD20RulebookContent.Challenges.Keys.ToList());
            var monsterIds = Shuffle(OneD20RulebookContent.Monsters.Keys.ToList());

            var challengeKeyByRoom = skillCheckRoomIds
                .Zip(challengeKeys.Take(skillCheckRoomIds.Count))
                .ToDictionary(pair => pair.First, pair => pair.Second);

            var monsterIdByRoom = fightRoomIds
                .Zip(monsterIds.Take(fightRoomIds.Count))
                .ToDictionary(pair => pair.First, pair => pair.Second);

            var itemIdByRoom = monsterIdByRoom.ToDictionary(
                kv => kv.Key,
                kv => OneD20RulebookContent.ItemIdByMonsterId[kv.Value]);

            return new OneD20SessionAssignment(challengeKeyByRoom, monsterIdByRoom, itemIdByRoom);
        }

        /// <summary>Fisher-Yates shuffle — uniform random permutation, used to draw N-of-M without replacement.</summary>
        private List<T> Shuffle<T>(List<T> items)
        {
            var result = new List<T>(items);
            for (int i = result.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (result[i], result[j]) = (result[j], result[i]);
            }
            return result;
        }
    }
}
