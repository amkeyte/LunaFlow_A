# 1.d20 World Module — Hollow Keep — Implementation Spec

**Status:** draft
**Companion to:** [`../world-module-extraction-spec.md`](../world-module-extraction-spec.md) — the
`IOneD20WorldModule` contract this content implements against; [`../../design/world-module.md`](../../design/world-module.md)
for the tier's rationale.
**Supersedes:** nothing. A second, independently-authored `IOneD20WorldModule` alongside
`DefaultOneD20WorldModule` — per ticket [0059](../../ClaudeWiki/Processes/Handoffs/0059-second-world-module-reskin-content.md),
proving the World Module tier is a genuine swap, not a relabel.

## Purpose

Supplies the content for the second `IOneD20WorldModule` implementation
`world-module-extraction-spec.md`'s acceptance criteria require: a full reskin of 1.d20's existing
riverside-warehouse content (9 rooms, 9 skill challenges, 5 monsters, 5 loot items, PC sheet, flavor
text) under a new setting — **Hollow Keep**, a collapsed wilderness ruin — with the same structural
shapes so it fills `IOneD20WorldModule` exactly as `DefaultOneD20WorldModule` does.

**This spec does not implement:** the `IOneD20WorldModule` interface itself, `OneD20SessionAssembler`'s
decoupling, or the validation pass — all `world-module-extraction-spec.md`'s work, in progress under
ticket [0060](../../ClaudeWiki/Processes/Handoffs/0060-implement-world-module-extraction-spec-md.md).
It also does not rebalance anything: every numeric value (DCs, ability modifiers, monster stat blocks,
PC stat block) is carried over unchanged from `1-d20-spec.md`/`design/1-d20.md` and
`loot-rom-content-expansion-spec.md` — a reskin proves the type-level claim; new balance numbers
weren't asked for and aren't supplied.

## Scope

**In:**
- A complete `HollowKeepWorldModule : IOneD20WorldModule` content package: 9 `OneD20RoomData` entries,
  9 `OneD20ChallengeTemplate` entries, 5 `OneD20MonsterTemplate` entries, a 5-entry
  `ItemIdByMonsterId` map, `OneD20CombatantStats` for the PC, `PlayerAbilityScores`, and a complete
  `FlavorText` dictionary resolving every key this content references.
- New setting, names, and prose throughout — different premise, different room names/descriptions,
  different monster names/descriptions, different item names, reworded combat/skill-check narration
  where it would otherwise leak the original setting's specifics (a rapier, a warehouse fixture).

**Out:**
- Any change to room *count*, grid position (`Row`/`Col`), or which of the 9 positions is `Fight` vs.
  `SkillCheck` — fixed by the grid/win-line topology `1-d20-spec.md`/`design/1-d20.md` established;
  rooms 1, 5, 8 stay `Fight`, the rest stay `SkillCheck`, same as `DefaultOneD20WorldModule`.
- Any change to which skill/ability/DC each challenge template uses — carried over unchanged from the
  9-challenge pool in `loot-rom-content-expansion-spec.md`'s Content data section. Only each
  challenge's flavor text changes.
- Any change to monster or PC stat blocks — carried over unchanged from `design/1-d20.md` (PC) and
  `loot-rom-content-expansion-spec.md`'s monster roster table. Only names/descriptions change.
- Any change to `OneD20SessionAssembler`, the validation pass, `Program.cs`'s wiring, or any ROM/
  Rulebook structural code — all `world-module-extraction-spec.md`'s / ticket 0060's work, unaffected
  by which `IOneD20WorldModule` gets loaded.
- Simultaneous loading of both World Modules — out of this phase per `world-module-extraction-spec.md`'s
  Scope; this content is exercised alone, swapped in for `DefaultOneD20WorldModule`.

## Premise

