# 0078 — Consolidate GridMap/Loot(/Reputation) into one implementation assembly; D20System stays separate

**Status:** in-review
**Opened by:** Amara (Architect)
**Opened:** 2026-08-08
**For:** Baraka (Impl Dev)

## Decision

Build item, spec follows below. Merge `example/D20SystemExampleB` (GridMap) and `ExampleD` (Loot)
into one project, one namespace per ROM. **`ExampleA` (D20System) stays exactly where and what it
is — its own separate assembly, per Arryn, for regression testability. Do not fold it in.** `ExampleC`
(the Cartridge contract surface) is also untouched. Sequencing relative to your live 0076 work is
your call — two acceptable paths given below, pick whichever costs you less right now. The
peer-isolation compliance check and assembly fingerprint that would make the merged side safe by
construction are **explicitly not part of this ticket** — deferred per `non-goals.md`, on Arryn's
direction. This ships with a disclosed convention-only gap on the GridMap/Loot side, not a hidden one.

## Details

### Context

Ticket [0077](0077-do-peer-roms-still-need-one-project-each-now-that-cross-assembly-correlation-is-proven.md)
asked whether peer ROMs still need one project each now that cross-assembly capability correlation is
proven three times over. First-pass ruling kept project-per-ROM as the default on a cost-benefit
argument; corrected same day, direct from Arryn, before this ticket was even seen: the argument only
counted the compiler-guarantee side. `example/D20SystemExampleA` is 625 lines across 8 files — a full
solution project for what's really a namespace's worth of content — and a future with 30-odd such
projects clutters Solution Explorer for a guarantee namespaces already give the separation half of.

**Second refinement, also before anyone acted on the first:** D20System specifically — the ROM cited
above as the size example — is the one that stays its own project, not the ones that merge. Not a
contradiction: the clutter argument is what justifies consolidating GridMap/Loot/future peers;
D20System's separateness is a distinct, standing exception, per Arryn, for regression testability —
it's the oldest, most load-bearing ROM (direct descendant of the pre-split 1.d20 content), and being
independently buildable/testable outweighs the clutter argument for this one case. Full reasoning in
[`cartridge.md`'s Structure section](../../design/cartridge.md#structure).

The substitute enforcement for "no peer names a sibling's concrete type" (a build-time compliance
check) and a companion assembly fingerprint (future security use) are both named and deliberately
deferred — see `non-goals.md`'s two new entries. This ticket ships the GridMap/Loot consolidation
without them, by explicit direction, accepting that peer isolation between GridMap and Loot becomes
convention-enforced the moment this lands. D20System's isolation from everything else stays
compiler-enforced, unaffected — it's not part of the merge.

### Ask

1. **Merge `ExampleB` (GridMap) and `ExampleD` (Loot) into one project.** Naming and exact
   `.csproj`/solution folder shape are your call — something that reads as "the D20 Cartridge's
   shared content," not a letter in the `ExampleN` sequence, since that sequence is retired for the
   ROMs that merge. Namespaces (`LunaFlow.Rom.GridMap`, `LunaFlow.Rom.Loot`) carry the separation the
   projects used to. **`ExampleA` (D20System) and `ExampleC` (`LunaFlow.Cartridge`) are not part of
   this merge — leave both exactly where and what they are.**
2. **`Program.cs`'s registration, `ComponentRegistry.Validate()`, and every existing acceptance
   criterion keep passing unchanged** — this is a project-topology move for two ROMs, not a behavior
   change. Nothing about `Capability`/`Provides`/`Requires`/facts/rules/handlers should need to change
   beyond namespace/using-directive churn from the merge itself. D20System's own project is untouched
   by this ticket entirely — no changes to it are in scope here.
3. **Fix references:** the test projects, `Program.cs`, and the `.sln` need repointing at the merged
   GridMap+Loot project instead of two separate ones (D20System's existing references are unaffected).
   Full suite green afterward, same bar as always.
4. **Sequencing against your live 0076 work — your call between two acceptable paths:**
   - **(a) Consolidate GridMap+Loot first, then build Reputation directly into that merged project**
     — cleanest if you haven't started item 7 yet.
   - **(b) Finish 2.11 with Reputation in its own project (`ExampleE`) as originally planned, then
     fold Reputation and Loot into GridMap's (or a new joint) project as an immediate fast-follow** —
     better if you're already mid-item-7 and a context switch costs more than the double-move.
   Either is fine. What's not fine is leaving Reputation in a standalone project indefinitely — that's
   the exact pattern this ticket exists to retire (for everything except D20System).
5. **Do not build the peer-isolation compliance check or the fingerprint as part of this ticket.**
   They're deliberately deferred (`non-goals.md`). If, while doing this merge, you spot an actual
   place where a ROM already reaches into a peer's concrete type instead of the Cartridge contract
   surface, that's a real finding — flag it back here rather than quietly fixing or routing around it.

### Response

Took path (a) — hadn't started item 7 yet, so consolidated first and built Reputation directly into
the merged project.

1. **Merged.** `example/D20SystemExampleB` (GridMap) and `ExampleD` (Loot) are gone; their content
   moved verbatim (namespaces unchanged: `LunaFlow.Rom.GridMap`, `LunaFlow.Rom.Loot`) into a new
   project, `example/OneD20SharedRoms/OneD20SharedRoms.csproj` — named to read as "the D20
   Cartridge's shared content," not a letter in the retired `ExampleN` sequence, per the ask.
   `ExampleA` (D20System) and `ExampleC` (`LunaFlow.Cartridge`) untouched, not referenced by the new
   project any differently than they were before.
2. **Reputation landed directly in the merged project** (`LunaFlow.Rom.Reputation` namespace) rather
   than a standalone `ExampleE` — per the corrected ruling on ticket 0077.
3. **References fixed:** `LunaFlow_A.sln` (removed ExampleB/D entries, added OneD20SharedRoms),
   `MyLunaFlowServer.csproj`, `LunaFlow.ContentTests.csproj`. `dotnet build LunaFlow_A.sln` is clean,
   0 errors. A full `dotnet run --project src/MyLunaFlowServer` confirms
   `ComponentRegistry.Validate()` still passes post-merge (transcript in 0076's Response).
4. **No peer-isolation compliance check or fingerprint built** — out of scope per the ruling,
   untouched. While doing the merge I did not find an existing place where GridMap or Loot already
   reached into the other's concrete type instead of the Cartridge contract surface — both still
   only reference `LunaFlow.Cartridge.*` contracts across the former project boundary, same as
   before. No finding to flag back here on that front.
5. **Test fallout, expected and not fixed here** (tests/ is Nadia's territory): two
   `LunaFlow.ContentTests` tests hardcode the pre-2.11 ROM/capability counts
   (`ComponentRegistryContentTests.ComponentRegistry_GridMapD20SystemLootAllRegistered_ValidatesSuccessfully`
   and `LootRomTests.Requires_NamesChallengeSolved_PairedWithIChallengeSolvedContract`) and now fail
   — this is Reputation joining `DependsOnRoms`/Loot's `Requires`, not a consolidation defect. Filed
   as ticket 0079 to Nadia alongside 0076's other test asks, same pattern as ticket 0070.

### Resolution

<Filled in by the opener when closing.>
