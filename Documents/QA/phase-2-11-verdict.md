# Phase 2.11 (Cross-ROM Capability Mechanism Stress Test) — QA Verdict

**Verdict:** PASS
**Date:** 2026-08-08
**Ticket:** [0080](../ClaudeWiki/Processes/Handoffs/0080-phase-2-11-qa-gate.md)
**Build:** `dotnet build LunaFlow_A.sln` — Build succeeded, 0 Warning(s), 0 Error(s) (final clean state, after this pass's own scratch artifact was removed — see Method below).
**Suite:** 181/181 (93 EngineTests + 88 ContentTests, arithmetic independently confirmed), read from a single merged `tests/LunaFlow.EngineTests/TestResults/latest.trx`. `runUser` attribute reads `ARRYNSTATION\amkey`; timestamp `2026-08-08T20:54:03` — today, same session as this verification. Same merged-trx shape already established for Phase 2.9/2.10.

## Method

Unlike Phase 2.10, this phase's automated suite has **real, targeted coverage** of its own new
mechanisms — `ComponentRegistryProbeTests` (items 6/8), `ReputationRuleGroupFixtureTests` (item 7's
accrual/thresholds), and four new/renamed `LootRuleGroupFixtureTests` cases (item 7's bonus-award
rule, the retroactive-grant guard, tier coexistence, skill-check exclusion). Per ticket 0080's explicit
instruction not to take the pasted messages/claims on faith, this pass:

1. Read `ComponentRegistry.cs`'s actual `Validate()`/`DetectCyclesDfs` source directly and confirmed
   the two thrown-message formats (`"Duplicate capability '{name}'..."` vs. `"Circular dependency
   detected..."`) are structurally distinguishable from each other and from the plain `"Unsatisfied
   requirement..."` message, before ever looking at the tests that assert on them.
2. Independently re-ran `ComponentRegistryProbeTests`'s both tests live (not just read the trx) —
   both passed, asserting exact message content, not just that an exception fired.
3. Confirmed `MyLunaFlowServer.csproj`'s full project-reference graph (`Engine`/`Rules`/`Rom`/
   `Rulebook`/`D20SystemExampleA`/`D20SystemExampleC`/`OneD20SharedRoms`) and each of those projects'
   own references — zero path to `LunaFlow.Rom.Probes`, direct or transitive. Only
   `LunaFlow.ContentTests.csproj` and the unrelated `Phase2_11Spike.csproj` reference it.