A wandering blade-for-hire is paid to find out why the ruins of **Hollow Keep** — abandoned for a
generation — have lately shown firelight in the windows and produced screams that didn't sound
entirely human. Nothing darker than "something's moved back in and it isn't paying rent."
Deliberately distinct in genre and tone from `1-d20-flavor.md`'s modern-day warehouse heist: wilderness
ruin instead of urban site, a lone freelance investigator instead of an organization operative,
bandits-and-worse instead of gang muscle.

## Fact / marker vocabulary

All shapes below are the existing `IOneD20WorldModule`/`OneD20RoomData`/`OneD20ChallengeTemplate`/
`OneD20MonsterTemplate`/`OneD20CombatantStats` records `world-module-extraction-spec.md` defines —
this spec supplies **instances**, not new shapes. Every `*Key` field below is typed here; its literal
text lives in the companion [`1-d20-worldmodule-hollow-keep-flavor.md`](1-d20-worldmodule-hollow-keep-flavor.md).

### Rooms (`IReadOnlyDictionary<int, OneD20RoomData>`, `StartRoomId = 5`)

Same 3×3 grid positions and `ChallengeType` per id as `DefaultOneD20WorldModule` — required by
`world-module-extraction-spec.md`'s validation pass (exactly keys `1..9`) and by this spec's own
match-room-for-room structural choice (Scope, above). `Row`/`Col` values: identical to
`1-d20-spec.md`'s grid, unchanged, not restated here.

| Id | ChallengeType | Name | DescriptionKey |
| --- | --- | --- | --- |
| 1 | Fight | The Outer Gate | `hk-room-1-desc` |
| 2 | SkillCheck | The Larder | `hk-room-2-desc` |
| 3 | SkillCheck | The Collapsed Stair | `hk-room-3-desc` |
| 4 | SkillCheck | The Guard Passage | `hk-room-4-desc` |
| 5 | Fight (start room) | The Hall of the Sunken Throne | `hk-room-5-desc` |
| 6 | SkillCheck | The Sunken Cistern | `hk-room-6-desc` |
| 7 | SkillCheck | The Warden's Study | `hk-room-7-desc` |
| 8 | Fight | The Watch Tower | `hk-room-8-desc` |
| 9 | SkillCheck | The Broken Rampart | `hk-room-9-desc` |

### Challenges (`IReadOnlyDictionary<string, OneD20ChallengeTemplate>`, `Challenges.Count == 9 ≥ 6`)

Skill/ability/DC columns are **unchanged** from `loot-rom-content-expansion-spec.md`'s 9-challenge
pool (Scope, above) — only `Key` naming and the `*Key` flavor fields are this content's own, keeping
the reskin's flavor dictionary self-contained rather than reaching into
`loot-rom-content-expansion-flavor.md`'s keys.

| Key | Skill | SkillAbility | DC | SuccessKey | FailureKey | PromptKey |
| --- | --- | --- | --- | --- | --- | --- |
| `hk-search` | Search | Int | 10 | `hk-search-success` | `hk-search-failure` | `hk-search-prompt` |
| `hk-climb` | Climb | Str | 12 | `hk-climb-success` | `hk-climb-failure` | `hk-climb-prompt` |
| `hk-listen` | Listen | Wis | 8 | `hk-listen-success` | `hk-listen-failure` | `hk-listen-prompt` |
| `hk-swim` | Swim | Str | 10 | `hk-swim-success` | `hk-swim-failure` | `hk-swim-prompt` |
| `hk-disabledevice` | Disable Device | Int | 12 | `hk-disabledevice-success` | `hk-disabledevice-failure` | `hk-disabledevice-prompt` |
| `hk-jump` | Jump | Str | 8 | `hk-jump-success` | `hk-jump-failure` | `hk-jump-prompt` |
| `hk-balance` | Balance | Dex | 11 | `hk-balance-success` | `hk-balance-failure` | `hk-balance-prompt` |
| `hk-spot` | Spot | Wis | 13 | `hk-spot-success` | `hk-spot-failure` | `hk-spot-prompt` |
| `hk-concentration` | Concentration | Con | 12 | `hk-concentration-success` | `hk-concentration-failure` | `hk-concentration-prompt` |

