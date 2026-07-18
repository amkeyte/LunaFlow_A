# 0004 — Phase 1.5 final verdict + QA report reconciliation

**Status:** resolved 2026-07-17 — PASS verified and closed by Kofi; Phase 1.5 marked done on the roadmap.
**Opened by:** Supreme Leader (Kofi)
**Opened:** 2026-07-17
**For:** QA (Lothar)
**Blocked by:** [0002](0002-phase-1-5-verification-build.md) — resolved; [0003](0003-phase-1-5-engine-fixture-tests.md) — resolved

**Evidence basis for the verdict (note the split):** the rule-engine behavior is covered by the
automated fixture ([0003], 63/63 pass); the command-handler / Announce pipeline including the 0001
room-entry behavior is covered by the **manual smoke playthrough in [0002]**, not automated — a
deliberate call for disposable Nine Rooms content. The final verdict must state plainly that this
one acceptance criterion rests on manual verification, so the sign-off isn't mistaken for full
automated coverage.

## Context

The final Phase 1.5 sign-off, gated on both the green build ([0002]) and Nadia's engine-fixture
suite ([0003]). This is the ticket that actually decides whether Phase 1.5 gets marked **done** on
the roadmap.

There is a pre-existing `Documents/QA/Phase_1.5_QA_Report.md` asserting Nine Rooms is "fully
playable end-to-end." It predates the [0001](0001-room-entry-announcement.md) fix **and** was
written without a build being possible. Treat it as unverified prior art — do not inherit its
conclusion; supersede it with a build-backed verdict.

## Ask

1. **Full done-bar verification** against the updated behavior (room descriptions on entry, per
   0001). Confirm each, citing the green suite from [0003] plus a spot manual run:
   - Room-5 challenge fires at startup (initial settle), before any command.
   - Entering an unsolved room: `Description` then challenge prompt, in that order.
   - Entering a solved room: `Description` only, no re-prompt.
   - Off-grid `go`: rejected, no fact change.
   - Completing a line: exactly one `WinMarker`; game-end names the correct line.
   - No `NotImplementedException` in the path.
2. **Reconcile** `Documents/QA/Phase_1.5_QA_Report.md`: update or replace it so there is one
   authoritative, build-backed QA verdict, not two reports disagreeing. Note explicitly that the
   verdict now rests on a real build + green suite, not inspection.
3. **Verdict to PM (Kofi):** pass or fail, with evidence. On pass, Kofi marks Phase 1.5 done on the
   roadmap; on fail, defects route back to the [Dev](../../Roles/dev.md) (implementation) or
   [Architect](../../Roles/architect.md) (spec) as appropriate.

Repo convention (`CLAUDE.md`): no state-changing git commands.

## Response

**PASS**, with one gap stated plainly rather than absorbed. Full verdict with evidence:
[`Documents/QA/phase-1.5-verdict.md`](../../../Documents/QA/phase-1.5-verdict.md).

Re-verified rather than inherited: re-ran `dotnet build` (still 0 errors after the test-suite
changes from 0003/0005/0006 landed), read `latest.trx` directly for the 63/63-pass claim instead
of trusting the ticket text, read `NineRoomsEngineFixtureTests.cs` directly to confirm its
docstring's "not automated" claim is now true, and ran a fresh manual spot-check for off-grid
rejection (not covered by either the suite or the 0002 transcript).

All six criteria: PASS. Three of them (startup-challenge timing, unsolved-room ordering,
solved-room suppression) rest **only** on the manual smoke playthrough in 0002, not on the
automated suite — this is Arryn's documented call from [0003](0003-phase-1-5-engine-fixture-tests.md),
not a gap I'm introducing, but the sign-off needs to say so plainly so it isn't mistaken for full
regression coverage later.

**Reconciled** `Documents/QA/Phase_1.5_QA_Report.md`: moved to
[`Documents/QA/Archive/Phase_1.5_QA_Report.md`](../../../Documents/QA/Archive/Phase_1.5_QA_Report.md)
with a superseded banner, not deleted. `Documents/QA/index.md` now points at
`phase-1.5-verdict.md` as the current authoritative record.

## Resolution

Verified and closed by Kofi (opener), 2026-07-17. **PASS accepted; Phase 1.5 done.**

Verified the verdict rather than the summary:
- Read `Documents/QA/phase-1.5-verdict.md` — six criteria, each mapped to evidence, with the three
  manual-only ones honestly tagged (not absorbed). Lothar re-ran the build, read `latest.trx`
  directly (63/0, corroborating the PM's own earlier independent read), confirmed the fixture
  docstring's "not automated" claim is now true, and ran a live off-grid spot-check.
- Confirmed reconciliation: `Documents/QA/index.md` points at `phase-1.5-verdict.md` as
  authoritative; the stale `Phase_1.5_QA_Report.md` is in `Archive/` with a clear superseded banner,
  not deleted. No duplicate/contradicting verdicts remain.
- The manual-only coverage gap is disclosed in four places now (fixture docstring, 0003 resolution,
  this verdict, and the Phase 1.5 roadmap note) — a choice on the record, not a silent hole.

Roadmap updated: `Roadmap/roadmap.md` and `Roadmap/phase-1-5.md` both mark Phase 1.5 **done**, citing
this verdict. Next up: Phase 2 (Rule pipeline MVP).
