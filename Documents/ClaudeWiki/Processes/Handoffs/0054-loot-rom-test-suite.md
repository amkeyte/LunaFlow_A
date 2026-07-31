# 0054 — Test suite needs updating for Phase 2.7.1 (Loot ROM & content expansion)

**Status:** open
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-07-30
**For:** Nadia (Test Dev)

## Details

### Context

[Ticket 0052](0052-implement-loot-rom-content-expansion-spec-md.md) (Phase 2.7.1, against
[`loot-rom-content-expansion-spec.md`](../../../Specs/loot-rom-content-expansion-spec.md)) is
implemented and manually verified end-to-end via the console host. `src/`+`example/` build clean;
`tests/LunaFlow.EngineTests` currently fails to compile against the new shapes — confirmed and
expected going in, same sequence as Phase 2.7's 0037 → 0038 → 0044.

Current compile errors are all in `Rom/D20System/D20SystemHandlerTests.cs` and
`Rom/D20System/D20SystemRuleFixtureTests.cs`, all traceable to the shape changes below.

### Ask

**Shape changes to build against:**

1. `OneD20RoomData` (`src/LunaFlow.Rulebook/OneD20/OneD20ScenarioData.cs`) is now grid-only:
   `(Id, Row, Col, ChallengeType, DescriptionKey)` — `Skill`/`SkillAbility`/`DC`/`SuccessKey`/
   `FailureKey`/`PromptKey` moved off it entirely.
2. New `OneD20ChallengeTemplate(Key, Skill, SkillAbility, DC, SuccessKey, FailureKey, PromptKey)` —
   the 9-entry pool, in `OneD20RulebookContent.Challenges`, keyed by challenge key (`"search"`,
   `"climb"`, etc., not room id).
3. `OneD20MonsterTemplate(Id, Name, DescriptionKey, Stats)` replaces the old singleton `Monster` —
   `OneD20RulebookContent.Monsters` is now a 5-entry `IReadOnlyDictionary<string,
   OneD20MonsterTemplate>`. `OneD20RulebookContent.Monster` (singular) no longer exists.
4. D20System's `MonsterTemplate` (`example/D20SystemExampleA/LunaFlow.Rom/D20System/Facts.cs`) is
   now `(string Id, string Name, string DescriptionKey, CombatantStats Stats)`, and
   `D20SystemRom.Monster` → `D20SystemRom.Monsters` (`IReadOnlyDictionary<string, MonsterTemplate>`,
   5 entries).
5. D20System's `RoomChallenge` gains a 9th positional field, `string? MonsterId` (null for
   SkillCheck rooms, set for Fight rooms).
6. New `OneD20SessionAssembler` (`src/LunaFlow.Rulebook/OneD20/OneD20SessionAssembler.cs`) —
   `OneD20SessionAssembler(Random? random = null)`, same injectable seam as ticket 0019's
   `D20ResolutionMechanic`. `Assemble()` returns `OneD20SessionAssignment` (room→challenge-key,
   room→monster-id, room→item-id maps) — the thing test fixtures now need to construct a
   `D20SystemRom`/`LootRom` deterministically (seed the `Random` for a known assignment, same
   pattern 0019 established).
7. New `LunaFlow.Rom.Loot` project (`example/D20SystemExampleD`) — `LootRom : IRom`, facts
   (`MonsterLootTable`, `LootAssignment`, `ItemAwarded`, `LootPickupText`), `LootRuleGroup`
   (`AwardItem` rule), `InventoryCommandHandler`. No test coverage exists yet.
8. `OneD20RuleBook.DependsOnRoms` now names three ROMs: `["GridMap", "D20System", "Loot"]`.
9. GridMap's `LookCommandHandler` constructor: `int monsterMaxHp` → `IReadOnlyDictionary<int, int>
   monsterMaxHpByRoom`.
10. `OneD20FlavorText.Text` is rekeyed — room-indexed `room-N-prompt`/`check-N-success`/
    `check-N-failure` keys are gone, replaced by challenge-indexed `{key}-prompt`/`{key}-success`/
    `{key}-failure` (9 challenges × 3). `monster-desc` replaced by 5 `monster-{id}-desc` keys. New
    `item-{id}-name`/`item-{id}-pickup`/`inventory-empty`/`inventory-header` keys. Full table in the
    file itself.

**New coverage this content needs** (beyond fixing the existing suite to compile against the shapes
above):

- `LootRom`: `Provides` empty, `Requires` names `ChallengeSolved` — the first pure-sink shape, worth
  its own assertion.
- `LootRuleGroup.AwardItem`: fires exactly once per fight-room `IChallengeSolved` with a matching
  `LootAssignment`, inserts `ItemAwarded`, never fires for a skill-check room's `IChallengeSolved`
  (no `LootAssignment` exists for those rooms).
- `InventoryCommandHandler`: empty case, populated case, reads only `ItemAwarded` (no cross-ROM
  read — worth asserting against the assembly graph too, same bar as GridMap/D20System's zero
  peer-reference checks).
- `OneD20SessionAssembler`: seeded `Random` produces a deterministic, reproducible assignment;
  assigns exactly 6 of 9 challenge keys and 3 of 5 monster ids; never assigns a challenge/monster
  position outside the fixed grid topology (fight positions `{1,5,8}`, skill-check `{2,3,4,6,7,9}`
  always).
- `ComponentRegistry.Validate()` with all three ROMs registered — confirms Loot doesn't trip the
  "two independent components both requiring the same capability" case now that `GridMap` and
  `Loot` both declare `Requires: [ChallengeSolved]`.
- Removing `Loot` from registration produces a clear load-time error, same bar as GridMap/D20System
  (ticket 0052's Done-When).

Two related tickets from the same build, for context: [0055](0055-loot-spec-mechanism-deviations.md)
(two implementation-mechanism calls, to Amara) covers why `RoomChallenge` isn't a live NRules fact
and why `LookCommandHandler` takes a dictionary — worth reading before writing fixtures against
either.

### Response

<Filled in by the assignee.>

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>
