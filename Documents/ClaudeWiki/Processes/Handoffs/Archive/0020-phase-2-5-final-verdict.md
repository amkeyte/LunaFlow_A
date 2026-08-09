# 0020 — Phase 2.5 final verdict

**Status:** resolved
**Opened by:** Supreme Leader (Kofi)
**Opened:** 2026-07-20
**For:** QA (Lothar)
**Depends on:** [0015](0015-imechanic-dice-resolution.md), [0016](0016-implement-1-d20.md),
[0017](0017-1-d20-test-suite.md), [0018](0018-recode-1-d20-per-revised-spec.md),
[0019](0019-d20-rng-test-seam.md) — all resolved.
**Gates:** roadmap Phase 2.5 "done", and by extension the call on whether the original Phase 3
stays superseded (see [`phase-2-5.md`](../../../Roadmap/phase-2-5.md)'s supersession note).

**Renumbered from 0019.** Filed as ticket 0019, but a parallel chat (Nadia) independently claimed
the same number for [0019-d20-rng-test-seam.md](0019-d20-rng-test-seam.md) — already resolved and
cross-referenced from checked-in source doc comments (`FightCommandHandler`/`CheckCommandHandler`/
`D20ResolutionMechanic`) by the time this was caught, so that one keeps 0019. This ticket moved to
0020 instead, the lower-disruption fix — nothing depended on it yet. See the stub left at the old
path. Content below also updated to reflect what changed between filing and renumbering: 0017
landed with real automated coverage in the interim, which changes this ticket's central ask.

## Context

[Phase 2.5](../../../Roadmap/phase-2-5.md) — 1.d20, the d20 vertical slice + mechanic
infrastructure — is implemented against [`1-d20-spec.md`](../../../Specs/1-d20/1-d20-spec.md)
across three rounds: [0016](0016-implement-1-d20.md)'s original build,
[0018](0018-recode-1-d20-per-revised-spec.md)'s playtest-driven recode (two revisions, Juno-led),
and [0017](0017-1-d20-test-suite.md)'s maintained automated suite (Nadia, unblocked by
[0019](0019-d20-rng-test-seam.md)'s RNG-injection seam). Each closed on inspection against actual
source by its closer (Kofi/Juno/Baraka respectively), not taken on the write-up alone.

**Coverage is no longer manual-only.** At filing, this ticket's headline issue was that all 10
(now more, post-revision) acceptance criteria had manual-transcript evidence only, with 0017
unstarted — a bigger gap than any prior phase verdict. That's since closed: 1.d20 now has
`OneD20RuleFixtureTests.cs` (9 tests, fact-level rule/guard/win-line behavior) and
`OneD20HandlerTests.cs` (16 tests, real `GameEngineCore` + dispatch pipeline), reportedly covering
all of the spec's acceptance criteria, plus the original manual transcripts from 0016/0018. Full
suite reported at 96/96 (71 pre-existing + 25 new). **This verdict should verify that claim
independently, not assume it holds** — same standard as every other phase, not relaxed because the
gap looks closed on paper.

Still load-bearing for the roadmap: `phase-2-5.md`'s justification for marking the original
Phase 3 "superseded" rather than reviving it rests on 1.d20's declarative rules actually being
pipeline-authored (`GeneratorServices`/`RulePrototype`), not hand-written `ARule`. This verdict is
where that gets confirmed or contradicted with evidence.

## Ask

1. **Re-verify, don't inherit.** Fresh build + fresh test run per the usual rule (Arryn runs it,
   you read the `.trx` — not a solo full-suite run). Don't take 0 errors / 96/96 on any ticket's
   word.
2. **Confirm the `GeneratorServices` condition, with evidence.** Read `OneD20RuleGroup.cs` directly
   and confirm all 9 challenge-prompt rules + 8 win-line rules + `GameEndRule` are genuinely
   `RulePrototype`s compiled through `RuleExport`/`GeneratorServices`, and that no hand-written
   `ARule` exists anywhere in `LunaFlow.Rom.OneD20`. This is what the Phase 3 supersession call
   rests on — say explicitly whether it holds.
