# Loot ROM & Content Expansion — Flavor Text

**Status:** draft, alongside [`loot-rom-content-expansion-spec.md`](loot-rom-content-expansion-spec.md).
Pure prose companion — per `content-spec-template.md`'s localization-style split, nothing in this
file is mechanical (no DCs, no fact shapes, no rule logic). Keyed by id; the technical spec
references these keys, never the literal text.

**Keying change from `1-d20-flavor.md`:** prompt/success/failure text moves from room-indexed
(`room-2-prompt`) to challenge-indexed (`search-prompt`), since a challenge can now land on any room
of its type. Room *description* keys (`room-1-desc` through `room-9-desc`) are unchanged and not
repeated here — see `1-d20-flavor.md`.

**Design rule applied throughout this file:** challenge and monster text is written deliberately
generic — it never references a specific room's fixtures — so it reads sensibly regardless of which
position it lands in this session. The room's own description, in turn, never references the
challenge inside it. Two of these room ↔ challenge splits already existed in `1-d20-flavor.md` and
needed rewording alongside the rekey (noted inline below); the rest carried over as written.

## Skill-check prompts and outcomes (challenge-indexed, replaces `1-d20-flavor.md`'s room-indexed table)

| Key | Skill (ability) | Prompt text (while unsolved) |
| --- | --- | --- |
| `search-prompt` | Search (Int) | Something here feels arranged, not left to chance — worth a closer look (Search). |
| `climb-prompt` | Climb (Str) | There's a way up, but it won't be easy (Climb). |
| `listen-prompt` | Listen (Wis) | Someone's close by — better time it right (Listen). |
| `swim-prompt` | Swim (Str) | There's water blocking the way — you'll have to get through it (Swim). |
| `disabledevice-prompt` | Disable Device (Int) | Something's locked up tight, and it looks worth getting into (Disable Device). |
| `jump-prompt` | Jump (Str) | A gap just wide enough to worry about (Jump). |
| `balance-prompt` | Balance (Dex) | There's a way across, if you can keep your footing (Balance). |
| `spot-prompt` | Spot (Wis) | Something here doesn't look right — worth a closer look (Spot). |
| `concentration-prompt` | Concentration (Con) | This is going to take steady focus, and it won't be easy to keep (Concentration). |

Reworded from `1-d20-flavor.md` during the rekey (named a specific room fixture that no longer
reliably applies): `search-prompt` dropped "not just stored" (`room-2-prompt`'s storeroom
reference); `disabledevice-prompt` dropped "a locked filing cabinet" in favor of "something's locked
up" (`room-7-prompt`'s office reference). `climb-prompt`/`listen-prompt`/`swim-prompt`/`jump-prompt`
carried over with only light wording changes to drop leftover room-specific phrasing ("the other
side" → "close by"; explicit wading detail trimmed).

| Key | Success text | Failure text |
| --- | --- | --- |
| `search-success` / `search-failure` | You spot the latch, cleverly hidden nearby. | Nothing obvious yet — worth another look. |
| `climb-success` / `climb-failure` | You get a grip and haul yourself up and over. | You lose your footing partway. No harm done — try again. |
| `listen-success` / `listen-failure` | You catch the rhythm of their movement — you know when it's clear. | Hard to tell over the noise. Listen again. |
| `swim-success` / `swim-failure` | You wade through — cold, but you make it across. | The current's stronger than it looks. You're pushed back to where you started. |
| `disabledevice-success` / `disabledevice-failure` | The lock gives with a click. | Trickier than it looks. No damage done — try again. |
| `jump-success` / `jump-failure` | You clear the gap easily. | You pull up short at the edge. No harm — just try again. |
| `balance-success` / `balance-failure` | You cross without a wobble. | You lose your footing halfway — no fall, no harm, just start over. |
| `spot-success` / `spot-failure` | There it is — you spot what you were missing. | Nothing catches your eye yet. Look again. |
| `concentration-success` / `concentration-failure` | You block out the distraction and get it done. | Your focus slips at the wrong moment. Try again. |

## Monster arrival descriptions (monster-indexed, replaces the single fixed `monster-desc` key)

| Key | Monster | Text |
| --- | --- | --- |
| `monster-duelist-desc` | The Duelist | A seedy-looking man steps out of the shadows, rapier already in hand. He doesn't look like he wants to talk. |
| `monster-brawler-desc` | The Brawler | A thick-necked man cracks his knuckles and squares up, grinning like this is the part of his day he enjoys. |
| `monster-marksman-desc` | The Marksman | A lean figure levels a pistol at you without a word of warning. |
| `monster-enforcer-desc` | The Enforcer | A heavyset man in a reinforced vest plants himself in your way, arms crossed, daring you to try something. |
| `monster-scrapper-desc` | The Scrapper | A wiry kid flicks open a knife, more nervous than dangerous, but not backing down either. |

`monster-duelist-desc` is `1-d20-flavor.md`'s original `monster-desc` text, carried over unchanged
and rekeyed.

## Combat narration (unchanged keys, two reworded to drop weapon-specific bleed)

| Key | Use | Text |
| --- | --- | --- |
| `monster-defeated` | Monster HP reaches 0 | He goes down and doesn't get up. |
| `monster-fled` | 3-round clock expires | He slips past you and out of sight before you can stop him — no catching him now. |
| `pc-hit` | PC's attack connects, monster still standing | Your sword connects — he staggers but he's still on his feet. |
| `pc-miss` | PC's attack misses | You swing and miss. |
| `monster-hit` | Monster's counterattack connects | He gets a hit in — that's going to leave a mark. |
| `monster-miss` | Monster's counterattack misses | He comes at you and you dodge clear. |
| `pc-flee` | Player uses `flee` | You break off and put the door between you and him. He doesn't follow. |
| `pc-lost` | PC's HP reaches 0 during `fight` | He gets the better of you this time — you're forced back, bruised. No going through him, not today. |
| `win-announce` | Any win line completes | Three for three — that's enough to call it. Mission complete. |

`monster-defeated` (was "He drops the rapier and doesn't get up.") and `monster-hit` (was "His
rapier catches you...") both named the original monster's weapon specifically — reworded once,
generically, rather than writing five weapon-specific variants. `pc-hit`/`pc-miss` still reference
the PC's sword deliberately: the PC's weapon doesn't vary with the monster roster, so no rewording
needed there.

## Loot — item names and pickup text

| Item Id | Name | Pickup text (announced on award) |
| --- | --- | --- |
| `rapier` | The Duelist's Rapier | You take his rapier — well-balanced, well cared for. |
| `knuckles` | Brass Knuckles | You pocket a pair of brass knuckles, still warm. |
| `pistol` | Backup Piece | You take the spare pistol from his jacket. |
| `vest` | Kevlar Vest | You strip the vest off him — scuffed, but solid. |
| `knife` | Folding Knife | You take the knife from his hand before he can grab it back. |

## Loot — `inventory` command text

| Key | Use | Text |
| --- | --- | --- |
| `inventory-empty` | No items collected yet | You're not carrying anything yet. |
| `inventory-header` | At least one item collected | You're carrying: *(followed by each held item's Name, one per line)* |

## Related documents

- [`loot-rom-content-expansion-spec.md`](loot-rom-content-expansion-spec.md) — the technical spec
  these keys are referenced from
- [`1-d20/1-d20-flavor.md`](1-d20/1-d20-flavor.md) — room-description keys (unchanged) and the
  original prompt/success/failure table this one supersedes for prompt/success/failure only
- [`../design/1-d20.md`](../design/1-d20.md) — concept doc
