# Handoff Ticket Index — Archive (0021–0039)

Split from [`_index.md`](_index.md) 2026-08-02 once the live table grew unwieldy again. Covers
Phase 2.6 through the Cartridge pivot (2.6.5/2.7 pre-content). See [`_index.md`](_index.md) for the
current/active window and for the process this index supports; see
[`index-archive.md`](index-archive.md) for 0001–0020 and [`index-archive-3.md`](index-archive-3.md)
for 0040–0057.

Ticket files themselves haven't moved — still in this same `Handoffs/` folder. Only the index rows
split out. Summaries below are one-line pointers, not full narratives; open the ticket for the real
Decision/Details.

| Ticket | Status | Opened by | For | Summary |
| --- | --- | --- | --- | --- |
| [0021](0021-phase-2-6-spec-alignment.md) | resolved (item 1) | Kofi (PM) | Juno (Game Designer) | Phase 2.6 spec alignment; item 2 later closed via 0026. |
| [0022](0022-implement-phase-2-6.md) | resolved | Kofi (PM) | Baraka (Impl Dev) | Phase 2.6 ROM/Rulebook retrofit — one-ROM shape PASS. |
| [0023](0023-cross-rom-fact-mediation.md) | rescinded | Juno (Game Designer) | Amara (Architect) | Cross-ROM fact mediation — superseded by 2.6.5's real mechanism. |
| [0024](0024-phase-2-6-5-scope.md) | resolved | Kofi (PM) | Amara (Architect) | Phase 2.6.5 scope ruling — `EngineTier` kept, `IScript`/`IRom.version` cut. |
| [0025](0025-rom-rulebook-boundary-qa-gate.md) | resolved | Kofi (PM) | Lothar (QA) | Standing QA gate — cross-ROM boundary verdict **PASS**, see [phase-2-7-verdict.md](../../../QA/phase-2-7-verdict.md). |
| [0026](0026-reconcile-1-d20-spec.md) | resolved | Kofi (PM) | Juno (Game Designer) | `1-d20-spec.md` reconciled to the shipped Phase 2.6 split. |
| [0027](0027-implement-phase-2-6-5.md) | resolved | Kofi (PM) | Baraka (Impl Dev) | Phase 2.6.5 infrastructure — identity/declaration/validation shipped. |
| [0028](0028-phase-2-6-5-test-suite.md) | resolved | Baraka (Impl Dev) | Nadia (Test Dev) | Phase 2.6.5 test suite — 17 tests, 113/113 full suite. |
| [0029](0029-phase-2-6-5-status-and-qa-gap.md) | resolved | Amara (Architect) | Kofi (PM) | Status/QA-gap raise — Done-When restated, QA-verdict rule written down. |
| [0030](0030-doc-reconcile-post-2-6-5.md) | resolved | Amara (Architect) | Amara (Architect) | Doc-reconcile log post-2.6.5, incl. `IGameSystem` ruling. All 13 items landed, link cleanup routed to and closed via 0035. |
| [0031](0031-code-currency-vs-2-6-5-design.md) | resolved | Amara (Architect) | Baraka + Nadia | Code-vs-design currency post-2.6.5 — six items, all verified; 119/119. |
| [0032](0032-remove-igamesystem.md) | rescinded | Kofi (PM) | Baraka (Impl Dev) | Duplicate of 0031 item 4. |
| [0033](0033-spike-does-nrules-match-facts-through-interfaces.md) | resolved | Amara (Architect) | Nadia (Test Dev) | Does NRules match through interfaces? Answered from source: yes. Spec §3 stands. |
| [0034](0034-handoff-juno-builds-against-cross-rom-capability-spec-md.md) | resolved | Kofi (PM) | Juno (Game Designer) | Build the ROM split per `cross-rom-capability-spec.md` §3; layout question routed to 0036. |
| [0035](0035-broken-relative-links-across-documents-cleanup-pass.md) | resolved | Kofi (PM) | Kofi (PM) | Broken-link cleanup — systemic pattern fixed across 12 tickets, 108 → 84. |
| [0036](0036-grid-rom-d20system-project-layout-decision.md) | resolved (superseded in part) | Juno (Game Designer) | Amara (Architect) | Three-project layout confirmed; `D20SystemInterop`-as-ROM framing superseded by 0039/0040. |
| [0037](0037-implement-the-gridmap-d20system-split-grid-rom-d20-split-spec-md.md) | resolved | Kofi (PM) | Baraka (Impl Dev) | GridMap/D20System split built — all four asks independently confirmed by Lothar's Phase 2.7 verdict. |
| [0038](0038-phase-2-7-test-suite-blocked-on-rom-implementations-in-src.md) | resolved | Nadia (Test Dev) | Baraka (Impl Dev) | Compile-blocker resolved — ROMs confirmed to stay in `example/`, not `src/`. |
| [0039](0039-cartridge-the-published-unit-and-no-upward-lookup-apis.md) | resolved | Amara (Architect) | Kofi (PM) | Cartridge named as the published unit; `IInteropRom.Resolve` cut. New `design/cartridge.md`. |

## Related

- [Handoff Ticket Index — current window](_index.md)
- [Handoff Ticket Index — Archive (0001–0020)](index-archive.md)
- [Handoff ticket process](../handoff-tickets.md)