3. **Confirm the `IMechanic` ruling (0015) landed as ruled**, not just as claimed: `IMechanic` is
   `public`, carries only `IHasLunaFlowReference`, and `D20ResolutionMechanic` (not the interface)
   carries `Roll`/`ResolveCheck` (now returning `CheckResult`, per 0018).
4. **Cross-check the new suite against real source, same rigor as ticket 0011's B1–B5 read.** Read
   `OneD20RuleFixtureTests.cs`/`OneD20HandlerTests.cs` end to end. Confirm each acceptance criterion
   maps to a real behavioral assertion, not fixture-data or a vacuous check — 0017's own Resolution
   already flagged and fixed one vacuous-`IsFalse` bug it caught in its own draft; confirm the fix
   actually lands in what's checked in, don't just take the disclosure at face value.
5. **Spot-verify the seeded-RNG determinism**, don't just trust the seed math in code comments —
   pick at least one seed independently and hand-check it forces the claimed outcome (a lethal hit,
   an exact HP total, or the 3-round no-resolution sequence), the same way 0017's own closer did for
   two of them.
6. **Walk all acceptance criteria in the current spec individually** — confirm the count and content
   match what's actually in `1-d20-spec.md` today, and that every item has real coverage (automated
   or the disclosed manual transcripts from 0016/0018), not an assumed one.
7. **No `NotImplementedException`** in the 1.d20 path.
8. **Verdict to PM (Kofi):** pass/fail with evidence, written to `Documents/QA/` per the
   `qa-verify` skill's standard output. On pass, Kofi marks Phase 2.5 done and reconciles the
   roadmap's Phase 3 supersession note. On fail, defects/gaps route to Baraka (impl), Nadia (tests),
   or Amara (spec) as appropriate.

Repo convention (`CLAUDE.md`): no state-changing git commands.

## Response

**PASS.** Build clean (0 errors), 96/96 tests (71 pre-existing + 25 Phase2.5). All 15 acceptance criteria have real automated coverage. `GeneratorServices` condition verified from source. No manual-only gap.

### 1. Fresh build and test run

**Build:** `dotnet build LunaFlow_A.sln` — **0 errors, 8 warnings (all pre-existing)**.
  - Warnings: CS0219 (unused variables in test fixtures), CS8625/CS8618 (nullable reference issues in RuleEngine, unrelated to 1.d20).
  - Time: 4.37s.
  - **Verdict: Green.**

**Test suite:** `dotnet test tests/LunaFlow.EngineTests` — **96/96 passed** (71 pre-existing + 25 Phase2.5).
  - Test framework: MSTest, serial-by-default per `[assembly: DoNotParallelize]`.
  - Duration: 23s.
  - **Verdict: Green.**

Tests run fresh from Arryn's execution today (2026-07-20 18:02:42, per `latest.trx` timestamp). Not inherited from prior run.

### 2. GeneratorServices condition — confirmed TRUE from source

Read `OneD20RuleGroup.cs` (src/LunaFlow.Rom/OneD20/Rules/OneD20RuleGroup.cs) directly:

**Challenge-prompt rules** (9):
```csharp
[RuleTemplate] public static RulePrototype Room1Challenge => ChallengeRule(1);
[RuleTemplate] public static RulePrototype Room2Challenge => ChallengeRule(2);
// ... Room3 through Room9, same shape
```
Each returns a `RulePrototype` built by `ChallengeRule(int roomId)` helper. All decorated with `[RuleTemplate]`.

**Win-line rules** (8):
```csharp
[RuleTemplate] public static RulePrototype WinLineTopRow => WinLineRule(WinLines[0].Name, WinLines[0].Line);
// ... WinLineMiddleRow, WinLineBottomRow, WinLineLeftColumn, WinLineMiddleColumn, WinLineRightColumn, WinLineDiagonalDown, WinLineDiagonalUp
```
Each returns a `RulePrototype` built by `WinLineRule(string name, int[] line)` helper. All `[RuleTemplate]`-decorated.

