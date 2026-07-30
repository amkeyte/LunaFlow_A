# 0025 — QA gate: ROM/Rulebook + cross-ROM boundary verdict

**Status:** resolved
**Opened by:** Supreme Leader (Kofi)
**Opened:** 2026-07-22
**For:** QA (Lothar)
**Depends on:** Phase 2.6.5 and Phase 2.7 implementation landing. Both are design-stage / blocked as
of filing — see [`phase-2-6-5.md`](../../../Roadmap/phase-2-6-5.md), [`phase-2-7.md`](../../../Roadmap/phase-2-7.md).

## Context

Phase 2.6 was closed administratively (2026-07-21) with the Lothar final-verdict **waived, not
fulfilled** — recorded honestly in [`phase-2-6.md`](../../../Roadmap/phase-2-6.md) and ticket
[0022](0022-implement-phase-2-6.md)'s close. The reasoning was sound: verifying the ROM/Rulebook
boundary against 2.6's *one-ROM* shape proves less than verifying the two-ROM shape that follows, and
[Phase 2.6.5](../../../Roadmap/phase-2-6-5.md) (ROM identity + capability mediation) and
[Phase 2.7](../../../Roadmap/phase-2-7.md) (the Grid/D20System split) both **rework that boundary** —
2.6.5 even deprecates `D20SystemContent.Configure`, part of what 2.6 shipped.

That gap is now **half-closed:** Lothar produced a PASS on 2.6's *one-ROM* shape (2026-07-22,
[`phase-2-6-verdict.md`](../../../QA/phase-2-6-verdict.md)) — the boundary as it shipped has been checked
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

Gate fired 2026-07-29. Full verdict: [`phase-2-7-verdict.md`](../../../QA/phase-2-7-verdict.md) — **FAIL**.

Items 1, 3, 4, 5 of the ask **PASS**: no `LunaFlow.Rulebook` source touches `ICommandHandler`/
`LunaFlowEngine.Host`/`RuleEngine`; `GridMapRom`/`D20SystemRom` correlate `PlayerPosition` (and three
other facts) through `Capability`-declared contract types with zero project reference between the
two ROM projects (checked the `.csproj` files directly, not just that it runs); `ComponentRegistry
.Validate()`'s duplicate-provider/missing-requirement/tier/cycle checks are all present and covered
by `ComponentRegistryTests` in the 119/119 suite; `D20SystemContent.Configure` is gone from both
content ROMs (retained, obsolete, for NineRooms regression only — a ticketed exception, not drift).

Item 2 **fails on a narrower point than the ask anticipated**: cross-ROM correlation itself is
correctly zero-reference, but the *load-time fail-fast on removing a ROM* half of item 2/the phase's
own Done-When rests on `DependsOnRoms` being validated — and it isn't. `ComponentRegistry.Validate()`
never reads it; nothing in `Program.cs` does either. Phase-2-7.md's own text already flagged this as
live work in progress; this pass confirms it's still unbuilt. Compounding that, the bootstrap contract
probe (§5 of `cross-rom-capability-spec.md`, an explicitly named 2.7 deliverable) doesn't exist at all
— no assignability probe, no member-resolution probe, no binding-exception barrier anywhere in the
codebase. Both filed as new defect tickets to Baraka: [0046](0046-bootstrap-contract-probe-not-built.md),
[0047](0047-dependsonroms-validation-not-enforced.md).

A third, already-open gap (ticket [0043](0043-generated-rules-have-no-seam-for-rulebook-flavor-text.md))
also blocks the grid-rom-d20-split-spec's "behavior unchanged" bar — not new, just confirmed still live.

Not requesting Kofi resolve this ticket — that call is yours per the ownership rule. This response is
the evidence; `phase-2-7-verdict.md` has the full criterion-by-criterion table.

**Update 2026-07-30 — gate re-fired, verdict now PASS.** All three gaps above are closed:
`DependsOnRoms` validation ([0047](0047-dependsonroms-validation-not-enforced.md)) and the bootstrap
probe ([0046](0046-bootstrap-contract-probe-not-built.md)) both built and independently
fault-injection-verified by me, not accepted on Baraka's word — I personally deleted the Cartridge
assembly and ran the built exe, and personally commented out GridMap's registration and ran the
server, both times reverting after confirming the exact failure mode. The flavor-text gap
([0043](0043-generated-rules-have-no-seam-for-rulebook-flavor-text.md)) is resolved too — read the
shipped `RoomChallengePrompt`/`GameEndAnnouncement` facts directly, confirmed real content, not
placeholder. Also found and closed via [0049](0049-delete-orphan-src-lunaflow-cartridge-project.md):
an orphan `src/LunaFlow.Cartridge` project my original pass flagged as a vestigial-concept red flag,
now fully removed from the `.sln`.

**One flag, not a technical blocker:** the `latest.trx` this verdict reads was run solo by an agent
per ticket [0050](0050-test-harness-needs-roomchallengeprompt-gameendannouncement-seeds.md)'s own
disclosure, not by Arryn — technically a violation of `agent-test-workflow.md`'s hard rule. I didn't
wave it through: I independently re-ran the relevant narrow slices myself (`ComponentRegistryTests`
23/23, `TestCategory=Rom_OneD20` 25/25) and corroborated the fault-injection claims by hand, so the
verdict rests on my own evidence, not the trx's provenance. Recommend Kofi have Arryn run one clean
full pass to produce a properly user-run `latest.trx` for the record, but I'm not gating Phase 2.7's
done-bar on that housekeeping item.

Full criterion table: [`phase-2-7-verdict.md`](../../../QA/phase-2-7-verdict.md), verdict **PASS**.

## Resolution

Closing on Lothar's 2026-07-30 PASS. All six ask items are met on evidence, not description: items
1/3/4/5 passed clean on the first pass; item 2's narrower gap (`DependsOnRoms` validation + the
missing bootstrap probe) was fixed and independently fault-injection-verified by Lothar himself
(0046, 0047), not accepted on Baraka's word; the already-open flavor-text gap (0043) is confirmed
resolved with real shipped content; and the orphan `src/LunaFlow.Cartridge` project Lothar's own pass
flagged as a red flag is gone (0049). This closes the honesty gap `phase-2-6.md` and `phase-2-6-5.md`
disclosed — the cross-ROM boundary now has a real verdict on its final, two-ROM shape, not a waiver.

One flagged, non-blocking process item: the `latest.trx` this verdict reads was solo-agent-run
(0050's own disclosure), not Arryn-run, technically against `agent-test-workflow.md`'s hard rule.
Lothar independently corroborated it by hand (`ComponentRegistryTests` 23/23,
`TestCategory=Rom_OneD20` 25/25) rather than waving it through, so the verdict stood on his own
evidence regardless. **Closed 2026-07-30:** Arryn ran a full clean pass herself — 119/119 — so the
flag is resolved outright, not just non-blocking. Updated in `phase-2-7-verdict.md` and
`Documents/QA/index.md`.

Roadmap updated: `roadmap.md` and `phase-2-7.md` both move Phase 2.7 to done, citing
[`phase-2-7-verdict.md`](../../../QA/phase-2-7-verdict.md). `phase-2-6-5.md`'s disclosed QA-verdict
exception is updated to reflect that this ticket has now actually fulfilled it.
