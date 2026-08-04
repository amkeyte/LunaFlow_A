# Cross-ROM Capability Mediation — Implementation Spec

**Status:** draft — §2's gating question is **resolved** (NRules matches through interfaces, verified against source); §3's authoring contract is stable and ready for content design to build against.
**Companion design doc:** [`../design/engine-core.md`](../design/engine-core.md) — "Component model: ROM, Rulebook, and the event bus."
**Related specs:** [`engine-host-spec.md`](engine-host-spec.md)

## Purpose

Phase 2.6.5 shipped capability *declaration* and load-time *validation*, but left `IInteropRom` an
empty marker — the broker it names does not exist. This spec defines what a ROM must do to let a peer
ROM correlate against facts it owns, with **zero assembly reference between the peers**, and what the
broker has to be for that to work.

## Scope

**In:** the shared-contract mechanism for cross-ROM fact correlation; where contract types live; the
broker's runtime surface; the replacement path for `D20SystemContent.Configure`; the authoring rules
a ROM or Rulebook author follows.

**Out:** which ROMs split apart and what content proves it (Game Designer's call, per
[`../Roadmap/phase-2-7.md`](../Roadmap/phase-2-7.md)); capability arbitration between competing
providers (2.6.5 defers it; v1 errors); turn-ordered event dispatch (`IEngineEventSubscriber`, still
deferred); ROM versioning.

---

## 1. The problem, stated precisely

NRules' `Match<T>` needs the CLR type `T` at compile time, in the matching assembly. A rule in ROM B
that wants to correlate against `PlayerPosition` — a fact ROM A owns — therefore needs
`PlayerPosition` to be a type ROM B can name. Peer ROMs may not reference each other's assemblies.
Those two requirements are in direct conflict, and that conflict is the whole of the problem
[ticket 0023](../ClaudeWiki/Processes/Handoffs/0023-cross-rom-fact-mediation.md) raised.

**The resolution: the matched type is not the owner's concrete type.** It is a *contract type* living
in an assembly both peers already reference — the interop assembly — which the owner's concrete fact
implements.

**One consequence, stated up front because it makes everything else smaller:** cross-ROM fact
correlation needs **no runtime broker call at all**. Once both peers share the contract type, ordinary
NRules matching does the work. The broker's job is load-time — declare, validate, resolve *who
provides what* — not to be a runtime service locator that rule code reaches into.

That property is load-bearing, not incidental. **Generated NRules rule code has no constructor or DI
seam** — the reason `D20SystemContent.Configure` existed as a static holder
([ticket 0022](../ClaudeWiki/Processes/Handoffs/0022-implement-phase-2-6.md)). Any design requiring
rule code to *call* the broker reintroduces a static global immediately. This one doesn't. Preserve
that.

## 2. The NRules assumption — resolved

§3 rests on one assumption about a third-party library: that NRules resolves facts polymorphically
**through interfaces**, not only through base classes. Given `class PlayerPosition : IPlayerLocation`,
a consuming ROM must be able to reach that fact while naming only the interface.

**It can — through both of the two mechanisms this design uses**, which are separate code paths and
were verified separately. Both read at the **v1.0.2 tag** — the version pinned in
`LunaFlow.Engine.csproj` and `LunaFlow.Rules.csproj` — rather than from the prose docs, which never
use the word "interface"
([ticket 0033](../ClaudeWiki/Processes/Handoffs/0033-spike-does-nrules-match-facts-through-interfaces.md)).

**Path 1 — `Match<T>` in a rule** (the alpha network):

- `TypeNode.IsSatisfiedBy` is `FilterType.IsAssignableFrom(fact.FactType)` — and
  `Type.IsAssignableFrom` is true for interface implementation.
- `RootNode.IsSatisfiedBy` returns `true` unconditionally.
- `AlphaNode.PropagateAssert` offers every fact to **every** child node, each filtering
  independently. There is no exact-type dispatch table anywhere in the path.

**Path 2 — `Query<T>` in a handler** (working memory, reached via `RuleEngine.Query<T>()` →
`ISession.Query<T>()`):

```csharp
// NRules Session.cs
public IQueryable<TFact> Query<TFact>()
    => _workingMemory.Facts.Select(x => x.Object).OfType<TFact>().AsQueryable();
```

`OfType<T>` filters on `is`-semantics, which likewise holds for interfaces. Verified separately
because it shares no code with Path 1 — a design that proved only the rule path and assumed the
handler path would follow would have been resting on an unchecked inference, which is the exact
failure 0033 was opened over.