### Monsters (`IReadOnlyDictionary<string, OneD20MonsterTemplate>`, `Monsters.Count == 5 ≥ 3`)

Stat blocks (`Defense`/`AttackBonus`/`DamageDie`/`DamageBonus`/`MaxHp`) are **unchanged**, id-for-id,
from `loot-rom-content-expansion-spec.md`'s monster roster (Scope, above) — same balance texture, new
names and descriptions.

| Key | Name | Defense | AttackBonus | DamageDie | DamageBonus | MaxHp | DescriptionKey |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `hk-outrider` | The Outrider | 13 | +3 | 1d6 | +1 | 8 | `hk-monster-outrider-desc` |
| `hk-brute` | The Brute | 11 | +4 | 1d4 | +3 | 12 | `hk-monster-brute-desc` |
| `hk-longbow` | The Longbowman | 12 | +5 | 2d6 | +0 | 6 | `hk-monster-longbow-desc` |
| `hk-ironclad` | The Ironclad Sergeant | 15 | +2 | 1d8 | +2 | 14 | `hk-monster-ironclad-desc` |
| `hk-footpad` | The Footpad | 14 | +2 | 1d4 | +1 | 6 | `hk-monster-footpad-desc` |

### Loot (`ItemIdByMonsterId`, 1:1 deterministic)

| MonsterId | ItemId | Item name |
| --- | --- | --- |
| `hk-outrider` | `hk-item-blade` | The Outrider's Blade |
| `hk-brute` | `hk-item-knuckledusters` | Studded Knuckledusters |
| `hk-longbow` | `hk-item-recurve` | The Longbowman's Recurve |
| `hk-ironclad` | `hk-item-cuirass` | Battered Boiled-Leather Cuirass |
| `hk-footpad` | `hk-item-knife` | Rusted Skinning Knife |

### Player character

`PlayerCharacter` (`OneD20CombatantStats`) — **unchanged** from `design/1-d20.md`'s Strong-hero fighter
(Scope, above): `Defense: 11, AttackBonus: 4, DamageDie: "1d8", DamageBonus: 3, MaxHp: 11`.

`PlayerAbilityScores` — **unchanged**: `Str: 16, Con: 16, Dex: 10, Int: 10, Wis: 10, Cha: 10`.

The pregenerated character himself is reflavored — a wandering blade-for-hire, not a Department-7
operative — but nothing about his sheet changes; see the flavor companion for his (unused
mechanically, purely narrative) framing text if any is added at implementation.

### FlavorText

