# QA Verdict Index

Tracks the current, authoritative QA verdict for each phase or ticket. Produced by the
[`qa-verify` skill](../ClaudeWiki/Tools/qa-verify-skill.md); one file per verdict, same pattern as
[`Handoffs/_index.md`](../ClaudeWiki/Processes/Handoffs/_index.md). Superseded reports move to
`Archive/` with a banner explaining what replaced them — never silently overwritten or deleted.

| Phase / Ticket | Verdict | Report | Notes |
| --- | --- | --- | --- |
| Phase 2.7 (Multi-ROM composition), full done-bar ([0025](../ClaudeWiki/Processes/Handoffs/0025-rom-rulebook-boundary-qa-gate.md) gate fired) | **PASS** (one process flag, not a technical blocker) | [phase-2-7-verdict.md](phase-2-7-verdict.md) | Build green, 119/119 suite. All 25 acceptance criteria across `phase-2-7.md`/`cross-rom-capability-spec.md`/`grid-rom-d20-split-spec.md` verified against source, not ticket prose — bootstrap probe and `DependsOnRoms` validation independently fault-injection-tested by QA (0046/0047), real flavor text confirmed in place of placeholders (0043), orphan `src/LunaFlow.Cartridge` confirmed fully gone from `.sln` (0049). **Flagged, not blocking:** the `latest.trx` this verdict reads was run solo by an agent per ticket [0050](../ClaudeWiki/Processes/Handoffs/0050-test-harness-needs-roomchallengeprompt-gameendannouncement-seeds.md)'s own disclosure, not by Arryn — a process-rule violation QA corroborated independently via narrower runs of its own rather than accepting at face value. |
| Phase 2.6 (ROM/Rulebook boundary fortification), one-ROM shape ([0022](../ClaudeWiki/Processes/Handoffs/0022-implement-phase-2-6.md)) | **PASS** — one-ROM boundary, filling the verdict `phase-2-6.md` recorded as waived at close | [phase-2-6-verdict.md](phase-2-6-verdict.md) | Build green (0 errors), 96/96 suite pass confirmed via fresh `latest.trx` (relocated `D20System` test classes present, not inherited). Rulebook source-clean of Engine (grep), `IRuleBook` shape, handler/rule-group relocation, `.sln` wiring all verified against source. Does **not** cover the two-ROM/cross-ROM shape — that's [0025](../ClaudeWiki/Processes/Handoffs/0025-rom-rulebook-boundary-qa-gate.md), now fired above. |
| Phase 2.5 (1.d20 Vertical Slice), full done-bar ([0020](../ClaudeWiki/Processes/Handoffs/0020-phase-2-5-final-verdict.md)) | **PASS** — automated coverage delivered via ticket 0017; `GeneratorServices` condition verified from source | [phase-2-5-verdict.md](phase-2-5-verdict.md) | Build green (0 errors), 96/96 suite pass (25 new Phase2.5 tests from 0017), independently confirmed via `latest.trx`. All 17 acceptance criteria covered by automated tests (report's own summary line undercounted at 15, corrected on close). IMechanic ruling verified, no manual-only gap. |
| Phase 2 (Rule Pipeline MVP), full done-bar ([0011](../ClaudeWiki/Processes/Handoffs/0011-phase-2-final-verdict.md)) | **PASS** — fully automated, no manual-only gap | [phase-2-verdict.md](phase-2-verdict.md) | Build green, 71/71 from a fresh full suite run (not inherited). B1–B5 cross-checked against actual pipeline source, not just the test file. |
| Phase 1.5 (Nine Rooms), full done-bar ([0004](../ClaudeWiki/Processes/Handoffs/0004-phase-1-5-final-verdict.md)) | **PASS** (documented manual-only gap on 3 of 6 criteria — see report) | [phase-1.5-verdict.md](phase-1.5-verdict.md) | Build green, 63/63 suite pass (read from `latest.trx` directly), off-grid rejection spot-checked live. Supersedes the stale report below. |
| Phase 1.5 — build + smoke gate ([0002](../ClaudeWiki/Processes/Handoffs/0002-phase-1-5-verification-build.md)) | **PASS** | [phase-1.5-build-smoke-verdict.md](phase-1.5-build-smoke-verdict.md) | Cheap gate only; superseded in scope by the full verdict above but kept as its cited evidence. |
| ~~Phase 1.5 (Nine Rooms), inspection-only~~ | superseded | [Archive/Phase_1.5_QA_Report.md](Archive/Phase_1.5_QA_Report.md) | Predates [0001](../ClaudeWiki/Processes/Handoffs/0001-room-entry-announcement.md) and any real build. Superseded by the row above. |

## Related

- [QA role](../ClaudeWiki/Roles/qa.md)
- [`qa-verify` skill](../ClaudeWiki/Tools/qa-verify-skill.md)
- [Handoff ticket process](../ClaudeWiki/Processes/handoff-tickets.md)
