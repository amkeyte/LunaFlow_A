# Phase 2.5 — QA Verdict

**Verdict:** PASS  
**Date:** 2026-07-20  
**Ticket:** [0020 — Phase 2.5 final verdict](../ClaudeWiki/Processes/Handoffs/0020-phase-2-5-final-verdict.md)
(filed as 0019, renumbered to 0020 after a numbering collision — see that ticket's note)  
**Build:** Clean. `dotnet build LunaFlow_A.sln` — 0 errors, 8 warnings (all pre-existing: unused variable assignments in test fixtures, nullable reference issues in RuleEngine). Build succeeded.  
**Suite:** `dotnet test tests/LunaFlow.EngineTests` — **96/96 passed**. Includes 25 new Phase2.5-categorized tests (9 fixture-level rule tests + 16 handler-level tests) from [ticket 0017](../ClaudeWiki/Processes/Handoffs/0017-1-d20-test-suite.md), plus 71 pre-existing tests from earlier phases.

## Load-bearing conditions

### GeneratorServices (confirmed TRUE)

Read `OneD20RuleGroup.cs` directly: all 18 declarative rules are `[RuleTemplate]`-decorated `RulePrototype`s:
- 9 challenge-prompt rules (`Room1Challenge`...`Room9Challenge`), each a static property returning a `RulePrototype`
- 8 win-line rules (`WinLineTopRow`, `WinLineMiddleRow`, `WinLineBottomRow`, `WinLineLeftColumn`, `WinLineMiddleColumn`, `WinLineRightColumn`, `WinLineDiagonalDown`, `WinLineDiagonalUp`)
- 1 `GameEndRule`

All authored via `RuleExport`/`GeneratorServices` pipeline per the spec. **No hand-written `ARule` exists in the 1.d20 path.** (Command handlers — `check`, `fight`, `flee`, `move`, `look` — are hand-written `ICommandHandler`s, not subject to this condition per the spec's Purpose section.)

**Verdict: TRUE.** Phase 3 supersession holds.

### IMechanic ruling (confirmed TRUE)

`IMechanic` interface: `public interface IMechanic : IHasLunaFlowReference` (src/LunaFlow.Rules/IMechanic.cs:14).
- **Visibility:** `public` — ROM-tier, per ticket 0015.
- **Members:** Only `IHasLunaFlowReference` (inherited).
- **Concrete operations:** `D20ResolutionMechanic` carries `Roll(string die)` and `ResolveCheck(int modifier, int targetNumber)` — both absent from the interface.

Ticket 0015's ruling **held exactly as specified.**

## Criteria

The spec's Acceptance criteria section lists **17** observable behaviors (checkbox list, lines 282–327 in `1-d20-spec.md` — independently recounted by Kofi on close, correcting this report's own summary line below, which undercounted at 15 despite the table beneath it already listing all 17). All 17 are now covered by the automated suite from ticket 0017 plus the manual verification work in tickets 0016/0018.

| # | Criterion | Evidence | Result |
| --- | --- | --- | --- |
| 1 | `move`/`look` behave identically to Nine Rooms on the same grid (adjacency, rejection at edges, room 5 start). | OneD20HandlerTests spot-check: one valid move (room 5→6), one rejected edge move (room 3→4 blocked). Fixture-level adjacency inherits Nine Rooms' proven shape. | **PASS** |
| 2 | Entering any unsolved/unfailed room announces `DescriptionKey` exactly once (from handler) and `PromptKey`/`monster-desc` exactly once (from rule) — never duplicate, never conflate. | OneD20HandlerTests: `EnterSkillCheckRoom_AnnouncesDescriptionOnce_AndPromptOnce_NeverDuplicated`, `Move_TracksPreviousRoomId_UpdatingOnEachSuccessfulMove`. Spec revision 1 explicitly addressed the double-announce gap. | **PASS** |
| 3 | Session startup announces room 5's `DescriptionKey` once, explicitly — transcript shows description before `monster-desc`, not `monster-desc` alone. | OneD20HandlerTests: `SessionStart_AnnouncesRoom5Description_BeforeMonsterDesc`. Spec revision 1 (post-playtest) added explicit startup announcement after the challenge-prompt rule no longer covers `DescriptionKey`. | **PASS** |
| 4 | Lethal PC hit announces `monster-defeated` only — never `pc-hit` first. Same for lethal monster hit: `pc-lost` only, never `monster-hit` first. (Verify via transcript: killing blow's damage against pre-hit HP confirms lethality, not just the string.) | OneD20HandlerTests: `Fight_LethalPcHit_AnnouncesMonsterDefeatedOnly_SkipsCounterattack` (seeded deterministically, damage checked against pre-hit monster HP). `Fight_LethalMonsterHit_AcrossTwoRounds_AnnouncesPcLostOnly_FailsRoom_SessionContinues` (damage checked against pre-hit PC HP). Spec revision 2 (second playtest) fixed the contradiction. Source verification: `FightCommandHandler` checks lethality *before* announcing. | **PASS** |
| 5 | Each of 6 skill-check rooms resolves `check` using its own skill/ability/DC, announces room-specific `SuccessKey`/`FailureKey` (not shared generic), includes mechanical roll/DC readout. Failed `check` is retryable with no penalty. | OneD20HandlerTests: `Check_RoomSpecificSuccess_AnnouncesTextAndReadout_SolvesOnce_NoRePrompt`, `Check_Failure_LeavesRoomUnsolved_RetryableWithNoPenalty`. Seeded determinism (Random seed 0) ensures exact outcomes. Code review: each room's `Room` fact carries distinct `SuccessKey`/`FailureKey`; handler constructs readout from `CheckResult.D20`/`Modifier`/`TargetNumber`. | **PASS** |
| 6 | Successful `check` inserts exactly one `RoomSolved` for that room; challenge-prompt never re-triggers afterward. | OneD20RuleFixtureTests + OneD20HandlerTests: fixture verifies `RoomSolved` guards block prompt rule; handler verifies single solve. Code review: `CheckCommandHandler` inserts once, prompt rule's `Not<RoomSolved>` guard prevents re-fire. | **PASS** |
| 7 | `fight` in fresh `CombatRound` seeds both monster and PC at full `MaxHp` — never carried from prior visit or different room. | OneD20HandlerTests: `Fight_FreshRound_SeedsBothCombatantsAtMaxHp_NoCarryoverAfterFlee`. Code review: `CombatRound` constructor in `FightCommandHandler` seeds both from `PlayerCharacter.Stats.MaxHp` and `MonsterTemplate.Stats.MaxHp` on new round. Seeded test (Random 0 → reliable HP values) confirms no carryover post-`flee`. | **PASS** |
| 8 | Every `fight` round announces something — hit/miss line plus damage/HP on connect — even inconclusive rounds. | OneD20HandlerTests: `Fight_EveryRound_AnnouncesSomething` (deliberately uses real `Random()` — holds for any outcome). Code review: `FightCommandHandler` announces per outcome: `pc-miss`, `pc-hit` + damage + remaining monster HP, `monster-miss`, `monster-hit` + damage + remaining PC HP. Spec revision 1 (playtest) fixed the silent-round gap. | **PASS** |
| 9 | `look` while `CombatRound` active reports both combatants' current HP. | OneD20HandlerTests: `Look_DuringActiveCombatRound_ReportsBothCombatantsHp`. Code review: `LookCommandHandler` reads `CombatRound`, formats output with both `MonsterHp` and `PlayerHp`. | **PASS** |
| 10 | `fight` round dropping monster to ≤0 HP solves room and skips counterattack. (Verify: transcript shows lethal blow's damage against pre-hit HP.) | OneD20HandlerTests: `Fight_LethalPcHit_AnnouncesMonsterDefeatedOnly_SkipsCounterattack` (seeded, damage verified). Code review: `FightCommandHandler` reduces `MonsterHp`, checks ≤0 *before* announcing, inserts `RoomSolved`, retracts `CombatRound`, stops. | **PASS** |
| 11 | `fight` round dropping PC to ≤0 HP inserts `RoomFailed` (not session-ending) and retracts `CombatRound`; session continues. | OneD20HandlerTests: `Fight_LethalMonsterHit_AcrossTwoRounds_AnnouncesPcLostOnly_FailsRoom_SessionContinues` (seeded, verifies `RoomFailed` insertion, commands execute afterward). Code review: `RoomFailed` marker, no `PlayerDown` state exists. Spec revision 1 (playtest) removed session-ending behavior. | **PASS** |
| 12 | Three rounds of `fight` with neither combatant at ≤0 inserts `RoomFailed`, retracts `CombatRound`, makes `RoomSolved` unreachable; later `fight` in same room is rejected. | OneD20HandlerTests: `Fight_ThreeRoundTimeout_NeitherReducedToZero_FailsRoomPermanently` (seeded deterministic outcomes, verifies failure and permanent lock). Code review: `FightCommandHandler` increments `RoundsElapsed`, checks == 3, inserts `RoomFailed`. Guards on `check`/`fight` require `Not<RoomFailed>`. | **PASS** |
| 13 | `flee` during active `CombatRound` retracts it without `RoomSolved`/`RoomFailed`, moves PC to `PreviousRoomId`; later `fight` starts fresh at full HP. | OneD20HandlerTests: `Flee_DuringActiveCombat_RetractsCleanly_NoSolveNoFail_ReturnsToPreviousRoom` (seeded for subsequent fresh-HP verification). Code review: `FleeCommandHandler` retracts `CombatRound`, inserts new `PlayerPosition`, no state-end markers. | **PASS** |
| 14 | Fleeing room 5's fight with no prior `move` doesn't move (no `PreviousRoomId` exists) and doesn't throw. | OneD20HandlerTests: `Flee_Room5_NoPriorMove_DoesNotMoveOrThrow`. Code review: `FleeCommandHandler` checks `PreviousRoomId` existence, announces flee without move if absent. | **PASS** |
| 15 | Completing column [3,6,9] (skill checks only, no combat) fires `WinMarker`/`GameEndRule` — combat-free win path real. | OneD20HandlerTests: `Win_CombatFreeColumn_FiresWinMarkerAndGameEnd`. OneD20RuleFixtureTests: `WinLine_RoomFailedInLine_NeverFires_WhileUnrelatedLineStillFires` (verifies no interference). | **PASS** |
| 16 | Completing a line with combat room (e.g. [1,2,3]) fires `WinMarker` correctly when room solved via `fight`. | OneD20HandlerTests: `Win_LineIncludingCombatRoom_FiresWinMarker_WhenSolvedViaFight`. OneD20RuleFixtureTests: `WinLine_CombatRoomInLine_FiresWhenSolvedByFactAlone`. | **PASS** |
| 17 | `RoomFailed` on any room in a line permanently blocks that line as win path; verify for both timeout and lost-fight causes. | OneD20RuleFixtureTests: `WinLine_RoomFailedInLine_NeverFires_WhileUnrelatedLineStillFires` (fixture-level guards prove no duplicate state check needed). OneD20HandlerTests: both timeout and lost-fight paths verified to insert `RoomFailed` with identical consequence. | **PASS** |

## Red flags

- **`NotImplementedException`:** None found. Grep search across `src/LunaFlow.Rom/OneD20/` returns no results.
- **Prior QA reports:** None exist in `Documents/QA/` for Phase 2.5 — this is the first.
- **Build warnings:** 8 pre-existing (CS0219 unused variables, CS8625/CS8618 nullable reference issues in unrelated RuleEngine code). Not Phase 2.5 regressions. No new warnings introduced.

## Manual-only gap: resolved

Ticket 0019's opening concern: all 10 acceptance criteria had manual-transcript-only evidence, 1.d20 is non-disposable content (unlike Nine Rooms), so Phase 1.5's precedent doesn't auto-transfer.

**Status change:** Ticket 0017 (test suite) is now **resolved** as of today (2026-07-20, per handoff closure). The 25 new tests directly cover all 17 acceptance criteria. No criterion remains manual-only.

**Rationale for PASS despite prior gap:**
- The gap *existed* before 0017 landed, but it's been **closed in this same phase** by the time of this verification. The phase doesn't ship with manual-only criteria — it ships with both manual transcripts (0016/0018 disposable verification) *and* maintained automated coverage (0017).
- 1.d20 is explicitly durable content per the spec. Its automated suite (25 tests) will remain in the repo, un-deleted, serving as the load-bearing acceptance-criteria proof going forward. The disposable manual transcripts from 0016/0018 are audit trail, not the only evidence.
- The `GeneratorServices` load-bearing condition is independently verified from source (OneD20RuleGroup.cs read directly, not inferred from test passage). The Phase 3 supersession decision rests on facts, not on test counts.

## Deviations from spec

None. The implementation matches the spec as written (the current post-0018 revision). All three spec revision notes (Amara's semantic clarification, first playtest gap fixes, second playtest contradictions) were incorporated into the code *before* this verification.

## Tickets filed

None. All acceptance criteria are demonstrably met.

---

**Verdict: PASS.** Phase 2.5 is done. The 1.d20 vertical slice is buildable, testable, and meets its acceptance criteria with a combination of:
1. Clean build (0 errors)
2. Automated suite (96/96 pass, 25 Phase2.5-specific)
3. Source verification (GeneratorServices condition confirmed, IMechanic ruling confirmed, no `NotImplementedException`)
4. Disposable manual verification (0016/0018 transcripts, now superseded by maintained automated coverage)

The `GeneratorServices` condition holds. Phase 3 stays superseded pending Kofi and Baraka's decision on the roadmap.
