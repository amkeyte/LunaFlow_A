namespace LunaFlow.Rom.D20System
{
    /// <summary>
    /// Pure narration-formatting helpers for the d20 System: combine a Rulebook-supplied flavor-text
    /// key (looked up via <see cref="D20SystemContent.FlavorText"/>) with handler-computed numeric
    /// readouts (roll totals, damage, HP). Contains no prose of its own — every string comes from
    /// the configured Rulebook; this class only owns the format shape.
    /// </summary>
    public static class D20SystemNarration
    {
        /// <summary>Builds the <c>GameEndRule</c> announcement: the win-announce text plus the winning line's room ids.</summary>
        public static string WinAnnouncement(IReadOnlyList<int> line) =>
            $"{D20SystemContent.FlavorText["win-announce"]} (rooms {string.Join(", ", line)})";

        /// <summary>Builds a <c>check</c> outcome's announcement: the room-specific flavor text plus the mechanical roll/DC readout.</summary>
        public static string CheckNarration(string key, string skill, int total, int dc) =>
            $"{D20SystemContent.FlavorText[key]} ({skill}: rolled {total} vs. DC {dc}.)";

        /// <summary>Builds a <c>fight</c> hit's announcement: the flavor text plus damage dealt and the target's remaining HP.</summary>
        public static string HitNarration(string key, int damage, int remainingHp) =>
            $"{D20SystemContent.FlavorText[key]} ({damage} damage — {remainingHp} HP remaining.)";

        /// <summary>Builds <c>look</c>'s mid-fight status line: both combatants' current/max HP.</summary>
        public static string CombatStatus(int playerHp, int playerMaxHp, int monsterHp, int monsterMaxHp) =>
            $"You're at {playerHp}/{playerMaxHp} HP. He's at {monsterHp}/{monsterMaxHp} HP.";
    }
}