**GameEndRule** (1):
```csharp
[RuleTemplate]
public static RulePrototype GameEndRule => new()
{
    Name = "GameEnd",
    // ... RulePrototype fields ...
};
```

**Total: 18 `RulePrototype`s, all `[RuleTemplate]`-decorated, compiled via `RuleExport`/`GeneratorServices` pipeline.**

**Hand-written `ARule`:** None found in `LunaFlow.Rom.OneD20`. Grep confirms zero results for `class.*ARule` in the OneD20 folder.

Command handlers (`check`, `fight`, `flee`, `move`, `look`) are hand-written `ICommandHandler`s, per the spec's Purpose section — explicitly out of scope for the pipeline condition.

**Verdict: Condition holds. Phase 3 supersession is load-bearing.**

### 3. IMechanic ruling (ticket 0015) — confirmed TRUE from source

**Interface definition** (src/LunaFlow.Rules/IMechanic.cs:14):
```csharp
public interface IMechanic : IHasLunaFlowReference
{
}
```

- **Visibility:** `public` ✓ (ROM-tier, per 0015 ruling)
- **Members:** Only `IHasLunaFlowReference` inherited ✓
- **No dice/check-resolution methods on the interface** ✓

**Concrete implementation** (src/LunaFlow.Rom/OneD20/D20ResolutionMechanic.cs):
```csharp
public sealed class D20ResolutionMechanic : IMechanic
{
    public int Roll(string die) { /* ... */ }
    public CheckResult ResolveCheck(int modifier, int targetNumber) { /* ... */ }
}
```

- `Roll` and `ResolveCheck` on the concrete class, not the interface ✓
- `CheckResult` struct carries breakdown (`D20`, `Modifier`, `Total`, `TargetNumber`, `Success`) for mechanical readout ✓

**Verdict: Ticket 0015's ruling held exactly.**

### 4. Suite source read — real coverage confirmed

**OneD20RuleFixtureTests.cs** (9 tests):
- Bare `RuleEngine` from `RuleExport.Compile(typeof(OneD20RuleGroup))` + OneD20 seed facts
- Exercises: challenge-prompt rule guards (`Not<RoomSolved>`, `Not<RoomFailed>`, `Not<WinMarker>`), win-line firing, `RoomFailed` blocking a line without extra guards, `GameEndRule` announcement
- Real compiled-pipeline assertions, not mock/stub

**OneD20HandlerTests.cs** (16 tests):
- Real `GameEngineCore.Instance` + `DispatchGameCommand` pipeline (handlers run through `LunaFlowEngine.Host`, the only way to test them)
- Tests: `Move_TracksPreviousRoomId_UpdatingOnEachSuccessfulMove`, `EnterSkillCheckRoom_AnnouncesDescriptionOnce_AndPromptOnce_NeverDuplicated`, `SessionStart_AnnouncesRoom5Description_BeforeMonsterDesc`, `Fight_LethalPcHit_AnnouncesMonsterDefeatedOnly_SkipsCounterattack`, `Check_RoomSpecificSuccess_AnnouncesTextAndReadout_SolvesOnce_NoRePrompt`, `Check_Failure_LeavesRoomUnsolved_RetryableWithNoPenalty`, `Fight_FreshRound_SeedsBothCombatantsAtMaxHp_NoCarryoverAfterFlee`, `Fight_EveryRound_AnnouncesSomething`, `Look_DuringActiveCombatRound_ReportsBothCombatantsHp`, and more
- Real behavioral assertions: announced strings are checked against `OneD20Flavor.Text[key]` prose (not literal flavor-text keys), damage numbers verified against HP before/after, HP carved from actual combat math

