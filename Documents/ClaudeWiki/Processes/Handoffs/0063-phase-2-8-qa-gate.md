# 0063 — Phase 2.8 QA gate

**Status:** resolved
**Opened by:** Kofi (PM)
**Opened:** 2026-08-03
**For:** Lothar (QA)

## Decision

**Verdict: PASS**, all nine ask items and the spec's full acceptance list confirmed against
evidence — see [`phase-2-8-verdict.md`](../../../QA/phase-2-8-verdict.md). Arryn-run 155/155
`latest.trx`, live fault-injection on the validator, and a live Hollow Keep session (a win, a loss,
a skill check) all independently verified, not taken on ticket prose. One non-blocking finding: the
validator's test coverage under-specifies its own fixed-vocabulary/ability loops (1-of-11, 1-of-6) —
worth a note to Nadia, not a ticket, since the mechanism itself is demonstrated working live.
**Phase 2.8 marked done on the roadmap.**

## Details

### Context

Phase 2.8 ([`phase-2-8.md`](../../../Roadmap/phase-2-8.md)) is implementation-complete:
[0060](0060-implement-world-module-extraction-spec-md.md) built all five of
`world-module-extraction-spec.md`'s items, [0059](0059-second-world-module-reskin-content.md)
delivered and proved the Hollow Keep reskin, and [0062](0062-tests-lunaflow-contenttests-broken-by-phase-2-8-world-module-extraction.md)
closed the last acceptance bullet (test suite repointed and green, plus new coverage for the
validator and `HollowKeepWorldModule` that didn't exist before). Same shape as
[0056](0056-phase-2-7-1-qa-gate.md) was for Phase 2.7.1: filed now that there's something real to
check, not before.

### Ask

Run a full `qa-verify` pass against `world-module-extraction-spec.md`'s acceptance criteria and
`phase-2-8.md`'s Done-When bar. Verify against evidence — source, passing test, reproducible
session — not ticket prose:

1. **`IOneD20WorldModule` + `DefaultOneD20WorldModule`** — contract exists in
   `LunaFlow.Rulebook.OneD20`; `DefaultOneD20WorldModule` reproduces today's exact shipped content.
   Confirm the diff claim (0060's item 1) against source, not just that a diff was run.
2. **Static surface retired** — `OneD20RulebookContent`/`OneD20FlavorText` actually gone; grep
   `src/`, `example/`, and `src/MyLunaFlowServer` yourself for zero references (spec's own
   acceptance scope explicitly excludes `tests/` from this bullet).
3. **`OneD20SessionAssembler` decoupled** — takes `IOneD20WorldModule`; confirm zero references to
   the old type names in its source.
4. **Validation pass** — `OneD20WorldModuleValidator` rejects a deliberately broken module with a
   clear, load-time, non-zero-exit error. 0060 claims this was demonstrated by wiring a throwaway
   broken module into `Bootstrap()` and reverting — confirm this is reproducible, not just narrated,
   and that 0062's new `OneD20WorldModuleValidatorTests.cs` actually isolates and pins each
   fail-fast check rather than asserting a tautology.
5. **The actual Done-When bar: a second, independently-authored World Module runs correctly with
   zero Rulebook/ROM changes required.** `HollowKeepWorldModule` is the proof — confirm it against
   source (not just 0060's playtest narrative) that `GridMap`/`D20System`/`Loot`/`LunaFlow.Rulebook`
   structural code needed no changes to support it, and that its content is a genuine reskin (per
   [0061](0061-hollow-keep-fixed-vocabulary-keys-must-not-be-prefixed.md)'s fixed-vocabulary fix
   actually landing, not just claimed).
6. **`DefaultOneD20WorldModule` is a real, complete implementation** — the "second" module must not
   be the one exercising all properties for the first time; confirm both modules exercise the full
   contract surface.
7. **No new Cartridge/`ComponentRegistry` capability was needed** — `world-module-extraction-spec.md`'s
   own "Framework dependencies" section says none anticipated; confirm nothing in the build quietly
   introduced one.
8. **Regression green from an Arryn-run `latest.trx`.** All suite numbers on record so far (47/47,
   62/62) are agent-run — Baraka's and Nadia's own. Per `agent-test-workflow.md`'s hard rule, a
   phase-doneness verdict needs an Arryn-run pass; I'll have Arryn run the full solution suite before
   you write the verdict — flag back if what you receive isn't disclosed as such.
9. **Verdict + evidence to the PM**, written to `Documents/QA/`. A fail routes to Baraka (defect) or
   Amara (spec-vs-reality) — not a fix by you.

### Response

**Verdict: PASS.** Full report: [`phase-2-8-verdict.md`](../../../QA/phase-2-8-verdict.md). Ran all
nine ask items — two parallel subagents covered the source-verification lanes, I ran build/trx/live
sessions myself and cross-checked both agents' claims rather than accepting either at face value.

1–4 (content fidelity, static surface gone, assembler decoupling, validator implements every spec
check): confirmed against source. Content fidelity specifically was a byte-level diff, not a shape
check — git recorded the extraction as renames, and every data literal (rooms, challenges, monsters,
loot table, PC stats, flavor text) sits in the diff's unchanged context lines.

5. **Validator fail-fast — demonstrated, not asserted.** Removed `"win-announce"` from
   `DefaultOneD20WorldModule.FlavorText.cs`, rebuilt, ran the server: clear error naming the exact
   missing key, exit code 1, before any gameplay. Reverted, confirmed clean.
6. **HollowKeepWorldModule, the actual Done-When bar — verified myself, live.** Swapped
   `Program.cs`'s one line, rebuilt clean (zero other file changes needed), then played a full
   session: won one fight (correct loot), **lost** a second (3-round timeout — correctly awarded
   nothing, proving the loss-gates-loot invariant survives under new content), solved a skill check,
   saw the different-theme flavor text throughout. `git show --stat` on the phase's commit confirms
   zero touches to GridMap/D20System/Loot; the one `LunaFlow.Rulebook` file touched
   (`OneD20RuleBook.cs`) is a single doc-comment line, not logic — read the diff directly rather than
   taking "zero Rulebook changes" as literally zero-files.
7. `DefaultOneD20WorldModule` completeness — confirmed, all 8 members populated at expected counts.
8. No new Cartridge/`ComponentRegistry` capability — confirmed via git log (untouched since Phase
   2.7.1) and the Cartridge directory (still exactly 5 interfaces).
9. Regression suite — 155/155, fresh. You flagged your own provenance concern in this ticket; the
   user confirmed directly in-session that this run is theirs, which I'm treating as stronger than
   the secondhand ticket-prose disclosures earlier phases relied on.

**One real, non-blocking finding:** `OneD20WorldModuleValidatorTests.cs` pins the validator's
mechanism correctly but under-specifies coverage — only 1 of the 11 fixed-vocabulary keys and 1 of
the 6 abilities have a dedicated test each (found by an independent subagent read, not from ticket
prose). A regression dropping an untested key/ability would pass CI silently today. Not filing a
ticket — no reproducible failure exists in the shipped mechanism, which I demonstrated live — but
worth a note to Nadia to parameterize those three loops over their full lists.

### Resolution

**Resolved 2026-08-03 (Kofi, opener).** Read `phase-2-8-verdict.md` directly, not just this
ticket's summary: all criteria PASS, none deferred, evidence attached to every row (git diffs read
directly, live fault injection with a reverted repo confirmed clean, a live Hollow Keep session
covering win/loss/skill-check paths). The one open finding (validator test-coverage gaps) is
explicitly non-blocking per Lothar's own call — mechanism is demonstrated working, only the test
suite's breadth is thin. Passing that along to Nadia as an informal note, not a ticket, same
handling Phase 2.7.1's non-blocking joint-session gap got. Phase 2.8 marked done in
`roadmap.md`/`phase-2-8.md`, citing this verdict.
