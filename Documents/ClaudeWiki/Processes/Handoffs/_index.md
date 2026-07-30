# Handoff Ticket Index

See [`../handoff-tickets.md`](../handoff-tickets.md) for the process this index supports.

**Tickets 0001–0020** (Phases 1.5, 2, 2.5 — all closed) split to
[`index-archive.md`](index-archive.md) 2026-07-26 once this table grew unwieldy. This page starts
at 0021.

**Rows are a one-line pointer, hard cap ~25 words, no bold, no sub-clauses.** The ask/ruling/why
lives in each ticket's own `## Decision` section (2–4 sentences) — that's what "open the ticket"
is for. If a row needs a second sentence to say what happened, it's already too long; trim it, don't
extend it. (Restated 2026-07-29 after this table bloated back up once already — the original
one-liner instruction wasn't specific enough to hold.)

| Ticket | Status | Opened by | For | Summary |
| --- | --- | --- | --- | --- |
| [0021](0021-phase-2-6-spec-alignment.md) | resolved (item 1) | Kofi (PM) | Juno (Game Designer) | Phase 2.6 spec alignment; item 2 later closed via 0026. |
| [0022](0022-implement-phase-2-6.md) | resolved | Kofi (PM) | Baraka (Impl Dev) | Phase 2.6 ROM/Rulebook retrofit — one-ROM shape PASS. |
| [0023](0023-cross-rom-fact-mediation.md) | rescinded | Juno (Game Designer) | Amara (Architect) | Cross-ROM fact mediation — superseded by 2.6.5's real mechanism. |
| [0024](0024-phase-2-6-5-scope.md) | resolved | Kofi (PM) | Amara (Architect) | Phase 2.6.5 scope ruling — `EngineTier` kept, `IScript`/`IRom.version` cut. |
| [0025](0025-rom-rulebook-boundary-qa-gate.md) | in-review | Kofi (PM) | Lothar (QA) | Standing QA gate fired — verdict FAIL, see [phase-2-7-verdict.md](../../../QA/phase-2-7-verdict.md). |
| [0026](0026-reconcile-1-d20-spec.md) | resolved | Kofi (PM) | Juno (Game Designer) | `1-d20-spec.md` reconciled to the shipped Phase 2.6 split. |
| [0027](0027-implement-phase-2-6-5.md) | resolved | Kofi (PM) | Baraka (Impl Dev) | Phase 2.6.5 infrastructure — identity/declaration/validation shipped. |
| [0028](0028-phase-2-6-5-test-suite.md) | resolved | Baraka (Impl Dev) | Nadia (Test Dev) | Phase 2.6.5 test suite — 17 tests, 113/113 full suite. |
| [0029](0029-phase-2-6-5-status-and-qa-gap.md) | resolved | Amara (Architect) | Kofi (PM) | Status/QA-gap raise — Done-When restated, QA-verdict rule written down. |
| [0030](0030-doc-reconcile-post-2-6-5.md) | open | Amara (Architect) | Amara (Architect) | Doc-reconcile log post-2.6.5, incl. `IGameSystem` ruling. |
| [0031](0031-code-currency-vs-2-6-5-design.md) | resolved | Amara (Architect) | Baraka + Nadia | Code-vs-design currency post-2.6.5 — six items, all verified; 119/119. |
| [0032](0032-remove-igamesystem.md) | rescinded | Kofi (PM) | Baraka (Impl Dev) | Duplicate of 0031 item 4. |
| [0033](0033-spike-does-nrules-match-facts-through-interfaces.md) | resolved | Amara (Architect) | Nadia (Test Dev) | Does NRules match through interfaces? Answered from source: yes. Spec §3 stands. |
| [0034](0034-handoff-juno-builds-against-cross-rom-capability-spec-md.md) | resolved | Kofi (PM) | Juno (Game Designer) | Build the ROM split per `cross-rom-capability-spec.md` §3; layout question routed to 0036. |
| [0035](0035-broken-relative-links-across-documents-cleanup-pass.md) | resolved | Kofi (PM) | Kofi (PM) | Broken-link cleanup — systemic pattern fixed across 12 tickets, 108 → 84. |
| [0036](0036-grid-rom-d20system-project-layout-decision.md) | resolved (superseded in part) | Juno (Game Designer) | Amara (Architect) | Three-project layout confirmed; `D20SystemInterop`-as-ROM framing superseded by 0039/0040. |
| [0037](0037-implement-the-gridmap-d20system-split-grid-rom-d20-split-spec-md.md) | in-review | Kofi (PM) | Baraka (Impl Dev) | GridMap/D20System split built, PM-reviewed; blocked on 0044 before a QA pass. |
| [0038](0038-phase-2-7-test-suite-blocked-on-rom-implementations-in-src.md) | resolved | Nadia (Test Dev) | Baraka (Impl Dev) | Compile-blocker resolved — ROMs confirmed to stay in `example/`, not `src/`. |
| [0039](0039-cartridge-the-published-unit-and-no-upward-lookup-apis.md) | resolved | Amara (Architect) | Kofi (PM) | Cartridge named as the published unit; `IInteropRom.Resolve` cut. New `design/cartridge.md`. |
| [0040](0040-pivot-to-2-6-5-and-re-sequence-phase-2-7.md) | resolved | Amara (Architect) | Kofi (PM) | Operational unwind of 0039 — Phase 2.6.5 untouched; no re-sequencing. |
| [0041](0041-correct-grid-rom-d20-split-spec-for-cartridge-pivot.md) | resolved | Kofi (PM) | Juno (Game Designer) | Spec's interop framing corrected to a Cartridge contract surface. Location fix in 0042. |
| [0042](0042-correct-cartridge-project-location-in-spec.md) | resolved | Kofi (PM) | Juno (Game Designer) | Same-day location fix to 0041 — contract surface stays in `example/`, namespace only renamed. |
| [0043](0043-generated-rules-have-no-seam-for-rulebook-flavor-text.md) | open | Baraka (Impl Dev) | Amara (Architect) | Generated rules have no DI seam for Rulebook flavor text — placeholder text kept, question raised. |
| [0044](0044-fix-d20systemhandlertests-after-0037.md) | in-review | Kofi (PM) | Nadia (Test Dev) | `tests/` compiling again after 0037's split. Three failures traced to a real defect, filed as 0045. |
| [0045](0045-challenge-prompt-rule-missing-solved-failed-winmarker-guards.md) | resolved | Nadia (Test Dev) | Baraka (Impl Dev) | Missing rule guards restored; new `IGameWon` Cartridge contract per Amara's ruling. 25/25 pass. |
| [0046](0046-bootstrap-contract-probe-not-built.md) | in-review | Lothar (QA) | Baraka (Impl Dev) | Bootstrap contract probe built per 0025's FAIL, against 0048's ruling. 119/119 suite green. |
| [0047](0047-dependsonroms-validation-not-enforced.md) | in-review | Lothar (QA) | Baraka (Impl Dev) | `DependsOnRoms` validation built per 0025's FAIL — both failure modes demonstrated. 119/119 green. |
| [0048](0048-capability-provider-model-vs-bootstrap-probe-spec.md) | resolved | Baraka (Impl Dev) | Amara (Architect) | `Capability` model didn't match the probe spec's assumption — `Provided<TContract,TProvider>` rules it. |
| [0049](0049-delete-orphan-src-lunaflow-cartridge-project.md) | open | Kofi (PM) | Baraka (Impl Dev) | Empty `src/LunaFlow.Cartridge` project, flagged by the QA verdict, contradicts 0042's ruling. Delete it. |
