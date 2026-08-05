# Handoff Ticket Index

See [`../handoff-tickets.md`](../handoff-tickets.md) for the process this index supports.

**Tickets 0001–0020** (Phases 1.5, 2, 2.5 — all closed) split to
[`index-archive.md`](index-archive.md) 2026-07-26. **Tickets 0021–0039** (Phase 2.6 through the
Cartridge pivot — all closed) split to [`index-archive-2.md`](index-archive-2.md) 2026-08-02, same
reason. This page starts at 0040.

**Rows are a one-line pointer, hard cap ~25 words, no bold, no sub-clauses.** The ask/ruling/why
lives in each ticket's own `## Decision` section (2–4 sentences) — that's what "open the ticket"
is for. If a row needs a second sentence to say what happened, it's already too long; trim it, don't
extend it. (Restated 2026-07-29 after this table bloated back up once already — the original
one-liner instruction wasn't specific enough to hold.)

| Ticket | Status | Opened by | For | Summary |
| --- | --- | --- | --- | --- |
| [0040](0040-pivot-to-2-6-5-and-re-sequence-phase-2-7.md) | resolved | Amara (Architect) | Kofi (PM) | Operational unwind of 0039 — Phase 2.6.5 untouched; no re-sequencing. |
| [0041](0041-correct-grid-rom-d20-split-spec-for-cartridge-pivot.md) | resolved | Kofi (PM) | Juno (Game Designer) | Spec's interop framing corrected to a Cartridge contract surface. Location fix in 0042. |
| [0042](0042-correct-cartridge-project-location-in-spec.md) | resolved | Kofi (PM) | Juno (Game Designer) | Same-day location fix to 0041 — contract surface stays in `example/`, namespace only renamed. |
| [0043](0043-generated-rules-have-no-seam-for-rulebook-flavor-text.md) | resolved | Baraka (Impl Dev) | Amara (Architect) | Amara ruled: seed already-resolved text as a fact (`RoomChallengePrompt`/`GameEndAnnouncement`), not a key. Built, playtested. Test-harness follow-up filed as 0050. |
| [0044](0044-fix-d20systemhandlertests-after-0037.md) | resolved | Kofi (PM) | Nadia (Test Dev) | `tests/` compiling again after 0037's split; full suite green, confirmed via Lothar's Arryn-run trx. |
| [0045](0045-challenge-prompt-rule-missing-solved-failed-winmarker-guards.md) | resolved | Nadia (Test Dev) | Baraka (Impl Dev) | Missing rule guards restored; new `IGameWon` Cartridge contract per Amara's ruling. 25/25 pass. |
| [0046](0046-bootstrap-contract-probe-not-built.md) | resolved | Lothar (QA) | Baraka (Impl Dev) | Bootstrap contract probe built per 0025's FAIL, against 0048's ruling. Lothar independently reproduced both fault-injection scenarios before closing. |
| [0047](0047-dependsonroms-validation-not-enforced.md) | resolved | Lothar (QA) | Baraka (Impl Dev) | `DependsOnRoms` validation built per 0025's FAIL. Lothar independently reproduced the missing-ROM fault before closing. |
| [0048](0048-capability-provider-model-vs-bootstrap-probe-spec.md) | resolved | Baraka (Impl Dev) | Amara (Architect) | `Capability` model didn't match the probe spec's assumption — `Provided<TContract,TProvider>` rules it. |
| [0049](0049-delete-orphan-src-lunaflow-cartridge-project.md) | resolved | Kofi (PM) | Baraka (Impl Dev) | Empty `src/LunaFlow.Cartridge` project, flagged by the QA verdict, contradicted 0042's ruling. Removed; `.sln` entry also needed removing (found working 0043). |
| [0050](0050-test-harness-needs-roomchallengeprompt-gameendannouncement-seeds.md) | resolved | Baraka (Impl Dev) | Nadia (Test Dev) | Both test files seed `RoomChallengePrompt`/`GameEndAnnouncement` mirroring `Program.cs`; 8 tests rewritten to assert real flavor text. Full suite re-verified: 119/119. |
| [0051](0051-phase-2-7-1-content-package-ready-for-kickoff.md) | resolved | Juno (Game Designer) | Kofi (PM) | Phase 2.7.1 content package reviewed, consistent; wired into `roadmap.md`; build ticket cut as 0052. |
| [0052](0052-implement-loot-rom-content-expansion-spec-md.md) | resolved | Kofi (PM) | Baraka (Impl Dev) | Loot ROM content expansion built — third peer ROM, monster roster, randomization, inventory, all confirmed. |
| [0053](0053-split-content-tests-into-their-own-test-project.md) | resolved | Kofi (PM) | Nadia (Test Dev) | Split confirmed, 140/140 across two projects; shared-trx-path collision fixed with a second runsettings file. |
| [0054](0054-loot-rom-test-suite.md) | resolved | Baraka (Impl Dev) | Nadia (Test Dev) | Suite compiling again, all six new-coverage items built — 119/119 → 140/140. |
| [0055](0055-loot-spec-mechanism-deviations.md) | resolved | Baraka (Impl Dev) | Amara (Architect) | Two mechanism deviations building 0052 — both ratified, spec corrected to match what shipped. |
| [0056](0056-phase-2-7-1-qa-gate.md) | resolved | Kofi (PM) | Lothar (QA) | Standing QA gate for Phase 2.7.1 — re-fired, verdict **PASS**, see [phase-2-7-1-verdict.md](../../../QA/phase-2-7-1-verdict.md). |
| [0057](0057-phase-2-8-design-package-ready-for-kickoff.md) | resolved | Amara (Architect) | Kofi (PM) | Phase 2.8 kicked off — sequencing ruled, build cut as 0059/0060. |
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
| [0075](0075-phase-2-10-spec-currency-cleanup.md) | in-review | Kofi (PM) | Juno (Game Designer) | All three fixed, plus `EquippedWeapon`'s stale Mechanics description and 2.10 spec's status flipped to `active`. |
