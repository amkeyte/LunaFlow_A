# 0082 — Implement Phase 2.11.1 rulebook-content-extraction-spec.md

**Status:** open
**Opened by:** Amara (Architect)
**Opened:** 2026-08-08
**For:** Baraka (Impl Dev)

## Decision

<Skip on a fresh `open` ticket per `handoff-tickets.md`'s convention — fill in once there's a
ruling: 2–4 sentences covering the ask, the ruling, and brief why.>

## Details

### Context

Arryn spotted that `src/LunaFlow.Rulebook` holds the entire OneD20 Rulebook's content (twelve files
under `OneD20/`) alongside its one piece of tier infrastructure (`IRuleBook.cs`) — the ROM tier went
through the equivalent cleanup back in Phase 2.7 (`D20System`/`GridMap`/`Loot`/`Reputation` all shed
out to dedicated `example/` projects), and Rulebook never got the same treatment. First framing
considered — merging the content into `OneD20SharedRoms` instead — was rejected: that project is the
D20 Cartridge's implementation, and Rulebook content living inside it would violate the Cartridge
structure rule and the roadmap's standing "Rulebook must not depend on a specific ROM assembly"
constraint. Landed instead on a new, separate `example/OneD20` project — content, not infrastructure,
same rule that already keeps the ROM examples out of `src/`.

### Ask

Build per [`Specs/rulebook-content-extraction-spec.md`](../../../Specs/rulebook-content-extraction-spec.md):
move the twelve files (plus the embedded `.lua` resource) currently under
`src/LunaFlow.Rulebook/OneD20/` into a new `example/OneD20/OneD20.csproj`, leaving
`src/LunaFlow.Rulebook` holding only `IRuleBook.cs`. No namespace changes — every moved type keeps
`LunaFlow.Rulebook.OneD20`. Update `MyLunaFlowServer` and `LunaFlow.ContentTests`'s project references
(new reference to `example/OneD20`; `LunaFlow.Rulebook`'s existing reference on `MyLunaFlowServer`
stays, per the spec's Consumer updates section) and register the new project in `LunaFlow_A.sln`. One
open question flagged in the spec, not resolved here — whether the new project should carry the
`GenerateApiReference` post-build target `LunaFlow.Rulebook.csproj` has (its nearer sibling,
`OneD20SharedRoms`, doesn't) — raise it back rather than silently picking one if the two precedents
don't settle it for you. Full acceptance criteria are in the spec; all of them are mechanical
(build/test parity), nothing here changes behavior.

### Response

<Filled in by the assignee.>

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>
