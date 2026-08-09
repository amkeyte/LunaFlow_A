# 0079 — Phase 2.11 maintained test coverage

**Status:** resolved
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-08-08
**For:** Nadia (Test Dev)

## Decision

Items 1–3 built as scoped, pinning every check the reference spike ran deterministically. Item 4
skipped — its literal wording (`LunaFlow.ContentTests.csproj` must never reference
`LunaFlow.Rom.Probes`) contradicts item 3's explicit ask that the test project reference it; see
Response for the reasoning and the fallback taken. `dotnet build` full-solution green,
`dotnet test tests/LunaFlow.ContentTests` **88/88** (77 baseline + 11 new).

## Details

### Context

Phase 2.11 ([ticket 0076](0076-implement-phase-2-11-capability-stress-test-spec-md.md), build
complete, in-review) shipped Reputation (a fourth peer ROM) and items 6/8's probe content
(`example/LunaFlow.Rom.Probes`), consolidated GridMap/Loot/Reputation into one project per
[ticket 0078](0078-consolidate-gridmap-d20system-loot-reputation-into-one-implementation-assembly.md).
Same split as every prior content phase (0052→0054, 0060→0062, 0065→0066, 0069→0070): implementation
build plus a throwaway/persisted verification spike is Baraka's; the maintained suite in `tests/` is
yours.

A checked-in, runnable spike at `example/Phase2_11Spike` (`dotnet run --project
example/Phase2_11Spike`) already exercises everything below once, deterministically, and prints
PASS/FAIL per check — worth reading before writing the maintained equivalents, not as a substitute
for them.

### Ask

1. **Fix the 2 broken `LunaFlow.ContentTests` tests** — expected fallout from Reputation joining
   `OneD20RuleBook.DependsOnRoms` and Loot's `Requires`:
   - `ComponentRegistryContentTests.ComponentRegistry_GridMapD20SystemLootAllRegistered_ValidatesSuccessfully`
     needs a fourth `ReputationRom` registered (or a new/renamed test reflecting the real four-ROM
     graph) before `ValidateDependsOn`/`Validate()` will pass again.
   - `LootRomTests.Requires_NamesChallengeSolved_PairedWithIChallengeSolvedContract` asserts
     `Requires.Count == 1`; Loot now requires both `ChallengeSolved` and `ReputationTier` (2).
2. **New coverage for Reputation + Loot's split award rule**, fixture-level (same shape as
   `LootRuleGroupFixtureTests.cs` — bare `RuleEngine`, `Insert`/`Fire`/`Query`, no command dispatch):
   - Accrual: first solve inserts `ReputationPoints(1)`; each subsequent solve replaces it
     incremented by one (retract+insert, not update) — confirm the fact identity actually changes,
     not just the value.
   - Thresholds: `ReputationTierReached("Veteran")` at Total==3, `("Legend")` at Total==6, each
     inserted exactly once even after further solves past the threshold.
   - **The retroactive-grant regression the spike caught (see 0076's Response) is worth a dedicated
     test, not just incidental coverage**: an `ItemAwarded` fact that exists *before* Veteran is
     reached must never gain a `BonusItemAwarded` later, even after the tier fact appears — assert
     this directly, since it's exactly the kind of thing a differently-shaped fixture (e.g. one that
     inserts all facts before a single `Fire()`) would silently fail to catch.
   - The three-Requires edge case: a fight solved once *both* Veteran and Legend tier facts already
     coexist should still produce exactly one `ItemAwarded` and one `BonusItemAwarded` for that room,
     not two (NRules agenda revision should retract the losing activation — the spike confirms this
     empirically at `example/Phase2_11Spike/Program.cs`'s room-8 scenario; worth a named test rather
     than leaving it spike-only).
   - Skill-check rooms (no `LootAssignment`) never produce `ItemAwarded` or `BonusItemAwarded`
     regardless of tier — mirrors the existing `SkillCheckRoomSolved_WithNoLootAssignment_...` case.