**A third property falls out of Path 1's code, and §3 depends on it too:** because each type node
filters independently, **one inserted fact satisfies every type node it is assignable to**. The
owning ROM can match its own concrete `PlayerPosition` while a peer matches `IPlayerLocation`, off a
single insert. Neither ROM constrains the other's matching.

**Accepted risk, disclosed:** no test pins any of this. It is third-party semantics the entire
cross-ROM boundary rests on, and a future NRules upgrade that narrowed polymorphic matching would
surface as "a rule mysteriously stopped firing," a long way from the cause. Accepted knowingly rather
than overlooked. *Revisit on any NRules version bump — that's the trigger.*

## 3. The authoring contract

*This is the part a ROM or Rulebook author needs. It is stable under either outcome of §2 in
everything but the declaration syntax of the contract type itself.*

### 3.1 A fact that crosses a ROM boundary is declared as a capability

The owning ROM advertises it in `Provides`; the consuming ROM declares it in `Requires`. The
`Capability`'s `ContractType` **is** the type the consumer matches on — the job that slot has been
waiting for since 2.6.5, where it was required at construction and read by nothing.

```csharp
// In the interop assembly — referenced by both peers, one-directionally.
public interface IPlayerLocation
{
    int RoomId { get; }
}
```

```csharp
// Owning ROM: its own concrete fact, its own extra state, implementing the contract.
public sealed class PlayerPosition : IPlayerLocation
{
    public int RoomId { get; }
    public int MovesTaken { get; }   // not on the contract; nobody else's business
}

public IReadOnlyList<Capability> Provides { get; } =
    new[] { new Capability("PlayerLocation", typeof(IPlayerLocation)) };
```

```csharp
// Consuming ROM: matches the contract, never the concrete type.
public IReadOnlyList<Capability> Requires { get; } =
    new[] { new Capability("PlayerLocation", typeof(IPlayerLocation)) };

// ...in a rule:
When().Match<IPlayerLocation>(() => loc, loc => loc.RoomId == 3);
```

**Keep contract types minimal.** Put on the interface only what actually crosses — `MovesTaken` above
stays off it deliberately, the same instinct that held `IEngineHost` to two members. A contract is a
promise to a peer, and every member is something you cannot change later without breaking them.

### 3.2 Rules for authors

1. **Peers never reference each other's assemblies.** Both reference the interop assembly. That
   one-directional edge is expected and governed; the peer-to-peer edge is the violation.
2. **Only the owner inserts, updates, or retracts its own facts.** A consumer observes. Cross-ROM
   *mutation* is not supported in v1, deliberately — see §5.
3. **Contract types live in the interop assembly, never in `LunaFlow.Rules`.** 2.6.5 closed that
   door: `LunaFlow.Rules` is NRules infrastructure, not a content home, however abstract the content
   gets.
4. **A consuming rule matches the contract type.** Matching a peer's concrete type is impossible by
   construction — if you can name it, the reference boundary has already been violated.
5. **`Capability.Name` is the match key and is unique registry-wide.** Two providers advertising one
   name is a hard load-time error, not an arbitration.

### 3.3 The two read paths — rules and handlers

A consumer reads peer facts from either side of the engine, naming only the contract type:

| From | Call | Mechanism |
| --- | --- | --- |
| A rule's `When()` | `Match<IPlayerLocation>(() => loc, loc => loc.RoomId == 3)` | Alpha network (§2, Path 1) |
| A command handler | `LunaFlowEngine.Host.Rules.Query<IPlayerLocation>()` | Working memory (§2, Path 2) |

The handler path is the existing pattern, not a new one — it's what Nine Rooms' `MoveCommandHandler`
already does with `Query<PlayerPosition>()`; the only change is that a *peer's* fact is reached by
its contract type rather than its concrete one.

### 3.4 What a consumer may not do from rule code

Rule code observes peer facts. It does not call peer behaviour. If a consuming ROM needs to *invoke*
something a peer owns — a calculation, a service, a movement operation — that happens in a **command
handler**, which has constructor injection. Not in a rule. See §4.

## 4. Behaviour across the boundary, and the `Configure` replacement

### 4.1 There is no broker — `Resolve` is cut

**Revised 2026-07-27.** An earlier draft of this spec gave `IInteropRom` a resolution surface —
`Resolve(Capability)` and `Resolve<T>(string)` — so a consuming ROM could obtain a peer's service
implementation at runtime. **That is cut, on two independent grounds**, and
[`../design/cartridge.md`](../design/cartridge.md) carries the full reasoning:

