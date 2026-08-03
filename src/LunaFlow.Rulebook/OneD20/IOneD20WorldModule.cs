namespace LunaFlow.Rulebook.OneD20
{
    /// <summary>
    /// The shape contract a World Module implements to supply 1.d20 its concrete content instances —
    /// the 9 rooms' fixed grid positions, the pool of skill-check challenges and monster roster
    /// session content draws from, the monster→item loot table, the pregenerated PC's sheet, ability
    /// scores, and flavor text. The Rulebook owns this contract and the record shapes it returns
    /// (<see cref="OneD20RoomData"/>/<see cref="OneD20ChallengeTemplate"/>/
    /// <see cref="OneD20MonsterTemplate"/>/<see cref="OneD20CombatantStats"/>); a World Module
    /// populates them, never redefines them. See <c>design/world-module.md</c>.
    /// </summary>
    public interface IOneD20WorldModule
    {
        /// <summary>The nine rooms' fixed grid position and slot-kind, keyed by <see cref="OneD20RoomData.Id"/>.</summary>
        IReadOnlyDictionary<int, OneD20RoomData> Rooms { get; }

        /// <summary>The room a new player starts in.</summary>
        int StartRoomId { get; }

        /// <summary>The pool of skill-check challenges session-assembly draws from, keyed by <see cref="OneD20ChallengeTemplate.Key"/>.</summary>
        IReadOnlyDictionary<string, OneD20ChallengeTemplate> Challenges { get; }

        /// <summary>The monster roster session-assembly draws from, keyed by <see cref="OneD20MonsterTemplate.Id"/>.</summary>
        IReadOnlyDictionary<string, OneD20MonsterTemplate> Monsters { get; }

        /// <summary>The static, deterministic monster→item drop table (1:1).</summary>
        IReadOnlyDictionary<string, string> ItemIdByMonsterId { get; }

        /// <summary>The pregenerated PC's combat stats.</summary>
        OneD20CombatantStats PlayerCharacter { get; }

        /// <summary>The PC's six ability scores.</summary>
        IReadOnlyDictionary<string, int> PlayerAbilityScores { get; }

        /// <summary>All flavor prose, keyed by the flavor keys <see cref="Rooms"/>/<see cref="Challenges"/>/<see cref="Monsters"/> reference.</summary>
        IReadOnlyDictionary<string, string> FlavorText { get; }
    }
}
