# 0039 — Cartridge: the published unit, and no upward lookup APIs

**Status:** resolved
**Opened by:** Amara (Architect)
**Opened:** 2026-07-27
**For:** Kofi (PM)

## Decision

**Two architecture decisions, recorded in [`design/cartridge.md`](../../../design/cartridge.md)
(new).** First: the tier stack is a **trust boundary**, not a layering scheme — data is pushed down
and vetted by the receiver, never pulled up, because a deep layer that reaches into shallower
territory inherits its trust level, and the shallow end is unreviewed script. This **cannot** be
delegated to `ComponentRegistry`, which only sees *declared* dependencies while a runtime lookup is
undeclared by construction; enforcement is by construction, so `IInteropRom.Resolve` is **cut**
rather than constrained. Second: a **Cartridge** — a contract surface plus the implementation behind
it — is the unit LunaFlow publishes, so a ROM is replaceable without recompiling above it and a
Rulebook never binds to a ROM assembly. Deliberately *not* built: any `ICartridge` runtime type,
manifest file, versioning, or loader — all deferred with named triggers. This ticket routes the
consequences, which land on three different roles.

## Details

### Context

Arryn design session, 2026-07-26/27, arising from a question Juno raised on
[CodePlan 0037](../../../Roadmap/CodePlans/2-7-supersceded.md) and a follow-on about where
flavor text should live. Both turned out to be symptoms of the same missing concept.

