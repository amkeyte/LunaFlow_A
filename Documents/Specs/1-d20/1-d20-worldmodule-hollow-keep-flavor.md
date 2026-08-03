# 1.d20 World Module — Hollow Keep — Flavor Text

**Status:** draft, alongside [`1-d20-worldmodule-hollow-keep-spec.md`](1-d20-worldmodule-hollow-keep-spec.md).
Pure prose companion — per `content-spec-template.md`'s localization-style split, nothing in this
file is mechanical (no DCs, no fact shapes, no rule logic). Keyed by id; the technical spec
references these keys, never the literal text.

**Site:** the ruins of Hollow Keep, abandoned a generation ago, until a woodcutter reported firelight
in the windows and screams that didn't sound entirely human. Nothing darker than "something's moved
in and it isn't paying rent."

## Room descriptions and prompts

Same design rule `loot-rom-content-expansion-flavor.md` established: a room's own description never
references what challenge landed in it (challenge/monster text is drawn from a session-randomized
pool and must read sensibly in any of its type's slots), and the challenge/monster text never
references a specific room's fixtures.

| Key | Room | Name | Description text (always, on entry) |
| --- | --- | --- | --- |
| `hk-room-1-desc` | 1 | The Outer Gate | The keep's outer gate hangs off one hinge. Fresh boot-prints in the mud say you're not the first visitor this week. |
| `hk-room-2-desc` | 2 | The Larder | Barrels and sacks, most rotted through, some clearly restocked recently. |
| `hk-room-3-desc` | 3 | The Collapsed Stair | Half the stairwell has caved into rubble, the other half still climbing into darkness. |
| `hk-room-4-desc` | 4 | The Guard Passage | A narrow stone passage, torchlit, with a door at the far end standing slightly open. |
| `hk-room-5-desc` | 5 | The Hall of the Sunken Throne | The keep's great hall — its floor long since given way to a flooded lower level, throne and all. This is where it starts. |
| `hk-room-6-desc` | 6 | The Sunken Cistern | An old cistern, half-collapsed, filled chest-deep with black water. |
| `hk-room-7-desc` | 7 | The Warden's Study | A ruined study, shelves collapsed, papers long gone to mush — except for one iron strongbox, still intact. |
| `hk-room-8-desc` | 8 | The Watch Tower | A crumbling watchtower overlooking the keep's grounds. Something's pacing up there. |
| `hk-room-9-desc` | 9 | The Broken Rampart | A stretch of rampart wall with a section collapsed clean through, open air on both sides. |

## Skill-check prompts and outcomes

| Key | Skill (ability) | Prompt text (while unsolved) |
| --- | --- | --- |
| `hk-search-prompt` | Search (Int) | Something here's been moved recently, not left to rot like the rest — worth a closer look (Search). |
| `hk-climb-prompt` | Climb (Str) | There's a way up through the rubble, but it won't be easy (Climb). |
| `hk-listen-prompt` | Listen (Wis) | Someone's moving on the other side of that door — better time it right (Listen). |
| `hk-swim-prompt` | Swim (Str) | No way around the water — you'll have to wade through (Swim). |
| `hk-disabledevice-prompt` | Disable Device (Int) | That strongbox looks worth getting into, if you can beat the lock (Disable Device). |
| `hk-jump-prompt` | Jump (Str) | A gap in the floor just wide enough to worry about (Jump). |
| `hk-balance-prompt` | Balance (Dex) | There's a way across the gap, if you can keep your footing (Balance). |
| `hk-spot-prompt` | Spot (Wis) | Something here doesn't look right — worth a closer look (Spot). |
| `hk-concentration-prompt` | Concentration (Con) | This is going to take steady focus, and it won't be easy to keep (Concentration). |

| Key | Success text | Failure text |
| --- | --- | --- |
| `hk-search-success` / `hk-search-failure` | You find it — a cache tucked behind a loose stone. | Nothing obvious yet — worth another look. |
| `hk-climb-success` / `hk-climb-failure` | You find your footing in the rubble and haul yourself up. | You lose your footing partway. No harm done — try again. |
| `hk-listen-success` / `hk-listen-failure` | You catch the rhythm of their movement — you know when it's clear. | Hard to tell over the sound of the wind through the ruin. Listen again. |
| `hk-swim-success` / `hk-swim-failure` | You wade through — cold, but you make it across. | The current's stronger than it looks. You're pushed back to where you started. |
| `hk-disabledevice-success` / `hk-disabledevice-failure` | The lock gives with a click. | Trickier than it looks. No damage done — try again. |
| `hk-jump-success` / `hk-jump-failure` | You clear the gap easily. | You pull up short at the edge. No harm — just try again. |
| `hk-balance-success` / `hk-balance-failure` | You cross without a wobble. | You lose your footing halfway — no fall, no harm, just start over. |
| `hk-spot-success` / `hk-spot-failure` | There it is — you spot what you were missing. | Nothing catches your eye yet. Look again. |
| `hk-concentration-success` / `hk-concentration-failure` | You block out the distraction and get it done. | Your focus slips at the wrong moment. Try again. |

## Monster arrival descriptions

| Key | Monster | Text |
| --- | --- | --- |
| `hk-monster-outrider-desc` | The Outrider | A rider's cloak, a rider's boots, and a blade already drawn — he steps out from behind a fallen column like he's been waiting. |
| `hk-monster-brute-desc` | The Brute | A thick-necked man cracks his knuckles and squares up, grinning like this is the part of his day he enjoys. |
| `hk-monster-longbow-desc` | The Longbowman | A figure in the shadows draws a bow to full pull without a word of warning. |
| `hk-monster-ironclad-desc` | The Ironclad Sergeant | A heavyset man in battered plate plants himself in your way, arms crossed, daring you to try something. |
| `hk-monster-footpad-desc` | The Footpad | A wiry figure flicks open a knife, more nervous than dangerous, but not backing down either. |

## Combat narration

Deliberately generic — no weapon named that doesn't belong to every monster in the roster, same
"overly generic" instinct `loot-rom-content-expansion-flavor.md` applied to its own combat lines.
`pc-hit`/`pc-miss` reference the PC's own sword, which doesn't vary with the monster faced.

| Key | Use | Text |
| --- | --- | --- |
| `hk-monster-defeated` | Monster HP reaches 0 | He goes down and doesn't get up. |
| `hk-monster-fled` | 3-round clock expires | He breaks away and vanishes into the ruin before you can stop him — no catching him now. |
| `hk-pc-hit` | PC's attack connects, monster still standing | Your sword connects — he staggers but he's still on his feet. |
| `hk-pc-miss` | PC's attack misses | You swing and miss. |
| `hk-monster-hit` | Monster's counterattack connects | He gets a hit in — that's going to leave a mark. |
| `hk-monster-miss` | Monster's counterattack misses | He comes at you and you dodge clear. |
| `hk-pc-flee` | Player uses `flee` | You break off and put a wall between you and him. He doesn't follow. |
| `hk-pc-lost` | PC's HP reaches 0 during `fight` | He gets the better of you this time — you're forced back, bruised. No going through him, not today. |
| `hk-win-announce` | Any win line completes | Three for three — that's enough to call it. Whatever's been living in Hollow Keep, it isn't anymore. |

## Loot — item names and pickup text

| Item Id | Name | Pickup text (announced on award) |
| --- | --- | --- |
| `hk-item-blade` | The Outrider's Blade | You take his blade — well-balanced, well cared for. |
| `hk-item-knuckledusters` | Studded Knuckledusters | You pocket a pair of studded knuckledusters, still warm. |
| `hk-item-recurve` | The Longbowman's Recurve | You take the bow from his hands before he can nock another arrow. |
| `hk-item-cuirass` | Battered Boiled-Leather Cuirass | You strip the cuirass off him — scuffed, but solid. |
| `hk-item-knife` | Rusted Skinning Knife | You take the knife from his hand before he can grab it back. |

## Inventory command text

| Key | Use | Text |
| --- | --- | --- |
| `hk-inventory-empty` | No items collected yet | You're not carrying anything yet. |
| `hk-inventory-header` | At least one item collected | You're carrying: *(followed by each held item's Name, one per line)* |

## Related documents

- [`1-d20-worldmodule-hollow-keep-spec.md`](1-d20-worldmodule-hollow-keep-spec.md) — the technical
  spec these keys are referenced from
- [`1-d20-flavor.md`](1-d20-flavor.md) — the original setting's flavor text this reskins
- [`../loot-rom-content-expansion-flavor.md`](../loot-rom-content-expansion-flavor.md) — the original
  9-challenge/5-monster flavor text this reskin parallels, key-for-key
