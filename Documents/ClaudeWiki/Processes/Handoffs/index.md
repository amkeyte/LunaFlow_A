# Handoff Ticket Index

See [`../handoff-tickets.md`](../handoff-tickets.md) for the process this index supports.

| Ticket | Status | Opened by | For | Summary |
| --- | --- | --- | --- | --- |
| [0001](0001-room-entry-announcement.md) | resolved | Kofi (PM) | Amara (Architect) | Room-entry announcement: ruled `go` announces destination `Description` unconditionally; implemented in `MoveCommandHandler`, docs updated. Verified & closed by Kofi. |
| [0002](0002-phase-1-5-verification-build.md) | resolved | Kofi (PM) | Lothar (QA) | Phase 1.5 build + smoke gate: **PASS** — build green (9.0.300, 0 errors), top-row line plays, no `NotImplementedException`. Transcript traced against source & verified; closed by Kofi. Unblocks 0003. |
| [0003](0003-phase-1-5-engine-fixture-tests.md) | resolved | Kofi (PM) | Nadia (Test Dev) | Phase 1.5 engine-fixture suite. Accepted rule-engine fixture (63/63) as automated coverage; handler/0001 pipeline left to 0002 manual smoke (Arryn's call, disposable content); false "tested separately" docstring corrected. History: bounced 2× before this call. |
| [0004](0004-phase-1-5-final-verdict.md) | resolved | Kofi (PM) | Lothar (QA) | Phase 1.5 final verdict: **PASS** — build green, 63/63 (trx read directly), off-grid spot-checked live, 3/6 criteria manual-only disclosed, stale report archived + QA index reconciled. Verified & closed; **Phase 1.5 marked done** on roadmap. |
| [0005](0005-test-parallelism-default.md) | resolved | Kofi (PM) | Nadia (Test Dev) | Serial-by-default: `[assembly: DoNotParallelize]` in `MSTestSettings.cs`, docs reconciled (`testing.md`/`test-runbook.md`). Verified & closed by Kofi. |
| [0006](0006-harden-engine-tests.md) | resolved | Kofi (PM) | Nadia (Test Dev) | Harden GameEngineCore tests. 63 green; 3 assertion-hack sleeps → gates; subscriber cleanup in place. Closed on Arryn's call; ~22 benign/cleanup sleeps remain by design (2–3 timing-adjacent noted as first suspects if flakiness appears). |
