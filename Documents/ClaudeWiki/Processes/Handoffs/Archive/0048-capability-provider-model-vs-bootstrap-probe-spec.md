# 0048 — `Capability`'s provider model doesn't match the bootstrap probe's assumptions

**Status:** resolved
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-07-29
**For:** Amara (Architect)

## Decision

**Ruling:** `Capability` gains a third, optional field — `ProviderType` — set only on `Provides`-side
entries via a new generic factory, `Capability.Provided<TContract, TProvider>(name)` where
`TProvider : TContract`. That constraint makes a mismatched pairing a **compile error**, which
subsumes §5.2's runtime assignability check entirely — nothing left to probe there. §5.3's
member-resolution forcing survives, relocated onto `Capability` itself (`ForceMemberResolution()`,
called once per registered component's `Provides` list from inside 0046's §5.1 barrier) — it's still
checking something a compile-time constraint can't: the *installed* contract assembly may have been
rebuilt with a changed member signature since this ROM last compiled, and that's a deployment-time
fact no generic constraint can see. `ComponentRegistry`/`Validate()` are untouched — this doesn't
touch how `Requires` is matched to `Provides` by name; it makes each `Provides` entry self-certifying
at the one point (construction) where the type information already exists. 0046 can proceed once
this lands: its scope shrinks to the §5.1 barrier plus calling `ForceMemberResolution()` per
component, no separate provider-instance lookup needed.

## Details

### Context

Reading [`cross-rom-capability-spec.md`](../../../Specs/cross-rom-capability-spec.md) §5 in full
before starting ticket [0046](0046-bootstrap-contract-probe-not-built.md) (Lothar's QA finding: the
bootstrap contract probe was never built). §5.2's assignability probe example is:

```csharp
if (!req.ContractType.IsAssignableFrom(provider.GetType()))
    throw new InvalidOperationException(...);
```

where "provider" is described as "the registered provider" — i.e., whichever component's `Provides`
list satisfied the requirement's `Capability.Name`. In this codebase, the only things `Program.cs`
registers with `ComponentRegistry` are the ROM classes themselves (`GridMapRom`, `D20SystemRom`), and
**neither ROM class implements the interfaces it declares in `Provides`**:

- `GridMapRom.Provides` lists `IPlayerLocation`/`IPlayerMovement`/`IGameWon` — but `GridMapRom : IRom`
  only. The actual implementers are separate objects never registered with `ComponentRegistry`: the
  fact `PlayerPosition : IPlayerLocation`, the fact `WinMarker : IGameWon`, and the adapter
  `GridMovementService : IPlayerMovement`.
- `D20SystemRom.Provides` lists `IChallengeSolved`/`ICombatStatus` — implemented by the facts
  `RoomSolved`/`CombatRound`, not by `D20SystemRom` itself.

Checked this isn't a one-off oversight in the current build: it's true of every capability in the
real, shipped, correctly-working GridMap/D20System pair. A literal implementation of §5.2's example —
`req.ContractType.IsAssignableFrom(registeredComponent.GetType())` — would fail **every** check
against this real content, not just a deliberately broken build. That directly contradicts
acceptance criterion 9 ("a provider that declares a capability without implementing its
`ContractType` fails... "; the implication being one that *does* implement it should pass — nothing
here does, today).

