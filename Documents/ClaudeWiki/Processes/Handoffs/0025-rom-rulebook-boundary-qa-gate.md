# 0025 — QA gate: ROM/Rulebook + cross-ROM boundary verdict

**Status:** open (parked — not actionable until Phase 2.7 is implementation-complete)
**Opened by:** Supreme Leader (Kofi)
**Opened:** 2026-07-22
**For:** QA (Lothar)
**Depends on:** Phase 2.6.5 and Phase 2.7 implementation landing. Both are design-stage / blocked as
of filing — see [`phase-2-6-5.md`](../../Roadmap/phase-2-6-5.md), [`phase-2-7.md`](../../Roadmap/phase-2-7.md).

## Context

Phase 2.6 was closed administratively (2026-07-21) with the Lothar final-verdict **waived, not
fulfilled** — recorded honestly in [`phase-2-6.md`](../../Roadmap/phase-2-6.md) and ticket
[0022](0022-implement-phase-2-6.md)'s close. The reasoning was sound: verifying the ROM/Rulebook
boundary against 2.6's *one-ROM* shape proves less than verifying the two-ROM shape that follows, and
[Phase 2.6.5](../../Roadmap/phase-2-6-5.md) (ROM identity + capability mediation) and
[Phase 2.7](../../Roadmap/phase-2-7.md) (the Grid/D20System split) both **rework that boundary** —
2.6.5 even deprecates `D20SystemContent.Configure`, part of what 2.6 shipped.

That gap is now **half-closed:** Lothar produced a PASS on 2.6's *one-ROM* shape (2026-07-22,
[`phase-2-6-verdict.md`](../../QA/phase-2-6-verdict.md)) — the boundary as it shipped has been checked
against source. This ticket is the **remaining half**: the *two-ROM / cross-ROM* boundary that 2.6.5
(capability mediation via `IInteropRom`) and 2.7 (the Grid/D20System split) actually ship — a shape
that doesn't exist in code yet. It gets one verdict on its final form, so the cross-ROM boundary is
verified once when it's real, rather than falling through the 2.6.5 → 2.7 seam. Lothar's 2.6 verdict
explicitly scoped itself to the one-ROM shape and pointed here for the rest; keep that split.

**Not actionable yet.** 2.6.5/2.7 aren't built. Do not attempt verification until Baraka's 2.7
done-bar is met and the suite is green. This ticket exists now so the commitment is tracked, not to
run immediately — leave it parked until 2.7 implementation lands.

## Ask

When 2.7 is implementation-complete, run a full `qa-verify` pass over the shipped boundary. At
minimum, verify against evidence (source, passing test, reproducible transcript — not a description):

1. **The 2.6 invariant, on the final shape** — no `LunaFlow.Rulebook` *source* touches a
   `LunaFlow.Engine` primitive (no `ICommandHandler`/`LunaFlowEngine.Host`/`RuleEngine`). This is the
   check 2.6's waived verdict would have made; make it now, across the post-split Rulebook. (Transitive
   `Engine.dll` via ROM is expected and correct — not a violation; see 0022's close.)
2. **Cross-ROM capability mediation (2.6.5/2.7 Done-When)** — Grid ROM and D20System correlate
   `PlayerPosition` through a `Capability`/`IInteropRom` with **zero project reference between the two
   ROMs**. Confirm the assembly graph, not just that it runs.
3. **The two-phase validator fails fast as specified** — a missing required capability and a
   duplicate provider each hard-error at load with a clear message; the cycle check trips on a real
   cycle. Fail-fast, not silent degradation.
4. **`D20SystemContent.Configure` is gone** — removed/migrated per 2.6.5, not lingering as dead
   static state.
5. **Regression green on the shipped shape** — read from the Arryn-run `latest.trx`
   (`tests/LunaFlow.EngineTests/TestResults/latest.trx`) per the hard rule in
   [`agent-test-workflow.md`](../agent-test-workflow.md); do **not** solo-run the full suite for the
   verdict.
6. **Verdict + evidence to the PM**, written to `Documents/QA/`. A fail is a ticket to Baraka
   (defect) or Amara (spec-vs-reality), not a fix by you.

Once this verdict lands (PASS), it's what lets the roadmap mark the ROM/Rulebook + cross-ROM boundary
fully-signed across 2.6/2.6.5/2.7 — closing the honesty gap `phase-2-6.md` currently records.

## Response

_(Lothar to fill in when the gate fires.)_

## Resolution

_(Kofi to fill in on close.)_
