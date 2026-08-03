namespace LunaFlow.Rulebook.OneD20
{
    /// <summary>
    /// Load-time validation for an <see cref="IOneD20WorldModule"/>, per
    /// <c>world-module-extraction-spec.md</c>'s Mechanics section. <c>Program.cs</c> calls
    /// <see cref="Validate"/> once, immediately after constructing a World Module and before anything
    /// else touches it. Every failure is a hard <see cref="InvalidOperationException"/>, fail-fast on
    /// the first violated check — same "hard fail at bootstrap, never a degradation during play"
    /// property <c>ComponentRegistry</c> holds one tier over.
    /// </summary>
    public static class OneD20WorldModuleValidator
    {
        private static readonly int[] ExpectedRoomIds = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        private static readonly string[] RequiredAbilities = { "Str", "Dex", "Con", "Int", "Wis", "Cha" };

        /// <summary>
        /// The eleven fixed-vocabulary <c>FlavorText</c> keys every command handler reads as a
        /// hardcoded literal rather than through a template's <c>*Key</c> field
        /// (<c>FightCommandHandler</c>/<c>FleeCommandHandler</c>/<c>InventoryCommandHandler</c>/
        /// <c>Program.cs</c>). These must appear verbatim, unprefixed, in every
        /// <see cref="IOneD20WorldModule"/> implementation — a module that namespaces one of them
        /// (e.g. under its own theme prefix, correct for a room/challenge/monster/item key but not for
        /// these) passes every other check and then throws <see cref="KeyNotFoundException"/> mid-
        /// session instead of failing at load. See ticket 0061.
        /// </summary>
        private static readonly string[] FixedVocabularyFlavorKeys =
        {
            "pc-hit", "pc-miss", "monster-hit", "monster-miss", "monster-defeated", "monster-fled",
            "pc-flee", "pc-lost", "win-announce", "inventory-empty", "inventory-header",
        };

        /// <summary>Validates <paramref name="module"/> against every check in the spec's Mechanics
        /// section, throwing on the first violation with a message naming what's wrong.</summary>
        public static void Validate(IOneD20WorldModule module)
        {
            if (module.Rooms.Count != ExpectedRoomIds.Length || !ExpectedRoomIds.All(module.Rooms.ContainsKey))
            {
                throw new InvalidOperationException(
                    $"World Module invalid: Rooms must have exactly the 9 keys 1..9 GridMap's fixed grid expects; " +
                    $"got {module.Rooms.Count} room(s) ({string.Join(",", module.Rooms.Keys.OrderBy(k => k))}).");
            }

            if (module.Challenges.Count < 6)
            {
                throw new InvalidOperationException(
                    $"World Module invalid: Challenges pool must have at least 6 entries to fill the grid's " +
                    $"fixed skill-check positions; got {module.Challenges.Count}.");
            }
            if (module.Monsters.Count < 3)
            {
                throw new InvalidOperationException(
                    $"World Module invalid: Monsters roster must have at least 3 entries to fill the grid's " +
                    $"fixed fight positions; got {module.Monsters.Count}.");
            }

            var skillCheckCount = module.Rooms.Values.Count(r => r.ChallengeType == OneD20ChallengeType.SkillCheck);
            var fightCount = module.Rooms.Values.Count(r => r.ChallengeType == OneD20ChallengeType.Fight);
            if (skillCheckCount > module.Challenges.Count)
            {
                throw new InvalidOperationException(
                    $"World Module invalid: {skillCheckCount} SkillCheck room(s) but only " +
                    $"{module.Challenges.Count} Challenges to draw from.");
            }
            if (fightCount > module.Monsters.Count)
            {
                throw new InvalidOperationException(
                    $"World Module invalid: {fightCount} Fight room(s) but only {module.Monsters.Count} " +
                    $"Monsters to draw from.");
            }

            foreach (var room in module.Rooms.Values)
            {
                RequireFlavorKey(module, room.DescriptionKey, $"Room {room.Id}'s DescriptionKey");
            }
            foreach (var challenge in module.Challenges.Values)
            {
                RequireFlavorKey(module, challenge.PromptKey, $"Challenge '{challenge.Key}'s PromptKey");
                RequireFlavorKey(module, challenge.SuccessKey, $"Challenge '{challenge.Key}'s SuccessKey");
                RequireFlavorKey(module, challenge.FailureKey, $"Challenge '{challenge.Key}'s FailureKey");
            }
            foreach (var monster in module.Monsters.Values)
            {
                RequireFlavorKey(module, monster.DescriptionKey, $"Monster '{monster.Id}'s DescriptionKey");
            }

            foreach (var ability in RequiredAbilities)
            {
                if (!module.PlayerAbilityScores.ContainsKey(ability))
                {
                    throw new InvalidOperationException(
                        $"World Module invalid: PlayerAbilityScores is missing required ability '{ability}'.");
                }
            }

            foreach (var key in FixedVocabularyFlavorKeys)
            {
                if (!module.FlavorText.ContainsKey(key))
                {
                    throw new InvalidOperationException(
                        $"World Module invalid: FlavorText is missing the fixed-vocabulary key '{key}' " +
                        $"(shared handler code reads this key literally, unprefixed, in every World Module).");
                }
            }
        }

        private static void RequireFlavorKey(IOneD20WorldModule module, string key, string what)
        {
            if (!module.FlavorText.ContainsKey(key))
            {
                throw new InvalidOperationException(
                    $"World Module invalid: {what} ('{key}') has no matching entry in FlavorText.");
            }
        }
    }
}
