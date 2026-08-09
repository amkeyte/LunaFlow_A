# Handoff Ticket Index — Archive (0040–0057)

Split from [`_index.md`](_index.md) 2026-08-09 once the live table grew unwieldy again. Covers the
Cartridge-pivot aftermath and Phase 2.7 close-out, Phase 2.7.1 (Loot ROM) build through its QA gate,
and Phase 2.8's design kickoff — all closed. See [`_index.md`](_index.md) for the current/active
window and for the process this index supports; see [`index-archive.md`](index-archive.md) for
0001–0020 and [`index-archive-2.md`](index-archive-2.md) for 0021–0039.

Ticket files themselves haven't moved — still in this same `Handoffs/` folder. Only the index rows
split out. Summaries below are one-line pointers, not full narratives; open the ticket for the real
Decision/Details.

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
