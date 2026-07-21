# QA Verdict Index

Tracks the current, authoritative QA verdict for each phase or ticket. Produced by the
[`qa-verify` skill](../ClaudeWiki/Tools/qa-verify-skill.md); one file per verdict, same pattern as
[`Handoffs/index.md`](../ClaudeWiki/Processes/Handoffs/index.md). Superseded reports move to
`Archive/` with a banner explaining what replaced them — never silently overwritten or deleted.

| Phase / Ticket | Verdict | Report | Notes |
| --- | --- | --- | --- |
| Phase 2.5 (1.d20 Vertical Slice), full done-bar ([0020](../ClaudeWiki/Processes/Handoffs/0020-phase-2-5-final-verdict.md)) | **PASS** — automated coverage delivered via ticket 0017; `GeneratorServices` condition verified from source | [phase-2-5-verdict.md](phase-2-5-verdict.md) | Build green (0 errors), 96/96 suite pass (25 new Phase2.5 tests from 0017), independently confirmed via `latest.trx`. All 17 acceptance criteria covered by automated tests (report's own summary line undercounted at 15, corrected on close). IMechanic ruling verified, no manual-only gap. |
| Phase 2 (Rule Pipeline MVP), full done-bar ([0011](../ClaudeWiki/Processes/Handoffs/0011-phase-2-final-verdict.md)) | **PASS** — fully automated, no manual-only gap | [phase-2-verdict.md](phase-2-verdict.md) | Build green, 71/71 from a fresh full suite run (not inherited). B1–B5 cross-checked against actual pipeline source, not just the test file. |
| Phase 1.5 (Nine Rooms), full done-bar ([0004](../ClaudeWiki/Processes/Handoffs/0004-phase-1-5-final-verdict.md)) | **PASS** (documented manual-only gap on 3 of 6 criteria — see report) | [phase-1.5-verdict.md](phase-1.5-verdict.md) | Build green, 63/63 suite pass (read from `latest.trx` directly), off-grid rejection spot-checked live. Supersedes the stale report below. |
| Phase 1.5 — build + smoke gate ([0002](../ClaudeWiki/Processes/Handoffs/0002-phase-1-5-verification-build.md)) | **PASS** | [phase-1.5-build-smoke-verdict.md](phase-1.5-build-smoke-verdict.md) | Cheap gate only; superseded in scope by the full verdict above but kept as its cited evidence. |
| ~~Phase 1.5 (Nine Rooms), inspection-only~~ | superseded | [Archive/Phase_1.5_QA_Report.md](Archive/Phase_1.5_QA_Report.md) | Predates [0001](../ClaudeWiki/Processes/Handoffs/0001-room-entry-announcement.md) and any real build. Superseded by the row above. |

## Related

- [QA role](../ClaudeWiki/Roles/qa.md)
- [`qa-verify` skill](../ClaudeWiki/Tools/qa-verify-skill.md)
- [Handoff ticket process](../ClaudeWiki/Processes/handoff-tickets.md)
