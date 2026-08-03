# 0054 — Test suite needs updating for Phase 2.7.1 (Loot ROM & content expansion)

**Status:** resolved
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-07-30
**For:** Nadia (Test Dev)

## Decision

Suite compiling again and all six new-coverage items built — 119/119 → 140/140. Fixed the two
existing test classes against the new shapes by pinning one fixed test session assignment (not
exercising `OneD20SessionAssembler`'s randomness there — that's its own new test class) chosen so
every already-pinned seeded-`Random` fight/check outcome kept passing unchanged. One factual
discrepancy found in this ticket's own text, not corrected here: `ComponentRegistry`'s "two
independent components both requiring the same capability" framing doesn't hold — `GridMapRom`
ships `Requires: []` (win-line rules match `IChallengeSolved` via NRules directly, not capability
mediation); only `Loot` declares the `Requires` entry. Note ticket 0053 later relocated every file
this ticket touches into `tests/LunaFlow.ContentTests` — paths below are where they landed, not
where this ticket's Ask describes them.

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

**Compile fix.** `D20SystemHandlerTests.cs` and `D20SystemRuleFixtureTests.cs` (now at
`tests/LunaFlow.ContentTests/Rom/D20System/`, moved there by ticket 0053) both build against the new
shapes by pinning one fixed test session assignment rather than exercising the assembler's own
randomness: fight rooms 1/5/8 → monster `"duelist"` (Defense 13, AttackBonus 3, `1d6+1` damage,
MaxHp 8 — exactly the old fixed singleton Monster's stats), skill-check rooms 2/3/6/9 → challenges
`search`/`climb`/`swim`/`jump` (exactly the old fixed per-room Skill/DC values). That preserved every
already-pinned seeded-`Random` outcome untouched; only the now-dead flavor keys (`room-N-prompt` →
challenge-keyed, `monster-desc` → monster-keyed) needed updating, via two small helpers
(`ChallengePromptFor`/`MonsterDescFor`) added to each class.

**New coverage, all six items from the Ask, now at `tests/LunaFlow.ContentTests/`:**

- `Rom/Loot/LootRomTests.cs` — `Provides` empty (pure-sink shape), `Requires` names `ChallengeSolved`
  paired with `IChallengeSolved`, plus an assembly-reference check (`GetReferencedAssemblies()`)
  confirming Loot's compiled output never references GridMap's or D20System's assembly.
- `Rom/Loot/LootRuleGroupFixtureTests.cs` — `AwardItem` fires exactly once per fight-room
  `IChallengeSolved` with a matching `LootAssignment`, never fires for a skill-check room (no
  assignment exists), never double-awards (`Not<ItemAwarded>` guard), and one generic rule handles
  multiple rooms independently in the same session.
- `Rom/Loot/InventoryCommandHandlerTests.cs` — empty case, populated case, `inv`-alias parity, and a
  case with no GridMap/D20System rule assemblies loaded at all to pin "reads only its own
  `ItemAwarded` facts."
- `Rulebook/OneD20/OneD20SessionAssemblerTests.cs` — seeded-`Random` reproducibility, exactly 6-of-9
  challenge keys and 3-of-5 monster ids drawn (all valid, all unique), fixed grid topology
  (`{1,5,8}` fight / `{2,3,4,6,7,9}` skill-check) held across 50 different seeds, and
  `ItemIdByRoom` correctly derived from each room's monster assignment.
- `Rom/ComponentRegistryContentTests.cs` — all three ROMs registered validates clean end-to-end
  (`ValidateDependsOn` + `Validate()`); omitting `Loot` while `OneD20RuleBook.DependsOnRoms` names it
  throws a clear, specific error naming both `"Loot"` and `"OneD20"` — same bar as ticket 0052's
  Done-When for the two-ROM case.

**Two build-fragility fixes along the way, not scope creep — both needed to make the above actually
pass, not just compile:**
- `tests/LunaFlow.EngineTests.csproj` (at the time; now `LunaFlow.ContentTests.csproj` post-0053) was
  missing the `D20SystemExampleD` project reference — nothing in `Rom.Loot` could compile without it.
- `RoslynRuleCompiler` needs `LunaFlow.Cartridge` force-loaded before compiling `LootRuleGroup` (same
  gotcha as the existing NRules-assembly force-load), which was intermittently missing depending on
  test run order/filter — fixed in both new fixture classes' `ClassSetup`.

**Verification:** `dotnet build LunaFlow_A.sln` clean, full suite 140/140 (confirmed both pre- and
post-0053's project split: 119 baseline → 140 with this ticket's coverage, then re-confirmed as
93 + 47 = 140 after the split relocated files across two projects).

### Resolution

Independently re-verified, not taken on the response alone:
- `dotnet build LunaFlow_A.sln` — clean, 0 errors, 0 warnings.
- `tests/LunaFlow.ContentTests.csproj` references `D20SystemExampleD` — confirmed.
- `dotnet test tests/LunaFlow.ContentTests --filter "FullyQualifiedName~Loot"` — 16/16 pass, spot
  verification of the new Loot coverage (didn't re-run the full 140 myself; per
  `agent-test-workflow.md` that pass is Arryn's, not mine to claim).

**One correction found and fixed, in my own ticket, not this one's:** ticket 0052's Response
incorrectly claimed "`GridMap` and `Loot` both declare `Requires: [ChallengeSolved]`" — the Decision
section here caught it correctly (`GridMapRom.Requires` is empty; win-line rules match
`IChallengeSolved` via NRules directly). Fixed in 0052, credited to this ticket's review. Good catch.

All six new-coverage items and the compile fix confirmed present and passing. Closed by Baraka
(opener).
