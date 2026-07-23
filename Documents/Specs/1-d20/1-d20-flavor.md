# 1.d20 — Flavor Text

**Status:** active, alongside [`1-d20-spec.md`](1-d20-spec.md) (see its status line). Pure prose companion to [`1-d20-spec.md`](1-d20-spec.md) — per
`content-spec-template.md`'s localization-style split, nothing in this file is mechanical (no DCs,
no fact shapes, no rule logic). Keyed by id; the technical spec references these keys, never the
literal text. Site: a riverside warehouse Department-7 is quietly searching after a tip — nothing
darker than "somebody's been using this place for something they shouldn't."

**Revised 2026-07-20** (playtest pass): added `PromptKey` per room (fixes the description/prompt
being the same text announced twice — see `1-d20-spec.md`'s revision note), replaced the two
generic `check-success`/`check-failure` keys with room-specific pairs, added combat's missing
per-round narration, and renamed `pc-down` → `pc-lost` to match the redesigned (non-session-ending)
loss state.

| Key | Room | Name | Description text (always, on entry) | Prompt text (only while unsolved) |
| --- | --- | --- | --- | --- |
| `room-1-desc` | 1 | The Loading Dock | Crates stacked head-high, a rolled-up delivery door letting in a slice of grey daylight. Someone's still here. | *(combat room — no prompt text; `monster-desc` covers it)* |
| `room-2-desc` / `room-2-prompt` | 2 | The Storeroom | Shelves of unlabeled boxes, floor to ceiling. | Something about this room feels arranged, not just stored — worth a closer look (Search). |
| `room-3-desc` / `room-3-prompt` | 3 | The Fence Line | A chain-link fence, taller than it looked from outside, cuts across the yard. | It's climbable, but it won't be easy (Climb). |
| `room-4-desc` / `room-4-prompt` | 4 | The Corridor | A narrow hallway with a door at the far end, slightly ajar. | Someone's moving on the other side — better time it right (Listen). |
| `room-5-desc` | 5 | The Central Bay | The warehouse's main floor — open, echoing, and impossible to cross quietly. This is where it starts. | *(combat room — no prompt text; `monster-desc` covers it)* |
| `room-6-desc` / `room-6-prompt` | 6 | The Flooded Passage | A burst pipe has turned this stretch of hallway into a shin-deep wading pool. | No way around it — you'll have to wade through (Swim). |
| `room-7-desc` / `room-7-prompt` | 7 | The Office | A cramped office, papers everywhere. | A locked filing cabinet looks worth getting into (Disable Device). |
| `room-8-desc` | 8 | The Loft | A half-collapsed mezzanine overlooking the bay below. Footsteps creak somewhere above. | *(combat room — no prompt text; `monster-desc` covers it)* |
| `room-9-desc` / `room-9-prompt` | 9 | The Catwalk | A metal catwalk with a section of grating torn away. | A gap just wide enough to worry about (Jump). |

| Key | Use | Text |
| --- | --- | --- |
| `monster-desc` | Combat room entered, unsolved/unfailed | A seedy-looking man steps out of the shadows, rapier already in hand. He doesn't look like he wants to talk. |
| `monster-defeated` | Monster HP reaches 0 | He drops the rapier and doesn't get up. |
| `monster-fled` | 3-round clock expires | He slips past you and out of sight before you can stop him — no catching him now. |
| `pc-hit` | PC's attack connects, monster still standing | Your sword connects — he staggers but he's still on his feet. |
| `pc-miss` | PC's attack misses | You swing and miss. |
| `monster-hit` | Monster's counterattack connects | His rapier catches you — that's going to leave a mark. |
| `monster-miss` | Monster's counterattack misses | He lunges and you sidestep it. |
| `pc-flee` | Player uses `flee` | You break off and put the door between you and him. He doesn't follow. |
| `pc-lost` | PC's HP reaches 0 during `fight` | He gets the better of you this time — you're forced back, bruised. No going through him, not today. |
| `win-announce` | Any win line completes | Three for three — that's enough to call it. Mission complete. |

Room-specific `check` outcomes (replaces the old shared `check-success`/`check-failure` pair):

| Room | Skill | Success key/text | Failure key/text |
| --- | --- | --- | --- |
| 2 | Search | `check-2-success` — You spot the latch, hidden behind a loose panel. | `check-2-failure` — Nothing obvious yet — worth another look. |
| 3 | Climb | `check-3-success` — You get a grip and haul yourself over the fence. | `check-3-failure` — You lose your footing halfway up. No harm done — try again. |
| 4 | Listen | `check-4-success` — You catch the rhythm of footsteps — you know when it's clear. | `check-4-failure` — Hard to tell over the warehouse's noise. Listen again. |
| 6 | Swim | `check-6-success` — You wade through — cold, but you make it across. | `check-6-failure` — The current's stronger than it looks. You're pushed back to where you started. |
| 7 | Disable Device | `check-7-success` — The lock gives with a click. | `check-7-failure` — Trickier than it looks. No damage done — try again. |
| 9 | Jump | `check-9-success` — You clear the gap easily. | `check-9-failure` — You pull up short at the edge. No harm — just try again. |

## Related documents

- [`1-d20-spec.md`](1-d20-spec.md) — the technical spec these keys are referenced from
- [`../../design/1-d20.md`](../../design/1-d20.md) — concept doc
