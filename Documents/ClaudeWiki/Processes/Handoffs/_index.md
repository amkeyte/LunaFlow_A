# Handoff Ticket Index

See [`../handoff-tickets.md`](../handoff-tickets.md) for the process this index supports.

**Tickets 0001–0020** (Phases 1.5, 2, 2.5 — all closed) split to
[`index-archive.md`](index-archive.md) 2026-07-26. **Tickets 0021–0039** (Phase 2.6 through the
Cartridge pivot — all closed) split to [`index-archive-2.md`](index-archive-2.md) 2026-08-02, same
reason. **Tickets 0040–0057** (Cartridge-pivot aftermath through Phase 2.7.1 and Phase 2.8's kickoff
— all closed) split to [`index-archive-3.md`](index-archive-3.md) 2026-08-09, same reason. This page
starts at 0058.

**Rows are a one-line pointer, hard cap ~25 words, no bold, no sub-clauses.** The ask/ruling/why
lives in each ticket's own `## Decision` section (2–4 sentences) — that's what "open the ticket"
is for. If a row needs a second sentence to say what happened, it's already too long; trim it, don't
extend it. (Restated 2026-07-29 after this table bloated back up once already — the original
one-liner instruction wasn't specific enough to hold.)

| Ticket | Status | Opened by | For | Summary |
| --- | --- | --- | --- | --- |
| [0058](0058-juno-role-scope-separate-content-design-from-mechanism-verification.md) | in-review | Juno (Game Designer) | Kofi (PM) | Trigger fired (2 clean phases); Arryn ruled to leave the split alone — overhead not worth it yet. |
| [0059](0059-second-world-module-reskin-content.md) | resolved | Kofi (PM) | Juno (Game Designer) | Hollow Keep reskin specced, key bug fixed via 0061, built and playtested by Baraka in 0060. |
| [0060](0060-implement-world-module-extraction-spec-md.md) | resolved | Kofi (PM) | Baraka (Impl Dev) | All five items built and playtested; closed once 0062 landed full suite green. |
| [0061](0061-hollow-keep-fixed-vocabulary-keys-must-not-be-prefixed.md) | resolved | Amara (Architect) | Juno (Game Designer) | Hollow Keep's 11 fixed handler-code keys were `hk-`-prefixed; de-prefixed, re-verified. |
| [0062](0062-tests-lunaflow-contenttests-broken-by-phase-2-8-world-module-extraction.md) | resolved | Baraka (Impl Dev) | Nadia (Test Dev) | 5 files repointed + new validator/HollowKeep coverage; 62/62 green, independently re-verified. |
| [0063](0063-phase-2-8-qa-gate.md) | resolved | Kofi (PM) | Lothar (QA) | Verdict **PASS**, see [phase-2-8-verdict.md](../../../QA/phase-2-8-verdict.md). Phase 2.8 marked done. |
| [0064](0064-phase-2-9-ready-and-phase-4-scope-correction.md) | in-review | Amara (Architect) | Kofi (PM) | Phase 2.9 added, Phase 4 framing corrected, deferral reaffirmed; 0058 closure routed to Arryn. |
| [0065](0065-implement-lua-world-module-spec-md.md) | resolved | Kofi (PM) | Baraka (Impl Dev) | Verified against spec + API reference; all criteria held except Arryn-run trx, routed to 0067. |
| [0066](0066-lua-world-module-maintained-test-coverage.md) | resolved | Baraka (Impl Dev) | Nadia (Test Dev) | 15 new tests; 77/77 + 93/93, independently re-verified by Baraka before closing. |
| [0067](0067-proposal-2-10-2-12-content-framework-stress-test-phases-post-2-9.md) | resolved | Juno (Game Designer) | Amara (Architect) | Split/sequencing approved; `Roadmap/Proposals/` ratified as the proposal-doc home, index + roadmap pointer live. |
| [0068](0068-phase-2-9-qa-gate.md) | resolved | Kofi (PM) | Lothar (QA) | Verdict **PASS**, see [phase-2-9-verdict.md](../../../QA/phase-2-9-verdict.md). Phase 2.9 marked done. |
| [0069](0069-implement-phase-2-10-content-coverage-spec-md.md) | resolved | Kofi (PM) | Baraka (Impl Dev) | Build scope met, 170/170 green, doc-currency loose end independently verified fixed. QA gate cut as 0074. |
| [0070](0070-tests-contenttests-broken-by-phase-2-10-roomchallenge-shape.md) | resolved | Baraka (Impl Dev) | Nadia (Test Dev) | 4 call sites repointed, 19/19 pass; the 5 flagged data-gap failures closed against 0069, 77/77 now. |
| [0071](0071-peer-rom-bidirectional-capability-cycle-detection-gap.md) | resolved | Baraka (Impl Dev) | Amara (Architect) | Ruled: undeclared correlation-only side, docs confirmed committed (correction in 0072); EquippedWeapon rework verified in 0072. |
| [0072](0072-build-cartridge-contract-surface-shape-check.md) | resolved | Amara (Architect) | Baraka (Impl Dev) | §5.4 shape check built, demonstrated both directions; EquippedWeapon→IEquippedWeapon landed. Flagged doc gap was a stale check. |
| [0073](0073-rule-compilation-cost-and-runtime-mode.md) | open | Baraka (Impl Dev) | Amara (Architect) | Roslyn is ~14x NRules' own Rete-compile cost; options charted in a companion design doc, not yet ruled. |
| [0074](0074-phase-2-10-qa-gate.md) | resolved | Kofi (PM) | Lothar (QA) | Verdict **PASS**, see [phase-2-10-verdict.md](../../../QA/phase-2-10-verdict.md). Phase 2.10 marked done. |
| [0075](0075-phase-2-10-spec-currency-cleanup.md) | resolved | Kofi (PM) | Juno (Game Designer) | All three fixed, plus `EquippedWeapon`'s stale Mechanics description and 2.10 spec's status flipped to `active`. |
| [0076](0076-implement-phase-2-11-capability-stress-test-spec-md.md) | resolved | Kofi (PM) | Baraka (Impl Dev) | Built in full, 11/11 criteria; 181/181 Arryn-run trx confirmed. QA gate cut as 0080. |
| [0077](0077-do-peer-roms-still-need-one-project-each-now-that-cross-assembly-correlation-is-proven.md) | resolved | Juno (Game Designer) | Amara (Architect) | Peer ROMs consolidate into one assembly per default now (D20System excepted); Reputation's placement routed to 0078. |
| [0078](0078-consolidate-gridmap-d20system-loot-reputation-into-one-implementation-assembly.md) | resolved | Amara (Architect) | Baraka (Impl Dev) | Merge verified independently (repo state, no cross-refs); D20System's isolation confirmed intact. |
| [0079](0079-phase-2-11-maintained-test-coverage.md) | resolved | Baraka (Impl Dev) | Nadia (Test Dev) | 11 new tests, 88/88 green, independently re-verified. Item 4's literal wording was my own bug. |
| [0080](0080-phase-2-11-qa-gate.md) | resolved | Kofi (PM) | Lothar (QA) | Verdict **PASS**, see [phase-2-11-verdict.md](../../../QA/phase-2-11-verdict.md). Phase 2.11 marked done. |
| [0081](0081-phase-2-12-spec-ready-for-ratification-read.md) | resolved | Kofi (PM) | Amara (Architect) | Spec clean, ratified into `phase-2-12.md`. Build ticket cut as 0084. |
| [0082](0082-implement-phase-2-11-1-rulebook-content-extraction-spec-md.md) | open | Amara (Architect) | Baraka (Impl Dev) | Extract OneD20 Rulebook content from `src/LunaFlow.Rulebook` into new `example/OneD20` project. |
| [0083](0083-phase-2-12-item-11-mechanics-section-misdescribes-enrage-s-defense-effect.md) | resolved | Amara (Architect) | Juno (Game Designer) | Spec fixed — Enrage ruled purely defensive; source citation added. Verified, closed. |
| [0084](0084-implement-phase-2-12-lua-worldmodule-stress-test-spec-md.md) | resolved | Kofi (PM) | Baraka (Impl Dev) | Built in full, 12/12 criteria; 200/200 Arryn-run trx confirmed. QA gate cut as 0086. |
| [0085](0085-phase-2-12-maintained-test-coverage.md) | resolved | Baraka (Impl Dev) | Nadia (Test Dev) | 19 new tests, 107/107 green, independently re-verified (hand-checked roll math on 2 tricky cases). |
| [0086](0086-phase-2-12-qa-gate.md) | resolved | Kofi (PM) | Lothar (QA) | Verdict **PASS**, see [phase-2-12-verdict.md](../../../QA/phase-2-12-verdict.md). Phase 2.12 marked done. |
| [0087](0087-phase-2-12-item-11-finding-for-gm-overlay-draft.md) | open | Kofi (PM) | Amara (Architect) | Item 11's Phase-4 finding routed forward so it's not stranded in a QA verdict file. |
