# 0053 — Split content tests into their own test project

**Status:** resolved
**Opened by:** Kofi (PM)
**Opened:** 2026-07-30
**For:** Nadia (Test Dev)

## Decision

Split content/ROM tests into a new `tests/LunaFlow.ContentTests` project — done, not just scoped,
since it turned out mechanical enough to execute directly (test-file reorg only, zero `src/`
behavior change). Neither `Core/` nor `GeneratorServices/` was fully content-free as assumed:
`Core/ComponentRegistryTests.cs` had two regions touching real `OneD20RuleBook`/ROM types, and
`Core/NineRoomsEngineFixtureTests.cs` is 100% content-dependent — both required a judgment call, not
just a folder move. One blocker surfaced that's outside my `tests/` lane: the new project needs an
`InternalsVisibleTo` grant in `src/LunaFlow.Engine`, which amkeyte is adding directly. The build
won't go green until that lands.

## Details

### Context

**Not blocking, not tied to a roadmap phase — backlog item, raised by Arryn.** All tests today live
in one project, `tests/LunaFlow.EngineTests`, with framework tests (`Core/`, `GeneratorServices/`)
and content/ROM tests (`Rom/`) side by side. That `Rom/` folder is already growing — Phase 2.7 added
GridMap/D20System suites, Phase 2.7.1 (ticket
[0052](0052-implement-loot-rom-content-expansion-spec-md.md)) adds Loot on top — and every future
content phase adds more, in the same project as the framework's own tests.

### Ask

Scope out (don't build yet, unless it's genuinely low-risk to do now) splitting content/ROM tests
into their own test project, separate from framework tests. Goal: being able to run "just the
content tests" or "just the framework tests" without a `--filter` on one shared project, and keeping
content-test growth from bloating the project that proves the engine/rule-pipeline itself. Consider:
whether this is a straight namespace/folder move to a new `tests/LunaFlow.ContentTests`-shaped
project, whether `Core`/`GeneratorServices` truly have zero content dependency today (worth checking
before assuming a clean split), and how this interacts with the `[DoNotParallelize]`-for-singleton
convention in `testing.md`. Report back with a plan; this doesn't need to ship before Phase 2.7.1 or
anything else in flight.

### Response

Built the split directly rather than only scoping it — see Decision for why (mechanical, no `src/`
behavior change, fully within `tests/`).

**What moved.** `Rom/D20System/*` (2 files), `Rom/Loot/*` (3 files), `Rulebook/OneD20/*` (1 file) —
all six files that build against `example/D20SystemExampleA-D` and `LunaFlow.Rulebook` content —
moved into `tests/LunaFlow.ContentTests`, namespaces rewritten `LunaFlow.EngineTests.*` →
`LunaFlow.ContentTests.*` (the compiled assembly's name should match; nothing depends on the old
namespace string). `tests/LunaFlow.EngineTests/Rom/` is now gone (emptied, then removed).

**Two things weren't a straight move — checked before assuming, per the ask:**

1. **`Core/ComponentRegistryTests.cs` wasn't content-free.** Two regions — the pre-existing
   `OneD20RuleBook`+synthetic-D20System concrete case, and the three-ROM case I added earlier this
   session — construct real `OneD20RuleBook`/`GridMapRom`/`D20SystemRom`/`LootRom` instances. Split
   those into a new `tests/LunaFlow.ContentTests/Rom/ComponentRegistryContentTests.cs`; the
   remaining `Core/ComponentRegistryTests.cs` now tests `Capability`/`ComponentRegistry` semantics
   against synthetic doubles only (`TestComponent`/`ComponentWithCapabilities`), zero content
   reference.
