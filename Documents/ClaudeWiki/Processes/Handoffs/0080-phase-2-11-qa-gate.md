# 0080 — Phase 2.11 QA gate

**Status:** resolved
**Opened by:** Kofi (PM)
**Opened:** 2026-08-08
**For:** Lothar (QA)

## Decision

Verdict **PASS** — [`phase-2-11-verdict.md`](../../../QA/phase-2-11-verdict.md). All 11 acceptance
criteria met with independently-reproduced evidence (source reads, live probe re-runs, a real 4-ROM
bootstrap, one scratch negative-case reproduction). Items 6/8 surfaced no framework finding — stated
explicitly per Amara's ruling, no ticket needed. One non-blocking evidence-tier deviation (fixture-level
proof instead of a live transcript for one bonus-item bullet) — accepted, reasoning below. Phase 2.11
marked **done (QA PASS)**.

## Details

### Context

Phase 2.11 ([`phase-2-11.md`](../../../Roadmap/phase-2-11.md)) is implementation-complete:
[0076](0076-implement-phase-2-11-capability-stress-test-spec-md.md) built
`phase-2-11-capability-stress-test-spec.md` in full, [0078](0078-consolidate-gridmap-d20system-loot-reputation-into-one-implementation-assembly.md)
consolidated GridMap/Loot/Reputation into `OneD20SharedRoms` (D20System untouched, independently
re-verified by Amara against the actual repo state), and [0079](0079-phase-2-11-maintained-test-coverage.md)
landed the maintained test coverage (88/88 `ContentTests`, independently re-verified by Baraka). I
closed 0076 against an Arryn-run `tests/LunaFlow.EngineTests/TestResults/latest.trx` —
181/181 passed, 0 failed, and 181 is exactly 93 (EngineTests baseline) + 88 (ContentTests) — so this
one run covers both projects despite being pointed at only one runsettings file. Same shape as
[0074](0074-phase-2-10-qa-gate.md): filed now that there's something real to check.

**Read before starting:** this phase is deliberately adversarial in two of its three items. Items 6
and 8 are *supposed* to fail `ComponentRegistry.Validate()` — a PASS verdict on those two items means
confirming the rejection happens for the right reason with the right error content, not that nothing
broke. Per Amara's standing ruling on ticket [0067](0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md),
if either probe's failure mode is unclear, wrong, or silently accepted, that's a real finding to
ticket to Amara — not something to note and move past.

### Ask

Run a full `qa-verify` pass against `phase-2-11-capability-stress-test-spec.md`'s 11-item acceptance
list and `phase-2-11.md`'s Done-When bar. Verify against evidence — source, passing test, reproducible
session — not ticket prose:

1. **Item 6 clash error content** — reproduce `BountyThreatProbe`/`HazardThreatProbe` registered
   together in an isolated `ComponentRegistry`; confirm `Validate()` throws naming both component
   names and `"ThreatLevel"` — don't take 0076/0079's pasted message on faith, read it off a live run
   or the test assertion directly against `ComponentRegistry.cs`'s actual thrown-message format.
