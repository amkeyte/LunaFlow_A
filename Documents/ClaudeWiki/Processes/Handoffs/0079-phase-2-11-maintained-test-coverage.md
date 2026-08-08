# 0079 — Phase 2.11 maintained test coverage

**Status:** open
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-08-08
**For:** Nadia (Test Dev)

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

<Filled in by the assignee.>

### Resolution

<Filled in by the opener when closing.>
