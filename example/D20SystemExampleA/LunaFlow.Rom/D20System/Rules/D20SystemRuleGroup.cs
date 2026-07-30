using LunaFlow.Rules.GeneratorServices;

namespace LunaFlow.Rom.D20System
{
    /// <summary>
    /// D20System's declarative rules: 9 challenge-prompt rules (one per room).
    /// Each rule matches IPlayerLocation to detect when a player enters a room,
    /// then announces the challenge prompt for that room's obstacle.
    /// </summary>
    [RuleGroup("D20System")]
    public static partial class D20SystemRuleGroup
    {
        /// <summary>
        /// Builds a challenge-prompt rule for a specific room: announces the challenge
        /// when the player enters the room (detected via IPlayerLocation match), unless this
        /// room's own challenge is already solved or failed, or the game has already been won
        /// (<c>Not&lt;IGameWon&gt;()</c> — the contract type <c>WinMarker</c> implements, per Amara's
        /// ruling on ticket 0045: same promote-to-Cartridge pattern as the other four contracts,
        /// never the concrete GridMap fact by name). The prompt text itself comes from a matched
        /// <see cref="RoomChallengePrompt"/> fact — already-resolved Rulebook flavor text, seeded
        /// once by <c>Program.cs</c>, never a key looked up inside generated code (ticket 0043,
        /// Amara's ruling).
        /// </summary>
        private static RulePrototype ChallengePromptRule(int roomId) => new()
        {
            Name = $"ChallengePrompt_Room{roomId}",
            Description = $"Announces the challenge prompt when player enters room {roomId}.",
            RequiredUsings = new() { "LunaFlow.Engine", "LunaFlow.Rom.D20System", "LunaFlow.Cartridge" },
            PreambleStatements = new() { "RoomChallengePrompt prompt = null!;" },
            WhenStatements = new()
            {
                $"Match<IPlayerLocation>(pl => pl.RoomId == {roomId})",
                $"Match<RoomChallengePrompt>(() => prompt, p => p.RoomId == {roomId})",
                $"Not<RoomSolved>(rs => rs.RoomId == {roomId})",
                $"Not<RoomFailed>(rf => rf.RoomId == {roomId})",
                "Not<IGameWon>()",
            },
            ThenStatements = new()
            {
                "Do(ctx => LunaFlowEngine.Host.Announce(prompt.Text))",
            },
        };

        /// <summary>Challenge prompt: room 1 (Fight).</summary>
        [RuleTemplate] public static RulePrototype ChallengePromptRoom1 => ChallengePromptRule(1);

        /// <summary>Challenge prompt: room 2 (Search/Int, DC 10).</summary>
        [RuleTemplate] public static RulePrototype ChallengePromptRoom2 => ChallengePromptRule(2);

        /// <summary>Challenge prompt: room 3 (Climb/Str, DC 12).</summary>
        [RuleTemplate] public static RulePrototype ChallengePromptRoom3 => ChallengePromptRule(3);

        /// <summary>Challenge prompt: room 4 (Listen/Wis, DC 8).</summary>
        [RuleTemplate] public static RulePrototype ChallengePromptRoom4 => ChallengePromptRule(4);

        /// <summary>Challenge prompt: room 5 (Fight).</summary>
        [RuleTemplate] public static RulePrototype ChallengePromptRoom5 => ChallengePromptRule(5);

        /// <summary>Challenge prompt: room 6 (Swim/Str, DC 10).</summary>
        [RuleTemplate] public static RulePrototype ChallengePromptRoom6 => ChallengePromptRule(6);

        /// <summary>Challenge prompt: room 7 (Disable Device/Int, DC 12).</summary>
        [RuleTemplate] public static RulePrototype ChallengePromptRoom7 => ChallengePromptRule(7);

        /// <summary>Challenge prompt: room 8 (Fight).</summary>
        [RuleTemplate] public static RulePrototype ChallengePromptRoom8 => ChallengePromptRule(8);

        /// <summary>Challenge prompt: room 9 (Jump/Str, DC 8).</summary>
        [RuleTemplate] public static RulePrototype ChallengePromptRoom9 => ChallengePromptRule(9);
    }
}