Every `*Key` referenced above, plus the fixed combat/session keys `pc-hit`/`pc-miss`/`monster-hit`/
`monster-miss`/`monster-defeated`/`monster-fled`/`pc-flee`/`pc-lost`/`win-announce` and the
`inventory-empty`/`inventory-header` keys `LootCommandHandler`-equivalent logic reads — all under this
module's own `hk-` prefix, resolved fully in the companion flavor file. No key in this module's
`FlavorText` may be missing; the validator (`world-module-extraction-spec.md`'s Mechanics section)
fails load-time on any gap.

## Mechanics / rules

None new. This content runs against `OneD20SessionAssembler`, the declarative rule group, and all five
command handlers exactly as shipped/being-built under ticket 0060 — the whole point of the World
Module boundary is that none of that code changes based on which `IOneD20WorldModule` is loaded. The
session-start randomization (6 of 9 challenges → the fixed skill-check slots, 3 of 5 monsters → the
fixed fight slots, per `loot-rom-content-expansion-spec.md`'s assembly logic) applies identically to
this module's pools.

## Commands & handlers

None new; unchanged from `1-d20-spec.md`/`loot-rom-content-expansion-spec.md` (`move`, `look`, `check`,
`fight`, `flee`, `inventory`/`inv`).

## Acceptance criteria

- [ ] `HollowKeepWorldModule` (or equivalent name — Baraka's naming call) implements
      `IOneD20WorldModule` completely: 9 `Rooms` (keys `1..9`, correct `ChallengeType` per id,
      `StartRoomId = 5`), 9 `Challenges`, 5 `Monsters`, a 5-entry `ItemIdByMonsterId`, `PlayerCharacter`,
      6-entry `PlayerAbilityScores`, and a `FlavorText` dictionary with zero missing keys.
- [ ] `world-module-extraction-spec.md`'s validation pass accepts this module without error (same
      check it runs against `DefaultOneD20WorldModule`).
- [ ] A full session run against this module — `move`/`look`/`check`/`fight`/`flee`/`inventory` all
      work — with zero changes to `LunaFlow.Rulebook`'s structural code, GridMap, D20System, or Loot,
      per `world-module-extraction-spec.md`'s own Done-When bar.
- [ ] Every room, challenge, and monster description reads as Hollow Keep content — no leftover
      warehouse-specific noun (crate, delivery door, filing cabinet, rapier-as-generic-weapon-name)
      anywhere in this module's flavor text.
- [ ] DCs, skill/ability pairings, and all monster/PC numeric stats are byte-for-byte identical to
      their `DefaultOneD20WorldModule` counterparts — diffable, not just "close."
- [ ] The combat-free win line (`[3,6,9]`, all `SkillCheck`) and the rest of the win-line topology are
      unaffected — verified by challenge-type positions matching `DefaultOneD20WorldModule` id-for-id.

## Framework dependencies

None. Pure content, built entirely against the `IOneD20WorldModule` contract
`world-module-extraction-spec.md` defines.

## Open questions / edge cases

- **PC narrative framing** (a line of flavor text introducing the blade-for-hire at session start,
  parallel to Department-7's framing in `1-d20-flavor.md`'s header) isn't pinned to a specific fact/
  key here — `IOneD20WorldModule` has no dedicated "opening narration" field, and inventing one is a
  framework question, not a content one. If Baraka's implementation wants an intro line, the simplest
  fit is folding it into room 5's `hk-room-5-desc` (the start room's description, already announced on
  session start per `1-d20-spec.md`'s startup-announcement fix) — noted here so it isn't quietly
  decided during implementation without a pointer back to this spec.
- **Whether `HollowKeepWorldModule`'s content lives as one file or split by concern** — same
  Baraka's-translation-call `world-module-extraction-spec.md` leaves open for `DefaultOneD20WorldModule`;
  not specced here for the same reason.

## Related documents

- [`../world-module-extraction-spec.md`](../world-module-extraction-spec.md) — the `IOneD20WorldModule`
  contract, validation pass, and session-assembler decoupling this content is built against
- [`../../design/world-module.md`](../../design/world-module.md) — the tier's design rationale
- [`1-d20-spec.md`](1-d20-spec.md) / [`1-d20-flavor.md`](1-d20-flavor.md) — the original content this
  reskins; source of the unchanged room/challenge-type/grid structure
- [`../loot-rom-content-expansion-spec.md`](../loot-rom-content-expansion-spec.md) /
  [`../loot-rom-content-expansion-flavor.md`](../loot-rom-content-expansion-flavor.md) — source of the
  unchanged 9-challenge pool, 5-monster roster, and loot mapping this reskin's numbers are drawn from
- [`1-d20-worldmodule-hollow-keep-flavor.md`](1-d20-worldmodule-hollow-keep-flavor.md) — this spec's
  flavor companion
- [Ticket 0059](../../ClaudeWiki/Processes/Handoffs/0059-second-world-module-reskin-content.md) — the
  ask this spec answers
- [`../../Roadmap/phase-2-8.md`](../../Roadmap/phase-2-8.md) — the phase this content's acceptance
  criterion gates
