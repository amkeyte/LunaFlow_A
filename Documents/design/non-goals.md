# Non-Goals

Things LunaFlow is deliberately not doing right now, and why. The planning transcripts this project grew out of ranged across AI, a web server, scripting, and multiplayer all at once — useful for exploring the space, bad as a build order. This doc exists so a design conversation that starts drifting toward one of these has somewhere to point.

Each entry says what would make it worth revisiting.

**Not a general-purpose game engine.** No rendering, no physics, no asset pipeline. LunaFlow is a rules/state engine for text- and GM-mediated tabletop-style play; visuals are entirely the client's problem, if a graphical client ever exists at all. *Revisit: never, by design — this is a scope boundary, not a sequencing question.*

**Not building a GUI client.** The interface layer (`interfaces.md`) is CLI and WebSocket/text-protocol only. A graphical client is out of scope for this repo; it would be a separate consumer of `LunaFlow.Interface`'s DTOs. *Revisit: once the WebSocket interface (Roadmap Phase 5) is stable enough for a client to build against.*

**Not targeting multiplayer scale.** No auth, no matchmaking, no persistence backend, no concurrent-session isolation beyond what the singleton `GameEngineCore` gives for free. The near-term target is one GM running one live session with one or a few connected clients. *Revisit: after Phase 5, once a real interface layer exists to even define "multiple sessions" against.*

**Not committing to a hosted/cloud LLM.** The AI GM assistant (`ai-gm-assistant.md`) targets a local Ollama model specifically so the tool works offline at a table. Cloud-API backends (OpenAI, Anthropic, etc.) aren't planned, though the design leaves a model-adapter seam that could support one later. *Revisit: only if local inference quality/speed turns out to be a real blocker, and only as an optional adapter, not a replacement.*

