# 0086 — Phase 2.12 QA gate

**Status:** resolved
**Opened by:** Kofi (PM)
**Opened:** 2026-08-09
**For:** Lothar (QA)

## Decision

Verdict **PASS** — [`phase-2-12-verdict.md`](../../../QA/phase-2-12-verdict.md). All 12 acceptance
criteria met with independently-reproduced evidence (spike re-run live, roster arithmetic
hand-recomputed before checking the spike's numbers, the "purely defensive" claim re-confirmed
directly against `FightCommandHandler.cs`'s source rather than test prose). One non-blocking
evidence-tier deviation (loss/timeout-while-enraged retraction proven structurally, not with a live
enraged instance) — accepted. Item 11's Phase-4 finding is real and load-bearing enough to route
onward, not just leave in the verdict file — see ticket 0087. Phase 2.12 marked **done (QA PASS)**.

## Details

### Context

Phase 2.12 ([`phase-2-12.md`](../../../Roadmap/phase-2-12.md)) is implementation-complete:
[0084](0084-implement-phase-2-12-lua-worldmodule-stress-test-spec-md.md) built
`phase-2-12-lua-worldmodule-stress-test-spec.md` in full (a checked-in spike,
`example/Phase2_12Spike`, caught two real bugs before they'd have shipped as false confidence — a
missing `move` between rooms, and a validator probe whose rejections were passing for the wrong
reason), and [0085](0085-phase-2-12-maintained-test-coverage.md) landed the maintained coverage
(107/107 `ContentTests`, independently re-verified by Baraka, including hand-derived roll arithmetic
for the two trickiest fixture cases). I closed 0084 against an Arryn-run
`tests/LunaFlow.EngineTests/TestResults/latest.trx` — 200/200 passed, 0 failed, and 200 is exactly 93
(EngineTests baseline) + 107 (ContentTests). Same shape as [0080](0080-phase-2-11-qa-gate.md): filed
now that there's something real to check.

**Item 11 (Enrage) is this phase's one framework-adjacent item** — it's the first extension to
`IOneD20WorldModule`'s fixed shape since Phase 2.8, and per Amara's ruling on ticket
[0067](0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md), its result is a
direct input to Phase 4's `gm-overlay.md` "effect fact" draft. Your verdict should state plainly
whether the shipped Enrage mechanism actually behaves the way item 11's write-up claims — that's the
data point Phase 4's kickoff pass will be reading off this ticket later, not just a pass/fail line.

### Ask

Run a full `qa-verify` pass against `phase-2-12-lua-worldmodule-stress-test-spec.md`'s 12-item
acceptance list and `phase-2-12.md`'s Done-When bar. Verify against evidence — source, passing test,
reproducible session — not ticket prose:

1. **Item 9 — "Signal Static" full playthrough** — reproduce a win via a combat-free line, a win via
   a combat line, and at least one `RoomFailed`, with zero changes to `IOneD20WorldModule`/
   `OneD20WorldModuleValidator`/`OneD20SessionAssembler` (confirm via `git diff` or equivalent, not
   just "the tests still pass").
2. **Item 9 — Save-kind challenge and `InflictsPoison` monster round-trip** — confirm actual field
   values in the constructed `IOneD20WorldModule`, not just that the session doesn't crash.
3. **Item 10 — procedural roster arithmetic** — recompute the five expected `Defense`/`MaxHp` values
   by hand from the base-plus-step formula yourself; confirm they match what the Lua loop actually
   produces, not just that a roster of five exists.
4. **Item 11 — Enrage triggers correctly and exactly once** — reproduce `MonsterEnraged` inserted at
   `MonsterHp <= Threshold * MaxHp`, not before, and confirm the same PC roll that hit at baseline
   Defense misses post-threshold against the boosted value.
5. **Item 11 — Enrage is purely defensive (ticket 0083's ruling)** — this is the one item 6-style
   "prove the negative" criterion in this phase. Confirm the monster's own attack roll/damage is
   byte-for-byte identical before and after Enrage triggers, for identical rolls — read
   `FightCommandHandler.cs`'s actual counterattack branch yourself to confirm it never reads
   `MonsterEnraged`/boosted Defense, don't just trust the test's inline comment.
6. **Item 11 — retraction on all four `CombatRound` exits** — win, loss, timeout, and flee, each a
   distinct check, plus the retry-after-flee case (identical roll that missed while enraged connects
   again at baseline).
7. **Item 11 — validator pairing/range check** — reproduce at least one rejection (a template with
   only one Enrage field set, or threshold outside `(0, 1]`) and the accept case at the inclusive
   upper bound (threshold == 1).
8. **Item 11 — full field-carry chain** — confirm end to end: Lua source → `LuaOneD20WorldModule`
   marshaling → translation → the ROM-tier `MonsterTemplate` fact `FightCommandHandler.cs` reads.
9. **Item 12 — both Lua-path failure probes** — confirm the marshal-failure probe throws before
   `OneD20WorldModuleValidator` is ever reached (verify the call-never-happened claim directly, not
   just the end exception), and the validator-failure probe constructs cleanly then fails at the
   pool-size check specifically.
10. **No existing criterion regressed** — spot-check against `lua-world-module-spec.md`,
    `1-d20-spec.md`, `loot-rom-content-expansion-spec.md`, and `phase-2-10-content-coverage-spec.md`.
11. **Regression green from an Arryn-run `latest.trx`.** Confirm the 200/200 run cited above is what
    you're reading (timestamp, `runUser`, and the 93+107 arithmetic), per the standing hard rule.
12. **Item 11's Phase-4 finding, stated explicitly.** Confirm whether Enrage's shipped shape is a
    clean, generalizable data point for `gm-overlay.md`'s "effect fact" idea or whether it surfaces a
    wrinkle worth flagging back to Amara before Phase 4's kickoff reads it — either answer is fine,
    but it has to be stated, not left implicit in a pass/fail line.
13. **Verdict + evidence to the PM**, written to `Documents/QA/phase-2-12-verdict.md`. A fail routes
    to Baraka (defect) or Amara/Juno (spec-vs-reality) — not a fix by you.

### Response

**Verdict: PASS.** Full report: [`Documents/QA/phase-2-12-verdict.md`](../../../QA/phase-2-12-verdict.md).

All 13 asks checked against source and an independent live re-run of the checked-in
`example/Phase2_12Spike` (47/47 checks, exit 0) — not trusted as evidence just because it's checked
in. Build green, 200/200 (merged trx, 93 EngineTests + 107 ContentTests) — `runUser`, timestamp, and
the 93+107 arithmetic confirmed directly from the trx.

Item 3 (roster arithmetic): hand-recomputed from the spec's own base-plus-step formula before looking
at the spike's hard-coded expected values — both agree, and both match the live output. Item 5 (the
"purely defensive" claim): read `FightCommandHandler.cs`'s counterattack branch directly rather than
trusting the test's inline comment — the monster's own attack check structurally never references
`effectiveDefense`/`monster.Defense` at all, which is a stronger claim than "tested and passed." Items
6/9 (retraction, field-carry chain): confirmed via source at every hop, not just the spike's black-box
assertions.

**Item 12's Phase-4 finding, stated explicitly:** Enrage proves extending a Rulebook-owned content
shape with a threshold-triggered modifier is cheap and needs zero framework involvement — genuinely
useful for `gm-overlay.md`. It does **not** prove or exercise the runtime-composable, GM-improvised
"effect fact" shape that draft is actually reaching for — Enrage is authored ahead of time in content
and interpreted by a bespoke rule, not declared live and interpreted generically. Whoever runs the
Phase 4 kickoff pass should read this as "the contract-extension side is cheap," not as "the
runtime-composable mechanism is proven." Not a defect, no ticket — a scoping note per the ticket's own
ask not to leave this implicit.

One evidence-tier flag, not blocking: loss-while-enraged and timeout-while-enraged retraction are
proven structurally (identical shared-helper call at all four `CombatRound` exits) rather than with a
live enraged instance for those two specific exits — win and flee both were driven live. Flagged in
the verdict's Deviations section.

### Resolution

**Resolved 2026-08-09 (Kofi, opener).** Read [`phase-2-12-verdict.md`](../../../QA/phase-2-12-verdict.md)
directly, not this ticket's Response summary. All 13 asks answered with evidence attached — the
checked-in spike independently re-run live (47/47 checks), item 10's arithmetic hand-recomputed
*before* looking at the spike's own numbers (both agreed), item 5's "purely defensive" claim
re-confirmed against `FightCommandHandler.cs`'s actual counterattack branch (structurally incapable
of reading `effectiveDefense`, not merely untested), and the field-carry chain read at every hop
rather than trusted from a black-box marshal assertion. Suite: 200/200, single merged trx,
`runUser=ARRYNSTATION\amkey`, timestamp matches the run cited when closing 0084.

**Deviation accepted:** loss/timeout-while-enraged retraction proven structurally (identical
`EnrageEffect.RetractIfPresent` call at all four `CombatRound` exits) rather than live for those two
specific exits. The helper is trivial enough that "one call, four identical sites" is solid evidence
without needing all four driven live — same reasoning tier this codebase already accepts elsewhere
(e.g. 2.11's fixture-vs-live call). Not asking for a follow-up live scenario.

**Item 11's Phase-4 finding is real, not just a verdict-file footnote.** Lothar's read is sharper than
the ticket's own ask expected: Enrage proves content-shape extension is cheap, but says nothing about
whether `gm-overlay.md`'s actual target — a runtime-composed, live-GM-improvised "effect fact" — is
buildable. Per `phase-2-12.md`'s own Done-When bar, this has to be *recorded somewhere the Phase 4
kickoff pass will actually see it*, not left sitting in a QA verdict file nobody's pointed at yet —
routed onward as ticket [0087](0087-phase-2-12-item-11-finding-for-gm-overlay-draft.md) to Amara.

**Phase 2.12 marked done (QA PASS)** in `roadmap.md` and `phase-2-12.md`, citing this ticket and the
verdict. This closes out the 2.10–2.12 proposal in full.
