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

## 4. The broker, and the `Configure` replacement

### 4.1 `IInteropRom` gains resolution

```csharp
public interface IInteropRom : IRom
{
    /// <summary>Resolves a declared capability to the instance providing it.</summary>
    /// <exception cref="InvalidOperationException">No registered provider for that capability.</exception>
    object Resolve(Capability capability);

    /// <summary>Typed convenience over <see cref="Resolve"/>.</summary>
    T Resolve<T>(string capabilityName) where T : class;
}
```

Resolution is **load-time-validated, runtime-cheap**: `ComponentRegistry.Validate()` has already
proven every `Requires` has exactly one provider before any `Resolve` call happens, so a throw from
`Resolve` means a registration bug, not a runtime condition to handle. It is not a general service
locator and must not grow into one — it resolves *declared* capabilities only.

### 4.2 Reaching it — handlers only, by constructor injection

The composition root resolves capabilities and constructs handlers with what they need. This is the
existing pattern rather than a new mechanism — the same shape as
[ticket 0019](../ClaudeWiki/Processes/Handoffs/0019-d20-rng-test-seam.md)'s `Random` seam.

```csharp
// Program.cs
var registry = new ComponentRegistry();
registry.Register(gridRom,  EngineTier.Rom,      "GridRom");
registry.Register(d20Rom,   EngineTier.Rom,      "D20System");
registry.Register(ruleBook, EngineTier.Rulebook, "OneD20");
registry.Validate();                       // fail fast, before anything is constructed

var locations = interop.Resolve<ILocationQuery>("LocationQuery");
dispatcher.Register(new FightCommandHandler(mechanic, locations, ...));
```

### 4.3 `D20SystemContent.Configure` is removed

2.6.5 deprecated it and named `IInteropRom` its successor without saying how. This is how: Scenario
data reaches ROM content by **constructor injection into handlers**, and cross-ROM facts reach rules
through **§3's shared contract**. Neither path needs a static mutable holder.

`Configure` existed because rule code can't take constructor arguments — but rule code never needed
the Scenario data; the handlers did. Delete `Configure` and its backing static state, and migrate its
call sites in `Program.cs` to constructor arguments.

## 5. Deliberately excluded

- **Cross-ROM fact mutation.** A consumer observes peer facts; it does not insert, update, or retract
  them. Permitting mutation makes every ROM's invariants everyone else's problem, and nothing in
  0023's case needs it. *Revisit only against a concrete case that cannot be expressed as the owner
  reacting to a fact of its own.*
- **Broker access from rule code.** §1's whole point; reintroducing it reintroduces a static global.
- **Capability arbitration.** v1 hard-errors on two providers (2.6.5).
- **Turn-ordered event dispatch** (`IEngineEventSubscriber`, `OnTurnAdvance` ordering). Still
  deferred; nothing here needs it.
- **`DependsOnRoms` validation** — declared on `IRuleBook` but unenforced, so a Rulebook naming an
  unregistered ROM currently fails silently. A real gap, tracked in
  [`../Roadmap/phase-2-7.md`](../Roadmap/phase-2-7.md)'s Deliverables; adjacent to this spec, not part
  of it.

## 6. Acceptance criteria

1. Two peer ROMs correlate a fact through a contract type with **zero project reference between them**
   — verified against the assembly graph, not merely by a passing test.
3. The consuming ROM's rule fires on a fact inserted by the owning ROM, matched via the contract type
   — and a consuming **handler** reaches the same fact via `Query<TContract>()`. Both paths, not one;
   they share no code (§2).
4. `IInteropRom.Resolve` returns the registered provider; resolving an undeclared capability throws.
5. **No rule code calls the broker**, verified by inspection of the generated sources.
6. `D20SystemContent.Configure` and its backing static state are gone; `Program.cs` passes the same
   data by constructor argument.
7. Removing either ROM from the registration list produces a clear load-time error, not a partial
   game.
8. Full suite green, read from an Arryn-run `latest.trx`.

## 7. Open

- **Where the interop assembly lives** — a third ROM-tier project, or an existing ROM designated as
  broker. Deliberately unresolved: it depends on whether the split produces a natural broker or needs
  a neutral one, which is content-shaped and not yet known. Does not block §3's authoring contract,
  which is identical either way.

## Related pages

- [`../design/engine-core.md`](../design/engine-core.md)
- [`../Roadmap/phase-2-7.md`](../Roadmap/phase-2-7.md)
- [Specs README](README.md)