The root cause: `Capability` as built is an **aggregate-level declaration** ("this ROM provides X,
via something inside it — a fact type, an adapter object"), not a promise that the registered
component itself is assignable to the contract. §5.2's example code assumes the latter. This gap
predates my work — it's how `Capability`/`ComponentRegistry` have worked since Phase 2.6.5
(`ContractType` "required at construction and read by nothing," per the glossary's own entry, and per
§5's own intro: "this is also where `Capability.ContractType` finally earns its slot").

I raised two resolution shapes with the user rather than picking one:

- **A.** Register the actual contract-implementing objects with `ComponentRegistry` (e.g.
  `GridMovementService` for `PlayerMovement`), and scope the assignability/member-resolution probe to
  capabilities that have a real, single provider instance — silently skip fact-shaped ones, since
  there's no one instance to check (a ROM can insert arbitrarily many fact instances over a session).
- **B.** Something else — Amara's call; I don't have full context on why `Capability` was shaped this
  way at 2.6.5 versus what §5 assumes.

**User's direction: hold all of ticket 0046 (including the otherwise-unblocked §5.1 barrier) and file
this as its own question rather than build around it.**

### Ask

Rule on how the assignability probe (§5.2) and member-resolution probe (§5.3) should reconcile with
`Capability`'s current aggregate-declaration shape, given that no registered `IRom` component in the
real GridMap/D20System pair implements any contract it declares. Candidate shapes (not exhaustive):

- Register the concrete contract-implementing objects (facts don't fit — no singleton instance;
  adapters like `GridMovementService` do) and scope the probe to what can genuinely be assigned an
  instance.
- Change what `Capability.Provides` is expected to name — perhaps the *fact type itself*
  (`typeof(PlayerPosition)`) rather than always the contract type, so the probe has something concrete
  to check per capability. Would need to reconcile with `req.ContractType.IsAssignableFrom(...)`'s
  current single-type shape either way.
- Some other model for what "provides" means at the `IRom` level versus what a probe can mechanically
  verify.

Once ruled, [0046](0046-bootstrap-contract-probe-not-built.md) can build §5.2/§5.3 against a shape
that won't immediately fail on real content. §5.1 (the bootstrap failure barrier itself — wrapping
construction/registration/validation in a try/catch for the binding exception family, reporting
module/contract, exiting non-zero) has no dependency on this question and could ship independently,
but the user asked to hold the whole ticket rather than split it.

### Response

Your diagnosis is right and the root cause is exactly what you named: `Capability` is an
aggregate-level declaration, and §5.2's example silently assumed it was an instance-level one.
Rather than pick between your two candidates (register the adapters; or repoint `Provides` at the
fact type), the two collapse into one move once you notice §5.1 already tells you where the check
belongs: *"a module's `Provides`/`Requires` are built with `typeof(IContract)` expressions, so type
resolution happens there, before the registry ever sees it."* That sentence describes construction
of the `Capability` object itself — so put the check there, not in a separate pass hunting for a
registered instance that, for a fact-shaped capability, was never going to exist.

**Shape change to `Capability`** (`src/LunaFlow.Engine/Components/Capability.cs`):

```csharp
public sealed record Capability
{
    // Existing 2-arg constructor, unchanged — still what a Requires entry uses.
    // A requirement doesn't own an implementation; there's nothing to check on that side.
    public Capability(string name, Type contractType) { /* ... as today ... */ }

    public Type? ProviderType { get; private init; }

    /// <summary>Declares a Provides-side capability. TProvider : TContract makes a mismatched
    /// pairing a compile error — this is what replaces §5.2's runtime IsAssignableFrom check.</summary>
    public static Capability Provided<TContract, TProvider>(string name) where TProvider : TContract
        => new Capability(name, typeof(TContract)) { ProviderType = typeof(TProvider) };

    /// <summary>§5.3's member-resolution probe, relocated here from a standalone pass. No-ops for
    /// Requires-side capabilities (ProviderType null). Called once per registered component's
    /// Provides list, from inside 0046's §5.1 barrier — forces JIT of the interface map without
    /// invoking anything, catching a signature change in the installed contract assembly that a
    /// type check alone (or a compile-time constraint against a *different* build) can't see.</summary>
    public void ForceMemberResolution()
    {
        if (ProviderType is null) return;
        var map = ProviderType.GetInterfaceMap(ContractType);
        foreach (var target in map.TargetMethods)
            System.Runtime.CompilerServices.RuntimeHelpers.PrepareMethod(target.MethodHandle);
    }
}
```

**Declaration sites** (`GridMapRom.Provides`, `D20SystemRom.Provides`) change from
`new("PlayerLocation", typeof(IPlayerLocation))` to
`Capability.Provided<IPlayerLocation, PlayerPosition>("PlayerLocation")`, and likewise for
`PlayerMovement`/`GridMovementService`, `GameWon`/`WinMarker`, `ChallengeSolved`/`RoomSolved`,
`CombatStatus`/`CombatRound`. `Requires`-side entries (`GridMapRom.Requires`,
`D20SystemRom.Requires`) are untouched — they keep the plain 2-arg constructor exactly as shipped.

**What this buys, concretely:**
- A ROM author who pairs a capability with a type that doesn't implement it gets a **build error**,
  not a bootstrap-time throw discovered later — strictly earlier than §5.2 itself would have caught
  it, and consistent with `cartridge.md`'s own "enforcement by construction, not by check" principle
  (same move already used for the trust boundary, applied once more here).
- §5.3's real job — catching a member-signature change in a contract assembly this ROM was compiled
  against but the deployed one no longer matches — survives untouched, just called as
  `capability.ForceMemberResolution()` per `Provides` entry instead of via a standalone
  provider-instance lookup that had nothing to look up.
- `ComponentRegistry.Validate()` needs no changes; `Requires`/`Provides` name-matching is exactly as
  shipped. This is additive to `Capability`, not a rework of the registry.

**Disclosed limit, not papered over:** nothing stops a future `Provides` entry from being written
with the raw 2-arg constructor instead of `Provided<,>()`, silently skipping both the compile-time
check and `ForceMemberResolution()`. Accepted for now — same class of convention-enforced-not-
type-enforced gap as `DependsOnRoms` itself being declared-but-unvalidated until this phase. Splitting
`Capability` into distinct `RequiredCapability`/`ProvidedCapability` types would close it structurally,
but that's a second real type for a problem with exactly one observed instance so far (you finding
this while building, not a defect in the wild) — not building it on the strength of that. Revisit if
a `Provided<,>()`-bypass actually happens.

I'll update `cross-rom-capability-spec.md` §5.2/§5.3 and its acceptance criteria 9–10 to match this
shape — that's mine to do, not yours. Once it's there, 0046 can build against it: the §5.1 barrier
(wrap construction/registration/validation in the try/catch, catch the binding-exception family,
report module + capability name + contract, exit non-zero) plus calling
`capability.ForceMemberResolution()` for every `Provides` entry on every registered component,
inside that same barrier. No separate assignability pass, no provider-instance registration.

### Resolution

Closed 2026-07-29. Implemented exactly per the ruling in [0046](0046-bootstrap-contract-probe-not-built.md):
`Capability.ProviderType`/`Provided<TContract, TProvider>()`/`ForceMemberResolution()` added to
`src/LunaFlow.Engine/Components/Capability.cs`; `GridMapRom`/`D20SystemRom`'s `Provides` entries
repointed at their concrete implementers (`PlayerPosition`, `GridMovementService`, `WinMarker`,
`RoomSolved`, `CombatRound`); `Requires` entries left untouched, as specified. Verified both halves of
the ruling work as intended: pairing a contract with a type that doesn't implement it is a compile
error (`CS0311`, demonstrated and reverted), and `ForceMemberResolution()` runs cleanly over the real
Provides list with no false failures — the exact scenario that would have broken under §5.2's
original runtime-assignability wording. Confirmed the spec file
(`cross-rom-capability-spec.md` §5.2/§5.3, criteria 9–11) already matches what got built — checked
against the live file, not just Amara's Response text. 0046 built against this and is in-review.
