# Phase 1.5 (Nine Rooms) — Final QA Verdict

**Verdict:** PASS — with one documented, deliberate coverage gap (see below, not a defect)
**Date:** 2026-07-17
**Ticket:** [handoff 0004](../ClaudeWiki/Processes/Handoffs/0004-phase-1-5-final-verdict.md)
**Supersedes:** [`Archive/Phase_1.5_QA_Report.md`](Archive/Phase_1.5_QA_Report.md) (inspection-only, predates the [0001](../ClaudeWiki/Processes/Handoffs/0001-room-entry-announcement.md) fix and any real build — not authoritative)
**Build:** `dotnet build LunaFlow_A.sln` on .NET 9.0.300 — **Build succeeded, 0 Errors, 4 Warnings** (pre-existing `CS0219` unused-variable in test files, unrelated to Nine Rooms).
**Suite:** `tests/LunaFlow.EngineTests/TestResults/latest.trx`, read directly (not taken on faith) — **63 `Passed`, 0 `Failed`**, matching the count claimed in [handoff 0003](../ClaudeWiki/Processes/Handoffs/0003-phase-1-5-engine-fixture-tests.md)'s response.

## Criteria (from handoff 0004)

| Criterion | Evidence | Result |
| --- | --- | --- |
| Room-5 challenge fires at startup, before any command | 0002 smoke transcript: `What is 6 × 6?` prints before the first typed command. Fixture's `StartupSettle_InitializesFacts` confirms seeding/fact state at the rule-engine level but not the announcement itself. | PASS — **manual-only** |
| Entering an unsolved room: `Description` then challenge, in order | 0002 smoke transcript (`go north` → `The Long Hall.` then the riddle) and this session's own spot-check reproduced the same ordering. Not automated — see gap below. | PASS — **manual-only** |
| Entering a solved room: `Description` only, no re-prompt | 0002 smoke transcript: re-entering the solved Long Hall (`go east`) printed `The Long Hall.` alone, no riddle repeat. Not automated. | PASS — **manual-only** |
| Off-grid `go`: rejected, no fact change | `GridAdjacency_ValidatesMovement` (automated, `TryMove` boundary logic) **+** a fresh manual spot-check this session: from room 2, `go north` → `You can't go north from here.`, followed by `look` confirming position unchanged (`The Long Hall.`). | PASS — automated (boundary logic) + manual (handler behavior) |
| Completing a line: exactly one `WinMarker`; game-end names the correct line | Automated: `SolveFullLine_InsertsWinMarkerOnce` (rule-engine level, asserts exactly one marker even after a second line's rooms are solved). Manual: 0002 smoke transcript reaches `You solved the top row! (rooms 1, 2, 3)`. | PASS — automated + manual |
| No `NotImplementedException` anywhere in the path | `grep -rn NotImplementedException src/` — no matches, re-checked this session after the test-suite changes landed. | PASS |

## The coverage gap, stated plainly

Three of the six criteria above rest **only** on the manual smoke playthrough in
[handoff 0002](../ClaudeWiki/Processes/Handoffs/0002-phase-1-5-verification-build.md), not on the
automated suite. This is not an oversight: `NineRoomsEngineFixtureTests.cs`'s class docstring
states it directly, and [handoff 0003](../ClaudeWiki/Processes/Handoffs/0003-phase-1-5-engine-fixture-tests.md)'s
resolution records it as Arryn's explicit call — the command-handler/`Announce` pipeline
(including the 0001 room-entry ordering) is not given automated regression coverage, because Nine
Rooms is disposable content and the fixture is scoped to rule-engine semantics only. I verified
that framing is accurate (read the fixture file directly — it really doesn't drive
`MoveCommandHandler`/`AnswerCommandHandler` or subscribe to `MessagePosted`) and re-ran the manual
check myself rather than inheriting 0002's transcript on trust alone.

**What this means going forward:** if `MoveCommandHandler` regresses the 0001 ordering later,
nothing in the automated suite will catch it — only a human playthrough would. That's an accepted
risk for throwaway content, not a blocker for Phase 1.5, but it should not be forgotten if Nine
Rooms' patterns get reused in real content.

## Deviations from spec

None beyond the already-ruled, already-documented `LunaFlowEngine.Host` vs. spec's `Engine.Host`
naming (see the archived report and handoff 0001) — not re-litigated here.

## Superseded reports

[`Archive/Phase_1.5_QA_Report.md`](Archive/Phase_1.5_QA_Report.md) — see the banner prepended to
that file for why.

## Tickets filed

None. No failing or unproven criteria as scoped by 0004; the manual-only gap above is disclosed,
not hidden, per Arryn's accepted risk in 0003.

## Recommendation to PM

Phase 1.5 done-bar is met. Kofi may mark it done on the roadmap, citing this verdict.