1. **It is a pull API across a trust boundary.** The tier stack is a trust gradient ending in
   unreviewed script; a deep layer that reaches upward to obtain something inherits the trust level
   of wherever it reached. And `ComponentRegistry.Validate()` **cannot** catch it — the validator
   inspects *declared* dependencies, while a runtime lookup is undeclared by construction and never
   appears in that graph. Enforcement has to be by construction: don't provide the mechanism.
2. **It is redundant.** A single composition root already knows every ROM and every implementation.
   It can hand the implementation over directly, which is explicit where `Resolve` is implicit — and
   consistent with decisions already on record: Phase 2.6.5 chose hardcoded registration over
   discovery, and [ticket 0019](../ClaudeWiki/Processes/Handoffs/0019-d20-rng-test-seam.md) chose
   constructor injection specifically to avoid a global mutable holder.

`IInteropRom` therefore gains nothing, and the assembly it named is **not a ROM at all** — it's the
Cartridge's contract surface. See §4.4.

### 4.2 Behaviour crosses by constructor injection, from the composition root

A consuming ROM names only the contract interface; the composition root supplies the implementation.

```csharp
// Program.cs
var registry = new ComponentRegistry();
registry.Register(gridRom,  EngineTier.Rom,      "GridMap");
registry.Register(d20Rom,   EngineTier.Rom,      "D20System");
registry.Register(ruleBook, EngineTier.Rulebook, "OneD20");
registry.Validate();                       // fail fast, before anything is constructed

// GridMap implements IPlayerMovement; D20System's handler names only the interface.
dispatcher.Register(new FleeCommandHandler(mechanic, gridMovementService));
```

`FleeCommandHandler`'s parameter is typed `IPlayerMovement` — declared in the contract surface, which
D20System already references. It never names `GridMap`, and it never asks anyone for anything.

**`Capability` remains meaningful and is not cut**: `Provides`/`Requires` plus
`ComponentRegistry.Validate()` still fail the load fast when a required contract has no provider, a
duplicate provider exists, or a dependency runs the wrong way. What changed is that a capability is
a **declaration validated at load time**, not a handle resolved at runtime.

### 4.3 `D20SystemContent.Configure` is removed

2.6.5 deprecated it and named `IInteropRom` its successor without saying how. This is how: Scenario
data reaches ROM content by **constructor injection into handlers**, and cross-ROM facts reach rules
through **§3's shared contract**. Neither path needs a static mutable holder.

`Configure` existed because rule code can't take constructor arguments — but rule code never needed
the Scenario data; the handlers did. Delete `Configure` and its backing static state, and migrate its
call sites in `Program.cs` to constructor arguments.

### 4.4 Where the contract surface lives — it is not a ROM

The assembly holding the contract types is the **Cartridge's contract surface**
([`../design/cartridge.md`](../design/cartridge.md)), not a third peer ROM. It does not implement
`IRom`, has no identity metadata, declares no capabilities, and is never registered with
`ComponentRegistry` — it contains interfaces and nothing else.

Modelling it as a ROM breaks the replaceability requirement outright: contracts living inside a ROM
means updating that ROM rebuilds the contracts, which forces a recompile of every consumer above —
exactly the coupling the structure exists to prevent.

**Consumers reference the contract surface only.** That includes the Rulebook: a Rulebook may not
reference a ROM implementation assembly, and must express its own content in Rulebook-owned types
that the composition root translates at construction.

## 5. Bootstrap contract probe — making "hard fail at bootstrap" true

The component model claims a mismatch is a hard failure at bootstrap. **Today that claim is false.**
Compile-time type checking proves a module was once consistent with *some* build of a contract
surface; nothing forces the runtime to check the installed one until it happens to need it — and the
CLR resolves lazily, so `TypeLoadException` or `MissingMethodException` can surface at first use,
potentially inside a rule firing several turns into a session, pointing at NRules rather than at the
module that is wrong.

