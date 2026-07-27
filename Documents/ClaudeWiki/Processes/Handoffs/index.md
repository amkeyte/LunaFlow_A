# Handoff Ticket Index

See [`../handoff-tickets.md`](../handoff-tickets.md) for the process this index supports.

**Tickets 0001–0020** (Phases 1.5, 2, 2.5 — all closed) split to
[`index-archive.md`](index-archive.md) 2026-07-26 once this table grew unwieldy. This page starts
at 0021.

Summaries below are one-line pointers, not full narratives — each ticket carries its own
`## Decision` section (2–4 sentences: ask, ruling, why) for that. Open the ticket for the real
story; don't expect this table to substitute for it.

| Ticket | Status | Opened by | For | Summary |
| --- | --- | --- | --- | --- |
| [0021](0021-phase-2-6-spec-alignment.md) | resolved (item 1) | Kofi (PM) | Juno (Game Designer) | Phase 2.6 spec alignment — item 1 done; item 2 blocked on implementation (later closed via 0026). |
| [0022](0022-implement-phase-2-6.md) | resolved | Kofi (PM) | Baraka (Impl Dev) | Phase 2.6 ROM/Rulebook retrofit implemented & verified — one-ROM shape PASS. |
| [0023](0023-cross-rom-fact-mediation.md) | rescinded | Juno (Game Designer) | Amara (Architect) | Cross-ROM fact mediation gap — superseded by Phase 2.6.5's real mechanism. |
| [0024](0024-phase-2-6-5-scope.md) | resolved | Kofi (PM) | Amara (Architect) | Phase 2.6.5 scope ruling — `EngineTier` kept, `IScript` cut, `IRom.version` cut. |
| [0025](0025-rom-rulebook-boundary-qa-gate.md) | open | Kofi (PM) | Lothar (QA) | Standing QA gate for the two-ROM/cross-ROM boundary — parked until Phase 2.7 ships. |
| [0026](0026-reconcile-1-d20-spec.md) | resolved | Kofi (PM) | Juno (Game Designer) | `1-d20-spec.md` reconciled to the shipped Phase 2.6 split. |
| [0027](0027-implement-phase-2-6-5.md) | resolved | Kofi (PM) | Baraka (Impl Dev) | Phase 2.6.5 infrastructure implemented & verified — identity/declaration/validation shipped, not cross-ROM mediation. |
| [0028](0028-phase-2-6-5-test-suite.md) | resolved | Baraka (Impl Dev) | Nadia (Test Dev) | Phase 2.6.5 test suite (17 tests, 113/113 full suite). |
| [0029](0029-phase-2-6-5-status-and-qa-gap.md) | resolved | Amara (Architect) | Kofi (PM) | Phase 2.6.5 status/QA-gap raise — Done-When restated, QA-verdict-required rule written down. Closed by Amara. |
| [0030](0030-doc-reconcile-post-2-6-5.md) | open | Amara (Architect) | Amara (Architect) | Doc-reconcile log post-2.6.5 (Amara's own pages) — includes the `IGameSystem` ruling. |
| [0031](0031-code-currency-vs-2-6-5-design.md) | resolved | Amara (Architect) | Baraka (Impl Dev) + Nadia (A2) | Code-vs-design currency post-2.6.5 — cycle-check defect, `IInteropRom` doc trim, `IGameSystem` deletion, plus addendum items A1 (component-name uniqueness) and A2 (cycle test coverage). All six verified against source; 119/119 on the 2026-07-26 trx. |
| [0032](0032-remove-igamesystem.md) | rescinded | Kofi (PM) | Baraka (Impl Dev) | Duplicate of 0031 item 4. |
| [0033](0033-spike-does-nrules-match-facts-through-interfaces.md) | resolved | Amara (Architect) | Nadia (Test Dev) | Spike gating [`cross-rom-capability-spec.md`](../../../Specs/cross-rom-capability-spec.md): does NRules match facts through *interfaces*, not just base classes? Docs imply yes, never say it, and nothing in the codebase exercises it. If it passes, keep it as a durable characterization test — the whole cross-ROM boundary rests on third-party behaviour we don't control. **Answered from NRules source at the pinned v1.0.2 tag, not from the spike: interfaces DO match.** `TypeNode.IsSatisfiedBy` is `FilterType.IsAssignableFrom(fact.FactType)`, root admits everything, and every child node filters independently — so one inserted fact also satisfies its concrete node *and* its contract node, settling the follow-on that would have sunk the design. The spike's round-1 negative was a **false negative** (fixture trouble, not a library limit); not chased, per Arryn. Round 2 (abstract-base fallback) withdrawn unbuilt. Spec §3 stands. Lesson recorded as a `CLAUDE.md` working convention — **upstream source before inference**. |
| [0034](0034-handoff-juno-builds-against-cross-rom-capability-spec-md.md) | resolved | Kofi (PM) | Juno (Game Designer) | Handoff: build the ROM split against [`cross-rom-capability-spec.md`](../../../Specs/cross-rom-capability-spec.md) §3 (stable authoring contract). Juno's split: GridMap (spatial) / trimmed D20System (challenge/combat), spec at [`grid-rom-d20-split-spec.md`](../../../Specs/grid-rom-d20-split-spec.md); no framework gap found, but interop-assembly location surfaced a project-layout question, resolved via 0036. Closed by Kofi, checked against the spec file itself. |
| [0035](0035-broken-relative-links-across-documents-cleanup-pass.md) | resolved | Kofi (PM) | Kofi (PM) | Broken-link cleanup, raised by 0030 item G. Fixed the systemic `../..`→`../../..` pattern in 12 Handoffs tickets (24 links); 108 → 84. Remainder (old archived tickets, Roadmap phase docs, deprecated Nine Rooms paths) itemized, not chased — low value relative to review cost. |
| [0036](0036-grid-rom-d20system-project-layout-decision.md) | resolved | Juno (Game Designer) | Amara (Architect) | GridMap/D20System project-layout decision — ruled directly by Arryn: three real projects (`GridMap`, `D20System`, and `D20SystemInterop` as a genuine third ROM implementing `IInteropRom`, not a passive contracts library), per the example scaffold in `example/D20SystemExampleA`/`B`/`C`. |
| [0037](0037-implement-the-gridmap-d20system-split-grid-rom-d20-split-spec-md.md) | open | Kofi (PM) | Baraka (Impl Dev) | Build [`grid-rom-d20-split-spec.md`](../../../Specs/grid-rom-d20-split-spec.md) in full, plus `IInteropRom.Resolve`/`Resolve<T>` per `cross-rom-capability-spec.md` §4.1. Both specs stable, no design work left. `DependsOnRoms` validation explicitly out of scope — separate, already-tracked work. |