4. Ran a real `dotnet run --project src/MyLunaFlowServer` bootstrap (item 7's explicit "not just a
   unit test" ask) — confirmed live: `ROMs registered: 4 (GridMap, D20System, Loot, Reputation) +
   Rulebook: OneD20`, `Rules loaded: 24 total (... Loot: 2, Reputation: 4)`.
5. Wrote one throwaway scratch test reproducing the `DependsOnRoms`-omits-`Reputation` negative case
   directly — the existing passing test in `ComponentRegistryContentTests.cs` only covers Loot being
   omitted, not Reputation, and item 4 explicitly asked for the Reputation case to be reproduced
   itself rather than inferred by analogy (the underlying code path is generic, but the ticket asked
   for it directly). Passed; the scratch file was deleted after use.
6. Read `LootRuleGroup.cs`'s `AwardItemWithBonus` source directly and confirmed the
   `Not<ItemAwarded>(ia => ia.RoomId == solved.RoomId)` guard is what closes the retroactive-grant gap
   — the same mechanism the test's own comment claims, not just a re-statement of the test's intent.
7. Confirmed via the trx's per-class test counts that no existing test file lost coverage in the
   0078 consolidation (`D20SystemHandlerTests`: 16, unchanged; `LootRuleGroupFixtureTests`: 8, four
   original + four Phase 2.11 additions, one renamed to reflect its extended scope, none dropped).
8. Solve site for item 7's spec-literal "early-session fight (no bonus) vs. late-session fight
   (bonus) in the same transcript" bullet: relied on the fixture-level proof rather than staging a
   fresh RNG-dependent live playthrough — see Deviations below for why, and what would close the gap
   if a live transcript is still wanted.

## Criteria (`phase-2-11-capability-stress-test-spec.md`'s Acceptance criteria list)

| Criterion | Evidence | Result |
| --- | --- | --- |
| Item 6: clash throws naming both components and `"ThreatLevel"` | Source: `ComponentRegistry.cs:74-75`. Live: independently re-ran `BountyAndHazardThreatProbes_BothRegistered_ThrowsNamingBothComponentsAndThreatLevel` — passed, asserts all three strings present and that the message does **not** read as item 8's cycle error. | PASS |
| Item 6: neither probe ROM referenced from `MyLunaFlowServer`/`Program.cs` | Read every `.csproj` in `MyLunaFlowServer`'s reference graph directly — zero path to `LunaFlow.Rom.Probes`. | PASS |
| Item 7: `ReputationRom` registers/validates alongside GridMap/D20System/Loot (4 ROMs) | Test: `ComponentRegistry_GridMapD20SystemLootReputationAllRegistered_ValidatesSuccessfully` (passed). Live: `dotnet run --project src/MyLunaFlowServer` prints `ROMs registered: 4 (GridMap, D20System, Loot, Reputation)`. | PASS |
| Item 7: `DependsOnRoms` names all four, verified as a distinct check | Source: `OneD20RuleBook.cs:21` — `["GridMap", "D20System", "Loot", "Reputation"]`. `ComponentRegistry.ValidateDependsOn` (`ComponentRegistry.cs:42-53`) is a separate method from `Validate()`, doesn't read `Provides`/`Requires` at all. Live negative case reproduced myself (existing test only covers Loot omitted): a scratch test omitting Reputation from registration threw naming both `"Reputation"` and `"OneD20"`. | PASS |
| Item 7: `ReputationTierReached("Veteran")` at 3 solves, exactly once; same for `("Legend")` at 6 | Source: `ReputationRuleGroup.cs`'s `VeteranReached`/`LegendReached` rules (`Match<ReputationPoints>(Total >= N)` + `Not<ReputationTierReached>(Tier == "...")` guard). Fixture: `ThirdSolve_CrossesVeteranThreshold_...`, `SolvesPastVeteran_NeverReinsertsVeteranTier`, `SixthSolve_CrossesLegendThreshold_...` — all against the real compiled `RuleExport.Compile(typeof(ReputationRuleGroup))`, not a hand-simulation. 0080 item 5 explicitly permits "live or fixture-reproduced" for this bullet. | PASS |
| Item 7: fight room's `ItemAwarded` produces `BonusItemAwarded` only at Veteran+, early-vs-late in one transcript | Source: `LootRuleGroup.cs`'s `AwardItem`/`AwardItemWithBonus` split, mutually exclusive via the tier `Match`/absence. Fixture: `FightRoomSolved_AtOrAboveVeteran_AwardsItemAndBonus_...` (positive) plus the pre-existing `FightRoomSolved_WithMatchingAssignment_...` (negative, no tier fact, no bonus) — both against the real compiled `LootRuleGroup`. **Not verified as a single live session transcript** — see Deviations. | PASS (evidence-tier deviation noted) |
| Item 7: skill-check rooms never produce `BonusItemAwarded` | Fixture: `SkillCheckRoomSolved_WithNoLootAssignment_NeverAwardsAnything_EvenAtVeteranOrAbove` — Veteran fact present, room has no `LootAssignment`, zero `ItemAwarded`/`BonusItemAwarded`. | PASS |
| Item 8: cycle throws identifying a dependency cycle, distinguishable from item 6 and from a plain missing-requirement error | Source: `ComponentRegistry.cs:160-162` (`"Circular dependency detected..."`, textually disjoint from `"Duplicate capability"` and `"Unsatisfied requirement"`). Live: independently re-ran `CycleProbeAlphaAndBeta_BothRegistered_ThrowsIdentifyingACircularDependency` — passed, explicitly asserts absence of both other messages' key phrases. | PASS |
| Item 8: neither probe ROM referenced from `MyLunaFlowServer`/`Program.cs` | Same assembly-graph check as item 6 (same project, same isolation). | PASS |
| No existing criterion from `1-d20-spec.md`/`loot-rom-content-expansion-spec.md`/`phase-2-10-content-coverage-spec.md` regressed | Per-class trx test counts confirm no coverage lost across the 0078 consolidation (`D20SystemHandlerTests`: 16 unchanged; `LootRuleGroupFixtureTests`: 8 = 4 original + 4 new, none dropped). D20System itself is untouched by 0078 per the ticket's own context (independently re-verified by Amara) — not re-derived here. | PASS |
| Full suite green, Arryn-run `latest.trx` | See Suite line above — `runUser`, timestamp, and 93+88 arithmetic all confirmed directly from the trx, not asserted. | PASS |

## Items 6/8 framework-finding call (per Amara's standing ruling, ticket 0067)

**Stated explicitly, not left to silence: neither probe surfaced anything for Amara.** Both
`ComponentRegistry.Validate()`'s failure modes fire for the right reason with clear, mutually
distinguishable, non-overlapping error content — item 6's duplicate-provider path and item 8's cycle
path are genuinely different code paths (`Validate()`'s Phase 1 vs. `DetectCyclesDfs`), producing
messages that name the right components and can't be confused with each other or with a bare
missing-requirement error. Confirmed against the actual thrown-message source, not inferred from the
tests' own assertions. `non-goals.md`'s capability-arbitration deferral holds: nothing here suggests
the current hard-reject-on-clash behavior is wrong or needs arbitration. No follow-up ticket to Amara.

## Deviations from spec

- **Item 7's "early-session fight (no bonus) vs. late-session fight (bonus) in the same transcript"
  bullet was verified via deterministic fixture-level proof against the real compiled `LootRuleGroup`,
  not a single live `dotnet run` session.** The fixture tests (`FightRoomSolved_WithMatchingAssignment_...`
  for the no-bonus case, `FightRoomSolved_AtOrAboveVeteran_AwardsItemAndBonus_...` for the bonus case)
  exercise the exact same `AwardItem`/`AwardItemWithBonus` rule pair a live session would, compiled
  and fired for real — the same evidence tier this codebase already treats as sufficient everywhere
  else content-vs-rule correctness is at stake (e.g. `D20SystemRuleFixtureTests`). A live playthrough
  was considered but rejected as *weaker* evidence for this specific claim: production dice are
  unseeded, so proving the early/late contrast in one live transcript would mean scripting retries
  around random check/fight outcomes, which is less rigorous than the fixtures' precise fact-level
  control, not more. If Kofi still wants a literal live transcript for the record (distinct from
  proving the mechanism), that's a cheap follow-up — flagging the choice rather than silently
  substituting one form of evidence for another.

## Superseded reports

None — first verdict for Phase 2.11.

## Tickets filed

None. No FAIL or UNPROVEN criterion, and items 6/8's explicit framework-finding call is a clean "no
issue" — stated per Amara's ruling, not a ticket.
