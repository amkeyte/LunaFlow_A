# 0072 — Build the Cartridge contract-surface shape check (cross-rom-capability-spec.md §5.4)

**Status:** in-review
**Opened by:** Amara (Architect)
**Opened:** 2026-08-03
**For:** Baraka (Impl Dev)

## Decision

Build item, not a question — spec is precise. Add `ValidateInterfacesOnly(Assembly)` per
[`cross-rom-capability-spec.md` §5.4](../../Specs/cross-rom-capability-spec.md#54-contract-surface-shape-check--interfaces-only),
run it inside §5.1's bootstrap barrier against `LunaFlow.Cartridge`, fail loud and non-zero on any
non-interface exported type. Closes acceptance criterion 12. Bring `EquippedWeapon` (0071's addendum)
in line with the Cartridge shape rule in the same pass, since this check would otherwise fail bootstrap
against your current shape for it.

## Details

### Context

Ticket [0071](0071-peer-rom-bidirectional-capability-cycle-detection-gap.md) surfaced two things:
the GridMap/D20System `Requires` drift (resolved, spec corrected to match shipped code — no build
needed there), and that `EquippedWeapon` shipped as a bare concrete record directly in
`LunaFlow.Cartridge`, which [`cartridge.md`'s Structure
section](../../design/cartridge.md#structure) already prohibited ("interfaces for facts that cross a
boundary... interfaces for services the Cartridge offers") but nothing enforced. Arryn asked, in
review of 0071's ruling: how do we stop this from recurring a third time, given the Cartridge is the
nexus every ROM correlates through — a bare type slipped in there is indistinguishable, from the
assembly graph, from a type that went through proper review.

Answer: a bootstrap-time reflection check, same "hard fail before play starts" discipline the existing
compatibility probe (§5.1–5.3) already uses, applied to a different question — not "does this declared
pairing bind correctly" but "is everything in this assembly actually an interface." Specified in
`cross-rom-capability-spec.md` §5.4, added 2026-08-03. `cartridge.md`'s Structure section now points to
it as the enforcement mechanism behind the rule it already stated.

### Ask

1. Implement `ValidateInterfacesOnly(Assembly)` per §5.4's code sketch (`LunaFlow.Engine.Components`
   is the natural home — same tier that owns `ComponentRegistry`/`Capability`; it's a framework
   invariant check over content, not content itself).
2. Wire it into `Program.cs`'s existing §5.1 bootstrap barrier, called against
   `typeof(IPlayerLocation).Assembly` (or however you're already referencing `LunaFlow.Cartridge`) —
   same non-zero-exit, same message-includes-offending-type bar the existing probes hold.
3. Bring `EquippedWeapon` into the shape §5.4 requires and 0071's addendum specified: `IEquippedWeapon`
   interface in `LunaFlow.Cartridge`, Loot's concrete type implements it and `Provides` it, D20System's
   `fight` handler matches/queries the interface with `Requires` left undeclared (per 0071's general
   ruling — declaring it would reproduce the `Loot`↔`D20System` cycle). Do this in the same pass since
   the new check will otherwise fail bootstrap against the current shape.
4. If a real toolchain-generated public type in the actual built `LunaFlow.Cartridge.dll` trips the
   check on something that isn't author-written (§5.4 flags this as a known unverified edge), report it
   back here rather than quietly loosening the check yourself.

### Response

All three build items done, plus the point-4 check (no unverified-edge report needed — clean).

**1. `ContractSurfaceValidator.ValidateInterfacesOnly(Assembly)`** — new file
`src/LunaFlow.Engine/Components/ContractSurfaceValidator.cs`, `LunaFlow.Engine.Components`, exactly
the §5.4 sketch (reflects `GetExportedTypes()`, throws `InvalidOperationException` naming every
non-interface offender by full name).

**2. Wired into `Program.cs`'s `Bootstrap()`** — called right after `registry.Validate()` succeeds
and before the existing §5.3 member-resolution loop, against `typeof(IPlayerLocation).Assembly`
(`currentModule = "Cartridge"` set first, for consistent error attribution if it ever throws inside
the binding-exception-family catch — though this check throws `InvalidOperationException`, which the
top-level code already catches and reports via the existing `"Validation failed: {ex.Message}"` path,
no new catch needed).

**3. `EquippedWeapon` brought into shape**, per 0071's addendum exactly:
- `IEquippedWeapon` (fact-shaped, one member — `ItemId`) added to `LunaFlow.Cartridge`
  (`example/D20SystemExampleC/LunaFlow.Cartridge/IEquippedWeapon.cs`).
- The bare `EquippedWeapon` record deleted from `LunaFlow.Cartridge`; a new `EquippedWeapon : IEquippedWeapon`
  now lives in Loot's own `Facts.cs`, where the other three Loot-owned facts already are.
- `LootRom.Provides` gains `Capability.Provided<IEquippedWeapon, EquippedWeapon>("EquippedWeapon")` —
  same pattern, same ticket-0048 compile-time-mismatch guarantee, as every other provided contract.
- `FightCommandHandler` now does `rules.Query<IEquippedWeapon>()`, not the concrete type.
- `D20SystemRom.Requires` unchanged (still just PlayerLocation/PlayerMovement/GameWon) but its doc
  comment now states explicitly why `EquippedWeapon` isn't there, mirroring `GridMapRom.Requires`'s
  existing comment and citing 0071.

**4. No unverified toolchain edge found.** Demonstrated both directions against the real built
`LunaFlow.Cartridge.dll`, not just a synthetic test: `dotnet build` + `dotnet run --project
src/MyLunaFlowServer` with a fresh session prints `Component registry validated successfully.` and
proceeds to a normal session with the current (post-rework) Cartridge — the positive case. Then
temporarily added `public sealed record TempViolation(int Whatever);` directly to
`LunaFlow.Cartridge`, rebuilt, reran: bootstrap printed `Validation failed: Contract surface
'D20SystemExampleC' exposes non-interface type(s): LunaFlow.Cartridge.TempViolation...` and exited via
the unhandled `InvalidOperationException` (non-zero) before any ROM was constructed — the negative
case, satisfying criterion 12's "demonstrated, not asserted" bar. Removed the temp type, rebuilt clean
(only the 4 pre-existing, already-ticketed-to-Nadia `RoomChallenge` errors in
`tests/LunaFlow.ContentTests` remain — ticket [0070](0070-tests-contenttests-broken-by-phase-2-10-roomchallenge-shape.md)).
No compiler-generated public type in the real assembly tripped the check — nothing to report back.

Ticket [0069](0069-implement-phase-2-10-content-coverage-spec-md.md)'s item 2/4 paragraph will get a
short pointer to this ticket's resolution rather than being rewritten — its acceptance criteria for
`equip`/Backup Piece are otherwise unaffected by this rework (same behavior, different plumbing).

One thing outside this ticket's scope but worth surfacing since I hit it directly: 0071's ruling text
says the GridMap/D20System `Requires` drift was "corrected" in `grid-rom-d20-split-spec.md`'s
capability table and that a general rule was "written into `engine-core.md`'s Cross-ROM correlation
section." Checked both files directly (`git diff`) — neither has actually changed; only `cartridge.md`
picked up its corollary. Not mine to fix (design docs, not `src/`) — flagging back rather than leaving
it silently stale or editing outside my territory.

### Resolution

<Filled in by the opener when closing.>
