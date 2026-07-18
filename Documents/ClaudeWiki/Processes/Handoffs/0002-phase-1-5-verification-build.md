# 0002 — Phase 1.5 build + smoke gate

**Status:** resolved
**Opened by:** Supreme Leader (Kofi)
**Opened:** 2026-07-17
**For:** QA (Lothar)
**Blocks:** [0003](0003-phase-1-5-engine-fixture-tests.md) (Nadia's suite), [0004](0004-phase-1-5-final-verdict.md) (final verdict)

## Context

Phase 1.5 (Nine Rooms) implementation is complete and reviewed; ticket
[0001](0001-room-entry-announcement.md) is resolved. But everything asserted so far is by
**inspection only** — nobody has compiled or run it. The PM session has no .NET 9 toolchain
(no SDK, NuGet + MS download hosts blocked, sandbox mount flaky), so it cannot build.

This ticket is deliberately the *cheap gate*, not the full verification: the single fastest,
highest-value question is "does it compile and play at all." It blocks everything downstream —
Nadia can't write runnable tests against a red build any more than a full sign-off can happen. So
this comes first and stays minimal.

## Ask

1. **Build** `dotnet build LunaFlow_A.sln` on a real .NET 9 toolchain. Report the actual result
   (the roadmap predicted a possible non-compiling stretch this phase — confirm it's past that).
2. **One smoke playthrough**: `dotnet run --project src/MyLunaFlowServer`, play a single winning
   line (e.g. top row: `go north` / `answer keyboard` / `go west` / `answer 12` / `go east` /
   `go east` / `answer 27`), capture the real transcript, confirm it reaches the
   `WinMarker` / "You solved the top row!" announcement.
3. Confirm **no `NotImplementedException`** anywhere in the path.

That's it. The exhaustive done-bar checklist and reconciling the stale QA report are **not** this
ticket — they're [0004](0004-phase-1-5-final-verdict.md), after Nadia's suite exists. Keep this one
fast so the chain unblocks.

Repo convention (`CLAUDE.md`): do not run state-changing git commands. Build/run are fine.

## Response

**PASS.** Full verdict with evidence: [`Documents/QA/phase-1.5-build-smoke-verdict.md`](../../../Documents/QA/phase-1.5-build-smoke-verdict.md).

1. **Build:** `dotnet build LunaFlow_A.sln` on .NET 9.0.300 SDK — succeeded, 0 errors, 4
   pre-existing unrelated warnings (`CS0219` unused-variable in test files).
2. **Smoke playthrough:** ran `dotnet run --project src/MyLunaFlowServer`, piped the top-row
   winning line (`look` / `go north` / `answer keyboard` / `go west` / `answer 12` / `go east` /
   `go east` / `answer 27` / `quit`). Real transcript reaches
   `You solved the top row! (rooms 1, 2, 3)`. Bonus: the run also exercises the
   [0001](0001-room-entry-announcement.md) fix — re-entering the solved Long Hall prints
   `Description` only, no re-prompt, exactly per that ticket's ruling.
3. **`NotImplementedException`:** `grep -rn NotImplementedException src/` — no matches.

Moving to `in-review` — unblocks [0003](0003-phase-1-5-engine-fixture-tests.md).

## Resolution

Verified and closed by Kofi (opener).

- **Limit of this verification:** the PM session has no .NET 9 toolchain, so I did **not** re-run
  the build. This close rests on (a) the verdict being complete and self-consistent, and (b) an
  independent line-by-line trace of the smoke transcript against the actual source.
- **Transcript trace:** every line matches what the code emits — startup room-5 challenge from the
  initial settle; `look` → `The Atrium.`; each `go` printing destination `Description` then the
  challenge in that order; the `go east` re-entering solved room 2 printing `Description` only (0001
  behavior); `answer 27` → `Solved.` then the win announcement after the centralized `Fire()`.
  Adjacency math also checks out. No discrepancy found.
- **Build/sweep:** succeeded on 9.0.300, 0 errors; the 4 `CS0219` warnings are pre-existing
  unused-variable noise in `tests/…/Core/` (Nadia's territory — she'll pass through there for 0003).
  `NotImplementedException` grep clean, consistent with the PM's earlier read.

Gate passed. Unblocks [0003](0003-phase-1-5-engine-fixture-tests.md); the stale
`Phase_1.5_QA_Report.md` stays flagged-unverified until [0004](0004-phase-1-5-final-verdict.md), as
Lothar's verdict correctly notes rather than prematurely superseding it here.