The fix is not to check compatibility ourselves. It's to **force the runtime to perform the check it
was always going to perform, at a moment we control.** No recorded state, nothing to keep in sync,
and the CLR's binding rules are the oracle. See
[`../design/cartridge.md`](../design/cartridge.md#compile-time-consistency-is-not-deployment-compatibility).

This is also where `Capability.ContractType` finally earns its slot — it has been required at
construction and read by nothing since Phase 2.6.5.

### 5.1 Bootstrap failure barrier

`Program.cs` wraps **module construction, registration, validation, and probing** in a single handler.
Construction must be inside it: a module's `Provides`/`Requires` are built with `typeof(IContract)`
expressions, so type resolution happens *there*, before the registry ever sees it.

Catch the binding family — `TypeLoadException`, `MissingMethodException`, `MissingFieldException`,
`FileLoadException`, `FileNotFoundException`, `BadImageFormatException` — report which module and
which contract was being processed, and **exit non-zero**.

**It must not degrade.** Catching, logging and continuing is worse than no probe at all: it converts
a loud failure into a quiet one, which is the exact outcome this exists to prevent.

### 5.2 Assignability — proven at compile time, not probed at bootstrap

**Revised 2026-07-29, [ticket 0048](../ClaudeWiki/Processes/Handoffs/0048-capability-provider-model-vs-bootstrap-probe-spec.md).**
The original draft of this section probed assignability at bootstrap, against "the registered
provider" — which assumed the object registered with `ComponentRegistry` (the ROM itself) is what
implements a `Provides` entry's contract. It never is: a fact-shaped capability is implemented by a
fact *type* (`PlayerPosition : IPlayerLocation`) that's never registered, and no single instance of
it need exist at bootstrap. A literal reading of the original probe would fail every real capability
in a correctly-working ROM pair, not just a broken one.

**The fix: make the pairing a compile-time fact instead of a runtime lookup.** `Capability` gains a
generic factory used on the `Provides` side only:

```csharp
public static Capability Provided<TContract, TProvider>(string name) where TProvider : TContract
    => new Capability(name, typeof(TContract)) { ProviderType = typeof(TProvider) };
```

A ROM declares `Capability.Provided<IPlayerLocation, PlayerPosition>("PlayerLocation")`; if
`PlayerPosition` doesn't implement `IPlayerLocation`, the generic constraint fails to compile. This
is strictly stronger than a bootstrap probe — it can't ship a mismatched pairing at all, in a
deliberately-broken build or otherwise — so there is no runtime assignability check left to run.
`Requires`-side capabilities keep the plain two-argument constructor unchanged; a requirement doesn't
own an implementation, so there's nothing to pair.

### 5.3 Member-resolution probe

Type loading does **not** resolve members, and §5.2's compile-time constraint only proves this ROM's
*own build* was consistent with *some* version of the contract — exactly the gap
[`cartridge.md`](../design/cartridge.md#compile-time-consistency-is-not-deployment-compatibility)
names: the installed contract assembly may have been rebuilt with a changed member signature since.
A contract type that still exists but whose members changed signature raises `MissingMethodException`
when a *call site* is JIT-compiled, so an edited interface is a likelier real-world event than a
deleted one. This is still real runtime work; §5.2's fix doesn't remove it.

Force it, without invoking anything — now a method on `Capability` itself, since it already holds
both types:

```csharp
public void ForceMemberResolution()
{
    if (ProviderType is null) return;   // Requires-side capability; nothing to force.
    var map = ProviderType.GetInterfaceMap(ContractType);
    foreach (var target in map.TargetMethods)
        RuntimeHelpers.PrepareMethod(target.MethodHandle);
}
```

Called once per `Provides` entry on every registered component, from inside §5.1's barrier.
`GetInterfaceMap` yields exactly the concrete methods implementing that contract — a bounded set, no
guessing — and `PrepareMethod` JIT-compiles each without calling it.

**Known limit, stated so nobody over-trusts this.** This covers **provider-side** skew. A *consumer*
whose compiled call sites reference a member the installed contract no longer has is still only
caught when that call site is JIT'd. Bounding the consumer-side set requires machine-enumerable
dependencies — the typed-slot direction in `cartridge.md` — and is not attempted here.

**Disclosed gap in the enforcement, not closed here:** nothing stops a future `Provides` entry from
using the plain two-argument constructor instead of `Provided<,>()`, silently skipping both the
compile-time constraint and `ForceMemberResolution()`. Accepted for now, same category as
`DependsOnRoms` being declared-but-unvalidated until this phase — convention-enforced, not
type-enforced. A `RequiredCapability`/`ProvidedCapability` type split would close it structurally;
not built on one observed near-miss. Revisit if a bypass actually ships.

### 5.4 Contract-surface shape check — interfaces only

**Added 2026-08-03, ticket [0071](../ClaudeWiki/Processes/Handoffs/0071-peer-rom-bidirectional-capability-cycle-detection-gap.md)'s
addendum.** §§5.1–5.3 prove that a *declared* capability's provider and contract bind correctly at
bootstrap. They say nothing about a type that was never declared as a capability at all — a plain
concrete class or record placed directly in the contract-surface assembly because two ROMs both
happen to need it, bypassing `Capability`/`Provides`/`Requires` entirely. That's not a binding-mismatch
problem §§5.1–5.3 catch; it's a shape problem — [`cartridge.md`](../design/cartridge.md#structure)
already states the rule ("interfaces for facts that cross a boundary, interfaces for services the
Cartridge offers") but nothing enforced it, and `EquippedWeapon` shipped as exactly this violation
before 0071 caught it in review. This closes that gap the same way §5.1 closes the compatibility gap:
force the check the convention was always implicitly claiming, at a moment bootstrap controls, instead
of trusting every future author to remember it.

**Check:** every publicly-exported type in the Cartridge's contract-surface assembly must be an
interface.

```csharp
public static void ValidateInterfacesOnly(Assembly contractSurface)
{
    var offenders = contractSurface.GetExportedTypes()
        .Where(t => !t.IsInterface)
        .ToList();

    if (offenders.Count > 0)
    {
        throw new InvalidOperationException(
            $"Contract surface '{contractSurface.GetName().Name}' exposes non-interface type(s): " +
            $"{string.Join(", ", offenders.Select(t => t.FullName))}. The contract surface may hold " +
            "interfaces only — see cartridge.md's Structure section. A shared concrete type belongs " +
            "behind an interface here, implemented by the owning ROM's concrete type, never named " +
            "directly outside the Cartridge.");
    }
}
```

**Where it runs:** inside §5.1's bootstrap barrier, alongside the per-`Capability` probes — same
non-zero-exit, fail-loud contract. `Program.cs` passes the contract-surface assembly explicitly
(e.g. `typeof(IPlayerLocation).Assembly`), the same push-not-discover pattern registration already
uses; there is no reflection-based hunt for "which assembly is a Cartridge's contract surface."

**Scope, deliberately narrow:**

- **Interfaces only, no allowlist for enums/structs/etc.** The five contracts shipped so far need
  nothing but interfaces and plain member types (`int`, `string`, `bool`) already satisfied by the
  interface signature itself — nothing today needs an auxiliary concrete type published in the
  surface. If a real case needs one, that's a deliberate, reviewed loosening of this check with its
  own ticket — not a default the check should anticipate.
- **One assembly, one pass, at bootstrap** — same discipline §5.5 states for the rest of §5. Not a
  general "audit any assembly" utility.
- **Toolchain-generated public types are a known edge the implementer should verify against the actual
  built assembly** (e.g. embedded compiler attributes on older target frameworks) rather than guessed
  at here; if the real `LunaFlow.Cartridge.dll` trips this on something that isn't author-written, that
  is worth a ticket back to reconcile the check, not worth pre-defending against in this spec.

**Relationship to existing acceptance criterion 5** (§7): criterion 5 already states the invariant
this check exists to verify — "contains interfaces and nothing else" — but qualifies it as "verified
by inspecting," i.e. manually. This turns that manual bar into an automated, bootstrap-enforced one;
see acceptance criterion 12 below.

### 5.5 What this is and isn't

A **binding smoke test**, not a compatibility proof. It cannot catch a semantic change: identical
signatures with different meaning (`RoomId` silently becoming 1-based) pass, and would also pass a
fingerprint check, so this is no worse than the deferred alternative. It covers what it forces and
nothing more. §5.4's shape check is a separate, narrower pass over a different question (what types
exist in the surface, not whether a declared pairing binds) — stated here so the two aren't conflated.

Scope discipline: declared contract types only (§§5.1–5.3) plus one shape pass over the whole surface
(§5.4), each running once, at one point. Not a general health-check
framework.

## 6. Deliberately excluded

- **Cross-ROM fact mutation.** A consumer observes peer facts; it does not insert, update, or retract
  them. Permitting mutation makes every ROM's invariants everyone else's problem, and nothing in
  0023's case needs it. *Revisit only against a concrete case that cannot be expressed as the owner
  reacting to a fact of its own.*
- **Any upward lookup API — permanently, not pending.** No `Resolve`, no service locator, no registry
  a component can query. This is a security boundary, not a sequencing decision: see §4.1 and
  [`../design/non-goals.md`](../design/non-goals.md). A concrete need for one is evidence of a
  modelling error, not of a missing feature.
- **Capability arbitration.** v1 hard-errors on two providers (2.6.5).
- **Turn-ordered event dispatch** (`IEngineEventSubscriber`, `OnTurnAdvance` ordering). Still
  deferred; nothing here needs it.
- **`DependsOnRoms` validation** — declared on `IRuleBook` but unenforced, so a Rulebook naming an
  unregistered ROM currently fails silently. A real gap, tracked in
  [`../Roadmap/phase-2-7.md`](../Roadmap/phase-2-7.md)'s Deliverables; adjacent to this spec, not part
  of it.

## 7. Acceptance criteria

1. Two peer ROMs correlate a fact through a contract type with **zero project reference between them**
   — verified against the assembly graph, not merely by a passing test.
2. The consuming ROM's rule fires on a fact inserted by the owning ROM, matched via the contract type
   — and a consuming **handler** reaches the same fact via `Query<TContract>()`. Both paths, not one;
   they share no code (§2).
3. **No lookup API exists anywhere in the shipped surface** — no `Resolve`, no service locator, no
   queryable registry. Verified by inspecting the contract surface and `IInteropRom`, not by testing
   that nobody calls one. Absence is the criterion.
4. A consuming ROM obtains a peer's service implementation **only** as a constructor argument typed
   to a contract interface, supplied by `Program.cs`.
5. **The contract surface assembly implements no `IRom`** and is not registered with
   `ComponentRegistry`. It contains interfaces and nothing else.
6. **The Rulebook references no ROM implementation assembly** — assembly-graph check, same as (1).
   `LunaFlow.Rulebook` must not reference `LunaFlow.Rom` or any ROM project, and no Rulebook source
   file may `using` a ROM implementation namespace.
7. `D20SystemContent.Configure` and its backing static state are gone; `Program.cs` passes the same
   data by constructor argument.
8. Removing either ROM from the registration list produces a clear load-time error, not a partial
   game.
9. **A provider/contract mismatch fails at compile time, not bootstrap.** Demonstrated by attempting
   to declare a `Provides` entry pairing a concrete type with a contract it doesn't implement (e.g.
   `Capability.Provided<IChallengeSolved, PlayerPosition>(...)`) and observing a build error, not a
   runtime exception. Revised 2026-07-29 per
   [ticket 0048](../ClaudeWiki/Processes/Handoffs/0048-capability-provider-model-vs-bootstrap-probe-spec.md) —
   §5.2 moved this check to compile time; the bootstrap probe no longer performs it.
10. **The probe forces member resolution, not merely type load** — verified by `GetInterfaceMap` +
    `PrepareMethod` being present over each declared `Provides` entry's `ProviderType`
    (`Capability.ForceMemberResolution()`), since a cast alone cannot catch signature skew (§5.3).
11. **The §5.1 barrier still fails loudly and non-zero** on a genuine binding-exception-family error
    (a stale build, a contract assembly rebuilt with a breaking change) — a caught exception that logs
    and continues fails this criterion, same bar the original criterion 9 held for the probe as a
    whole.
12. **`ValidateInterfacesOnly` (§5.4) runs at bootstrap against the Cartridge's contract-surface
    assembly and fails loudly, non-zero, before any component is constructed, if a non-interface type
    is exported from it.** Demonstrated by temporarily adding a public concrete type to
    `LunaFlow.Cartridge` and observing bootstrap refuse to start with a message naming the offending
    type — not a passing test alone, same assembly-graph-first bar as criteria 1 and 6. Upgrades
    criterion 5's "verified by inspecting" to an automated, enforced check. Added 2026-08-03, ticket
    [0071](../ClaudeWiki/Processes/Handoffs/0071-peer-rom-bidirectional-capability-cycle-detection-gap.md).
13. Full suite green, read from an Arryn-run `latest.trx`.

## 8. Open

- **Which Cartridge the Phase 2.7 split produces, and what its contract surface project is called.**
  Content-shaped, and not blocking: §3's authoring contract and §4's injection pattern are identical
  regardless. What *is* settled is that the surface is not a ROM (§4.4).

## Related pages

- [`../design/cartridge.md`](../design/cartridge.md) — the published unit this spec's contracts form
  the surface of, and the trust rationale behind §4.1
- [`../design/engine-core.md`](../design/engine-core.md)
- [`../design/non-goals.md`](../design/non-goals.md)
- [`../Roadmap/phase-2-7.md`](../Roadmap/phase-2-7.md)
- [Specs README](README.md)
