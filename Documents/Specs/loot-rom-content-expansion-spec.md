# Loot ROM & Content Expansion — Implementation Spec

**Status:** draft
**Companion to:** [`../design/1-d20.md`](../design/1-d20.md) — the concept doc this expands; also
exercises [`../design/cartridge.md`](../design/cartridge.md)'s mechanism past the two-ROM case
Phase 2.7 proved.
**Supersedes:** nothing. Additive to [`grid-rom-d20-split-spec.md`](grid-rom-d20-split-spec.md) —
every fact/capability shipped there is unchanged; this spec adds a third peer ROM and expands
1.d20's room/monster content on top of it.

## Purpose

Turns [`phase-2-7-1.md`](../Roadmap/phase-2-7-1.md) into buildable content: a third peer ROM
(**Loot**) inside the existing D20 Cartridge, a five-monster roster replacing 1.d20's single fixed
monster, three additional skill-check challenges, and session-start randomization of which specific
challenge/monster fills each of the grid's fixed room-type slots.

**This spec does not implement:** any new framework capability. Everything here builds against the
toolbox Phase 2.7 already shipped — `Capability`/`IRom`/`ComponentRegistry`, the Cartridge contract
surface, `GeneratorServices`. Where this spec's content needed to choose between two ways of using
that toolbox (e.g., whether the session's randomized loot assignment is code or data), the choice is
stated and justified, not left for Baraka to guess.

## Scope

**In:**
- A new peer ROM, **Loot**, requiring the existing `IChallengeSolved` capability and providing
  none — a pure-sink shape not yet exercised.
- Five monster templates (replacing the single fixed monster), each with its own stats, name, and
  room-arrival flavor.
- Three additional skill-check challenges (6 → 9 pool).
- Session-start randomization: which 6 of 9 skill-checks and which 3 of 5 monsters fill the grid's
  existing fixed room-type slots. Room *type*/position is unchanged from Phase 2.7 — only content
  within each slot varies.
- A static, deterministic monster→item mapping, and a session-derived room→item assignment fed to
  Loot as data.
- A new `inventory`/`inv` command.

**Out:**
- Room *type* randomization (which physical positions are fight vs. skill-check) — fixed, because
  the win-line topology (including the combat-free right column) was designed around it.
- Equipping items or any mechanical effect from inventory — items are collectible/display only in
  this pass. Revisit only against a concrete case that needs one.
- Skill-check-room loot — only defeating a monster awards an item.
- Any change to GridMap. Its facts, content, and commands are untouched by this entire spec.
- Multiple simultaneous Rulebooks — all new content assembles into the one active Rulebook.
- Any new Cartridge-tier contract for `LootRom`'s own capabilities — it provides none.

## Fact / marker vocabulary

### D20System (extended; existing facts unchanged except where noted)

`RoomChallenge` grows one field, parallel to its existing nullable `Skill`/`SkillAbility`/`DC`:

```csharp
public sealed record RoomChallenge(
    int Id,
    RoomChallengeType ChallengeType,
    string? Skill, string? SkillAbility, int? DC,
    string? SuccessKey, string? FailureKey, string? PromptKey,
    string? MonsterId);   // NEW — Fight rooms only; null for SkillCheck rooms
```

`MonsterTemplate` changes from an implicit singleton to an identified, keyed record — this is the
shape change that makes a roster possible:

```csharp
public sealed record MonsterTemplate(
    string Id, string Name, string DescriptionKey, CombatantStats Stats);
```

Static content (constructor-injected, alongside the existing `RoomChallenge` dictionary):

```csharp
IReadOnlyDictionary<string, MonsterTemplate> Monsters   // 5 entries this spec adds, keyed by Id
```

`CombatRound`, `RoomSolved`, `RoomFailed` — **unchanged**. A fight still seeds `CombatRound` from
whichever `MonsterTemplate` this session's `RoomChallenge.MonsterId` resolves to; the fact itself
doesn't care which monster it is.

### Loot (`LunaFlow.Rom.Loot`, new project — `example/D20SystemExampleD`, following the existing
naming convention; content, not framework, so it stays in `example/` alongside GridMap/D20System,
same reasoning ticket 0038 already established)

Static content (constructor-injected):

```csharp
public sealed record MonsterLootTable(IReadOnlyDictionary<string, string> ItemIdByMonsterId);
    // 1:1, deterministic — same monster always drops the same item. Rulebook-authored.
```

Facts (NRules, Loot-owned):