**The tell was repetition.** Four separate times, from four directions, someone tried to route
something through the capability mechanism that didn't belong there: the Rulebook's `Requires`
declaring contracts the ROMs exchange with each other (Juno's catch); my own conflation of
fact-contracts with behaviour-capabilities; the flavor-text question; and my own specced
`IInteropRom.Resolve`. Four people misreading one type the same way isn't four mistakes — it's the
model under-communicating.

**What was actually missing.** Two things, and they converge:

1. **Why tier ordering exists.** It has been documented as layering hygiene. It is a **trust
   boundary**: [`architecture.md`](../../../design/architecture.md)'s role table is a trust gradient
   ending in unreviewed script. Push (downward injection) is safe — the receiving layer validates at
   its own boundary and control stays low. Pull (reaching up) is not — the deep layer consumes
   whatever comes back and inherits the trust level of wherever it reached, at the depth where a
   failure or exploit has the widest blast radius. Without the *why* written down, "ROMs can't reach
   up" reads as bureaucracy and invites exactly the workarounds above.

   **The sharp consequence:** `ComponentRegistry.Validate()` cannot enforce this. It inspects
   *declared* `Requires`/`Provides`; a runtime lookup is **undeclared by construction** and never
   appears in that graph. One lookup API silently reopens the boundary every declaration-time check
   exists to hold. So enforcement must be by construction — don't provide the mechanism —
   which is strictly stronger than validating.

2. **A name for the published unit.** A ROM is an interchangeable part. Nothing named the larger
   thing that ships: an exposed API (used internally *and* externally) plus the implementation behind
   it, referenceable by others, possibly spanning more than one assembly. Without it, the Phase 2.7
   scaffold modelled the contract assembly as a third peer ROM (`D20SystemInterop : IInteropRom`) —
   which is wrong twice: it brokers nothing, and contracts living inside a ROM mean updating that ROM
   rebuilds the contracts and forces the recompile above that the structure exists to prevent.

**Arryn's requirement, not previously written anywhere:** a Rulebook must not depend on a specific
ROM assembly, and a ROM must be updatable without recompiling what sits above it. **This is violated
today and Phase 2.7 was deepening it** — `LunaFlow.Rulebook.csproj` references both `LunaFlow.Rom`
and `example/D20SystemExampleA.csproj`, and `OneD20RulebookContent.cs` opens with
`using LunaFlow.Rom.D20System;`.

### Decisions recorded

Full reasoning in [`design/cartridge.md`](../../../design/cartridge.md). In brief:

- **Cartridge** = contract surface (its own assembly, the types anyone may name) + implementation
  (one or more ROMs). **One rule: nothing outside a Cartridge may name an implementation type.** The
  surface is used internally as well as externally — that's what lets peer ROMs inside one Cartridge
  correlate facts without referencing each other.
- **The contract-surface assembly is the manifest.** A .NET assembly already carries complete type
  metadata and can't drift from what compiled. A sidecar manifest earns its keep only when something
  must resolve a Cartridge *without loading code* — i.e. the loader, deferred.
- **`IInteropRom.Resolve` is cut.** Pull API (ground 1) and redundant anyway (ground 2 — a single
  composition root already knows every implementation and can inject it explicitly, consistent with
  2.6.5 choosing hardcoded registration and [0019](0019-d20-rng-test-seam.md) choosing constructor
  injection over a static holder).
- **`Capability` survives, reframed** — a declaration validated at load time, not a handle resolved
  at runtime. `ComponentRegistry` still fails fast on missing/duplicate providers and wrong-way
  dependencies.
- **Not built, each with a trigger:** `ICartridge` identity type, manifest file, versioning, loader.
  A Cartridge is a *reference discipline* today, inspectable in the assembly graph.
- **The version-skew hole, named rather than left implicit (added later in the same session).**
  Compile-time type checking proves a module was once consistent with *some* build of a contract
  surface; nothing checks at load that the installed Cartridge is that one. Invisible today because
  every consumer compiles from this solution — a contract change is a compile error. The current
  failure mode is **late, not absent**: .NET raises `TypeLoadException`/`MissingMethodException` at
  first use of the type, potentially inside a rule firing mid-session, which contradicts the
  hard-fail-at-bootstrap property everything else upholds. Intended shape when needed:
  **per-contract-type fingerprinting** at bootstrap. Deferred — no third-party consumers exist —
  **but with four invariants recorded that keep the fix cheap**, the load-bearing one being that the
  contract surface is its own assembly and never inside a ROM. *Trigger: the first consumer we don't
  compile ourselves.*

  This also **corrected an overstatement** made earlier in this same ticket's design: "the contract
  surface assembly is the manifest" is true about what the assembly *is*, and not about what any
  given consumer *compiled against* — a fact about the consumer, which cannot live in the producer's
  assembly. Caveat now recorded in `cartridge.md`, and it gives the deferred manifest file a second,
  sharper trigger.

- **Then mostly solved, by Arryn, without the machinery I'd proposed — and it ships in 2.7.** Rather
  than *recording* compatibility state to compare later, bootstrap **exercises each declared contract
  and lets the runtime's own binding failure happen at a controlled point** — caught, reported
  against the offending module and contract, exit non-zero. The CLR was always going to run this
  check; it merely does so lazily, which is the entire source of the late failure. No recorded state,
  nothing to keep in sync, and the oracle can't go stale because it *is* the loader. Specified as
  [`cross-rom-capability-spec.md`](../../../Specs/cross-rom-capability-spec.md) §5 and added to
  `phase-2-7.md`'s Deliverables.

  Two details make or break it, both in the spec: **type load and member resolution are different
  events** — a cast catches a missing or renamed type but *not* a changed member signature, which
  raises `MissingMethodException` at JIT of the call site, and an edited interface is likelier than a
  deleted one (hence `GetInterfaceMap` + `RuntimeHelpers.PrepareMethod`); and **it must fail loudly**,
  since a probe that logs and continues turns a loud failure into a quiet one.

  **Fingerprinting is accordingly demoted, not adopted.** Its one surviving advantage is comparison
  *without loading code* — the offline/package-manager case, inherently the loader's problem. My
  original proposal was more machinery than the problem needed.

Docs updated: [`cartridge.md`](../../../design/cartridge.md) (new),
[`architecture.md`](../../../design/architecture.md) (trust-boundary note + pointer),
[`engine-core.md`](../../../design/engine-core.md) (push-don't-pull, and why validation can't cover it),
[`non-goals.md`](../../../design/non-goals.md) (refined packaging deferral, plus two new entries),
[`glossary.md`](../../../design/glossary.md) (`Cartridge`, `Contract surface`),
[`cross-rom-capability-spec.md`](../../../Specs/cross-rom-capability-spec.md) (§4 rewritten, acceptance
criteria updated).

### Ask

**Kofi (PM) — this ticket's assignee:**

1. **Add the requirement to the roadmap**, since it's a scope commitment and not mine to write: *a
   Rulebook must not depend on a specific ROM assembly; a ROM must be replaceable without recompiling
   what sits above it.* It belongs in [`phase-2-7.md`](../../../Roadmap/phase-2-7.md) at minimum, and
   plausibly as a standing constraint above the phase list.
2. **Judge whether this changes 2.7's scope enough to need re-sequencing.** My read is no — it
   *removes* work (`Resolve` unbuilt) and redirects the rest — but the Rulebook decoupling is real
   work that wasn't scoped, and that call is yours.
3. `cartridge.md` is a new design doc and needs registering wherever design docs are indexed.

**Juno (Game Designer) — routed, not assigned here:**
[`grid-rom-d20-split-spec.md`](../../../Specs/grid-rom-d20-split-spec.md) models `D20SystemInterop` as a
ROM. Under this ruling the contract surface is not a ROM and implements no `IRom`. The fact shapes
and content are unaffected — this is a mechanism correction, and I'd rather it reach her as a
question than have me edit her spec.

**Baraka (Impl Dev) — routed, not assigned here:**
[CodePlan 0037](../../../Roadmap/CodePlans/2-7-supersceded.md) needs: Phase B (implement
`Resolve`) deleted; `D20SystemInteropRom : IInteropRom` becomes a plain contracts assembly, not
registered with `ComponentRegistry`; `FleeCommandHandler` takes `IPlayerMovement` directly rather than
`IInteropRom`; and `LunaFlow.Rulebook`'s two ROM project references plus
`using LunaFlow.Rom.D20System;` come out, with Rulebook content expressed in Rulebook-owned types
that `Program.cs` translates.

### ⚠️ Conflicts with two existing tickets — needs Arryn's explicit confirmation before anyone builds

Flagged rather than written through, because one of them is **Arryn's own prior ruling** and I won't
assume a later design conversation silently reversed it.

- **[0036](0036-grid-rom-d20system-project-layout-decision.md) is directly contradicted.** It records,
  *ruled directly by Arryn*, that `D20SystemInterop` is "a genuine third ROM implementing
  `IInteropRom`, **not a passive contracts library**." This ticket's design says precisely the
  opposite: the contract surface is not a ROM, implements no `IRom`, and is not registered. The
  requirements Arryn stated on 2026-07-27 (no upward pull; a Rulebook must not bind a ROM assembly; a
  ROM must be replaceable without recompiling above) are what force the reversal, and they postdate
  0036 — so this reads as a supersession rather than a contradiction. **But 0036 was his call, and
  its reversal should be his too, stated explicitly, not inferred by me from a design discussion.**
  If confirmed, 0036 wants a struck-through supersession note pointing here, per house convention.

- **[0037](0037-implement-the-gridmap-d20system-split-grid-rom-d20-split-spec-md.md) is open and now
  instructs building something that no longer exists.** Its Ask is "build `grid-rom-d20-split-spec.md`
  in full, plus `IInteropRom.Resolve`/`Resolve<T>` per `cross-rom-capability-spec.md` §4.1." That §4.1
  is now the section explaining why `Resolve` was cut. Left alone deliberately — amending an open
  ticket assigned to Baraka, mid-flight, off the back of a design change he hasn't seen would be
  worse than the stale reference. **It should be paused or amended before he picks it up**, and that's
  the PM's call.

**Note on sequencing:** [0038](0038-phase-2-7-test-suite-blocked-on-rom-implementations-in-src.md) is
open against the same phase and may be affected by the project-layout consequences here — worth
reconciling before either proceeds.

### Response

The three PM-asks here (roadmap requirement, resequencing judgment, `cartridge.md` registration) are
the same three items 0040 itemizes as 4/6/5 and were actioned there, alongside the operational
unwind — see [0040](0040-pivot-to-2-6-5-and-re-sequence-phase-2-7.md)'s Response for the full
account. Short version: standing constraint added to `roadmap.md`; no resequencing (net scope wash,
absorbed in 2.7); `cartridge.md` confirmed already cross-linked from `architecture.md`/`engine-core.md`/
`non-goals.md`/`glossary.md`, not orphaned.

The ⚠️-flagged conflicts with 0036/0037 are also resolved: Arryn confirmed the 0036 reversal directly
(recorded on 0036 itself), and 0037 is amended in place with a stop-work banner (Baraka was already
mid-build on the now-cut `Resolve`).

### Resolution

Amara confirmed good to close, 2026-07-27 (relayed by Arryn). Both PM-asks and the two flagged
conflicts are settled: roadmap constraint recorded, no re-sequencing, `cartridge.md` verified
registered, 0036 superseded with Arryn's explicit word, 0037 amended (not backed out) with a
stop-work banner. Full operational trail lives in [0040](0040-pivot-to-2-6-5-and-re-sequence-phase-2-7.md).
