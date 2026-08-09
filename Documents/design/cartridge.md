# Cartridge — the published unit

Implementation status: see [`../Roadmap/roadmap.md`](../Roadmap/roadmap.md).

A **Cartridge** is the organizational unit LunaFlow publishes and consumes. A [ROM](architecture.md)
is an interchangeable part; a Cartridge is the thing that ships — an exposed contract surface plus
the implementation behind it.

The name is literal about the metaphor already in "ROM." A cartridge is a memory chip *plus the
pinout that makes it usable*. The console binds to the pinout, never to the chip. Swap the chip,
keep the pinout, and the console doesn't care — which is exactly the property this document exists
to protect.

## Why this exists

Two requirements produce it, and the first is the load-bearing one.

### 1. The tier stack is a trust boundary, not a filing scheme

[`architecture.md`](architecture.md)'s role table describes a **trust gradient**: the engine core is
the most trusted layer, and each tier above it is authored by a less-trusted contributor with a
broader authoring surface — down to World Modules, GM Overlays, and eventually Player rules, which
are *script*, authored at runtime, unreviewed, and (per
[`scripting-content.md`](scripting-content.md)) sandboxed precisely because they cannot be trusted.

The direction a dependency runs decides who is exposed to whom:

- **Push (downward injection) is safe.** A lower layer is *handed* data and validates it at its own
  boundary before use. Control stays with the layer that has the most to lose. A ROM given a
  dictionary of strings can check it, reject it, or ignore it.
- **Pull (reaching upward) is not.** A deep layer that initiates a reach into shallower territory
  consumes whatever comes back — and inherits the trust level of wherever it reached. A ROM that
  can fetch something a World Module or a GM Overlay registered has, in that moment, taken a
  dependency on script it cannot audit, at a depth where a failure or an exploit has the widest
  blast radius.

That asymmetry is the rule: **data flows down, control stays down. No component obtains anything by
reaching up.**

**This cannot be enforced by validation, and that point is easy to miss.**
[`engine-core.md`](engine-core.md)'s `ComponentRegistry` validates *declared* dependencies — it
compares `Requires` against `Provides` and rejects a declared dependency on a higher tier. A runtime
lookup API (`Resolve("something")`, a service locator, a registry a component can query) is
**undeclared by construction**: it never appears in the graph the validator inspects, so the
validator is structurally blind to it. A single such API silently reopens the boundary that every
declaration-time check exists to hold.

So the enforcement is **by construction, not by check**: the mechanism is not provided. There is no
upward lookup API to call. This is strictly stronger than validating, and it is the reason
`IInteropRom.Resolve` — proposed and specced during Phase 2.7 design — was cut rather than
constrained. A composition root that injects the same object explicitly achieves the identical
result with no pull surface at all. (`IInteropRom` itself — not just `Resolve` — was later retired
outright, 2026-08-09: zero implementers ever shipped once the contract surface took over peer
correlation. See `glossary.md`.)

### 2. A ROM must be replaceable without recompiling what sits above it

A Rulebook must not bind to a specific ROM assembly. If it does, updating that ROM forces a rebuild
of every Rulebook above it, and "pluggable ROM" — a claim `architecture.md` has made since before
any of it was real — is false in practice however true it is on paper.

Binding to a **stable contract surface** rather than to implementation types is what makes the claim
real. Note this is the same structure the trust requirement produces, arrived at from a completely
different direction. That convergence is the main evidence the shape is right.

## Structure

A Cartridge has exactly two parts:

- **Contract surface** — the types anyone may name. Interfaces for facts that cross a boundary,
  interfaces for services the Cartridge offers. Its own assembly.
- **Implementation** — one or more ROMs, in one or more assemblies. No type here may be named from
  outside the Cartridge.

**One rule, and everything else follows from it:**

> Nothing outside a Cartridge may name an implementation type. Consumers reference the contract
> surface only.

**Corollary, stated explicitly after it was nearly missed once (ticket
[0071](../ClaudeWiki/Processes/Handoffs/0071-peer-rom-bidirectional-capability-cycle-detection-gap.md)):
a shared data type does not earn a place in the contract surface just by being shared.** The contract
surface holds *interfaces* — the pinout, not the chip. A bare concrete record placed directly in
`LunaFlow.Cartridge` because two ROMs both need to see it is an implementation type wearing the
contract surface's address; it skips the one thing that makes the surface governed (an interface
every implementer commits to, reviewable and stable independent of any one ROM's internals) while
looking, from the assembly graph, exactly like it belongs there. Anything a peer needs to
observe — a fact, a piece of state, an action — gets an interface here, same as the five contracts
in the worked example below; the owning ROM's concrete type implements it and is never named outside
the Cartridge. This is the mechanism, not a style preference: it's what keeps "which types can a mod
author reach past review" a question the project structure answers, rather than one that depends on
every future contributor re-deriving the discipline from first principles under deadline pressure.