```csharp
// Seeded once at session start — one per fight room, derived from that session's monster
// assignment. Not present for skill-check rooms; that absence is what keeps them loot-free.
public sealed record LootAssignment(int RoomId, string ItemId);

// Accumulates, never retracted — same convention as RoomSolved/RoomFailed.
public sealed record ItemAwarded(string ItemId, int RoomId);
```

No contract interfaces are added to `LunaFlow.Cartridge` for this ROM. Loot only **requires** the
already-published `IChallengeSolved`; it provides nothing, so nothing needs a type to consume from
it. First real proof that a peer can be a pure sink in this mechanism.

### Capability declarations

```csharp
// GridMap — unchanged from grid-rom-d20-split-spec.md
Provides: [ PlayerLocation, PlayerMovement ]
Requires: [ ChallengeSolved ]

// D20System — unchanged
Provides: [ ChallengeSolved, CombatStatus ]
Requires: [ PlayerLocation, PlayerMovement ]

// Loot — new
Provides: [ ]
Requires: [ new("ChallengeSolved", typeof(IChallengeSolved)) ]
```

`ChallengeSolved` now has **two** independent requirers (GridMap's win-line rules, Loot's award
rule) off **one** provider (D20System). Nothing about `ComponentRegistry.Validate()`'s existing
two-phase check needs to change for this — it already validates "every declared `Requires` has
exactly one provider," which doesn't care how many components declare the same one. First time it's
actually exercised that way, not a new check.

`OneD20RuleBook.DependsOnRoms` grows a third entry: `["GridMap", "D20System", "Loot"]`.

## Mechanics / rules

### D20System — challenge-prompt rule, combat branch (changed)

The existing challenge-prompt rule's combat branch currently announces a fixed `"monster-desc"`
key, the same text for all three fight rooms. It now needs to announce **whichever monster this
session assigned to this room's** `DescriptionKey`. That requires the rule to read the room's
`MonsterId` at fire time — which resolves an open question `grid-rom-d20-split-spec.md` left open
("whether `RoomChallenge` needs to be seeded as an NRules fact"): **yes, now it does.** The rule
needs `Match<RoomChallenge>(() => challenge, c => c.Id == roomId)` to read `challenge.MonsterId`,
then look up `Monsters[challenge.MonsterId].DescriptionKey` in `Then()` — the same shape the
original (pre-split) `Match<Room>` used to read `room.PromptKey`, just carried one field further.

Skill-check rooms are unaffected — `PromptKey` is still read directly off `RoomChallenge`, unchanged.

### D20System — `fight` (changed: parametrized, not re-mechanic'd)

`FightCommandHandler` resolves the room's `RoomChallenge.MonsterId` to a `MonsterTemplate` via the
injected `Monsters` dictionary, and seeds `CombatRound` from that template's `Stats` instead of a
single fixed `MonsterTemplate`. Every other rule — lethality-first ordering, the 3-round clock, no
HP carrying over between fights — is **unchanged**; only which stat block feeds it varies per room,
per session.

### Loot — award rule (new)

**One generic rule**, not a per-room family — this is the point of seeding `LootAssignment` as data
rather than baking room IDs into generated code the way `ChallengeRule`/`WinLineRule` do:

```
When().Match<IChallengeSolved>(() => solved)
      .Match<LootAssignment>(() => award, a => a.RoomId == solved.RoomId)
      .Not<ItemAwarded>(ia => ia.RoomId == solved.RoomId)
Then: insert ItemAwarded(award.ItemId, solved.RoomId); announce pickup flavor for award.ItemId
```

Fires once per fight room that both solves and has a loot assignment. Skill-check rooms produce
`IChallengeSolved` too (via `RoomSolved : IChallengeSolved`) but never have a matching
`LootAssignment`, so the rule simply never fires for them — no type-level distinction between
challenge kinds is needed here at all.

### Loot — `inventory`/`inv` (new command, owned by Loot)

Reads only `Query<ItemAwarded>()` — no cross-ROM read of any kind. Lists each item's display name
via Loot's own injected flavor lookup. No items: says so plainly, doesn't error.

### Session-start content assembly (Rulebook-tier, not `Program.cs`)

Per Arryn's standing note against content code depending on `Program.cs`'s shape, the following
logic lives in Rulebook-tier content-assembly code (e.g. a method on `OneD20RulebookContent` or a
sibling type) — `Program.cs` calls it and passes the result to ROM constructors, exactly like it
already does for today's static content:

1. Choose 6 of the 9 skill-check challenges and assign them to the grid's 6 fixed skill-check
   positions (2, 3, 4, 6, 7, 9).
2. Choose 3 of the 5 monsters and assign them to the grid's 3 fixed fight positions (1, 5, 8).
3. Derive `LootAssignment(RoomId, ItemId)` for each fight room from step 2's monster assignment and
   the static `MonsterLootTable` — same session, same source of truth, no separate randomization.

All three steps take the same injectable `Random` (mirroring ticket 0019's seam — constructor
parameter, defaults to a real `Random` if not supplied, swappable for a seeded one in tests).

## Commands & handlers

| Command | Owner ROM | Reads | Writes | Cross-ROM |
| --- | --- | --- | --- | --- |
| `inventory` / `inv` | Loot | `ItemAwarded` (own) | — | none |

All five of GridMap's/D20System's existing commands (`move`/`look`/`check`/`fight`/`flee`) are
otherwise unchanged from `grid-rom-d20-split-spec.md` — `fight`'s only change is which stat block it
reads, per Mechanics above, not its shape.

## Content data

### Monster roster

Same balance instinct `1-d20.md`'s original monster used (attack/Defense numbers close enough that
a fight can plausibly go either way inside the 3-round clock), deliberately varied per monster so
the pool has real texture rather than five palette-swaps of the same fight:

| Id | Name | Defense | Atk bonus | Damage | HP | Feel |
| --- | --- | --- | --- | --- | --- | --- |
| duelist | The Duelist | 13 | +3 | 1d6+1 | 8 | Unchanged from 1-d20.md's original monster |
| brawler | The Brawler | 11 | +4 | 1d4+3 | 12 | Even odds, high HP — runs long, more likely to hit the 3-round clock |
| marksman | The Marksman | 12 | +5 | 2d6 | 6 | Glass cannon — hits hard and dies fast, swingiest fight in the pool. Realizes `1-d20.md`'s own anticipated "ranged option" |
| enforcer | The Enforcer | 15 | +2 | 1d8+2 | 14 | Tanky — hardest to hit, longest fight |
| scrapper | The Scrapper | 14 | +2 | 1d4+1 | 6 | Low-HP, low-damage — the pool's easy option |

All five checked against the PC (attack +4, Defense 11) and land in a plausible-either-way band, per
the same math `1-d20.md` used to validate its original monster.

### New skill checks

Same "resolvable as a pure number, nothing Bluff/Diplomacy-shaped" constraint, drawn from the same
real d20 Modern skill list, filling in abilities the existing six under-use (Dex and Con weren't
used by any of the original six):

| Skill (ability) | DC | Modifier | Success |
| --- | --- | --- | --- |
| Balance (Dex) | 11 | +0 | 50% |
| Spot (Wis) | 13 | +0 | 40% |
| Concentration (Con) | 12 | +3 | 60% |

### Monster → item (1:1, deterministic)

| Monster | Item |
| --- | --- |
| duelist | The Duelist's Rapier |
| brawler | Brass Knuckles |
| marksman | Backup Piece |
| enforcer | Kevlar Vest |
| scrapper | Folding Knife |

Names above are authoritative; the actual flavor prose (monster arrival descriptions, pickup lines,
inventory listing text, and the rekeyed skill-check prompts/outcomes) lives in the companion
[`loot-rom-content-expansion-flavor.md`](loot-rom-content-expansion-flavor.md), per
`content-spec-template.md`'s localization-style split — not inlined here.

## Acceptance criteria

- [ ] `LootRom : IRom` is registered, `Requires: [ChallengeSolved]`, `Provides: []`;
      `ComponentRegistry.Validate()` passes with three ROMs registered against `LunaFlow.Cartridge`.
- [ ] `OneD20RuleBook.DependsOnRoms` names all three: `"GridMap"`, `"D20System"`, `"Loot"`.
- [ ] `LootRom`'s project has zero reference to `GridMap` or `D20System` — assembly graph, not just
      a passing test, same bar as `grid-rom-d20-split-spec.md`.
- [ ] Two independent components (`GridMap`, `Loot`) both declare `Requires: [ChallengeSolved]`
      against `D20System`'s one `Provides` entry, and both fire correctly off the same `RoomSolved`
      insert in the same session.
- [ ] Each of the three fight rooms announces the correct assigned monster's `DescriptionKey` on
      arrival — verified across at least two different sessions with different monster assignments,
      not just one fixed run.
- [ ] Defeating a monster inserts exactly one `ItemAwarded` for that room, matching the session's
      `MonsterLootTable` assignment; a lost or fled fight awards nothing.
