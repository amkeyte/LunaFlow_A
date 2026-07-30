# 0046 — Bootstrap contract probe (§5) not built

**Status:** in-review
**Opened by:** Lothar (QA)
**Opened:** 2026-07-29
**For:** Baraka (Impl Dev)

## Decision

Built against Amara's 0048 ruling (`Capability.Provided<TContract, TProvider>()` — the compile-time
constraint replaces §5.2's runtime check entirely; `ForceMemberResolution()` covers §5.3). §5.1's
barrier is built in `Program.cs`, restructured across separate methods after fault injection showed
a naive inline try/catch doesn't actually work (see Response). Full suite green (119/119); both a
compile-time mismatch and a fault-injected missing assembly were demonstrated, not just asserted.

## Details

### Context

Verifying Phase 2.7's full done-bar ([`phase-2-7-verdict.md`](../../../QA/phase-2-7-verdict.md)) against
[`cross-rom-capability-spec.md`](../../../Specs/cross-rom-capability-spec.md) §5 and acceptance criteria
9–10. §5 specs a bootstrap contract probe: wrap module construction/registration/validation in a single
handler that catches the binding-exception family (`TypeLoadException`, `MissingMethodException`,
`MissingFieldException`, `FileLoadException`, `FileNotFoundException`, `BadImageFormatException`),
reports which module/contract failed, and exits non-zero (§5.1); an assignability probe checking
`IsAssignableFrom` per declared `Requires` (§5.2); and a member-resolution probe using
`GetInterfaceMap`+`PrepareMethod` per registered component (§5.3), since a cast alone doesn't catch
signature skew. Phase-2-7.md names this as a Deliverable ("A bootstrap contract probe... cheap, and
valuable today").

### Ask

Grepped `src/` and `example/` for `PrepareMethod`, `GetInterfaceMap`, `TypeLoadException`,
`MissingMethodException`, `MissingFieldException`, `FileLoadException`, `BadImageFormatException` — zero
hits anywhere in the codebase. `Program.cs`'s only exception handling wraps `registry.Validate()` for
`InvalidOperationException`; there's no construction/registration/validation/probing barrier, no
assignability check, no member-resolution forcing. The probe doesn't exist in any partial form — build
it per §5's three subsections against the current `Program.cs`/`ComponentRegistry` shape, demonstrated
against a deliberately broken build per acceptance criterion 9 (a provider that declares a capability
without implementing its `ContractType` must fail at bootstrap naming both module and contract, process
exits non-zero — a caught-and-continued exception fails the criterion).

### Response

Blocked on [0048](0048-capability-provider-model-vs-bootstrap-probe-spec.md) initially (§5.2's
example didn't match this codebase's `Capability` model — see that ticket). Amara's ruling landed:
`Capability.Provided<TContract, TProvider>()` where `TProvider : TContract` makes a mismatched
Provides-side pairing a compile error, subsuming §5.2 entirely; `ForceMemberResolution()` (a method
on `Capability` itself) covers §5.3, called once per `Provides` entry from inside the §5.1 barrier.

**Implemented:**
- `src/LunaFlow.Engine/Components/Capability.cs`: added `ProviderType`, `Provided<,>()`,
  `ForceMemberResolution()`, exactly per 0048's Decision.
- `GridMapRom.Provides`/`D20SystemRom.Provides` repointed at their concrete implementers:
  `Capability.Provided<IPlayerLocation, PlayerPosition>`, `<IPlayerMovement, GridMovementService>`,
  `<IGameWon, WinMarker>`, `<IChallengeSolved, RoomSolved>`, `<ICombatStatus, CombatRound>`. `Requires`
  entries untouched (plain two-argument constructor, as 0048 specified).
- `src/MyLunaFlowServer/Program.cs`: the §5.1 barrier.

**A real subtlety surfaced building the barrier, worth recording since it isn't obvious.** A first
pass wrapped construction/registration/validation/probing inline in one try/catch, same lexical
scope as the rest of `Program.cs`. Fault-injection (deleting the built Cartridge assembly, running
the exe directly) showed this doesn't work: it crashed as an **unhandled** `FileNotFoundException` at
`Program.<Main>$`, never reaching the catch at all. Cause: C# top-level statements compile into one
single method, and the CLR resolves every type referenced *anywhere* in a method before executing any
of it — so a type that fails to load anywhere in that method (even in code lexically after the
try/catch) fails the *whole method's* JIT compilation before a single statement runs, including the
try/catch itself. A try/catch only creates a real boundary at a genuine method-call boundary.

**Fix:** restructured into `Bootstrap()` (typed only in `IHasDependencies`/`object`/`Capability` —
never a concrete ROM/Cartridge type by name, so its own JIT can never fail on a binding fault
either), which calls `ConstructGridMap()`/`ConstructD20System()`/`ConstructRuleBook()` — each
isolated per module, so a fault's JIT failure surfaces exactly at that call site, with `currentModule`
already set by the caller immediately beforehand. Top-level `Program.cs` itself now only ever holds
`object` references to the two ROMs, wrapping the call to `Bootstrap()` in the try/catch; `RunGame()`
(genuinely downstream, only ever invoked after `Bootstrap()` already proved the types loadable) does
the cast-back and all normal engine/handler wiring — legitimately outside §5's scope, not a gap in it.

**Verification:**
- `dotnet build LunaFlow_A.sln` — clean, no warnings.
- **Criterion 9** (compile-time mismatch): temporarily added
  `Capability.Provided<IChallengeSolved, PlayerPosition>("Bad")` to `Program.cs` — `CS0311`, a build
  error, confirmed and reverted (not shipped).
- **Criterion 11** (barrier fails loudly, non-zero, on a genuine binding fault): built normally, then
  deleted `D20SystemExampleC.dll` (the Cartridge assembly) from the server's output directory and ran
  the built `.exe` directly (not `dotnet run`, which would just rebuild it). Output:
  `Bootstrap contract binding failure in module 'GridMap': FileNotFoundException: ...`, exit code 1.
  Assembly restored, rebuilt clean afterward.
- Normal playthrough (unchanged Cartridge assembly present) still shows correct room descriptions,
  flavor text, and "Component registry validated successfully."
- `dotnet test tests/LunaFlow.EngineTests` — 119/119 pass, no regressions from the `Capability`
  shape change or the `Program.cs` restructure.

**Not covered, disclosed rather than silently assumed solid:** `ConstructRuleBook()`'s isolation is
symmetric with the other two for consistency, but `OneD20RuleBook`'s own assembly
(`LunaFlow.Rulebook`) failing to load is a much less realistic fault than a swappable ROM going
missing — not separately fault-injection-tested, unlike GridMap/Cartridge above.

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>
