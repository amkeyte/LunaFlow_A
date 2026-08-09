using System.Reflection;
using LunaFlow.Scripting;
using MoonSharp.Interpreter;

namespace LunaFlow.Rulebook.OneD20
{
    /// <summary>
    /// A second, independently-authored <see cref="IOneD20WorldModule"/> — populated by executing a Lua
    /// chunk (via <see cref="ISandboxedScriptHost"/>) and marshaling its single top-level
    /// <c>return {...}</c> table into the exact same shape <see cref="DefaultOneD20WorldModule"/>/
    /// <see cref="HollowKeepWorldModule"/> populate in C#. Executes once, at construction (no hot-reload,
    /// same lifecycle as the C# implementations) — every property is a materialized, already-built
    /// dictionary by the time the constructor returns; nothing re-executes the Lua chunk on later
    /// property access. Two-layer fail-fast per <c>lua-world-module-spec.md</c>: this class's own
    /// marshal-shape checks run first, throwing <see cref="InvalidOperationException"/> naming the
    /// script (<c>sourceName</c>) and the specific field at fault; <see cref="OneD20WorldModuleValidator"/>
    /// (unchanged) then runs its own content-validity checks on top, exactly as it does for any other
    /// <see cref="IOneD20WorldModule"/>.
    /// </summary>
    public sealed class LuaOneD20WorldModule : IOneD20WorldModule
    {
        /// <inheritdoc/>
        public IReadOnlyDictionary<int, OneD20RoomData> Rooms { get; }