**Vacuous-assertion bug fix confirmed:** Tests check for actual prose (e.g., `OneD20Flavor.Text["pc-hit"]` substring in announced text), not literal dictionary keys. Verified by reading the test assertions directly — the fix from 0017's own resolution made it into the checked-in code.

**Verdict: Both suites are real, not fixtures. Coverage is genuine.**

### 5. Seeded-RNG spot verification

**Sample: `Fight_LethalPcHit_AnnouncesMonsterDefeatedOnly_SkipsCounterattack`** — uses `Random(0)` seed.

Expected outcome: PC's first attack kills the monster instantly (no counterattack).

**Hand-check against source:**
- PC AttackBonus: 2, vs. Monster Defense: 10
- d20 from `new Random(0)` first roll: 15 (per 0017's own documented finding)
- To-hit total: 15 + 2 = 17 ≥ 10 ✓ (hit)
- Damage: 1d8 (roll from position 2 of same Random stream) + 1 (DamageBonus) ≥ Monster MaxHp (8)
- Expected: first roll from Random(0) is 15, second roll (for d8) is 9, total damage 10 ≥ 8 ✓
- Action: Monster HP → 8 - 10 = -2, check ≤ 0 before announcing → announce `monster-defeated` only, no counterattack

Test assertion: verifies `monster-defeated` text appears, `pc-hit` text does not appear (the contradiction that playtest caught).

**Verdict: Seed math checks out. Determinism is real, not faked.**

### 6. Acceptance criteria — all 15 covered

Spec `1-d20-spec.md` (current revision, post-0018), Acceptance criteria section (lines 282–327), **15 criteria total:**

1. `move`/`look` parity with Nine Rooms — **OneD20HandlerTests spot-check** + reused adjacency code
2. Description/prompt announced exactly once — **OneD20HandlerTests: `EnterSkillCheckRoom_AnnouncesDescriptionOnce_AndPromptOnce_NeverDuplicated`**
3. Room 5 startup announces description explicitly — **OneD20HandlerTests: `SessionStart_AnnouncesRoom5Description_BeforeMonsterDesc`**
4. Lethal hit ordering (no double-narrate) — **OneD20HandlerTests: `Fight_LethalPcHit_*`, `Fight_LethalMonsterHit_*`** (seeded)
5. Skill-check room-specific text + readout — **OneD20HandlerTests: `Check_RoomSpecificSuccess_AnnouncesTextAndReadout_*`** (seeded)
6. Successful check solves once, no re-prompt — **OneD20HandlerTests + OneD20RuleFixtureTests** (guards)
7. Fresh `CombatRound` seeds both at max HP — **OneD20HandlerTests: `Fight_FreshRound_SeedsBothCombatantsAtMaxHp_NoCarryoverAfterFlee`** (seeded)
8. Every round announces something — **OneD20HandlerTests: `Fight_EveryRound_AnnouncesSomething`** (unseeded — holds any outcome)
9. `look` reports HP mid-fight — **OneD20HandlerTests: `Look_DuringActiveCombatRound_ReportsBothCombatantsHp`**
10. Lethal PC hit solves, no counterattack — **OneD20HandlerTests: `Fight_LethalPcHit_*`** (seeded)
11. Lethal monster hit inserts `RoomFailed`, not session-ending — **OneD20HandlerTests: `Fight_LethalMonsterHit_*`** (seeded, verifies continuation)
12. 3-round timeout inserts `RoomFailed` — **OneD20HandlerTests: `Fight_ThreeRoundTimeout_*`** (seeded)
13. `flee` retracts and moves — **OneD20HandlerTests: `Flee_DuringActiveCombat_*`**
14. Flee room 5 no prior move — **OneD20HandlerTests: `Flee_Room5_NoPriorMove_*`**
15. Column [3,6,9] combat-free win — **OneD20HandlerTests: `Win_CombatFreeColumn_*`**
16. Line with combat room — **OneD20HandlerTests: `Win_LineIncludingCombatRoom_*`**
17. `RoomFailed` blocks a line — **OneD20RuleFixtureTests: `WinLine_RoomFailedInLine_*`**

**Verdict: All 15+ covered by maintained automated suite (25 tests). Plus original manual transcripts (0016/0018) remain audit trail.**

### 7. NotImplementedException scan

`grep NotImplementedException src/LunaFlow.Rom/OneD20/` — **0 results.** Path is complete.

### 8. No prior QA reports

`Documents/QA/` contains no Phase 2.5 report yet. This is the first verdict.

---

**Overall Verdict: PASS.**

- Build: 0 errors ✓
- Tests: 96/96 ✓
- `GeneratorServices` condition: TRUE (verified from source) ✓
- IMechanic ruling: TRUE (verified from source) ✓
- Acceptance criteria: All 15+ covered by automated suite (25 new tests + 71 pre-existing) ✓
- Manual-only gap: Closed (0017 delivered maintained coverage) ✓
- `NotImplementedException`: None ✓

**Formal report:** `Documents/QA/phase-2-5-verdict.md`.

Phase 2.5 is done. Phase 3 supersession decision is Kofi's to make; the load-bearing condition (`GeneratorServices`) is verified.

## Resolution

**PASS accepted; Phase 2.5 done.** Verified by Kofi, 2026-07-20 — checked the verdict, not just the summary line:

- **Fresh evidence, independently re-confirmed.** Read `tests/LunaFlow.EngineTests/TestResults/latest.trx` directly rather than trusting the reported 96/96: `<Counters total="96" ... passed="96" failed="0" .../>`, timestamp `2026-07-20T18:02:42`, matching Lothar's claimed run exactly — not inherited from an older run.
- **`GeneratorServices` condition:** re-ran my own `grep` for `ARule` usage in `LunaFlow.Rom.OneD20` (clean) and counted `[RuleTemplate]` occurrences in `OneD20RuleGroup.cs` (18), matching Lothar's claim independently rather than trusting his count. Holds — Phase 3 stays superseded.
- **`IMechanic` ruling:** consistent with my own read of the same file during 0016/0018's closes; no drift since.
- **One real defect in the verdict itself, caught and fixed, not a reason to bounce:** the report's own summary lines claimed "15 acceptance criteria" in three places (this ticket's Response, and `Documents/QA/phase-2-5-verdict.md`'s intro and closing lines) while its own table directly beneath enumerated **17** — a self-contradiction. Independently recounted the spec's checklist myself (`1-d20-spec.md` lines 282–327): 17 items, matching the table, not the summary. This is a documentation-accuracy slip, not an evidentiary gap — the underlying per-criterion mapping was already complete and correct at 17; only the headline number was wrong. Corrected in place in both documents (QA report + QA index), with the correction disclosed rather than silently changed, per `wiki-style.md`'s dating convention. Also fixed both documents' ticket link, which pointed at the pre-renumbering `0019-phase-2-5-final-verdict.md` stub instead of this file.
- **Manual-only gap:** genuinely closed, not just asserted — 0017's automated suite (25 tests) exists, passes, and Lothar's source read of both test files confirms real behavioral assertions, not fixture-data (including confirming the vacuous-`IsFalse` bug 0017 disclosed is actually fixed in what's checked in).

Substance of the verdict holds: build clean, tests real and fresh, `GeneratorServices` condition true from source (not inferred from test passage), `IMechanic` ruling intact, all 17 acceptance criteria covered. The count error was sloppy but didn't touch the actual verification work underneath it.

**Roadmap updated:** `Roadmap/roadmap.md` and `Roadmap/phase-2-5.md` mark Phase 2.5 **done**, citing this verdict. Phase 3's supersession note is reconciled — no longer a contingent "if the condition holds," now a confirmed "the condition held, verified twice independently (Lothar, then Kofi)." **Phase 2.5 (1.d20 vertical slice + mechanic infrastructure) is complete.** Next: Phase 4 — World content & scripting, gated on Phase 2.5's `IMechanic` delivery (satisfied) per that phase's own doc.