2. **`Core/NineRoomsEngineFixtureTests.cs` is 100% content-dependent (`LunaFlow.Rom.NineRooms`) but
   I left it in `LunaFlow.EngineTests`.** Call: NineRooms is a toolbox-tier ROM shipped directly in
   `src/LunaFlow.Rom` (per `architecture.md`'s "today: D20System, NineRooms"), not an `example/`-tier
   game — it's the one-time Phase 1.5 proof that the engine loop works, not the growing
   example-game content this ticket is actually about (GridMap/D20System/Loot, all in `example/`,
   all still growing every content phase). `LunaFlow.EngineTests` keeps its `LunaFlow.Rom` reference
   for exactly this file; it needs nothing else content-related. Reversible with a one-line move if
   Kofi disagrees with the call.

**`[DoNotParallelize]`-for-singleton interaction.** The attribute is assembly-scoped
(`MSTestSettings.cs`), so it doesn't cross project boundaries — `tests/LunaFlow.ContentTests` got
its own copy (`tests/LunaFlow.ContentTests/MSTestSettings.cs`), same `[assembly: DoNotParallelize]`,
same rationale comment. Each project is its own test-host process, so `GameEngineCore.Instance`'s
state can't leak between them; the convention just needs re-declaring per-project, not re-designed.

**Reference cleanup.** `LunaFlow.EngineTests.csproj` dropped `example/D20SystemExampleA/B/C/D` and
`LunaFlow.Rulebook` entirely (confirmed via grep: nothing left in the project references
`LunaFlow.Cartridge`, `LunaFlow.Rom.D20System`/`.GridMap`/`.Loot`, or `LunaFlow.Rulebook.*`) — down
to `LunaFlow.Engine` + `LunaFlow.Rom` only. `tests/LunaFlow.ContentTests.csproj` picked up the six
references EngineTests dropped, minus `LunaFlow.Rom` itself (nothing in the moved content needs
NineRooms — confirmed, avoids a `LunaFlow.Rom.D20System` namespace collision risk between
`src/LunaFlow.Rom`'s `[Obsolete]` legacy `D20SystemContent.cs` and `example/D20SystemExampleA`'s
real `D20SystemRom`, though it turned out not to be a real collision either way — see side-note
below). `LunaFlow_A.sln` updated with the new project + Debug/Release config lines.

**Blocker found, not fixed (outside my lane).** `GameEngineCore` is `internal`, visible to test code
only via `[assembly: InternalsVisibleTo("LunaFlow.EngineTests")]` in
`src/LunaFlow.Engine/GlobalSuppressions.cs`. `tests/LunaFlow.ContentTests`'s handler-level tests
(`D20SystemHandlerTests`, `InventoryCommandHandlerTests`) need `GameEngineCore.Instance` for the same
"drive the real path, not a hand-poked fact" reason every other handler test in this suite does — so
the build fails until `"LunaFlow.ContentTests"` is added alongside the existing entry. One line, zero
behavior change, but it's `src/`, so I didn't touch it — amkeyte is adding it directly. Solution
won't build clean until then; flagging so nobody's surprised by a red build in the interim.

**Side note, not actioned (out of scope for this ticket):** `Documents/ApiReference/LunaFlow.Rom/`
has stale pages for `MonsterTemplate`/`RoomChallengeType`/`D20SystemNarration` under
`LunaFlow/Rom/D20System/` — leftover from before the Phase 2.7 GridMap/D20System split moved those
types to `example/`. `src/LunaFlow.Rom/D20SystemContent.cs` is marked `[Obsolete]` and deliberately
uses `object?` fields to avoid the type dependency now, so there's no real namespace collision, just
an orphaned doc page `DefaultDocumentation` never cleaned up. Worth a doc-lint pass sometime; not
mine to fix and not blocking anything here.

**Test counts — confirmed.** After amkeyte added the `InternalsVisibleTo` line: `dotnet build
LunaFlow_A.sln` clean, `LunaFlow.EngineTests` 93/93, `LunaFlow.ContentTests` 47/47 — 140 total,
exactly matching the pre-split count.

**Second gap found running both, not fixed (process convention, not just code):**
`LunaFlow_A.runsettings` (repo root) pins `ResultsDirectory`/`LogFileName` to a single
`tests/LunaFlow.EngineTests/TestResults/latest.trx` path, documented in the file itself for
`agent-test-workflow.md`'s "one known path" convention. Running `ContentTests` with the same
`--settings` file overwrites that same trx — whichever project runs second wins, silently. Splitting
the test suite means either two `.runsettings` files or one convention for where a two-project run's
combined/separate results land; that's a call for whoever owns `agent-test-workflow.md`
(Arryn/Kofi), not mine to make unilaterally since Lothar's QA verdicts and doneness runs depend on
the current single-path assumption.

### Resolution

**Resolved 2026-08-02 (Kofi, opener).** Split confirmed against source, not just the Response
summary: `tests/LunaFlow.ContentTests.csproj` exists, `tests/LunaFlow.EngineTests/Rom/` is gone,
`LunaFlow_A.sln` carries the new project, and the two judgment calls (`ComponentRegistryTests`'
content-touching regions split out; `NineRoomsEngineFixtureTests` staying in `EngineTests` as a
toolbox-tier ROM, not `example/`-tier content) both hold up under the same reasoning `architecture.md`
and ticket 0038 already established. Test counts confirmed post-fix: 93/93 + 47/47 = 140, matching
the pre-split count exactly.

**Blocker resolved:** the `InternalsVisibleTo` line landed in `src/LunaFlow.Engine` directly, build
is clean.

**The one real open item Nadia escalated — the shared `latest.trx` path silently overwriting between
the two projects — is now closed too**, same session: `LunaFlow_A.ContentTests.runsettings` added
(sibling to the existing file, own `ResultsDirectory`), `agent-test-workflow.md`'s "Fixed path
convention" updated from one path to two, and a "full suite green" claim now explicitly means both
trx files, not whichever ran last. One caveat carried forward, not resolved: whether VS2022 Test
Explorer's "Run All Tests" across both projects in one session already merges into a single trx
regardless of which settings file is selected — noted inline in both runsettings files and
`agent-test-workflow.md` for Arryn to spot-check once; the CLI path is unambiguous either way.

Also updated [0056](0056-phase-2-7-1-qa-gate.md)'s ask to read both trx files, so Lothar's live
QA pass isn't blocked on this.
