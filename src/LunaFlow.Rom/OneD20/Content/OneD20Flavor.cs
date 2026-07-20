namespace LunaFlow.Rom.OneD20
{
    /// <summary>
    /// The 1.d20 flavor-text table, keyed exactly as in <c>1-d20-flavor.md</c>. The only place
    /// this prose is allowed to live — rules and handlers look text up by key, never inline it.
    /// </summary>
    public static class OneD20Flavor
    {
        /// <summary>Flavor text, keyed per <c>1-d20-flavor.md</c>.</summary>
        public static readonly IReadOnlyDictionary<string, string> Text = new Dictionary<string, string>
        {
            ["room-1-desc"] = "Crates stacked head-high, a rolled-up delivery door letting in a slice of grey daylight. Someone's still here.",
            ["room-2-desc"] = "Shelves of unlabeled boxes, floor to ceiling. Something about this room feels arranged, not just stored.",
            ["room-3-desc"] = "A chain-link fence, taller than it looked from outside, is the only way through to the next stretch of the yard.",
            ["room-4-desc"] = "A narrow hallway with a door at the far end, slightly ajar. Someone's moving on the other side.",
            ["room-5-desc"] = "The warehouse's main floor — open, echoing, and impossible to cross quietly. This is where it starts.",
            ["room-6-desc"] = "A burst pipe has turned this stretch of hallway into a shin-deep wading pool. No way around it.",
            ["room-7-desc"] = "A cramped office, papers everywhere, and a locked filing cabinet that looks worth a closer look.",
            ["room-8-desc"] = "A half-collapsed mezzanine overlooking the bay below. Footsteps creak somewhere above.",
            ["room-9-desc"] = "A metal catwalk with a section of grating torn away — a gap just wide enough to worry about.",
            ["monster-desc"] = "A seedy-looking man steps out of the shadows, rapier already in hand. He doesn't look like he wants to talk.",
            ["monster-defeated"] = "He drops the rapier and doesn't get up.",
            ["monster-fled"] = "He slips past you and out of sight before you can stop him — no catching him now.",
            ["pc-flee"] = "You break off and put the door between you and him. He doesn't follow.",
            ["pc-down"] = "You're down. The mission's over for today — time to regroup.",
            ["check-success"] = "Got it.",
            ["check-failure"] = "Not this time — worth another look.",
            ["win-announce"] = "Three for three — that's enough to call it. Mission complete.",
        };

        /// <summary>Builds the <c>GameEndRule</c> announcement: the win-announce text plus the winning line's room ids.</summary>
        public static string WinAnnouncement(IReadOnlyList<int> line) =>
            $"{Text["win-announce"]} (rooms {string.Join(", ", line)})";
    }
}
