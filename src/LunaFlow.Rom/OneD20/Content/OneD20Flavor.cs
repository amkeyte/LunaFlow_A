namespace LunaFlow.Rom.OneD20
{
    /// <summary>
    /// The 1.d20 flavor-text table, keyed exactly as in <c>1-d20-flavor.md</c>. The only place
    /// this prose is allowed to live — rules and handlers look text up by key, never inline it.
    /// Flavor keys never contain numbers; the numeric readouts (damage, HP, roll, DC) are always
    /// handler-constructed and combined with flavor text via this class's narration helpers, never
    /// hardcoded into the flavor text itself.
    /// </summary>
    public static class OneD20Flavor
    {
        /// <summary>Flavor text, keyed per <c>1-d20-flavor.md</c>.</summary>
        public static readonly IReadOnlyDictionary<string, string> Text = new Dictionary<string, string>
        {
            ["room-1-desc"] = "Crates stacked head-high, a rolled-up delivery door letting in a slice of grey daylight. Someone's still here.",
            ["room-2-desc"] = "Shelves of unlabeled boxes, floor to ceiling.",
            ["room-2-prompt"] = "Something about this room feels arranged, not just stored — worth a closer look (Search).",
            ["room-3-desc"] = "A chain-link fence, taller than it looked from outside, cuts across the yard.",
            ["room-3-prompt"] = "It's climbable, but it won't be easy (Climb).",
            ["room-4-desc"] = "A narrow hallway with a door at the far end, slightly ajar.",
            ["room-4-prompt"] = "Someone's moving on the other side — better time it right (Listen).",
            ["room-5-desc"] = "The warehouse's main floor — open, echoing, and impossible to cross quietly. This is where it starts.",
            ["room-6-desc"] = "A burst pipe has turned this stretch of hallway into a shin-deep wading pool.",
            ["room-6-prompt"] = "No way around it — you'll have to wade through (Swim).",
            ["room-7-desc"] = "A cramped office, papers everywhere.",
            ["room-7-prompt"] = "A locked filing cabinet looks worth getting into (Disable Device).",
            ["room-8-desc"] = "A half-collapsed mezzanine overlooking the bay below. Footsteps creak somewhere above.",
            ["room-9-desc"] = "A metal catwalk with a section of grating torn away.",
            ["room-9-prompt"] = "A gap just wide enough to worry about (Jump).",
            ["monster-desc"] = "A seedy-looking man steps out of the shadows, rapier already in hand. He doesn't look like he wants to talk.",
            ["monster-defeated"] = "He drops the rapier and doesn't get up.",
            ["monster-fled"] = "He slips past you and out of sight before you can stop him — no catching him now.",
            ["pc-hit"] = "Your sword connects — he staggers but he's still on his feet.",
            ["pc-miss"] = "You swing and miss.",
            ["monster-hit"] = "His rapier catches you — that's going to leave a mark.",
            ["monster-miss"] = "He lunges and you sidestep it.",
            ["pc-flee"] = "You break off and put the door between you and him. He doesn't follow.",
            ["pc-lost"] = "He gets the better of you this time — you're forced back, bruised. No going through him, not today.",
            ["win-announce"] = "Three for three — that's enough to call it. Mission complete.",

            ["check-2-success"] = "You spot the latch, hidden behind a loose panel.",
            ["check-2-failure"] = "Nothing obvious yet — worth another look.",
            ["check-3-success"] = "You get a grip and haul yourself over the fence.",
            ["check-3-failure"] = "You lose your footing halfway up. No harm done — try again.",
            ["check-4-success"] = "You catch the rhythm of footsteps — you know when it's clear.",
            ["check-4-failure"] = "Hard to tell over the warehouse's noise. Listen again.",
            ["check-6-success"] = "You wade through — cold, but you make it across.",
            ["check-6-failure"] = "The current's stronger than it looks. You're pushed back to where you started.",
            ["check-7-success"] = "The lock gives with a click.",
            ["check-7-failure"] = "Trickier than it looks. No damage done — try again.",
            ["check-9-success"] = "You clear the gap easily.",
            ["check-9-failure"] = "You pull up short at the edge. No harm — just try again.",
        };

        /// <summary>Builds the <c>GameEndRule</c> announcement: the win-announce text plus the winning line's room ids.</summary>
        public static string WinAnnouncement(IReadOnlyList<int> line) =>
            $"{Text["win-announce"]} (rooms {string.Join(", ", line)})";

        /// <summary>Builds a <c>check</c> outcome's announcement: the room-specific flavor text plus the mechanical roll/DC readout.</summary>
        public static string CheckNarration(string key, string skill, int total, int dc) =>
            $"{Text[key]} ({skill}: rolled {total} vs. DC {dc}.)";

        /// <summary>Builds a <c>fight</c> hit's announcement: the flavor text plus damage dealt and the target's remaining HP.</summary>
        public static string HitNarration(string key, int damage, int remainingHp) =>
            $"{Text[key]} ({damage} damage — {remainingHp} HP remaining.)";

        /// <summary>Builds <c>look</c>'s mid-fight status line: both combatants' current/max HP.</summary>
        public static string CombatStatus(int playerHp, int playerMaxHp, int monsterHp, int monsterMaxHp) =>
            $"You're at {playerHp}/{playerMaxHp} HP. He's at {monsterHp}/{monsterMaxHp} HP.";
    }
}
