namespace LunaFlow.Rulebook.OneD20
{
    /// <summary>
    /// The 1.d20 flavor-text table, keyed exactly as in <c>loot-rom-content-expansion-flavor.md</c>
    /// (which supersedes <c>1-d20-flavor.md</c>'s prompt/success/failure table only — room
    /// description keys are unchanged and carried over from there verbatim). The only place this
    /// prose is allowed to live — GridMap's/D20System's/Loot's rules and handlers look text up by
    /// key, never inline it. Flavor keys never contain numbers; the numeric readouts (damage, HP,
    /// roll, DC) are always handler-constructed and combined with flavor text separately, never
    /// hardcoded into the flavor text itself.
    /// </summary>
    public static class OneD20FlavorText
    {
        /// <summary>Flavor text, keyed per <c>loot-rom-content-expansion-flavor.md</c>.</summary>
        public static readonly IReadOnlyDictionary<string, string> Text = new Dictionary<string, string>
        {
            // Room descriptions — unchanged from 1-d20-flavor.md; room position/topology never varies per session.
            ["room-1-desc"] = "Crates stacked head-high, a rolled-up delivery door letting in a slice of grey daylight. Someone's still here.",
            ["room-2-desc"] = "Shelves of unlabeled boxes, floor to ceiling.",
            ["room-3-desc"] = "A chain-link fence, taller than it looked from outside, cuts across the yard.",
            ["room-4-desc"] = "A narrow hallway with a door at the far end, slightly ajar.",
            ["room-5-desc"] = "The warehouse's main floor — open, echoing, and impossible to cross quietly. This is where it starts.",
            ["room-6-desc"] = "A burst pipe has turned this stretch of hallway into a shin-deep wading pool.",
            ["room-7-desc"] = "A cramped office, papers everywhere.",
            ["room-8-desc"] = "A half-collapsed mezzanine overlooking the bay below. Footsteps creak somewhere above.",
            ["room-9-desc"] = "A metal catwalk with a section of grating torn away.",

            // Skill-check prompts (challenge-indexed — a challenge can now land on any room of its type).
            ["search-prompt"] = "Something here feels arranged, not left to chance — worth a closer look (Search).",
            ["climb-prompt"] = "There's a way up, but it won't be easy (Climb).",
            ["listen-prompt"] = "Someone's close by — better time it right (Listen).",
            ["swim-prompt"] = "There's water blocking the way — you'll have to get through it (Swim).",
            ["disabledevice-prompt"] = "Something's locked up tight, and it looks worth getting into (Disable Device).",
            ["jump-prompt"] = "A gap just wide enough to worry about (Jump).",
            ["balance-prompt"] = "There's a way across, if you can keep your footing (Balance).",
            ["spot-prompt"] = "Something here doesn't look right — worth a closer look (Spot).",
            ["concentration-prompt"] = "This is going to take steady focus, and it won't be easy to keep (Concentration).",

            // Skill-check success/failure (challenge-indexed).
            ["search-success"] = "You spot the latch, cleverly hidden nearby.",
            ["search-failure"] = "Nothing obvious yet — worth another look.",
            ["climb-success"] = "You get a grip and haul yourself up and over.",
            ["climb-failure"] = "You lose your footing partway. No harm done — try again.",
            ["listen-success"] = "You catch the rhythm of their movement — you know when it's clear.",
            ["listen-failure"] = "Hard to tell over the noise. Listen again.",
            ["swim-success"] = "You wade through — cold, but you make it across.",
            ["swim-failure"] = "The current's stronger than it looks. You're pushed back to where you started.",
            ["disabledevice-success"] = "The lock gives with a click.",
            ["disabledevice-failure"] = "Trickier than it looks. No damage done — try again.",
            ["jump-success"] = "You clear the gap easily.",
            ["jump-failure"] = "You pull up short at the edge. No harm — just try again.",
            ["balance-success"] = "You cross without a wobble.",
            ["balance-failure"] = "You lose your footing halfway — no fall, no harm, just start over.",
            ["spot-success"] = "There it is — you spot what you were missing.",
            ["spot-failure"] = "Nothing catches your eye yet. Look again.",
            ["concentration-success"] = "You block out the distraction and get it done.",
            ["concentration-failure"] = "Your focus slips at the wrong moment. Try again.",

            // Monster arrival descriptions (monster-indexed — replaces the single fixed monster-desc key).
            ["monster-duelist-desc"] = "A seedy-looking man steps out of the shadows, rapier already in hand. He doesn't look like he wants to talk.",
            ["monster-brawler-desc"] = "A thick-necked man cracks his knuckles and squares up, grinning like this is the part of his day he enjoys.",
            ["monster-marksman-desc"] = "A lean figure levels a pistol at you without a word of warning.",
            ["monster-enforcer-desc"] = "A heavyset man in a reinforced vest plants himself in your way, arms crossed, daring you to try something.",
            ["monster-scrapper-desc"] = "A wiry kid flicks open a knife, more nervous than dangerous, but not backing down either.",

            // Combat narration — monster-defeated/monster-hit reworded to drop weapon-specific bleed
            // (the original text named "the rapier" specifically); the rest unchanged.
            ["monster-defeated"] = "He goes down and doesn't get up.",
            ["monster-fled"] = "He slips past you and out of sight before you can stop him — no catching him now.",
            ["pc-hit"] = "Your sword connects — he staggers but he's still on his feet.",
            ["pc-miss"] = "You swing and miss.",
            ["monster-hit"] = "He gets a hit in — that's going to leave a mark.",
            ["monster-miss"] = "He comes at you and you dodge clear.",
            ["pc-flee"] = "You break off and put the door between you and him. He doesn't follow.",
            ["pc-lost"] = "He gets the better of you this time — you're forced back, bruised. No going through him, not today.",
            ["win-announce"] = "Three for three — that's enough to call it. Mission complete.",

            // Loot — item names and pickup text.
            ["item-rapier-name"] = "The Duelist's Rapier",
            ["item-rapier-pickup"] = "You take his rapier — well-balanced, well cared for.",
            ["item-knuckles-name"] = "Brass Knuckles",
            ["item-knuckles-pickup"] = "You pocket a pair of brass knuckles, still warm.",
            ["item-pistol-name"] = "Backup Piece",
            ["item-pistol-pickup"] = "You take the spare pistol from his jacket.",
            ["item-vest-name"] = "Kevlar Vest",
            ["item-vest-pickup"] = "You strip the vest off him — scuffed, but solid.",
            ["item-knife-name"] = "Folding Knife",
            ["item-knife-pickup"] = "You take the knife from his hand before he can grab it back.",

            // Loot — inventory command text.
            ["inventory-empty"] = "You're not carrying anything yet.",
            ["inventory-header"] = "You're carrying:",
        };
    }
}