2. **Item 6 assembly-graph check** — confirm `MyLunaFlowServer.csproj` has no reference, direct or
   transitive, to `LunaFlow.Rom.Probes`. Note: `LunaFlow.ContentTests.csproj` *does* legitimately
   reference it now (needed to write item 6/8's fixtures) — that's correct per 0079's Response, not a
   violation; the binding criterion is only the real composition root.
3. **Item 7 four-ROM registration** — confirm `ReputationRom` registers and validates inside the live
   Cartridge alongside GridMap/D20System/Loot via a real `dotnet run --project src/MyLunaFlowServer`
   bootstrap, not just a unit test.
4. **Item 7 `DependsOnRoms` as a distinct check** — confirm `OneD20RuleBook.DependsOnRoms` names all
   four ROMs, and that this is verified separately from `ComponentRegistry.Validate()` passing (that
   check doesn't read `DependsOnRoms` at all) — reproduce the negative case (Reputation registered but
   omitted from `DependsOnRoms`) yourself if not already covered by a passing test you can point to.
5. **Item 7 tier-threshold behavior** — live or fixture-reproduced: `ReputationTierReached("Veteran")`
   at 3 solves, `("Legend")` at 6, each inserted exactly once across further solves past threshold.
6. **Item 7 retroactive-grant guard** — this is the one real defect the spike caught pre-ship (see
   0076's Response): an `ItemAwarded` fact predating Veteran must never gain a `BonusItemAwarded` after
   the tier fact appears later. Confirm the two-pass fixture shape in
   `LootRuleGroupFixtureTests.ItemAwardedBelowVeteran_NeverGainsBonusItemAwarded_AfterVeteranReachedLater`
   actually exercises this (insert/fire, *then* insert Veteran/fire again) rather than seeding
   everything before one `Fire()` — a single-pass fixture would pass without proving anything.
7. **Item 7 three-Requires coexistence edge case** — a room solved once both Veteran and Legend tier
   facts already exist produces exactly one `ItemAwarded` and one `BonusItemAwarded`, not two.
8. **Item 7 skill-check exclusion holds** — skill-check rooms still never produce `ItemAwarded` or
   `BonusItemAwarded` regardless of tier.
9. **Item 8 cycle error content, distinguishable from item 6 and from a plain missing-requirement
   error** — reproduce `CycleProbeAlpha`/`CycleProbeBeta` in an isolated registry; confirm the thrown
   message identifies a dependency cycle and does not overlap item 6's or a bare "unsatisfied
   requirement" message's wording. Confirm neither probe ROM is referenced from `Program.cs`.
10. **No existing acceptance criterion regressed** — spot-check against `1-d20-spec.md` and
    `loot-rom-content-expansion-spec.md`'s own criteria, not just "tests still pass."
11. **Regression green from an Arryn-run `latest.trx`.** Confirm the 181/181 run cited above is what
    you're reading (timestamp, `runUser`, and the 93+88 arithmetic), per the standing hard rule — not
    an agent-run number relabeled.
12. **Items 6/8 framework-finding call.** State explicitly whether either probe surfaced anything
    Amara needs to see (ambiguous message, silent acceptance, wrong rejection) or confirms current
    behavior is fine — either answer is a legitimate finding per her ruling, but it has to be stated,
    not implied by silence.
13. **Verdict + evidence to the PM**, written to `Documents/QA/phase-2-11-verdict.md`. A fail routes to
    Baraka (defect) or Amara (spec-vs-reality) — not a fix by you.

### Response

**Verdict: PASS.** Full report: [`Documents/QA/phase-2-11-verdict.md`](../../../QA/phase-2-11-verdict.md).

All 13 asks checked against source, an independent live re-run of both adversarial probe tests, a
real 4-ROM `dotnet run` bootstrap, and one scratch reproduction of a negative case the existing suite
didn't cover directly (`DependsOnRoms` omitting Reputation specifically — the existing test only
covers Loot). Build green, 181/181 (merged trx, 93 EngineTests + 88 ContentTests) — `runUser`,
timestamp, and the 93+88 arithmetic all confirmed directly from the trx itself, not asserted.

Unlike 2.10, this phase's automated suite has real targeted coverage of its own new mechanisms
(`ComponentRegistryProbeTests` for items 6/8, `ReputationRuleGroupFixtureTests` and four
new/renamed `LootRuleGroupFixtureTests` cases for item 7) — I read `ComponentRegistry.cs`'s actual
`Validate()`/`DetectCyclesDfs` source before looking at what the tests assert, then independently
re-ran both probe tests live to confirm rather than trust the trx alone. Confirmed
`MyLunaFlowServer.csproj`'s full reference graph has zero path, direct or transitive, to
`LunaFlow.Rom.Probes`.

**Items 6/8 framework-finding call, stated explicitly per Amara's ruling: neither probe surfaced
anything.** Both failure modes fire for the right reason with clear, mutually distinguishable error
content, confirmed against source. No ticket to Amara.

One evidence-tier deviation, not blocking: item 7's "early vs. late fight in the same transcript"
bullet is verified via deterministic fixture-level proof against the real compiled `LootRuleGroup`
rather than a single live session — reasoned in the verdict to be equal-or-stronger evidence given
production's unseeded dice, but flagged rather than silently substituted. Your call whether a literal
live transcript is still wanted for the record.

### Resolution

**Resolved 2026-08-08 (Kofi, opener).** Read [`phase-2-11-verdict.md`](../../../QA/phase-2-11-verdict.md)
directly, not this ticket's Response summary. All 13 asks answered with evidence attached — source
reads of `ComponentRegistry.cs`'s actual thrown-message formats *before* looking at the tests that
assert on them, both adversarial probe tests independently re-run live (not trusted off the trx), a
real `dotnet run` 4-ROM bootstrap, a scratch reproduction of the `DependsOnRoms`-omits-Reputation
negative case the existing suite didn't cover directly, and per-class trx counts confirming no test
coverage was lost in 0078's consolidation. Suite: 181/181, single merged trx, `runUser=ARRYNSTATION\amkey`,
timestamp matches the run cited when closing 0076 — same evidence, not a second unverified number.

**Items 6/8 framework-finding call: no issue, stated explicitly, not implied by silence** — both
failure modes are genuinely distinct code paths (`Validate()`'s duplicate-provider check vs.
`DetectCyclesDfs`) producing non-overlapping, correctly-scoped error content. `non-goals.md`'s
capability-arbitration deferral holds. No ticket to Amara.

**Deviation accepted:** item 7's "early vs. late fight in the same transcript" bullet was verified via
deterministic fixture-level proof against the real compiled `LootRuleGroup` rather than a live
`dotnet run` session. Lothar's reasoning holds — production dice are unseeded, so a live transcript
proving this specific contrast would mean scripting around random outcomes, which is *weaker*
evidence than the fixtures' precise fact-level control, not stronger. Not asking for a follow-up live
transcript; the mechanism is proven at the tier this codebase already treats as sufficient elsewhere
(`D20SystemRuleFixtureTests` et al.).

**Phase 2.11 marked done (QA PASS)** in `roadmap.md` and `phase-2-11.md`, citing this ticket and the
verdict.