**Enforced, not just stated.** A convention nobody checks is a convention the next deadline erodes —
which is exactly how `EquippedWeapon` shipped. [`cross-rom-capability-spec.md`
§5.4](../Specs/cross-rom-capability-spec.md#54-contract-surface-shape-check--interfaces-only) specifies
a bootstrap check that enumerates every publicly-exported type in a Cartridge's contract-surface
assembly and refuses to start if any of them isn't an interface — the same "hard fail at bootstrap,
never a silent degradation" bar §5's compatibility probe already holds, applied to this invariant
instead.

**Project-per-ROM is a convention, not a requirement — clarified 2026-08-08, ticket
[0077](../ClaudeWiki/Processes/Handoffs/0077-do-peer-roms-still-need-one-project-each-now-that-cross-assembly-correlation-is-proven.md);
**default corrected same day, direct from Arryn, before the ticket closed.** "One or more assemblies"
above was never softened language for a rule that actually meant one — every peer ROM shipped so far
(GridMap, D20System, Loot) happening to get its own project under `example/` is [ticket
0038](../ClaudeWiki/Processes/Handoffs/0038-phase-2-7-test-suite-blocked-on-rom-implementations-in-src.md)'s
practical byproduct (it settled *location* — `example/`, not `src/` — not project count), reinforced
since only because a dedicated project makes "zero project reference between peers" trivially
checkable against the assembly graph.

**The invariant that actually matters is narrower than "separate assemblies": no peer ROM names
another peer ROM's concrete implementation type.** Correlation happens only through the Cartridge's
contract surface (§3 above, and `cross-rom-capability-spec.md` §3.2 rule 1). A dedicated project per
ROM is one *enforcement mechanism* for that invariant — there's no `ProjectReference` to misuse
because there's nothing to reference — not the invariant itself. This is also why consolidating peer
ROMs doesn't reopen the trust-gradient problem [§1](#1-the-tier-stack-is-a-trust-boundary-not-a-filing-scheme)
above exists to guard: that asymmetry is about a *deeper* tier pulling from a *shallower* one. Two
peer ROMs sit at the same tier: nothing about GridMap and D20System sharing a compilation unit hands
either one authority the other doesn't already have.

**Ruling, corrected: a Cartridge's implementation defaults to one shared assembly, with one named
exception.** The first pass of this ruling weighed the compiler-enforced isolation as free and
stopped there; it isn't — a project per ROM is a real, compounding cost against the thing an author
actually looks at all day, and "30-odd ROM/Rulebook assemblies" cluttering one solution is a plausible
future, not a hypothetical one. Namespaces are what separation is *for*; a project boundary was always
a stronger tool than the problem needed, adopted because Phase 2.7 got it for free alongside 0038's
location ruling, not because peer ROMs require it. **Corrected default: GridMap, Loot, and future
peers (Reputation, etc.) consolidate into one implementation assembly, separated by namespace**
(`LunaFlow.Rom.GridMap`, `LunaFlow.Rom.Loot`, etc.) — the same pattern `src/LunaFlow.Rom`'s own
namespace layout already uses one tier down.

**D20System (`example/D20SystemExampleA`) is the one named exception and stays its own separate
assembly — per Arryn, for regression testability.** It's the oldest, most load-bearing ROM (direct
descendant of the pre-split 1.d20 content, the one every other peer's `IChallengeSolved` correlation
is ultimately checked against), and keeping it independently buildable and testable outweighs the
consolidation argument for this one case. This is a standing exception, not a temporary migration
step — future rulings on "does ROM X join the shared assembly" should treat D20System's separateness
as the precedent for *why* something might stay out, not as leftover inertia to eventually clean up.
**The contract surface stays its own assembly regardless of any of this** — that separation is
load-bearing for a different reason (independent versioning, `non-goals.md`'s deferred fingerprinting)
and neither the default nor the exception touches it.

**What the consolidated side costs, stated plainly rather than assumed away:** moving peer isolation
from compiler-enforced to convention-enforced is a real, immediate gap the moment GridMap and Loot
(and future consolidated peers) share a project — not a someday risk, and not a gap D20System's
separateness closes for the ROMs that do consolidate. Per Arryn's direction, the substitute
enforcement — a build-time check inspecting a shared assembly for a ROM naming a sibling's concrete
type — and a companion assembly fingerprint for future security use are **both named and deliberately
deferred**, not built alongside this change; see `non-goals.md`. Consolidating without that check is a
conscious, accepted trade for solution cleanliness and lower per-ROM overhead, not an oversight —
worth restating if this default is ever cited without the caveat attached. D20System's boundary against
the consolidated assembly stays compiler-enforced throughout, same as today — that part of the picture
doesn't change.

The contract surface is used **internally as well as externally**. A Cartridge's own ROMs talk to
each other through it — that is not a second mechanism bolted on, it is the same surface serving
both roles. This is what makes peer ROMs inside one Cartridge able to correlate facts without
referencing each other's assemblies (see
[`../Specs/cross-rom-capability-spec.md`](../Specs/cross-rom-capability-spec.md)).

