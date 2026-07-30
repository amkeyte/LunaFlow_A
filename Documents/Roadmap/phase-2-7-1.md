# Phase 2.7.1 — Content expansion: Loot ROM, monster roster, room randomization

**Status:** pending

## Overview

Phase 2.7 proved the Cartridge mechanism holds for exactly two peer ROMs coordinating through one
contract surface. This phase is deliberately **content-only — no new framework capability** — and
is specifically designed to exercise parts of that mechanism a two-ROM proof couldn't reach: a
third peer ROM inside the same Cartridge (testing "one or more ROMs" past the pair case), a
capability consumed by more than one peer relationship, and a ROM that's a pure sink (`Requires`
without `Provides`) — none of which Phase 2.7's GridMap/D20System pair had reason to exercise.

Alongside that, it expands 1.d20's actual content: session-randomized room challenges and monsters
(replayability the fixed layout doesn't have today) and a small inventory/loot system tied to which
monster was fought.

**Explicitly not in scope:** any new framework capability. This phase builds entirely against the
existing toolbox — `Capability`/`IRom`/`ComponentRegistry`, the Cartridge contract surface, the
`GeneratorServices` rule pipeline. If content design surfaces something that toolbox can't express,
that's a ticket to the Architect, not something this phase works around — same toolbox-first
approach ticket 0034 established for Phase 2.7 itself.

Also explicitly not in scope: multiple simultaneous Rulebooks. All new content assembles into the
one active Rulebook as additional content modules (room/grid data, challenge/monster data, loot
data) — real multi-Rulebook support is a distinct, real, and deferred need, not attempted here.

## Ownership

- Content design: [Game Designer (Juno)](../ClaudeWiki/Roles/game-designer.md) — the Loot ROM's
  design, the monster roster, randomization behavior, and the buildable spec.
- Implementation: [Dev (Baraka)](../ClaudeWiki/Roles/dev.md), against Juno's spec.
- No framework/boundary work anticipated ([Architect (Amara)](../ClaudeWiki/Roles/architect.md)) —
  if content design surfaces a real gap in the existing mechanism, that's ticketed through the
  established seam, not assumed as part of this phase's scope up front.

## Deliverables

- **A third peer ROM (Loot)** inside the existing D20 Cartridge — its own `IRom`, requiring the
  already-shipped `IChallengeSolved` capability, providing none. First proof of a pure-sink ROM
  shape, and first capability (`IChallengeSolved`) consumed by more than one peer.
- **Five distinct monster templates** (stats, name, description) replacing the single fixed
  monster `1-d20.md` shipped with — explicitly anticipated there as "a content addition for later,
  not required now." `RoomChallenge` grows a nullable `MonsterId` field, parallel to its existing
  `Skill`/`SkillAbility`/`DC` fields.
- **Three additional skill-check challenges** (6 → 9 pool), drawn from the same real d20 Modern
  skill list and "resolvable as a pure number" constraint `1-d20.md` already established.
- **Session-start randomization**: 6 of 9 skill-checks and 3 of 5 monsters assigned to the grid's
  existing fixed room-type slots (room *type*/position unchanged from Phase 2.7 — only which
  specific challenge/monster fills each slot varies per session), via an injectable `Random` seam
  matching ticket 0019's existing pattern. Implemented as Rulebook-tier content-assembly logic, not
  inline in `Program.cs` — content must not become dependent on the composition root's current
  shape, which is expected to keep changing.
- **Monster-to-loot**: a static, Rulebook-authored `MonsterLootTable` (`MonsterId → ItemId`, 1:1
  deterministic — learnable, not randomized per fight), and a session-derived `RoomId → ItemId`
  assignment seeded as data (`LootAssignment` facts), not baked into generated per-room rule code.
- **One generic declarative rule** correlating `IChallengeSolved` + `LootAssignment` → `ItemAwarded`
  — replaces what would otherwise need a per-room generated family, since the assignment is
  session-data, not a content-generation-time constant.
- **A new `inventory`/`inv` command**, owned by Loot, reading only Loot's own facts — no new
  Cartridge contract needed for display.

## Done when

- Three ROMs (GridMap, D20System, Loot) registered, validated, and coordinating inside one
  Cartridge; removing any one produces a clear load-time error — same bar Phase 2.7 established for
  two, now proven for three.
- A full 1.d20 session plays through with randomized room content that varies run to run, monster
  fights awarding the correct 1:1 loot, and `inventory` correctly reporting collected items.
- Skill-check rooms remain loot-free; combat-free win line ([3,6,9]) and overall win-line topology
  are unaffected by randomization (room *type* never moves).
- Full suite green.
- No new Cartridge-tier framework capability was needed to ship this. If one was, it's ticketed to
  the Architect and this phase's Done-When doesn't quietly close around a workaround.

## Related phases

- Previous: [Phase 2.7 — Multi-ROM composition](phase-2-7.md) — the Cartridge mechanism this phase's
  content exercises further, without adding to it.
- Next: none scheduled against this phase; [Phase 2.8 — World Module extraction](phase-2-8.md)
  proceeds independently.

## Related documents

- [`../design/1-d20.md`](../design/1-d20.md) — the concept doc this expands; explicitly names a
  second monster type as a future content addition, not an engine change
- [`../design/cartridge.md`](../design/cartridge.md) — the published-unit mechanism this phase's
  third ROM exercises past the pair case
- [`../Specs/cross-rom-capability-spec.md`](../Specs/cross-rom-capability-spec.md) — the authoring
  contract this phase's content builds against, unchanged
- [`phase-2-7.md`](phase-2-7.md) — the phase whose mechanism this one exercises further
- [`../Specs/loot-rom-content-expansion-spec.md`](../Specs/loot-rom-content-expansion-spec.md) — the
  buildable content spec for this phase