**Not abstracting over multiple rule engines.** NRules is a direct, committed dependency (`ARule` derives from `NRules.Fluent.Dsl.Rule` directly), not hidden behind an engine-agnostic interface. `GeneratorServices` abstracts *rule authoring* (C# vs. script), not *rule execution*. *Revisit: only if NRules itself becomes a blocker (licensing, performance, maintenance) — not preemptively.*

**Not building the player-contributed rule sandbox.** `PlayerRule` is named and roughly scoped in `architecture.md`'s role table, but no sandboxing mechanism, review flow, or authoring UI has been designed. This is explicitly the last rung of the role ladder and depends on every other role's tooling existing first. *Revisit: after Roadmap Phase 3, once there's a real game loop worth letting players decorate.*

**Not designing rule schema versioning yet.** `rule-pipeline.md` flags this as an open question rather than a decision; there's no `version:` tag or migration story for scripted rule content today. *Revisit: once `scripting-content.md`'s script format is actually chosen (Phase 4) — versioning a format that doesn't exist yet is premature.*

**Not building the general ROM/Rulebook component model yet.** `engine-core.md` describes a target shape — `IEngineEventSubscriber`, `AEngineComponent`, tiered dependency validation, an event bus for startup/turn/result-feedback — but it's explicitly future design, not something to build against Nine Rooms or a single ROM. Today's loading is a hardcoded `RuleEngineOptions.FromAssembly(...)` call in `Program.cs`, and that's sufficient as long as there's only ever one real content assembly to load. *Revisit: once a second real ROM (or the first real Rulebook) needs dependency ordering or lifecycle coordination the hardcoded call can't express — not preemptively.*

**Partially triggered, 2026-07-21 (Phase 2.6.5):** the trigger condition fired for *half* of this entry, not all of it. Real identity (`IRom`/`IInteropRom`) and real tiered dependency declaration/validation (`Capability`/`IHasDependencies`/`EngineTier`, in `LunaFlow.Engine.Components`) are being built — that's the "dependency ordering the hardcoded call can't express" case landing for real, against Grid ROM/D20System as the second real ROM. What's *not* built: `IEngineEventSubscriber` and the tier-and-dependency-ordered `OnTurnAdvance` dispatch pass `engine-core.md` describes — no concrete case needs turn-by-turn event ordering across components yet, so that half stays deferred, non-goal as originally stated.

**Not enforcing the ROM conduit at the access-modifier level yet.** `GameEngineCore.Rules` and `Post` are public, so nothing currently stops Rulebook-tier code from bypassing a ROM and touching NRules directly — "ROM as conduit" (`engine-core.md`) is a documented convention, not a compiler-enforced one. Acceptable for now since nothing is violating it on purpose. *Revisit: once there's an actual instance of code bypassing the convention, or a second ROM/Rulebook pairing makes the boundary worth enforcing for real — not preemptively.*

**Not building a real Reference parent/child hierarchy for generated rules yet.** `GeneratedRuleReference` (`rule-pipeline.md`'s D3 decision) gives every generated rule a flat dotted-path `FullPath` (`GroupName.RuleName`) with `Parent => null`, the same simplicity tier as Nine Rooms' hand-written `NineRoomsReference` — not a real object graph where a group-level reference is `Parent` and its rules are `Subreferences`. `IRuleBook`/`IRuleGroup` are still empty marker interfaces with nowhere for that chain to attach. *Revisit: once `IRuleBook`/`IRuleGroup` get real bodies — building the chain against the pipeline's first real consumer, before there's a second shape to generalize from, would be premature.*

**Not building config-driven ROM/Rulebook packaging yet.** Which ROM modules and which Rulebook assembly to load is a compile-time decision (a literal call in `Program.cs`), not something read from a manifest at startup. A config-defined package format only earns its cost once a Rulebook is meant to be swapped without recompiling the host — "play D&D or Monopoly with the same engine" as an actual runtime choice, not just an architectural claim. *Revisit: when Rulebook work starts in earnest (Roadmap Phase 2+) and there's a second Rulebook/ROM combination to ship.*

**Refined 2026-07-27 (Cartridge design).** Two distinctions this entry previously blurred, now that [`cartridge.md`](cartridge.md) names the unit:

- **Preserving hot-swap is not the same as building it, and only the first is in scope.** A Rulebook binding to a Cartridge's contract surface rather than to ROM implementation types costs nothing today and keeps the door open. Building the loader is what stays deferred — `Program.cs` constructing ROMs by hand is a compile-time binding no matter how clean the contracts are.
- **The manifest already exists.** A Cartridge's contract-surface assembly carries complete type metadata and cannot drift from what compiled. A sidecar manifest file earns its keep only when something must resolve a Cartridge *without loading code* — which is this same loader. *That* is the trigger, more precisely than "a second combination to ship."

**Not building the Cartridge runtime model yet.** [`cartridge.md`](cartridge.md) defines a Cartridge as a **reference discipline** — a contract surface, an implementation, and one rule about which types may be named from outside — enforced by project structure and inspectable in the assembly graph. There is deliberately no `ICartridge` identity type carrying `Name`/`Credits`/`Provides` the way `IRom` does, and no per-Cartridge registration: `ComponentRegistry` registers ROMs, nothing consumes a Cartridge at runtime, and recutting registration would rework Phase 2.6.5's model against zero drivers. *Revisit: when something genuinely needs to reason about Cartridges at runtime — realistically the loader above, not before.*

**Not building a Cartridge compatibility gate yet — but the invariants that keep it cheap are load-bearing now.** A module is type-checked against a Cartridge's contract surface *at compile time*; nothing checks at load that the installed Cartridge is the one it was built against. The gap is invisible today because every consumer is compiled from this solution, so a contract change is a compile error. It is not invisible once anything we don't compile ourselves binds to a Cartridge — and the current failure mode is *late*, not absent: .NET raises `TypeLoadException`/`MissingMethodException` at first use of the type, which can be mid-session inside a rule firing, contradicting the hard-fail-at-bootstrap property everything else upholds. **Mostly answered without new machinery, and that part is *not* deferred:** rather than recording compatibility state, bootstrap **exercises each declared contract and lets the runtime's own binding failure happen at a controlled point** — caught, reported with the offending module and contract, exit non-zero. The CLR was always going to perform this check; it just does so lazily. That ships in Phase 2.7 ([`../Specs/cross-rom-capability-spec.md`](../Specs/cross-rom-capability-spec.md) §5). What stays deferred is **fingerprinting**, whose one surviving advantage is comparison *without loading code* — the offline/package-manager case, which is the loader's problem. See [`cartridge.md`](cartridge.md#compile-time-consistency-is-not-deployment-compatibility). **Second motivating case added 2026-08-08, ticket 0077's consolidation discussion:** a fingerprint over a (now potentially multi-ROM) implementation assembly could also serve peer-isolation/tamper-evidence purposes, not just deployment compatibility — see the peer-isolation entry above. Still one deferred mechanism, not two; scope both cases together when it's built. **The deferral is only prudent while four invariants hold** — contract surface is its own assembly and never inside a ROM; a module's required contracts are machine-enumerable; nothing outside a Cartridge names an implementation type; contract surfaces stay small. A proposal that breaks one is a proposal to pay this cost early, and should be read that way. *Revisit: the first consumer we do not compile ourselves — not a date, and not "when we do versioning."*

**Not generalizing `IOneD20WorldModule` into a shared `IWorldModule` yet.** [`world-module.md`](world-module.md)
(Phase 2.8) gives OneD20 a Rulebook-owned shape contract a World Module implements, but the contract
names OneD20's own record types (`OneD20RoomData`, `OneD20MonsterTemplate`, etc.) directly rather than
some engine-defined generic shape. This is the same discipline `ARom`/`IRom` already went through
once: don't abstract a per-tier contract from a single consumer. *Revisit: once a second Rulebook
needs its own World Module tier and the two contracts have enough in common to be worth factoring —
not before.*

**Not registering World Modules with `ComponentRegistry` yet.** A World Module is a plain object a
Rulebook validates and consumes, constructor-supplied by `Program.cs` — not an `IHasDependencies`
component, not `EngineTier`-enforced beyond the enum value already existing. It doesn't correlate
NRules facts with a peer the way a ROM does, and this phase proves exactly one World Module feeding
one Rulebook. *Revisit: when a second Rulebook needs the contract generalized (above), or the
additive-composition principle (`architecture.md`) is actually exercised with two World Modules
loaded simultaneously and something needs to arbitrate between them.*

**Reconsidered and reaffirmed, 2026-08-03 (Phase 2.8/2.9 gate):** neither Phase 2.9 (Lua authoring)
nor Phase 4 as actually scoped needs two World Modules loaded at once — Phase 4's literal Done-When
bar doesn't require it either. Checked deliberately rather than left to rot; the deferral holds
exactly as stated above, not revisited preemptively.

**Not building a generic, reflection-based Lua World Module host yet.** [`world-module.md`](world-module.md#two-authoring-paths-into-one-contract)
gives OneD20 a concrete `LuaOneD20WorldModule : IOneD20WorldModule` — it marshals a Lua table
directly into OneD20's own record shapes (`OneD20RoomData`, etc.), the same way
`DefaultOneD20WorldModule` does by hand. It is deliberately not a `LuaWorldModuleHost<T>` that
reflects over any Rulebook's world-module contract and could host a second Rulebook's Lua content
for free. Same discipline `IOneD20WorldModule` itself was built under, one tier up: don't abstract a
mechanism from its single real consumer. *Revisit: once a second Rulebook needs the same
Lua-hosting mechanism and the two concrete hosts have enough in common to be worth factoring — not
before.*

**Not building rule-scale guardrails yet.** Pinned in conversation, 2026-08-03, while stress-testing
the GM-Overlay reload draft against a hypothetical procedural ROM — nothing currently in the pipeline
warns or protects against a rule set growing large or slow to compile. Three checks are pinned for
later, not designed or built:

- **(a) Per-module rule-count warning** — a module (ROM, Rulebook, GM Overlay, any future procedural
  generator) that contributes more than some threshold *X* rules to the pipeline gets a warning at
  the point its rules are gathered, before compilation.
- **(b) Total rule-count warning** — the aggregate rule count across everything being compiled
  together exceeds some threshold *Y*, independent of whether any single module is individually over
  its own limit — catches the case where several modules are each reasonable but the sum isn't.
- **(c) Compile/processing-time warning** — actual measured wall-clock time for a compile exceeds
  some threshold *T*, regardless of what the rule count was. This is the one that actually matters
  most and can't be replaced by (a)/(b): Rete's cost isn't a function of rule *count* alone, it's rule
  count crossed with join complexity crossed with matching fact volume. A handful of rules with wide
  joins across a large fact population can be far more expensive than ten thousand flat rules with
  simple conditions — a count-based check gives false confidence in exactly the case that hurts most.

**The motivating (hypothetical, not scoped) example:** a future ROM simulating basic chemistry —
reactions between materials, possibly modeled on real periodic-table elements. Its *rule count* could
look small and safe while its *join structure* (matching combinations of elements/compounds against
each other) is combinatorially explosive — the textbook way a Rete ruleset "grows cancer" without
tripping a naive rule-count ceiling. Consistent with `engine-core.md`'s push-down/never-pull-up
framing elsewhere: **this stays the ROM author's responsibility to understand, not something the
engine prevents.** These three checks are diagnostic — warnings a module author or a GM would see —
not a load-time hard-fail in `ComponentRegistry.Validate()`'s style. That's a deliberate difference
from this project's usual "fail fast, never silent degradation" convention: rule-set scale is a
performance concern an author needs visibility into, not a correctness invariant the engine can
unilaterally refuse to load against — nobody's shipping a chemistry ROM by accident.
*Revisit: once a real ROM proposal risks this scale — check whether it's actually proposing many
**rules** (this concern) or many **facts** matched by a handful of rules (which NRules already
handles natively and this doesn't apply to) before assuming it needs any of this.* Same conversation
also produced the framing this entry's motivating example stress-tested against — see
[`gm-overlay.md`](gm-overlay.md)'s draft "The real axis is volatility, not trust": a rule source needs
session isolation for cost-containment reasons independent of whether it also needs it for trust
reasons, and a procedurally volatile-but-trusted ROM would be the case that actually exercises this,
not GM Overlay.

**Not registering GM Overlays with `ComponentRegistry` yet.** `EngineTier.GmOverlay` exists as a
shipped enum value, but nothing is tagged with it — [`gm-overlay.md`](gm-overlay.md) (Phase 4 kickoff,
2026-08-03) designs the tier's boundary and rules that the script-driven rule adapter belongs there,
without deciding whether a GM Overlay ever becomes an `IHasDependencies` component with declared
`Capability` requirements. Same discipline `WorldModule` went through at 2.8: there's exactly one
kind of consumer in view (a script compiled into the live session) and no peer-correlation problem
yet. *Revisit when a GM Overlay rule needs to declare a dependency on something a specific ROM or
Rulebook provides — not preemptively.*

**Not building a peer-isolation compliance check yet — even though consolidating peer ROMs into one
assembly (`cartridge.md`'s Structure section, corrected 2026-08-08, ticket
[0077](../ClaudeWiki/Processes/Handoffs/0077-do-peer-roms-still-need-one-project-each-now-that-cross-assembly-correlation-is-proven.md))
is the trigger firing now, not a future one.** Named explicitly rather than left implicit, because the
usual framing here ("revisit once a concrete driver exists") doesn't quite fit: the driver exists as
of this entry. Once a Cartridge's ROMs share an implementation assembly, "no peer names a sibling's
concrete type" (`cross-rom-capability-spec.md` §3.2 rule 1) stops being provable from the assembly
graph and becomes convention only — nothing today catches a ROM that reaches past the contract surface
into a peer's internals just because it now compiles. The obvious fix is a build-time check (a
source-inspection pass over each ROM's namespace, flagging a reference into a sibling ROM's namespace
that isn't through the Cartridge contract surface — same shape as `doc-lint`'s drift checks, run
against code instead of docs) that fails the build the way §5.4's contract-surface shape check fails
bootstrap. **Deferred anyway, on Arryn's explicit direction:** consolidation ships first, the gap is
accepted and disclosed rather than quietly left implicit (see `cartridge.md`'s corrected Structure
section), and this check is built once, deliberately, rather than folded into whichever ticket happens
to be consolidating ROMs at the time. *Revisit: the first time this gap actually produces a real
violation, or — preferably — proactively, before a second Cartridge exists and the check needs to be
written generically instead of against one worked example.*

**Not building an assembly fingerprint for peer-isolation/security purposes yet — folds into the
Cartridge fingerprinting deferral already on record below, not a second mechanism.** Arryn raised this
alongside the peer-isolation check above: once ROMs share an assembly, a fingerprint over that assembly
could serve a second purpose beyond the deployment-compatibility case `non-goals.md` already defers
(below) — attesting *what shipped* for tamper-evidence/security reasons, independent of whether a
consumer compiled against a matching version. Recorded here as an additional motivating case for that
same deferred mechanism, not a new one to design separately; when fingerprinting is eventually built,
scope it against both use cases at once rather than shipping the compatibility case first and
retrofitting security-attestation onto it later.

**Not providing any upward lookup API — permanently, on security grounds.** This is a scope boundary rather than a sequencing question, so it differs in kind from everything else here. No service locator, no queryable registry, no `Resolve(name)` a component can call to obtain something registered by a higher tier. The tier stack is a trust gradient ending in unreviewed script; a deep layer that *pulls* from a shallower one inherits its trust level at the depth where a failure has the widest blast radius. Crucially, this **cannot be delegated to validation** — `ComponentRegistry` checks *declared* dependencies, and a runtime lookup is undeclared by construction, so one such API silently reopens the boundary every declaration-time check exists to hold. Enforcement is by construction: the mechanism is not provided. Data is pushed down by the composition root and vetted by the receiver. *Revisit: never, by design. A concrete need for one is evidence of a modelling error, not of a missing feature.*