        /// <inheritdoc/>
        public int StartRoomId { get; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, OneD20ChallengeTemplate> Challenges { get; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, OneD20MonsterTemplate> Monsters { get; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> ItemIdByMonsterId { get; }

        /// <inheritdoc/>
        public OneD20CombatantStats PlayerCharacter { get; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, int> PlayerAbilityScores { get; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> FlavorText { get; }

        /// <summary>
        /// Executes <paramref name="luaSource"/> through <paramref name="host"/> (a fresh
        /// <see cref="SandboxedScriptHost"/> if omitted) and marshals its returned table into this
        /// module's properties, synchronously, before returning. <paramref name="sourceName"/> — a file
        /// path, embedded-resource name, or any other human-readable label for where this script came
        /// from — is threaded into every exception this constructor throws, so a failure names *which*
        /// script broke, not just what broke; it has no effect on marshaling itself.
        /// </summary>
        public LuaOneD20WorldModule(
            string luaSource,
            string sourceName = "<inline script>",
            ISandboxedScriptHost? host = null)
        {
            host ??= new SandboxedScriptHost();

            DynValue result;
            try
            {
                result = host.Execute(luaSource);
            }
            catch (InterpreterException ex)
            {
                throw new InvalidOperationException(
                    $"Lua World Module script '{sourceName}' failed to execute: {ex.Message}", ex);
            }

            if (result.Type != DataType.Table)
            {
                throw new InvalidOperationException(
                    $"Lua World Module script '{sourceName}' invalid: top-level return must be a table, " +
                    $"got {result.Type}.");
            }

            var root = result.Table;

            var roomsTable = RequireTable(root, "rooms", sourceName, "top level");
            var challengesTable = RequireTable(root, "challenges", sourceName, "top level");
            var monstersTable = RequireTable(root, "monsters", sourceName, "top level");
            var itemIdByMonsterIdTable = RequireTable(root, "itemIdByMonsterId", sourceName, "top level");
            var playerCharacterTable = RequireTable(root, "playerCharacter", sourceName, "top level");
            var playerAbilityScoresTable = RequireTable(root, "playerAbilityScores", sourceName, "top level");
            var flavorTextTable = RequireTable(root, "flavorText", sourceName, "top level");
            StartRoomId = (int)RequireNumber(root, "startRoomId", sourceName, "top level");

            Rooms = MarshalRooms(roomsTable, sourceName);
            Challenges = MarshalChallenges(challengesTable, sourceName);
            Monsters = MarshalMonsters(monstersTable, sourceName);
            ItemIdByMonsterId = MarshalStringDictionary(itemIdByMonsterIdTable, sourceName, "itemIdByMonsterId");
            PlayerCharacter = MarshalCombatantStats(playerCharacterTable, sourceName, "playerCharacter");
            PlayerAbilityScores = MarshalIntDictionary(playerAbilityScoresTable, sourceName, "playerAbilityScores");
            FlavorText = MarshalStringDictionary(flavorTextTable, sourceName, "flavorText");
        }

        /// <summary>Constructs a <see cref="LuaOneD20WorldModule"/> from a Lua source embedded as a
        /// resource in <paramref name="assembly"/> (this assembly, by default) — the path
        /// <c>Program.cs</c> uses for the shipped demonstration script.
        /// <paramref name="resourceName"/> doubles as the constructed instance's <c>sourceName</c>.</summary>
        public static LuaOneD20WorldModule FromEmbeddedResource(
            Assembly? assembly = null,
            string resourceName = "LunaFlow.Rulebook.OneD20.Content.DefaultOneD20WorldModule.lua",
            ISandboxedScriptHost? host = null)
        {
            assembly ??= typeof(LuaOneD20WorldModule).Assembly;
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException(
                    $"Lua World Module embedded resource '{resourceName}' was not found in assembly " +
                    $"'{assembly.GetName().Name}'.");
            using var reader = new StreamReader(stream);
            var source = reader.ReadToEnd();
            return new LuaOneD20WorldModule(source, resourceName, host);
        }

        /// <summary>Constructs a <see cref="LuaOneD20WorldModule"/> by reading Lua source from a file on
        /// disk. <paramref name="path"/> doubles as the constructed instance's <c>sourceName</c>.</summary>
        public static LuaOneD20WorldModule FromFile(string path, ISandboxedScriptHost? host = null)
        {
            var source = File.ReadAllText(path);
            return new LuaOneD20WorldModule(source, path, host);
        }

        private static Dictionary<int, OneD20RoomData> MarshalRooms(Table roomsTable, string sourceName)
        {
            var rooms = new Dictionary<int, OneD20RoomData>();
            foreach (var pair in roomsTable.Pairs)
            {
                if (pair.Key.Type != DataType.Number)
                {
                    throw new InvalidOperationException(
                        $"Lua World Module script '{sourceName}' invalid: 'rooms' has a non-numeric key " +
                        $"({pair.Key.Type}).");
                }
                var id = (int)pair.Key.Number;

                if (pair.Value.Type != DataType.Table)
                {
                    throw new InvalidOperationException(
                        $"Lua World Module script '{sourceName}' invalid: Room {id} must be a table, got " +
                        $"{pair.Value.Type}.");
                }
                var roomTable = pair.Value.Table;
                var context = $"Room {id}";

                var row = (int)RequireNumber(roomTable, "row", sourceName, context);
                var col = (int)RequireNumber(roomTable, "col", sourceName, context);
                var challengeTypeStr = RequireString(roomTable, "challengeType", sourceName, context);
                var descriptionKey = RequireString(roomTable, "descriptionKey", sourceName, context);

                if (!Enum.TryParse<OneD20ChallengeType>(challengeTypeStr, ignoreCase: true, out var challengeType))
                {
                    throw new InvalidOperationException(
                        $"Lua World Module script '{sourceName}' invalid: {context}'s challengeType " +
                        $"('{challengeTypeStr}') is not a recognized OneD20ChallengeType.");
                }

                rooms[id] = new OneD20RoomData(id, row, col, challengeType, descriptionKey);
            }
            return rooms;
        }

        private static Dictionary<string, OneD20ChallengeTemplate> MarshalChallenges(Table challengesTable, string sourceName)
        {
            var challenges = new Dictionary<string, OneD20ChallengeTemplate>();
            foreach (var pair in challengesTable.Pairs)
            {
                if (pair.Key.Type != DataType.String)
                {
                    throw new InvalidOperationException(
                        $"Lua World Module script '{sourceName}' invalid: 'challenges' has a non-string " +
                        $"key ({pair.Key.Type}).");
                }
                var key = pair.Key.String;

                if (pair.Value.Type != DataType.Table)
                {
                    throw new InvalidOperationException(
                        $"Lua World Module script '{sourceName}' invalid: Challenge '{key}' must be a " +
                        $"table, got {pair.Value.Type}.");
                }
                var t = pair.Value.Table;
                var context = $"Challenge '{key}'";

                // "kind" is optional, defaulting to "skill" — every script predating
                // phase-2-10-content-coverage-spec.md's Save kind (including the shipped
                // DefaultOneD20WorldModule.lua) omits it and must keep marshaling unchanged.
                var kindStr = OptionalString(t, "kind") ?? "skill";
                if (!Enum.TryParse<OneD20CheckKind>(kindStr, ignoreCase: true, out var kind))
                {
                    throw new InvalidOperationException(
                        $"Lua World Module script '{sourceName}' invalid: {context}'s kind ('{kindStr}') " +
                        $"is not a recognized OneD20CheckKind.");
                }

                string? skill = null, skillAbility = null, saveType = null;
                if (kind == OneD20CheckKind.Save)
                {
                    saveType = RequireString(t, "saveType", sourceName, context);
                }
                else
                {
                    skill = RequireString(t, "skill", sourceName, context);
                    skillAbility = RequireString(t, "skillAbility", sourceName, context);
                }
                var dc = (int)RequireNumber(t, "dc", sourceName, context);
                var successKey = RequireString(t, "successKey", sourceName, context);
                var failureKey = RequireString(t, "failureKey", sourceName, context);
                var promptKey = RequireString(t, "promptKey", sourceName, context);

                challenges[key] = new OneD20ChallengeTemplate(
                    key, skill, skillAbility, dc, successKey, failureKey, promptKey, kind, saveType);
            }
            return challenges;
        }

        private static Dictionary<string, OneD20MonsterTemplate> MarshalMonsters(Table monstersTable, string sourceName)
        {
            var monsters = new Dictionary<string, OneD20MonsterTemplate>();
            foreach (var pair in monstersTable.Pairs)
            {
                if (pair.Key.Type != DataType.String)
                {
                    throw new InvalidOperationException(
                        $"Lua World Module script '{sourceName}' invalid: 'monsters' has a non-string key " +
                        $"({pair.Key.Type}).");
                }
                var id = pair.Key.String;

                if (pair.Value.Type != DataType.Table)
                {
                    throw new InvalidOperationException(
                        $"Lua World Module script '{sourceName}' invalid: Monster '{id}' must be a table, " +
                        $"got {pair.Value.Type}.");
                }
                var t = pair.Value.Table;
                var context = $"Monster '{id}'";

                var name = RequireString(t, "name", sourceName, context);
                var descriptionKey = RequireString(t, "descriptionKey", sourceName, context);
                var statsTable = RequireTable(t, "stats", sourceName, context);
                var stats = MarshalCombatantStats(statsTable, sourceName, $"{context}'s stats");
                // "inflictsPoison" is optional, defaulting to false — scripts predating
                // phase-2-10-content-coverage-spec.md omit it.
                var inflictsPoison = OptionalBool(t, "inflictsPoison") ?? false;
                // "enrageThresholdFraction"/"enrageDefenseBonus" are both optional, defaulting to
                // null — scripts predating phase-2-12-lua-worldmodule-stress-test-spec.md omit both.
                // Pairing/range validity (both-or-neither, threshold in (0,1]) is
                // OneD20WorldModuleValidator's job, not the marshaler's — this only reads what's
                // present, same division as every other field here.
                var enrageThresholdFraction = OptionalNumber(t, "enrageThresholdFraction");
                var enrageDefenseBonus = OptionalNumber(t, "enrageDefenseBonus");

                monsters[id] = new OneD20MonsterTemplate(id, name, descriptionKey, stats, inflictsPoison,
                    enrageThresholdFraction,
                    enrageDefenseBonus.HasValue ? (int)enrageDefenseBonus.Value : null);
            }
            return monsters;
        }

        private static OneD20CombatantStats MarshalCombatantStats(Table t, string sourceName, string context)
        {
            var defense = (int)RequireNumber(t, "defense", sourceName, context);
            var attackBonus = (int)RequireNumber(t, "attackBonus", sourceName, context);
            var damageDie = RequireString(t, "damageDie", sourceName, context);
            var damageBonus = (int)RequireNumber(t, "damageBonus", sourceName, context);
            var maxHp = (int)RequireNumber(t, "maxHp", sourceName, context);
            return new OneD20CombatantStats(defense, attackBonus, damageDie, damageBonus, maxHp);
        }

        private static Dictionary<string, string> MarshalStringDictionary(Table t, string sourceName, string tableName)
        {
            var dict = new Dictionary<string, string>();
            foreach (var pair in t.Pairs)
            {
                if (pair.Key.Type != DataType.String)
                {
                    throw new InvalidOperationException(
                        $"Lua World Module script '{sourceName}' invalid: '{tableName}' has a non-string " +
                        $"key ({pair.Key.Type}).");
                }
                if (pair.Value.Type != DataType.String)
                {
                    throw new InvalidOperationException(
                        $"Lua World Module script '{sourceName}' invalid: '{tableName}[{pair.Key.String}]' " +
                        $"must be a string, got {pair.Value.Type}.");
                }
                dict[pair.Key.String] = pair.Value.String;
            }
            return dict;
        }

        private static Dictionary<string, int> MarshalIntDictionary(Table t, string sourceName, string tableName)
        {
            var dict = new Dictionary<string, int>();
            foreach (var pair in t.Pairs)
            {
                if (pair.Key.Type != DataType.String)
                {
                    throw new InvalidOperationException(
                        $"Lua World Module script '{sourceName}' invalid: '{tableName}' has a non-string " +
                        $"key ({pair.Key.Type}).");
                }
                if (pair.Value.Type != DataType.Number)
                {
                    throw new InvalidOperationException(
                        $"Lua World Module script '{sourceName}' invalid: '{tableName}[{pair.Key.String}]' " +
                        $"must be a number, got {pair.Value.Type}.");
                }
                dict[pair.Key.String] = (int)pair.Value.Number;
            }
            return dict;
        }

        /// <summary>Returns the string field <paramref name="key"/> if present and string-typed, else
        /// null (missing/absent is not an error for an optional field, unlike <see cref="RequireString"/>).</summary>
        private static string? OptionalString(Table parent, string key)
        {
            var value = parent.Get(key);
            return value.Type == DataType.String ? value.String : null;
        }

        /// <summary>Returns the boolean field <paramref name="key"/> if present and boolean-typed, else
        /// null.</summary>
        private static bool? OptionalBool(Table parent, string key)
        {
            var value = parent.Get(key);
            return value.Type == DataType.Boolean ? value.Boolean : null;
        }

        /// <summary>Returns the numeric field <paramref name="key"/> if present and number-typed, else
        /// null (missing/absent is not an error for an optional field, unlike <see cref="RequireNumber"/>).</summary>
        private static double? OptionalNumber(Table parent, string key)
        {
            var value = parent.Get(key);
            return value.Type == DataType.Number ? value.Number : null;
        }

        private static Table RequireTable(Table parent, string key, string sourceName, string context)
        {
            var value = parent.Get(key);
            if (value.Type != DataType.Table)
            {
                throw new InvalidOperationException(
                    $"Lua World Module script '{sourceName}' invalid: {context} is missing required table " +
                    $"field '{key}' (got {value.Type}).");
            }
            return value.Table;
        }

        private static string RequireString(Table parent, string key, string sourceName, string context)
        {
            var value = parent.Get(key);
            if (value.Type != DataType.String)
            {
                throw new InvalidOperationException(
                    $"Lua World Module script '{sourceName}' invalid: {context} is missing required string " +
                    $"field '{key}' (got {value.Type}).");
            }
            return value.String;
        }

        private static double RequireNumber(Table parent, string key, string sourceName, string context)
        {
            var value = parent.Get(key);
            if (value.Type != DataType.Number)
            {
                throw new InvalidOperationException(
                    $"Lua World Module script '{sourceName}' invalid: {context} is missing required number " +
                    $"field '{key}' (got {value.Type}).");
            }
            return value.Number;
        }
    }
}
