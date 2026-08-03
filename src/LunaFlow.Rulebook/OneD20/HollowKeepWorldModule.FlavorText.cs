namespace LunaFlow.Rulebook.OneD20
{
    /// <summary>
    /// <see cref="HollowKeepWorldModule.FlavorText"/> — keyed per
    /// <c>1-d20-worldmodule-hollow-keep-flavor.md</c>. Room/challenge/monster/item keys are namespaced
    /// under this module's own <c>hk-</c> prefix; the eleven fixed-vocabulary keys (combat narration
    /// plus inventory text) are deliberately unprefixed, matching the shared handler code's hardcoded
    /// literal lookups exactly — see ticket 0061.
    /// </summary>
    public sealed partial class HollowKeepWorldModule
    {
        /// <summary>Flavor text, keyed per <c>1-d20-worldmodule-hollow-keep-flavor.md</c>.</summary>
        public IReadOnlyDictionary<string, string> FlavorText { get; } = new Dictionary<string, string>
        {
            // Room descriptions.
            ["hk-room-1-desc"] = "The keep's outer gate hangs off one hinge. Fresh boot-prints in the mud say you're not the first visitor this week.",
            ["hk-room-2-desc"] = "Barrels and sacks, most rotted through, some clearly restocked recently.",
            ["hk-room-3-desc"] = "Half the stairwell has caved into rubble, the other half still climbing into darkness.",
            ["hk-room-4-desc"] = "A narrow stone passage, torchlit, with a door at the far end standing slightly open.",
            ["hk-room-5-desc"] = "The keep's great hall — its floor long since given way to a flooded lower level, throne and all. This is where it starts.",
            ["hk-room-6-desc"] = "An old cistern, half-collapsed, filled chest-deep with black water.",
            ["hk-room-7-desc"] = "A ruined study, shelves collapsed, papers long gone to mush — except for one iron strongbox, still intact.",
            ["hk-room-8-desc"] = "A crumbling watchtower overlooking the keep's grounds. Something's pacing up there.",
            ["hk-room-9-desc"] = "A stretch of rampart wall with a section collapsed clean through, open air on both sides.",

            // Skill-check prompts.
            ["hk-search-prompt"] = "Something here's been moved recently, not left to rot like the rest — worth a closer look (Search).",
            ["hk-climb-prompt"] = "There's a way up through the rubble, but it won't be easy (Climb).",
            ["hk-listen-prompt"] = "Someone's moving on the other side of that door — better time it right (Listen).",
            ["hk-swim-prompt"] = "No way around the water — you'll have to wade through (Swim).",
            ["hk-disabledevice-prompt"] = "That strongbox looks worth getting into, if you can beat the lock (Disable Device).",
            ["hk-jump-prompt"] = "A gap in the floor just wide enough to worry about (Jump).",
            ["hk-balance-prompt"] = "There's a way across the gap, if you can keep your footing (Balance).",
            ["hk-spot-prompt"] = "Something here doesn't look right — worth a closer look (Spot).",
            ["hk-concentration-prompt"] = "This is going to take steady focus, and it won't be easy to keep (Concentration).",

            // Skill-check success/failure.
            ["hk-search-success"] = "You find it — a cache tucked behind a loose stone.",
            ["hk-search-failure"] = "Nothing obvious yet — worth another look.",
            ["hk-climb-success"] = "You find your footing in the rubble and haul yourself up.",
            ["hk-climb-failure"] = "You lose your footing partway. No harm done — try again.",
            ["hk-listen-success"] = "You catch the rhythm of their movement — you know when it's clear.",
            ["hk-listen-failure"] = "Hard to tell over the sound of the wind through the ruin. Listen again.",
            ["hk-swim-success"] = "You wade through — cold, but you make it across.",
            ["hk-swim-failure"] = "The current's stronger than it looks. You're pushed back to where you started.",
            ["hk-disabledevice-success"] = "The lock gives with a click.",
            ["hk-disabledevice-failure"] = "Trickier than it looks. No damage done — try again.",
            ["hk-jump-success"] = "You clear the gap easily.",
            ["hk-jump-failure"] = "You pull up short at the edge. No harm — just try again.",
            ["hk-balance-success"] = "You cross without a wobble.",
            ["hk-balance-failure"] = "You lose your footing halfway — no fall, no harm, just start over.",
            ["hk-spot-success"] = "There it is — you spot what you were missing.",
            ["hk-spot-failure"] = "Nothing catches your eye yet. Look again.",
            ["hk-concentration-success"] = "You block out the distraction and get it done.",
            ["hk-concentration-failure"] = "Your focus slips at the wrong moment. Try again.",

            // Monster arrival descriptions.
            ["hk-monster-outrider-desc"] = "A rider's cloak, a rider's boots, and a blade already drawn — he steps out from behind a fallen column like he's been waiting.",
            ["hk-monster-brute-desc"] = "A thick-necked man cracks his knuckles and squares up, grinning like this is the part of his day he enjoys.",
            ["hk-monster-longbow-desc"] = "A figure in the shadows draws a bow to full pull without a word of warning.",
            ["hk-monster-ironclad-desc"] = "A heavyset man in battered plate plants himself in your way, arms crossed, daring you to try something.",
            ["hk-monster-footpad-desc"] = "A wiry figure flicks open a knife, more nervous than dangerous, but not backing down either.",

            // Combat narration — unprefixed by design (ticket 0061): hardcoded literal lookups inside
            // FightCommandHandler/FleeCommandHandler, shared by every IOneD20WorldModule unchanged.
            ["monster-defeated"] = "He goes down and doesn't get up.",
            ["monster-fled"] = "He breaks away and vanishes into the ruin before you can stop him — no catching him now.",
            ["pc-hit"] = "Your sword connects — he staggers but he's still on his feet.",
            ["pc-miss"] = "You swing and miss.",
            ["monster-hit"] = "He gets a hit in — that's going to leave a mark.",
            ["monster-miss"] = "He comes at you and you dodge clear.",
            ["pc-flee"] = "You break off and put a wall between you and him. He doesn't follow.",
            ["pc-lost"] = "He gets the better of you this time — you're forced back, bruised. No going through him, not today.",
            ["win-announce"] = "Three for three — that's enough to call it. Whatever's been living in Hollow Keep, it isn't anymore.",

            // Loot — item names and pickup text. Keys follow the shared handler's fixed
            // "item-{ItemId}-name"/"item-{ItemId}-pickup" pattern, where ItemId is this module's own
            // hk-item-* string (InventoryCommandHandler.cs / Program.cs's LootPickupText seeding).
            ["item-hk-item-blade-name"] = "The Outrider's Blade",
            ["item-hk-item-blade-pickup"] = "You take his blade — well-balanced, well cared for.",
            ["item-hk-item-knuckledusters-name"] = "Studded Knuckledusters",
            ["item-hk-item-knuckledusters-pickup"] = "You pocket a pair of studded knuckledusters, still warm.",
            ["item-hk-item-recurve-name"] = "The Longbowman's Recurve",
            ["item-hk-item-recurve-pickup"] = "You take the bow from his hands before he can nock another arrow.",
            ["item-hk-item-cuirass-name"] = "Battered Boiled-Leather Cuirass",
            ["item-hk-item-cuirass-pickup"] = "You strip the cuirass off him — scuffed, but solid.",
            ["item-hk-item-knife-name"] = "Rusted Skinning Knife",
            ["item-hk-item-knife-pickup"] = "You take the knife from his hand before he can grab it back.",

            // Inventory command text — unprefixed by design (ticket 0061), same reasoning as combat narration.
            ["inventory-empty"] = "You're not carrying anything yet.",
            ["inventory-header"] = "You're carrying:",
        };
    }
}