- [ ] Solving a skill-check room never inserts `ItemAwarded` — verified across all 9 skill-check
      challenges, not assumed from the type-level argument alone.
- [ ] `inventory` lists every awarded item by name and says so plainly when empty; no cross-ROM
      reference appears in its implementation.
- [ ] Two separate session runs (different RNG seeds) produce different room→challenge and
      room→monster assignments — randomization is real, not cosmetic.
- [ ] Room *type* (which positions are fight vs. skill-check) is identical across every session —
      the combat-free right column (`[3,6,9]`) and the rest of the win-line topology from
      `1-d20-spec.md` are unaffected.
- [ ] Full suite green, read from an Arryn-run `latest.trx`.

## Framework dependencies

None anticipated. This spec is built entirely against Phase 2.7's shipped toolbox
(`Capability`/`IRom`/`ComponentRegistry`, the Cartridge contract surface, `GeneratorServices`). If
implementation surfaces a real gap this toolbox can't express, that's a ticket to the Architect —
not assumed here, and not something this spec's Done-When should quietly work around.

## Open questions / edge cases

- **Room description vs. randomized challenge — resolved, not just accepted.** `GridRoomData.DescriptionKey`
  stays fixed per room position, completely unchanged (GridMap is untouched by this spec). Rather
  than redesign per-room descriptions to track the session's assignment — real new GridMap↔D20System
  coupling for a flavor nicety — the challenge/monster-side text is written **deliberately generic**
  instead, so it never presupposes which room it lands in. See the flavor file for the actual text;
  the room's own description never mentions the challenge, and the challenge/monster text never
  mentions the room.
- **Prompt/success/failure text keys move from room-indexed to challenge-indexed** (e.g.
  `search-prompt` rather than `room-2-prompt`), since a challenge can now land on any of its type's
  positions. This is a real change to `1-d20-flavor.md`'s existing key scheme, not just an addition
  — flagged here so it isn't discovered mid-implementation. Two of the six existing prompts
  (`room-2-prompt`'s "not just stored," `room-7-prompt`'s "filing cabinet") named room fixtures
  directly and needed rewording along with the rekey; the other four were already generic enough to
  carry over as-is.
- **Combat narration also had monster-specific bleed, caught during the same pass:** `monster-defeated`
  and `monster-hit` named "the rapier" specifically (the original single monster's weapon), which
  breaks the moment an unarmed Brawler or a Marksman with a pistol is in the fight. Genericized
  once, shared across all five monsters, rather than writing five weapon-specific variants of each
  line — same "overly generic" instinct applied to the combat lines, not just the arrival
  description.
- **Monster HP/stat variety and the 3-round clock:** tankier monsters (Enforcer, Brawler) are more
  likely to hit the clock and flee rather than being defeated outright. That's accepted as real
  texture, not a balance defect — the clock's escape valve exists precisely for this case.
- **Whether items should ever be consumable/equippable** — explicitly out of scope, per Scope
  above. Revisit only against a concrete gameplay case that needs it.

## Related documents

- [`grid-rom-d20-split-spec.md`](grid-rom-d20-split-spec.md) — the two-ROM split this spec adds a
  third peer to; every fact/capability there is unchanged
- [`1-d20/1-d20-spec.md`](1-d20/1-d20-spec.md) — the base 1.d20 spec this expands
- [`1-d20/1-d20-flavor.md`](1-d20/1-d20-flavor.md) — existing flavor keys (room descriptions,
  unchanged); this spec's companion flavor file supersedes its prompt/success/failure table only
- [`loot-rom-content-expansion-flavor.md`](loot-rom-content-expansion-flavor.md) — this spec's
  flavor companion: rekeyed skill-check prompts/outcomes, monster arrival descriptions, item names/
  pickup text, `inventory` command text
- [`../design/1-d20.md`](../design/1-d20.md) — concept doc; explicitly anticipates a second monster
  type and a ranged option as future content, both realized here
- [`../design/cartridge.md`](../design/cartridge.md) — the mechanism this spec's third ROM exercises
- [`cross-rom-capability-spec.md`](cross-rom-capability-spec.md) — the authoring contract, unchanged
- [`../Roadmap/phase-2-7-1.md`](../Roadmap/phase-2-7-1.md) — the phase this spec belongs to
- [`../Roadmap/phase-2-7.md`](../Roadmap/phase-2-7.md) — the phase whose mechanism this exercises
  further
