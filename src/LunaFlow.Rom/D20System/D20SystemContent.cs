namespace LunaFlow.Rom.D20System
{
    /// <summary>
    /// The d20 System's runtime registration point for Scenario-supplied data: room definitions,
    /// combatant stats, ability scores, and flavor text. Populated once by the composition root
    /// (<c>Program.cs</c>) via <see cref="Configure"/> before rules compile/fire, per Phase 2.6's
    /// "direct system access" split — Rulebook content never touches <c>LunaFlow.Engine</c>, so it
    /// can't itself call <c>RuleExport.Compile</c> or wire handlers; it only ever hands data across
    /// this boundary.
    /// </summary>
    /// <remarks>
    /// Exists specifically because the declarative rule group's generated code is compiled and
    /// instantiated by NRules itself (via <see cref="LunaFlow.Rules.GeneratorServices.RuleExport"/>),
    /// not by application code — it can't take constructor arguments the way a hand-written
    /// <c>ICommandHandler</c> can. Command handlers also fall back to this registry when their own
    /// optional constructor parameters aren't supplied, mirroring the existing
    /// <c>D20ResolutionMechanic? mechanic = null</c> seam from ticket 0019.
    /// </remarks>
    public static class D20SystemContent
    {
        /// <summary>The nine rooms, keyed by <see cref="Room.Id"/>, as configured by the current Rulebook.</summary>
        public static IReadOnlyDictionary<int, Room> Rooms { get; private set; } = new Dictionary<int, Room>();

        /// <summary>Flavor text, keyed by the Rulebook's own scheme (e.g. <c>1-d20-flavor.md</c>'s keys).</summary>
        public static IReadOnlyDictionary<string, string> FlavorText { get; private set; } = new Dictionary<string, string>();

        /// <summary>The pregenerated PC, as configured by the current Rulebook.</summary>
        public static PlayerCharacter PlayerCharacter { get; private set; } = new(new CombatantStats(0, 0, "1d1", 0, 1));

        /// <summary>The monster template, as configured by the current Rulebook.</summary>
        public static MonsterTemplate Monster { get; private set; } = new(new CombatantStats(0, 0, "1d1", 0, 1));

        /// <summary>The PC's six ability scores, as configured by the current Rulebook.</summary>
        public static IReadOnlyDictionary<string, int> PlayerAbilityScores { get; private set; } = new Dictionary<string, int>();

        /// <summary>The room a new player starts in, as configured by the current Rulebook.</summary>
        public static int StartRoomId { get; private set; }

        /// <summary>The shared resolution Mechanic every <c>check</c>/<c>fight</c> roll goes through.</summary>
        public static D20ResolutionMechanic Mechanic { get; private set; } = new();

        /// <summary>The d20 System's ability modifier formula: <c>(score / 2) - 5</c>, rounded down.</summary>
        public static int AbilityModifier(int score) => (int)Math.Floor(score / 2.0) - 5;

        /// <summary>
        /// Resolves moving from <paramref name="fromId"/> in <paramref name="direction"/> ("north"/"south"/"east"/"west").
        /// Pure grid adjacency math — identical shape to Nine Rooms' grid, not Scenario-specific.
        /// </summary>
        public static bool TryMove(int fromId, string direction, out int toId)
        {
            switch (direction.ToLowerInvariant())
            {
                case "north":
                    toId = fromId - 3;
                    return toId >= 1;
                case "south":
                    toId = fromId + 3;
                    return toId <= 9;
                case "east":
                    toId = fromId + 1;
                    return (fromId - 1) % 3 != 2;
                case "west":
                    toId = fromId - 1;
                    return (fromId - 1) % 3 != 0;
                default:
                    toId = -1;
                    return false;
            }
        }

        /// <summary>
        /// Registers the current Rulebook's data. Called once by the composition root
        /// (<c>Program.cs</c>) before <c>RuleExport.Compile</c> and before handlers are registered.
        /// </summary>
        public static void Configure(
            IReadOnlyDictionary<int, Room> rooms,
            IReadOnlyDictionary<string, string> flavorText,
            PlayerCharacter playerCharacter,
            MonsterTemplate monster,
            IReadOnlyDictionary<string, int> playerAbilityScores,
            int startRoomId,
            D20ResolutionMechanic? mechanic = null)
        {
            Rooms = rooms;
            FlavorText = flavorText;
            PlayerCharacter = playerCharacter;
            Monster = monster;
            PlayerAbilityScores = playerAbilityScores;
            StartRoomId = startRoomId;
            Mechanic = mechanic ?? new D20ResolutionMechanic();
        }
    }
}
