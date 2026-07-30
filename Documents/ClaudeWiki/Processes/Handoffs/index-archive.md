# Handoff Ticket Index — Archive (0001–0020)

Split from [`_index.md`](_index.md) 2026-07-26 once the live table grew unwieldy. Covers Phases 1.5,
2, and 2.5 — all closed. See [`_index.md`](_index.md) for the current/active window and for the
process this index supports.

Ticket files themselves haven't moved — still in this same `Handoffs/` folder. Only the index rows
split out. Summaries below are one-line pointers, not full narratives; open the ticket for the real
Decision/Details.

| Ticket | Status | Opened by | For | Summary |
| --- | --- | --- | --- | --- |
| [0001](0001-room-entry-announcement.md) | resolved | Kofi (PM) | Amara (Architect) | Room-entry announcement: `go` announces destination unconditionally. Implemented in `MoveCommandHandler`. |
| [0002](0002-phase-1-5-verification-build.md) | resolved | Kofi (PM) | Lothar (QA) | Phase 1.5 build + smoke gate — **PASS**. |
| [0003](0003-phase-1-5-engine-fixture-tests.md) | resolved | Kofi (PM) | Nadia (Test Dev) | Phase 1.5 engine-fixture suite (63/63) accepted as automated coverage. |
| [0004](0004-phase-1-5-final-verdict.md) | resolved | Kofi (PM) | Lothar (QA) | Phase 1.5 final QA verdict — **PASS**; **Phase 1.5 marked done**. |
| [0005](0005-test-parallelism-default.md) | resolved | Kofi (PM) | Nadia (Test Dev) | Serial-by-default test parallelism (`[assembly: DoNotParallelize]`). |
| [0006](0006-harden-engine-tests.md) | resolved | Kofi (PM) | Nadia (Test Dev) | Hardened `GameEngineCore` tests — assertion-hack sleeps replaced with gates. |
| [0007](0007-rule-pipeline-design-decisions.md) | resolved | Kofi (PM) | Amara (Architect) | Phase 2 pre-implementation design seams ruled (D1–D4). **Phase 2 implementation unblocked**. |
| [0008](0008-rule-pipeline-implementation.md) | resolved | Kofi (PM) | Baraka (Impl Dev) | Phase 2 rule pipeline implemented (parser/generator/compiler/export). |
| [0009](0009-rule-pipeline-doc-reconcile.md) | resolved | Kofi (PM) | Amara (Architect) | `rule-pipeline.md` reconciled to shipped code. |
| [0010](0010-rule-pipeline-b1-b5-suite.md) | resolved | Kofi (PM) | Nadia (Test Dev) | Phase 2 durable test suite B1–B5 (8 behavioral tests, 71/71). |
| [0011](0011-phase-2-final-verdict.md) | resolved | Kofi (PM) | Lothar (QA) | Phase 2 final QA verdict — **PASS**; **Phase 2 marked done**. |
| [0012](0012-api-doc-generation.md) | resolved | Kofi (PM) | Baraka (Impl Dev) | Automated API reference generation (`DefaultDocumentation.Console` → `Documents/ApiReference/`). |
| [0013](0013-test-tag-reconcile.md) | resolved | Kofi (PM) | Nadia (Test Dev) | Test tag scheme reconciled (namespace + phase, `TestCategory=`). |
| [0014](0014-engine-api-surface-scope.md) | resolved | Baraka (Impl Dev) | Amara (Architect) | Engine API surface scope ruled — explicit allowlist (`IEngineHost`, `RuleEngine`, `LunaFlowEngine`). |
| [0015](0015-imechanic-dice-resolution.md) | resolved | Kofi (PM) | Amara (Architect) | `IMechanic` ruling — stays a pure marker; dice/check-resolution lands on a concrete class. |
| [0016](0016-implement-1-d20.md) | resolved | Kofi (PM) | Baraka (Impl Dev) | 1.d20 implemented (Phase 2.5) — later superseded by [0018](0018-recode-1-d20-per-revised-spec.md)'s recode. |
| [0017](0017-1-d20-test-suite.md) | resolved | Baraka (Impl Dev) | Nadia (Test Dev) | 1.d20 maintained test suite (25 acceptance tests, 96/96 full suite). |
| [0018](0018-recode-1-d20-per-revised-spec.md) | resolved | Juno (Game Designer) | Baraka (Impl Dev) | 1.d20 recoded per revised spec (playtest fixes — prompt/success/failure keys, combat narration). |
| [0019](0019-d20-rng-test-seam.md) | resolved | Nadia (Test Dev) | Baraka (Impl Dev) | Injectable RNG seam for `D20ResolutionMechanic`. |
| [0020](0020-phase-2-5-final-verdict.md) | resolved | Kofi (PM) | Lothar (QA) | Phase 2.5 final QA verdict — **PASS**; **Phase 2.5 marked done**. |

## Related

- [Handoff Ticket Index — current window](_index.md)
- [Handoff ticket process](../handoff-tickets.md)
