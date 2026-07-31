# 0052 — Implement loot-rom-content-expansion-spec.md

**Status:** in-review
**Opened by:** Kofi (PM)
**Opened:** 2026-07-30
**For:** Baraka (Impl Dev)

## Decision

All four items built and manually verified end-to-end against the console host (randomization,
correct per-room monster description, correct 1:1 loot, empty/populated `inventory`, loot-free
skill-check rooms, unchanged room-type topology). `src/`+`example/` build clean; `tests/` does not
compile against the new shapes — expected, tracked as [0054](0054-loot-rom-test-suite.md). Two
implementation-mechanism calls diverge from the spec's literal text without changing player-visible
behavior — tracked as [0055](0055-loot-spec-mechanism-deviations.md) for Amara's ratification. Ready
for Kofi to verify and mark the phase; done-bar is met pending 0054/0055's resolution.

## Details

### Context

Phase 2.7.1 ([`phase-2-7-1.md`](../../../Roadmap/phase-2-7-1.md)) is content-only, wired into
`roadmap.md`. Juno's package — spec and flavor companion — is reviewed and ready
([0051](0051-phase-2-7-1-content-package-ready-for-kickoff.md)). Same pattern as 0034 → CodePlan
0037 for Phase 2.7.

### Ask

Build [`loot-rom-content-expansion-spec.md`](../../../Specs/loot-rom-content-expansion-spec.md) in
full, flavor text from [`loot-rom-content-expansion-flavor.md`](../../../Specs/loot-rom-content-expansion-flavor.md).
Four items:

1. **Loot ROM** (`example/D20SystemExampleD`, `LunaFlow.Rom.Loot`) — `Requires: [ChallengeSolved]`,
   `Provides: []`. First pure-sink ROM; zero project reference to GridMap/D20System.
2. **Monster roster** — `MonsterTemplate` keyed record (5 entries), `RoomChallenge.MonsterId` field,
   `fight`/challenge-prompt read the assigned monster per session.
3. **9-pool skill checks + session-start randomization** — Rulebook-tier content-assembly code
   (not `Program.cs`), injectable `Random` per ticket 0019's seam. `MonsterLootTable` (static) +
   `LootAssignment` facts (session-derived) feed the one generic award rule.
4. **`inventory`/`inv` command** — Loot-owned, reads only its own facts.

`OneD20RuleBook.DependsOnRoms` grows to `["GridMap", "D20System", "Loot"]`. Acceptance criteria are
the spec's own checklist — work each one, don't summarize around it. If content needs something the
toolbox (`Capability`/`IRom`/`ComponentRegistry`/Cartridge surface) doesn't offer, that's a ticket to
Amara, not a workaround.

### Response

Built per [CodePlan](../../../Roadmap/CodePlans/2-7-1.md), all four items:

1. **Loot ROM** — `example/D20SystemExampleD` (`LunaFlow.Rom.Loot`), registered in
   `LunaFlow_A.sln`. `LootRom : IRom`, `Provides: []`, `Requires: [ChallengeSolved]`. Zero project
   reference to `D20SystemExampleA`/`B` (only `D20SystemExampleC`, the Cartridge contract surface,
   plus `LunaFlow.Rom`/`Rules`/`Engine`). `ComponentRegistry.Validate()` passes with three ROMs
   registered; `GridMap` and `Loot` both declare `Requires: [ChallengeSolved]` against D20System's
   one `Provides` entry with no change needed to `Validate()` itself, confirming the spec's claim
   about the existing two-phase check.
2. **Monster roster** — D20System's `MonsterTemplate` is now `(Id, Name, DescriptionKey, Stats)`,
   5 entries; `RoomChallenge` gained `MonsterId`. `FightCommandHandler` resolves the room's assigned
   monster per session; `fight`'s mechanic (attack rolls, lethality-first ordering, 3-round clock) is
   byte-for-byte unchanged, only which stat block feeds it varies.
3. **9-pool skill checks + session-start randomization** — new `OneD20SessionAssembler`
   (`src/LunaFlow.Rulebook/OneD20/`), injectable `Random` (ticket 0019's seam), pure Rulebook-tier
   data transform. `MonsterLootTable` (static, full 5 entries always) + `LootAssignment` facts
   (session-derived, 3 of 5) feed `LootRuleGroup.AwardItem` — one generic rule, not a per-room
   family, exactly as specced.
4. **`inventory`/`inv`** — `InventoryCommandHandler`, Loot-owned, reads only `Query<ItemAwarded>()`.

`OneD20RuleBook.DependsOnRoms` → `["GridMap", "D20System", "Loot"]`.

**Two deviations from the spec's literal mechanism** (behavior-identical, framework-gap-free —
detailed in [0055](0055-loot-spec-mechanism-deviations.md)): `ChallengePromptRule` was not given a
`Match<RoomChallenge>` (generated rule code has no DI seam to resolve flavor text from a dictionary
lookup, per ticket 0043's already-generalized rule); `LookCommandHandler` takes a per-room
monster-max-HP dictionary instead of a scalar (forced once the roster stopped being a singleton).

**Verification:**
- `dotnet build LunaFlow_A.sln` — `src/`+`example/` build clean, 0 errors. `tests/LunaFlow.EngineTests`
  fails to compile against the new shapes (13 errors, all in `Rom/D20System/*Tests.cs`, all directly
  attributable to the shape changes above) — expected per this ticket's confirmed scope boundary
  (Baraka doesn't touch `tests/`), tracked as [0054](0054-loot-rom-test-suite.md).
- Manual playtest via `dotnet run --project src/MyLunaFlowServer`, multiple sessions:
  - Room→challenge and room→monster assignment differs session to session (confirmed: room 5's
    monster varied across runs — Scrapper, Enforcer, Duelist observed — and room 2's/3's/6's
    assigned challenge varied — Balance, Search, Jump, Concentration observed).
  - Each fight room announces its assigned monster's own arrival text (not the old fixed
    `"monster-desc"`), confirmed across sessions with different assignments.
  - Defeating a monster inserts exactly one `ItemAwarded` matching that monster's loot-table entry
    (Scrapper → Folding Knife, Duelist → The Duelist's Rapier observed) and `inventory` lists it by
    name; a lost fight (`RoomFailed`, HP to 0) awards nothing and leaves `inventory` empty.
  - Solving 3 different skill-check challenges (Search, Jump, Concentration) in one session never
    inserted `ItemAwarded` — `inventory` stayed empty throughout.
  - Room *type* topology unaffected across every session observed: fight always at 1/5/8,
    skill-check always at 2/3/4/6/7/9 — guaranteed by construction (`OneD20SessionAssembler` never
    reassigns which positions are which kind, only which specific content fills them).

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>