### The contract surface assembly is the manifest — with one limit

A .NET assembly already carries complete type metadata; the contract surface assembly *is* a
machine-readable declaration of the exposed API, and an authoritative one — it cannot drift from
what compiled, which a hand-maintained sidecar file inevitably does.

**The limit, stated because it is easy to over-read the above:** the assembly is authoritative about
what it **is**, not about what any given consumer **compiled against**. That second fact is a
property of the consumer and cannot live in the producer's assembly. See
[Compile-time consistency is not deployment compatibility](#compile-time-consistency-is-not-deployment-compatibility).

A separate manifest file earns its keep under two conditions, neither of which holds today: when
something must resolve a Cartridge **without loading code** (the config-driven loader), or when a
consumer must **record which contract version it was built against**. See
[`non-goals.md`](non-goals.md).

## Compile-time consistency is not deployment compatibility

Type checking happens in two places, and only one of them currently exists.

**At compile time**, a module is checked against a specific build of a Cartridge's contract surface.
This proves the module was once consistent with *some* version. The compiler is gone by deployment;
all that survives in the binary is a type name and an assembly identity.

**At load time**, nothing checks that the installed Cartridge is the one the module was built
against. Today that gap is invisible because every consumer is compiled in this repo, from this
solution — a contract change surfaces immediately as a compile error. **It stops being invisible the
moment anything we did not compile ourselves binds to a Cartridge.**

**The current failure mode is worse than absent, because it is late.** .NET does eventually catch
genuine incompatibility — `TypeLoadException`, `MissingMethodException` — but at *first use of the
type*, not at load. In this architecture that means it can surface inside a rule firing, several
turns into a session, with a stack trace pointing at NRules rather than at the module that is
actually wrong. That directly contradicts the property the component model otherwise holds:
**a mismatch is a hard failure at bootstrap, never a degradation during play.**

### The answer: force the check early rather than record what to check

The instinct is to record compatibility state — each consumer stores what it built against, and
bootstrap compares. That works, and it is more machinery than the problem needs, because **the
runtime already knows.** The CLR will perform exactly this check; it simply does so lazily, at first
use, which is what makes the failure late and confusing.

So don't reimplement the check. **Exercise each requirement at bootstrap and let the runtime's own
binding failure happen at a moment we control** — catch it, report which module and which contract,
exit non-zero. No recorded state, nothing to keep in sync, and the oracle is always current because
it *is* the loader. Specified as a Phase 2.7 deliverable in
[`../Specs/cross-rom-capability-spec.md`](../Specs/cross-rom-capability-spec.md) §5.

Two details decide whether it works:

- **Type load and member resolution are different events.** Casting or touching a type catches a
  renamed type, a missing type, or a wrong assembly identity. It does **not** catch a member whose
  signature changed — that raises `MissingMethodException` when a *call site* is JIT-compiled. Since
  editing an interface is likelier than deleting one, a probe that only casts would miss the common
  case. `RuntimeHelpers.PrepareMethod` over a provider's interface map forces member resolution
  without invoking anything.
- **It must fail loudly.** A probe that catches, logs and continues converts a loud failure into a
  quiet one, which is worse than having no probe.

This is a **binding smoke test, not a compatibility proof**: it covers what it forces, and it cannot
detect semantic change behind identical signatures.

### What the probe doesn't cover

**Offline checking.** A probe needs the process to reach bootstrap, so it can't validate a Cartridge
*without loading it* — the package-manager case, which belongs to the deferred loader. That's the one
job a recorded fingerprint would still do, and the only reason to build one.

**Behaviour.** An implementation with correct signatures can do the wrong thing. That's true of every
interface in every language, it's the implementer's problem, and no mechanism here changes it —
listed only so nobody mistakes a green probe for a correctness guarantee. Version skew that changes a
member's *meaning* without changing its shape is the same class of problem, and is already addressed
by the decision on record in [`non-goals.md`](non-goals.md) to use standard SemVer when versioning
arrives: signalling breaking change is what SemVer is for.

### What must stay true so this stays cheap

The deferral is only prudent while the fix remains inexpensive to add. These are the invariants that
keep it so — **treat a proposal that breaks one as a proposal to pay this cost early**, not as a
detail:

1. **The contract surface is its own assembly, never inside a ROM.** This is the load-bearing one.
   Contracts living inside an implementation cannot be versioned independently of it, and the fix
   above becomes unavailable rather than merely deferred.
2. **A module's set of required contracts is enumerable by machine**, not scattered through method
   bodies. It is what the probe iterates and what a fingerprint would cover; if it cannot be
   enumerated, it cannot be checked by either.
3. **Nothing outside a Cartridge names an implementation type** (the rule above). Otherwise
   compatibility checking would have to extend to implementation internals, which is unbounded.
4. **Contract surfaces stay small.** Every published member is a permanent compatibility liability.
   The cost of this deferral scales with surface area.

*Trigger for the remaining deferred piece (offline/fingerprint checking): the first consumer we do not
compile ourselves.* Not "when we get around to versioning" — the moment something outside this
repository binds to a Cartridge, this stops being theoretical. **The bootstrap probe is not deferred**
and lands in Phase 2.7: it is cheap, it has value today (a stale build artefact or a partially
rebuilt solution produces exactly this failure), and it is far easier to install while bootstrap is
simple than to retrofit later. Until it exists, the hard-fail-at-bootstrap property this document
relies on is aspirational rather than true.

## Worked example — Phase 2.7

The Grid/D20System split is one Cartridge, not three peers:

```
Cartridge: D20
├── Contract surface  (its own assembly)
│     IPlayerLocation, IChallengeSolved, ICombatStatus, IPlayerMovement, IGameWon
└── Implementation
      GridMap ROM      — spatial: position, movement, adjacency, win lines
      D20System ROM    — challenge and combat resolution
```

`IGameWon` is the fifth contract, added after this Cartridge's first build — an empty marker
`WinMarker` implements, letting D20System's challenge-prompt rules guard against "the game is
already won" without naming GridMap's fact directly. See
[`../Specs/grid-rom-d20-split-spec.md`](../Specs/grid-rom-d20-split-spec.md) and
[ticket 0045](../ClaudeWiki/Processes/Handoffs/0045-challenge-prompt-rule-missing-solved-failed-winmarker-guards.md)
for the gap that surfaced it and why a zero-member interface was the right size.

`GridMap` and `D20System` are peers *inside* one Cartridge, coordinating through its contract
surface. A Rulebook (`OneD20`) references the contract surface and nothing else — never `GridMap`,
never `D20System`.

This is why the scaffolded `D20SystemInterop` should **not** be a ROM, and not an `IInteropRom`:
it isn't a third peer participating in the game, it's the Cartridge's published face. Modelling it
as a ROM also defeats requirement 2 — contracts living inside a ROM means updating that ROM rebuilds
the contracts and forces the recompile above that the whole structure exists to avoid.

## Deliberately not built yet

Each of these has a named trigger. The concept earns its keep immediately just by naming what the
contract surface *is* and giving the boundary a rule; the machinery around it should wait for a
second Cartridge to generalize from, per [`non-goals.md`](non-goals.md)'s standing discipline.

- **A Cartridge identity type** (`ICartridge` with `Name`/`Credits`/`Provides`, the way `IRom` has
  one). Nothing consumes it — `ComponentRegistry` registers ROMs, and re-cutting registration to be
  per-Cartridge is a rework of Phase 2.6.5's model with no driver. Today a Cartridge is a
  **reference discipline** enforced by project structure and the rule above, not a runtime object.
  *Revisit when something needs to reason about Cartridges at runtime — realistically, the loader.*
- **A manifest file.** *Revisit when a loader must resolve a Cartridge without loading its code.*
- **Cartridge versioning.** *Revisit per `non-goals.md` — independent maintenance, or a second
  Rulebook pinning an older Cartridge. The answer is expected to be NuGet/SemVer via `nbgv`, not a
  bespoke scheme.*
- **A loader / true hot-swap.** Preserving the option costs nothing and is done by the rule above;
  building it is the config-driven packaging non-goal. `Program.cs` constructing ROMs by hand is
  still a compile-time binding no matter how clean the contracts are. *Revisit when a second
  Rulebook/Cartridge combination actually ships.*

## Related documents

- [`architecture.md`](architecture.md) — the tier stack and trust gradient this protects
- [`engine-core.md`](engine-core.md) — `ComponentRegistry`, `EngineTier`, and why declaration-time
  validation can't cover a pull API
- [`non-goals.md`](non-goals.md) — the deferrals above, with their triggers
- [`../Specs/cross-rom-capability-spec.md`](../Specs/cross-rom-capability-spec.md) — the contract
  mechanism a Cartridge's surface is built from
- [`scripting-content.md`](scripting-content.md) — the untrusted tiers requirement 1 protects against
