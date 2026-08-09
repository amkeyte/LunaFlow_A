# 0047 — `DependsOnRoms` declared but never validated

**Status:** resolved
**Opened by:** Lothar (QA)
**Opened:** 2026-07-29
**For:** Baraka (Impl Dev)

## Decision

Added `ComponentRegistry.ValidateDependsOn(dependentName, dependsOnRoms)` — a new method, kept
separate from `Validate()` since `IHasDependencies` (what `Validate()` operates on) stays ROM-agnostic
and has no `DependsOnRoms` concept; the composition root, which can see both `LunaFlow.Engine` and
`LunaFlow.Rulebook`, supplies the Rulebook's declared list explicitly. Wired into `Program.cs`,
checked *before* `Validate()` specifically so the DependsOnRoms-specific message is what's reported
when a ROM is missing, not the pre-existing capability-mismatch message that happens to fire for the
same underlying cause in this particular ROM pair. Both halves of the acceptance criterion
demonstrated directly (temporarily commenting out each ROM's registration line, one at a time),
119/119 suite green.

## Details

### Context

Verifying Phase 2.7's full done-bar ([`phase-2-7-verdict.md`](../../../QA/phase-2-7-verdict.md)).
`phase-2-7.md`'s Deliverables section itself flags this as half-shipped since 2026-07-26: `IRuleBook
.DependsOnRoms` exists (`src/LunaFlow.Rulebook/IRuleBook.cs:22`, populated on `OneD20RuleBook`), but
`ComponentRegistry.Validate()` never reads it — confirmed by reading the full file
(`src/LunaFlow.Engine/Components/ComponentRegistry.cs`): it checks `Provides`/`Requires`/tier-ordering/
cycles only. `Program.cs` doesn't check `DependsOnRoms` either. A Rulebook naming a ROM that isn't
registered still fails silently rather than with the "clear, immediate error" the phase's own Done-When
requires.

**Not fully masked by the current content, but not proven safe either:** in the shipped GridMap/
D20System pair, each ROM's `Requires` happens to name a capability only the other provides, so removing
either one *does* trip `ComponentRegistry.Validate()`'s "unsatisfied requirement" check today — but
that's an artifact of this specific pair's capability graph, not `DependsOnRoms` being enforced. A
Rulebook naming a ROM it has no capability-level dependency on would still fail silently.

### Ask

Add `DependsOnRoms` enforcement to `ComponentRegistry.Validate()` (or an equivalent check `Program.cs`
runs before proceeding): for every registered Rulebook, confirm every name in `DependsOnRoms` matches
a registered component; error out clearly, naming the missing ROM, if not. Acceptance criterion:
removing either `GridMap` or `D20System`'s registration line in `Program.cs` produces a clear load-time
error attributable to the missing ROM specifically (not an incidental capability-mismatch message that
happens to fire for unrelated reasons).

### Response

**Implemented:**
- `src/LunaFlow.Engine/Components/ComponentRegistry.cs`: new public method
  `ValidateDependsOn(string dependentName, IReadOnlyList<string> dependsOnRoms)` — for each name in
  the list, confirms some registration in `_registrations` carries that exact name; throws
  `InvalidOperationException` naming both the dependent and the missing name if not. Kept off
  `Validate()` deliberately: `IHasDependencies` (the type `Validate()` operates over) is intentionally
  ROM-agnostic — it has no `DependsOnRoms` member, and adding one would mean every `IRom` also needs
  to carry a (for ROMs, always-empty) named-dependency list just so the registry could discover it
  structurally. The composition root already knows both tiers, so it supplies the list explicitly.
- `src/MyLunaFlowServer/Program.cs`: `ConstructRuleBook()` now returns
  `(IHasDependencies Component, IReadOnlyList<string> DependsOnRoms)` instead of just the component —
  reading `IRuleBook.DependsOnRoms` needs the concrete `OneD20RuleBook`/`IRuleBook` type, which
  `Bootstrap()` itself is deliberately kept clear of (same JIT-isolation reasoning as ticket 0046);
  returning the list as plain strings alongside the component keeps `Bootstrap()`'s own signature
  usage safe. `Bootstrap()` calls `registry.ValidateDependsOn("OneD20", dependsOnRoms)` immediately
  after registering all three components, **before** `registry.Validate()` — ordering matters here,
  see below.

**Verification, both halves of the acceptance criterion, demonstrated not asserted:**
- Temporarily commented out `registry.Register(gridMap, EngineTier.Rom, "GridMap");`, ran the server:
  `Component registry validation failed: 'OneD20' declares DependsOnRoms naming 'GridMap', but no
  component named 'GridMap' is registered.` — reverted immediately after confirming.
- Same for D20System's registration line:
  `'OneD20' declares DependsOnRoms naming 'D20System', but no component named 'D20System' is
  registered.` — reverted immediately after confirming.
- In both cases this is the message actually reported, not the pre-existing "unsatisfied capability
  requirement" message that (per this ticket's own Context) would *also* fire for this specific
  ROM pair — `ValidateDependsOn` runs first, deliberately, so its message is what surfaces.
- `dotnet build LunaFlow_A.sln` clean; normal playthrough (both ROMs registered) unaffected;
  `dotnet test tests/LunaFlow.EngineTests` — 119/119 pass, no regressions.

### Resolution

Independently reproduced. Read `ComponentRegistry.cs` in full: `ValidateDependsOn(string
dependentName, IReadOnlyList<string> dependsOnRoms)` exists, iterates `dependsOnRoms`, throws
`InvalidOperationException` naming both the dependent and the missing ROM if any name has no matching
registration. Read `Program.cs`'s `Bootstrap()`: called as `registry.ValidateDependsOn("OneD20",
dependsOnRoms)`, immediately after all three components register and explicitly **before**
`registry.Validate()` — so its message wins over the incidental capability-mismatch one, exactly as
the Response describes.

Then reproduced the fault myself rather than trusting the pasted transcript: commented out
`registry.Register(gridMap, EngineTier.Rom, "GridMap");` in `Program.cs`, ran `dotnet run --project
src/MyLunaFlowServer`. Output: `Component registry validation failed: 'OneD20' declares DependsOnRoms
naming 'GridMap', but no component named 'GridMap' is registered.` — an unhandled
`InvalidOperationException` with that exact message, not a silent failure or a partial game. Reverted
the edit immediately, rebuilt clean (0 errors) to confirm no leftover state.

Also ran `dotnet test --filter "FullyQualifiedName~ComponentRegistryTests"` myself as a narrow
one-off: 23/23 pass — the new method is additive and doesn't disturb existing coverage. No automated
test exists yet for `ValidateDependsOn` itself (grepped `tests/` — no hits); not blocking this ticket,
since the acceptance criterion asked for a demonstrated, attributable load-time error and got exactly
that, but worth a follow-up test so this doesn't silently regress later.

**Same open item as 0046:** `latest.trx` predates this change; a fresh Arryn-run full-suite pass is
still owed before the Phase 2.7 verdict overall can move off FAIL.

Resolved — DependsOnRoms enforcement built and demonstrated firsthand, both ROMs.