3. **Maintained isolated-`ComponentRegistry` fixtures for items 6/8** — the actual "Nadia's
   territory" piece ticket 0076 named explicitly: construct a standalone `ComponentRegistry` inside
   a test fixture (never touching `Program.cs`'s real composition), register
   `BountyThreatProbe`+`HazardThreatProbe`, call `Validate()`, assert the thrown message's *content*
   (names both components and `"ThreatLevel"`) — not just that some exception fires. Same shape for
   `CycleProbeAlpha`+`CycleProbeBeta`, asserting the message identifies a cycle and is textually
   distinguishable from item 6's clash message and from a plain missing-requirement message. Reference
   `example/LunaFlow.Rom.Probes.csproj` from the test project for this (it's built to be referenced
   only by test/spike code — see that project's own header comment).
4. **Assembly-graph check**, if you want a test-enforced version rather than trusting a one-time
   manual `.csproj` inspection: `MyLunaFlowServer.csproj`/`LunaFlow.ContentTests.csproj` must never
   gain a `ProjectReference` to `LunaFlow.Rom.Probes`. Optional — 0076 verified this by inspection: if
   it's cheap to assert in code, do; if it's not, a manual re-check at QA time is an acceptable bar,
   same as prior peer-boundary criteria in this spec family.

### Response

**All items done except 4 (skipped, see below).**

**Item 1** (repointed the 2 broken tests) — see the earlier addendum above; unchanged.

**Item 2 — Reputation accrual/thresholds + Loot's split award rule, fixture-level.**

New file `tests/LunaFlow.ContentTests/Rom/Reputation/ReputationRuleGroupFixtureTests.cs` (5 tests),
same shape as `LootRuleGroupFixtureTests.cs` (bare `RuleEngine`, `Insert`/`Fire`/`Query`, only
`ReputationRuleGroup` compiled/loaded):
- `FirstSolve_InsertsReputationPointsAtOne`
- `SecondSolve_RetractsPriorPoints_LeavesExactlyOneFactAtIncrementedTotal` — asserts exactly one
  `ReputationPoints` fact survives the second solve (not two), which is what actually distinguishes
  retract-then-insert from an additive insert; a bare `ReferenceEquals` check on the record wouldn't
  have, since `new ReputationPoints(...)` always yields a fresh CLR instance regardless.
- `ThirdSolve_CrossesVeteranThreshold_InsertsReputationTierReachedExactlyOnce`
- `SolvesPastVeteran_NeverReinsertsVeteranTier`
- `SixthSolve_CrossesLegendThreshold_InsertsLegendExactlyOnce_VeteranStillExactlyOnce`

Extended `LootRuleGroupFixtureTests.cs` (still only `LootRuleGroup` compiled/loaded — Reputation's
tier facts are inserted directly as synthetic `ReputationTierReached` instances, since these tests
exercise Loot's own rule logic in isolation, not Reputation's accrual) with 4 new tests, class doc
comment updated to describe the `AwardItem`/`AwardItemWithBonus` split:
- `FightRoomSolved_AtOrAboveVeteran_AwardsItemAndBonus_AnnouncesBothPickupTexts`
- `ItemAwardedBelowVeteran_NeverGainsBonusItemAwarded_AfterVeteranReachedLater` — the named
  retroactive-grant regression: solves room 1 below Veteran (only `ItemAwarded`), *then* inserts the
  Veteran fact and fires again, and asserts room 1 never gains a `BonusItemAwarded` — pins the
  `Not<ItemAwarded>` guard on `AwardItemWithBonus` that makes this impossible by construction.
- `RoomSolvedWithBothVeteranAndLegendAlreadyPresent_AwardsExactlyOnceEach_NoDoubleFire` — the
  three-Requires coexistence edge case (both tier facts present before the room solves): exactly one
  `ItemAwarded` and one `BonusItemAwarded`, not two.
- `SkillCheckRoomSolved_WithNoLootAssignment_NeverAwardsAnything_EvenAtVeteranOrAbove`

All five checks mirror `example/Phase2_11Spike/Program.cs`'s `RunItem7()` sequence, which is worth
naming since that's where the exact numbers (Veteran=3, Legend=6, room assignments) came from —
these are the maintained equivalent, not a re-derivation.

**Item 3 — isolated `ComponentRegistry` fixtures for probes 6/8.**

Added `<ProjectReference Include="..\..\example\LunaFlow.Rom.Probes\LunaFlow.Rom.Probes.csproj" />`
to `LunaFlow.ContentTests.csproj` (test-only, per that project's own header comment — never referenced
by `MyLunaFlowServer.csproj`). New file
`tests/LunaFlow.ContentTests/Rom/ComponentRegistryProbeTests.cs` (2 tests), each building its own
standalone `ComponentRegistry`, never touching `Program.cs`'s real composition:
- `BountyAndHazardThreatProbes_BothRegistered_ThrowsNamingBothComponentsAndThreatLevel` — asserts the
  message contains `"BountyThreatProbe"`, `"HazardThreatProbe"`, `"ThreatLevel"`, and does NOT contain
  `"Circular"` (distinguishable from item 8's error).
- `CycleProbeAlphaAndBeta_BothRegistered_ThrowsIdentifyingACircularDependency` — asserts the message
  contains `"Circular"` and both probe names, and does NOT contain `"Duplicate capability"` or
  `"Unsatisfied requirement"` (distinguishable from item 6's error and from a plain missing-req error).

Read `src/LunaFlow.Engine/Components/ComponentRegistry.cs` directly to assert against its exact
thrown-message wording rather than guessing/loosely matching, per the Upstream-source-before-inference
convention.

**Item 4 — skipped.** The literal ask (`MyLunaFlowServer.csproj`**/**`LunaFlow.ContentTests.csproj`
must never reference `LunaFlow.Rom.Probes`) contradicts item 3's explicit instruction to reference
`LunaFlow.Rom.Probes` from the test project — `LunaFlow.ContentTests.csproj` now legitimately does.
The half of the invariant that's actually meaningful — `MyLunaFlowServer.csproj` (the real composition
root) never referencing `Probes` — isn't cheaply test-enforceable from `LunaFlow.ContentTests`: the
test project has no `ProjectReference` to `MyLunaFlowServer.csproj` to reflect against in-process
(the technique `LootRomTests.Assembly_ReferencesNeitherGridMapNorD20SystemAssembly` uses), and adding
one just for this guard, or parsing `MyLunaFlowServer.csproj`'s XML by file path at test time (no
precedent elsewhere in this suite, and path-resolution-fragile across dev/CI), both felt like the
wrong side of the ticket's own cost/benefit call. Falling back to the ticket's own sanctioned
alternative: a manual `.csproj` re-check at QA time. Flagging the wording conflict here rather than
guessing which project it actually meant.

**Verification:** `dotnet build LunaFlow_A.sln` — full solution green, 0 warnings, 0 errors.
`dotnet test tests/LunaFlow.ContentTests` (full suite, no filter) — **88/88 pass** (77 baseline + 11
new: 5 Reputation + 4 Loot + 2 probe). Updated/added `.playlist` files (`ContentTestsLoot.playlist`,
`ContentTestsComponentRegistry.playlist`, `ContentTestsAll.playlist`, new
`ContentTestsReputation.playlist`) and cross-checked every entry against `dotnet test --list-tests`
discovery in both directions (no phantom entries, no discovered test missing from `ContentTestsAll`).

- `ComponentRegistryContentTests.cs` — added `using LunaFlow.Rom.Reputation;`, a `NewReputation()`
  helper (`new ReputationRom()`, no fixture data needed), registered it as a fourth `EngineTier.Rom`
  alongside GridMap/D20System/Loot, and renamed the test to
  `ComponentRegistry_GridMapD20SystemLootReputationAllRegistered_ValidatesSuccessfully` to name the
  real four-ROM graph. Region comment and in-test comment updated to describe the four-ROM shape
  (Reputation's own `Requires: [ChallengeSolved]` satisfied by the same D20System provider Loot
  already depends on). Confirmed `ComponentRegistry_OneD20DependsOnRoms_LootNotRegistered_ThrowsClearError`
  needed no change — `ValidateDependsOn` throws on the first missing name in declared order
  (`GridMap, D20System, Loot, Reputation`), so the deliberately-omitted-Loot case still throws on
  `Loot` before ever reaching `Reputation`.
- `LootRomTests.cs` — `Requires_NamesChallengeSolved_PairedWithIChallengeSolvedContract` renamed to
  `Requires_NamesChallengeSolvedAndReputationTier_PairedWithTheirContracts`, now asserts
  `Requires.Count == 2` and both entries (`ChallengeSolved`/`IChallengeSolved`,
  `ReputationTier`/`IReputationTier`) in `LootRom.Requires`'s declared order. Class doc comment
  updated to match.

`dotnet build LunaFlow_A.sln` — full solution green, 0 warnings, 0 errors. `dotnet test
tests/LunaFlow.ContentTests` (full suite, no filter) — **77/77 pass**. Also updated the two
`.playlist` files that referenced the old method names (`ContentTestsComponentRegistry.playlist`,
`ContentTestsLoot.playlist`, `ContentTestsAll.playlist`) so Test Explorer playlists don't go stale
against the rename.

### Resolution

**Resolved 2026-08-08 (Baraka, opener).** Didn't take the Response on faith — ran it myself:
`dotnet build LunaFlow_A.sln` clean, `dotnet test tests/LunaFlow.ContentTests` **88/88**, and
`--list-tests` discovery also reports 88, matching (no phantom entries, nothing undiscovered).

Read the new/changed test files directly, not just the write-up:
- `ReputationRuleGroupFixtureTests.cs` — the retract-vs-additive-insert distinction is checked the
  right way (`Assert.AreEqual(1, points.Count)` after the second solve, not just the latest total).
- `LootRuleGroupFixtureTests.cs`'s `ItemAwardedBelowVeteran_NeverGainsBonusItemAwarded_AfterVeteranReachedLater`
  — correctly uses **two separate `Insert`+`Fire()` passes** (solve room 1, fire, *then* insert the
  Veteran fact, fire again) rather than seeding everything before one `Fire()` — that two-pass shape
  is exactly what's needed to actually exercise the retroactive-join failure mode the spike caught;
  a single-pass fixture would have passed without proving anything. Good catch that this distinction
  mattered — it's called out in the ticket's own ask for a reason and she applied it correctly.
- `ComponentRegistry_OneD20DependsOnRoms_LootNotRegistered_ThrowsClearError` correctly left unchanged
  — verified her reasoning: `ValidateDependsOn` iterates `DependsOnRoms` in declared order
  (`GridMap, D20System, Loot, Reputation`) and throws on the first missing name, so the
  deliberately-omitted-Loot case still throws on `Loot` before Reputation is ever reached. Confirmed
  against `ComponentRegistry.cs` source, not assumed.
- `ComponentRegistryProbeTests.cs` asserts against the exact exception wording read off
  `ComponentRegistry.cs` directly (upstream-source-before-inference), including the negative checks
  (clash message doesn't contain "Circular", cycle message doesn't contain "Duplicate capability" or
  "Unsatisfied requirement") — matches the spike's own assertions.

**Item 4's skip is correct, and the contradiction is my mistake, not hers.** Ticket 0076's binding
criterion was always "`Program.cs`/`MyLunaFlowServer` never references the probes" — I sloppily
transcribed that into 0079 as *both* projects, which directly conflicts with item 3's own requirement
that `LunaFlow.ContentTests` reference `LunaFlow.Rom.Probes` to write the fixtures at all. Her fallback
(manual `.csproj` inspection, same bar every other peer-boundary criterion in this spec family already
accepts) is the right call — independently re-confirmed here: `MyLunaFlowServer.csproj`'s reference
list still names only `D20SystemExampleA`, `D20SystemExampleC`, `OneD20SharedRoms`.

Playlist hygiene checked too — `ContentTestsReputation.playlist` exists, `ContentTestsAll`/
`ContentTestsComponentRegistry`/`ContentTestsLoot` all updated for the renames, no stale method-name
references found.

No defects found. No fix requested. Phase 2.11's full-suite-green bar (0076) is now unblocked on the
test side — still needs an Arryn-run `latest.trx` per the standing convention before 0076 itself can
close.
