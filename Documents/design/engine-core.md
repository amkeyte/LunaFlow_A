# Engine Core

Implementation status: see [`../Roadmap/roadmap.md`](../Roadmap/roadmap.md).

This document describes the intended shape of the engine host and loop, and calls out where today's code diverges from it. For the exact current code layout, see [`CLAUDE.md`](../../CLAUDE.md).

## What the engine core owns

Per the role breakdown in [`architecture.md`](architecture.md), the engine core is the Developer's layer: session management, the turn loop, the command pipeline, and the guarantees every rule author can rely on without having to reimplement them. Rulebook authors should never need to know how the loop advances turns or how commands get parsed — they write rules and trust the engine to schedule them correctly.

## Current state vs. target

**Resolved:** the code used to have two independent, non-interoperating engine hosts (`EngineHost`+`GameEngine` vs. `ConsoleEngine`+`GameEngineCore`). `EngineHost` and `GameEngine` are deleted from the codebase; `ConsoleEngine` + `GameEngineCore` is the single host, wired up in `MyLunaFlowServer/Program.cs`, matching the target described below.

- `GameEngineCore` (in `Game/GameEngineCore.cs`) is an internal singleton (`GameEngineCore.Instance`) running a background-thread loop (`GameEngineCore.RunLoop`) driven by a command queue (`BlockingCollection<IGameCommand>`) and a manual turn gate (`FinishTurn()` / `TurnTick` event) — an explicit command queue plus an explicit turn-advance signal, rather than a tight synchronous loop, so it can serve more than one interface (CLI today, WebSocket later, per [`interfaces.md`](interfaces.md)) without those interfaces blocking each other on `Console.ReadLine()`-style calls.
- `ConsoleEngine.Run()` is a working CLI translator: it reads input, parses it (`DefaultCommandParser.Parse` is implemented), and submits a `DispatchGameCommand` to `GameEngineCore`, blocking until that command completes before reading the next line.

**Corrected 2026-07-20 (Amara doc pass):** this paragraph used to describe the layer above the host
as not-yet-built. It's built: a narrow `IEngineHost` (`Rules`, `Announce`) is reached via a public
`LunaFlowEngine.Host` gateway exactly as `../Specs/engine-host-spec.md` specifies — superseding
`nine-rooms-spec.md`'s original bare-`Rules`-property/public-`Post` proposal since `GameEngineCore`
is `internal sealed` and can't be named from outside `LunaFlow.Engine` at all — and `RuleEngine`
has a working `Query<T>()` passthrough. Both are live and in real use today: `MoveCommandHandler`
(`LunaFlow.Rom.NineRooms`) calls `LunaFlowEngine.Host.Rules.Query<PlayerPosition>()`/`.Insert`/
`.Retract` and `LunaFlowEngine.Host.Announce(...)` directly. Tracked in
[`../Roadmap/roadmap.md`](../Roadmap/roadmap.md) as Phase 1.1 and Phase 1.5 — both **done**.

## Translators

`IGameEngineTranslator` is the intended seam between the singleton engine and whatever is driving it — a console session, a WebSocket connection, a test harness. The doc comment on the interface states the model directly: the engine is a singleton, and it starts running when the first translator touches its instance. `ConsoleEngine` is the first (partial) translator implementation; a WebSocket translator would be `LunaFlow.Interface.Web`'s equivalent, submitting `IGameCommand`s to the same `GameEngineCore.Instance` rather than owning a second engine.

## Command pipeline

Commands flow through three roles, already scaffolded in `LunaFlow.Engine.Commands`:

- `ICommandParser` turns raw input (a console line, a WebSocket message) into an `ICommand` (`Name` + `Arguments`). `DefaultCommandParser.Parse` is implemented (splits on whitespace, first token lowercased as the command name) — this line previously contradicted the "Current state vs. target" section above, which had it right; corrected 2026-07-20.
- `ICommandDispatcher` routes a parsed `ICommand` to a registered `ICommandHandler` by name. `DefaultCommandDispatcher` is implemented and works standalone.
- `ICommandHandler` implementations do the actual game-state mutation for one command name.

This pipeline is shared by both hosts today and should stay host-agnostic — it has no dependency on `Console` or sockets, which is what makes it reusable across interfaces.

## Turn loop and engine guarantees

`GameEngineCore.RunLoop` separates two concerns that a synchronous loop conflates: draining submitted commands (which can happen any time, from any connected client) and advancing a turn (which only happens when something calls `FinishTurn()` and at least one listener is subscribed via `TurnTick`). A loop with no `TurnTick` subscribers still processes commands but never advances turns — this is what lets admin/debug commands work against an otherwise idle engine.

Once rule evaluation is wired into the turn advance, the engine is expected to provide guarantees that individual rules shouldn't have to reimplement by hand:

- **Settling markers** — once a marker fact like an end-of-game condition is present, further rule evaluation for that session stops. Rule authors shouldn't have to add `.Not<ExitGameMarker>()` to every rule by hand. **Implemented** (corrected 2026-07-20): `[StandingRule(typeof(T))]` on a `RuleTemplate` does exactly this — `RuleCodeGenerator` emits `.Not<T>()` into the generated rule's `When()` automatically, verified in `RuleCodeGenerator.cs` and covered by Nadia's B1–B5 suite (property-level pickup, union across guards, dedup on the same type).
- **One-time constraints** — a rule that should fire at most once per fact lifetime. Still open; no evidence either way found in a spot check, unlike the other three bullets here.
- **Initial settle** — `GameEngineCore.Start()` seeds initial facts before any command is ever dispatched (per [`../NineRooms/nine-rooms-spec.md`](../NineRooms/nine-rooms-spec.md)'s `RuleEngineOptions.Seed`). If a rule's condition is already satisfied by those seed facts — Nine Rooms' start-room challenge is the concrete case — it needs a chance to fire before turn one, not just on the first command. **Implemented** (corrected 2026-07-20): `GameEngineCore.Start()` calls `Rules.Fire()` once, unconditionally, right after `Options.Rules.ApplyTo(Rules)` — the source comment on that line literally reads `"initial settle": rules already satisfied by seeded facts get a chance to fire before turn one`, word-for-word this guarantee.
- **Evaluation phasing** — grouping rules into phases (e.g. via NRules agenda groups) so that, for example, all "reaction" rules resolve before the next command is accepted. Still open; no evidence found of agenda-group usage in a spot check.

**Corrected 2026-07-20:** this paragraph used to claim none of the above was implemented and that
`IEngineHost`/`RuleEngine.Query<T>()` were undelivered target contracts. Both claims were stale —
see the correction earlier in this doc under "Current state vs. target." Settling markers and
initial settle are real, verified against source, not just IR-level plumbing. `RuleEngine` exposes
`Insert`/`Update`/`Retract`/`Fire`/`Query<T>` (five members now, not four — `Query<T>` shipped with
`IEngineHost`) plus `LoadRulesFromAssembly`/`LoadRulesFromType(s)`/`Clear`; still no phasing or
marker-injection *on `RuleEngine` itself* — that logic lives in each generated rule's own `.Not<T>()`
call, injected at generation time, not as a `RuleEngine`-level feature. One-time constraints and
evaluation phasing remain genuinely open — no source evidence either way, not confirmed done.

## Component model: ROM, Rulebook, and the event bus

**Status: general/future design, not what Nine Rooms uses.** [`nine-rooms-spec.md`](../NineRooms/nine-rooms-spec.md) deliberately loads its one rule assembly through `RuleEngineOptions.FromAssembly(...)` called directly in `Program.cs` — hardcoded, no registry, no abstraction — and [`nine-rooms.md`](../NineRooms/nine-rooms.md) explicitly declines to extract a shared ROM contract on the strength of a single example. Everything below is where the ROM/Rulebook story grows *to*, once there's a second real ROM and dependency/ordering concerns between components stop being hypothetical. Building it now, against zero real consumers, would repeat the mistake already flagged in [`rule-pipeline.md`](rule-pipeline.md) about `GeneratorServices`.

### Two interfaces, not one

ROM and Rulebook (and eventually World Modules, GM Overlays, and network-facing Translators) all want to react to the same kinds of engine events, which makes a single shared "component" interface tempting. Worth resisting: a `Translator` and a ROM aren't peers. A `Translator` bridges an external, eventually-untrusted transport; a ROM is trusted, in-process code with real dependency relationships to other ROMs and to whatever Rulebook consumes it. One contract serving both would either carry fields the `Translator` doesn't need (`Tier`, `DependsOn`) or water down the ROM contract to fit a transport bridge.

So, two contracts, one nested in the other:

- **`IEngineEventSubscriber`** — small, widely implementable: "here are the events I care about" (`OnStartup`, `OnTurnAdvance`, `OnResultFeedback`). Anything that wants to observe the engine without being a governed game-logic component implements just this — a `Translator` relaying events to a socket, or eventually the AI GM assistant's narration layer (see [`ai-gm-assistant.md`](ai-gm-assistant.md)) listening for `OnResultFeedback` to narrate what just happened.
- **`AEngineComponent : IHasLunaFlowReference, IEngineEventSubscriber`** — the fuller contract for governed game-logic components: adds a `Tier` and a `DependsOn` list. Only `ARom`, and eventually a Rulebook-level equivalent, derive from this. This is what a future registry would dependency-check and tier-order; a `Translator` never goes through it.

```csharp
public interface IEngineEventSubscriber
{
    void OnStartup(IEngineContext context) { }
    void OnTurnAdvance(IEngineContext context) { }
    void OnResultFeedback(IEngineContext context, ResultFeedback feedback) { }
}

public enum EngineTier
{
    Rom,
    Rulebook,
    WorldModule,
    GmOverlay,
    Player,
}

public abstract class AEngineComponent : IHasLunaFlowReference, IEngineEventSubscriber
{
    public abstract ILunaFlowReference Reference { get; }
    public abstract EngineTier Tier { get; }
    public virtual IEnumerable<Type> DependsOn => Array.Empty<Type>();

    public virtual void OnStartup(IEngineContext context) { }
    public virtual void OnTurnAdvance(IEngineContext context) { }
    public virtual void OnResultFeedback(IEngineContext context, ResultFeedback feedback) { }
}

public abstract class ARom : AEngineComponent
{
    public override EngineTier Tier => EngineTier.Rom;
}
```

`EngineTier` is deliberately sized for the full stack in [`architecture.md`](architecture.md#system-setting-and-session) (Core / ROM / Rulebook / World Module / GM Overlay / Player), not just Rom and Rulebook, even though only ROM exists as code today. That's what makes the tag useful for more than labeling: a future registry can reject a component whose `DependsOn` points at a *higher* tier than itself (a Rulebook may depend on a ROM; a ROM depending on a Rulebook is a registration error, not a warning), and event delivery for a given turn can process lower tiers before higher ones, so a Rulebook's rules never observe a half-updated ROM mid-`OnTurnAdvance`.

One decision worth recording now even though neither side is built yet: `OnResultFeedback`'s structured payload is meant to eventually *supersede* Nine Rooms' plain-string `Post`/`Announce` channel, not run alongside it indefinitely. A rule announcing outcomes as raw text and a rule producing structured feedback (what fact, what rule, why) are the same underlying need with two different shapes — likely to end up as legacy-vs-current rather than two permanent parallel channels. When that reconciliation happens, favor the structured version.

**Superseded in part, 2026-07-21 ([Phase 2.6.5](../Roadmap/phase-2-6-5.md) design):** the sketch
above is no longer purely hypothetical, but what actually got built is narrower than this section,
and lands in different places than sketched. Real, in `LunaFlow.Engine.Components`: `EngineTier`
and tier-ordering enforcement (a component depending on a higher tier is a load-time error, exactly
as described above), plus `Capability`/`IHasDependencies` — a `Provides`/`Requires` contract, not
the single `DependsOn: IEnumerable<Type>` shown above. `IRom` (implementing
`IHasDependencies`) lives in `LunaFlow.Rom`, not as a generic `ARom : AEngineComponent` sketched
here — Phase 2.6.5 rejected a single shared base class in favor of each tier project owning its own
identity type, all implementing the same minimal Engine-defined contract. Peer-tier dependencies
(ROM-to-ROM) resolve through the Cartridge contract surface — peers declare `Provides`/`Requires`,
NRules matches the shared contract type directly — not through `DependsOn` naming a concrete type
directly. **Corrected 2026-08-09:** this paragraph previously named `IInteropRom` as the resolution
mechanism, current as of Phase 2.6.5's design but overtaken by the Cartridge pivot two phases later;
`IInteropRom` was a proposed runtime capability broker that never shipped real behavior and is now
retired (see `glossary.md`). Still not built, still deferred, non-goal as originally stated:
`IEngineEventSubscriber` and the tier-and-dependency-ordered `OnTurnAdvance` dispatch pass described
below — 2.6.5 only validates dependencies at load time, it doesn't touch turn-by-turn event
delivery. Treat the `AEngineComponent`/`ARom` code block above as historical (the design's first
draft), and `phase-2-6-5.md` as the authoritative shape for identity and dependency declaration.

### Turn-ending: ordering, not arbitration

A second ROM raises an obvious-sounding question — who gets to decide a turn is over? — that turns out to be the wrong question. `FinishTurn()` stays single-authority: one caller (the Rulebook or command layer, never a ROM) signals the action phase is done, exactly as it works today. Ending a turn was never a ROM-level concern — a ROM has no more say in when a turn ends than `Dice` has a say in when it gets rolled. No component votes on this, now or later.

What a second component *does* make real is delivery order for `OnTurnAdvance`, and today's mechanism isn't ready for that: `TurnTick` is a plain C# `event EventHandler`, and multicast delegates invoke subscribers in subscription order — whatever order things happened to register in, not tier order. With one subscriber that's invisible. With two, it's a latent bug: if a Rulebook-tier handler happens to subscribe before a ROM-tier one, it observes last turn's ROM state instead of this turn's. Once `AEngineComponent` exists, `OnTurnAdvance` dispatch has to become a tier-and-dependency-ordered pass — `EngineTier` order first (ROM before Rulebook before World Module before GM Overlay before Player), then a topological sort on `DependsOn` within a tier (a Combat ROM depending on a Physics ROM runs after it) — rather than a bare multicast invocation. "The turn is over" just means that ordered pass completed; there's no vote because no component is competing for authority, each one gets a guaranteed slot.

No quorum or "report in when you're done" protocol is needed for this, and it's worth being explicit about why: that pattern belongs to async/distributed coordination, where a participant might genuinely still be mid-work when you ask if it's finished. Nothing here works that way yet. `RuleEngine.Fire()` is synchronous — it runs the full NRules cascade to a fixed point before returning, so by the time a command handler's `Fire()` call returns, everything fact-driven that was going to happen already has. Even a multi-turn fuse timer (the arrow-in-the-keg scenario from earlier design discussion) doesn't need a "not done yet" signal — it's a fact with a turn-count, decremented once per `OnTurnAdvance`, not a background process. The one place genuine async, wait-for-completion semantics are likely to become necessary is the AI GM assistant (`ai-gm-assistant.md`) — a local LLM call takes real wall-clock time, and "has the assistant resolved Pass 1's fact requests yet" is a legitimate not-done-yet question in a way nothing else in this design currently is. That's the actual trigger for building real completion-gating later, not a second ROM.

**Two concrete cases of this were resolved during Nine Rooms implementation**, worth recording as settled, tested behavior rather than leaving as hypothetical:

- **`FinishTurn()` doesn't queue, by design.** It's a level-triggered gate — a `bool` plus a `ManualResetEventSlim` — not a counter. N rapid calls collapse to however many turns the background loop actually catches between them, not N; a batch of test failures initially read this as a bug (`FinishTurn_MultipleTimes_QueuesCorrectly` and four others expected N calls to produce N turns). It isn't one: "N calls, N turns" isn't a meaningful request without real state change between calls, the doc comment on `TurnFinished` already says "advance *exactly one* turn," and the only real consumer (Nine Rooms) doesn't use `FinishTurn`/`TurnTick` at all. The five tests were fixed to wait for each turn to actually fire before calling `FinishTurn()` again, matching this semantics, rather than the engine being changed to add queueing. If a genuine need for queued turns shows up later — distinct from this, and narrower: not losing a signal that arrives *during* processing — that's a separate, smaller feature to design against a real requirement, not a reason to add general queueing now.
- **A synchronous `FinishTurn()` call from inside its own `TurnTick` handler is ignored, not chained.** `AdvanceOneTurnSafe()` resets the gate *after* the handler runs (in a `finally`, so a throwing handler doesn't leave the gate wedged) rather than before, so a handler calling `FinishTurn()` on itself doesn't re-arm a fresh signal and trigger another turn on the next loop pass. (The reset-before-invoke ordering this replaced was a day-one authoring slip, not a later regression — confirmed via `git log -p`, introduced together with a comment stating the *opposite* intent in the same commit that first wrote `GameEngineCore.cs`, most likely a direct translation of the `planning/chats/*.txt` brainstorming into code without checking the two agreed.) This matches why `FinishTurn()` exists in the first place: it represents a deliberate external decision — the command/Rulebook layer saying a turn is over — not something the engine's own turn-advance notification should be able to trigger on itself. A handler needing cascading effects should rely on NRules' fact-settling (`Insert` a fact, `Fire()`, let the rule network cascade to a fixed point), not recursive turn-advancement at the loop level — letting `TurnTick` handlers re-enter `FinishTurn()` would open a second, uncontrolled cascade mechanism alongside the one NRules already handles cleanly.

### Authored content vs. infrastructure — what the Rulebook/Engine rule actually bans

**Added 2026-07-26** (owed since Phase 2.6.5's design and not delivered until now — ticket
[0030](../ClaudeWiki/Processes/Handoffs/0030-doc-reconcile-post-2-6-5.md)). Phase 2.6 established
the load-bearing invariant that **a Rulebook-tier content module never touches a `LunaFlow.Engine`
primitive directly** — ROM is the conduit. Phase 2.6.5 then had `IRuleBook` implement
`IHasDependencies`, which *is* a `LunaFlow.Engine.Components` type. That looks like a violation and
isn't, and the distinction needs to be written where a future session will hit it rather than left
in a phase doc.

The rule governs **authored content** — what a Rulebook Author writes into a content class like
`OneD20RuleBookContent`: no `ICommandHandler` implementations, no `LunaFlowEngine.Host`, no
`RuleEngine` calls, no NRules facts touched directly. It does **not** govern the tier's own
**infrastructure type** participating in the engine's module system. `IRuleBook` declaring
`Provides`/`Requires` is the Rulebook tier *registering itself* with the component registry — that's
how a tier joins the system at all, not content reaching around its conduit.

Two related non-violations, recorded at the same time so the rule isn't over-read as a blanket ban:

- **A transitive `Engine.dll` reference through ROM is expected and correct.** `LunaFlow.Rulebook`
  references `LunaFlow.Engine` and `LunaFlow.Rom`; what matters is that Rulebook *source* is clean of
  Engine primitives, not that the assembly graph has no edge. This was settled during 2.6's
  implementation — ticket [0022](../ClaudeWiki/Processes/Handoffs/0022-implement-phase-2-6.md) had to
  correct its own Ask, which wrongly said "directly or transitively."
- **Composition-root code is exempt outright.** `Program.cs` wires every tier together by
  construction; that's its job.

The test to apply, when it's unclear which side of the line something falls on: *is this the content
an author writes, or the plumbing that lets the tier exist?* Plumbing is exempt. Content is not.

### Push down, never pull up — the trust rule behind tier ordering

**Added 2026-07-27** alongside [`cartridge.md`](cartridge.md), which carries the full rationale.
`EngineTier` and `ComponentRegistry`'s tier check are usually described as layering hygiene. They
are not — they are a **trust boundary**, and the distinction changes what's permitted.

[`architecture.md`](architecture.md)'s role table is a trust gradient: each tier above the engine
core is authored by a less-trusted contributor, ending in World Modules, GM Overlays and Player
rules, which are unreviewed script. So the direction a dependency runs decides who is exposed to
whom. **Pushing data down is safe** — a lower layer is handed something and validates it at its own
boundary, so control stays with the layer that has the most to lose. **Pulling up is not** — a deep
layer that reaches into shallower territory consumes whatever comes back and inherits its trust
level, at the depth where a failure or exploit has the widest blast radius.

**The part that matters for this document:** `ComponentRegistry.Validate()` cannot enforce this. It
checks *declared* dependencies — `Requires` against `Provides`, plus the tier comparison. A runtime
lookup API (`Resolve(name)`, a service locator, any registry a component can query) is **undeclared
by construction**, never appears in the graph the validator inspects, and so is structurally
invisible to it. One such API reopens the boundary that every declaration-time check exists to hold.

Enforcement is therefore **by construction**: no upward lookup mechanism is provided at all. This is
why `IInteropRom.Resolve` was cut during Phase 2.7 design rather than constrained — a composition
root injecting the same object explicitly achieves an identical result with no pull surface. Static
content reaching a ROM by constructor injection from `Program.cs` (see "Three kinds of state" below,
category 2) is the sanctioned pattern precisely because it is a push.

### Cross-ROM correlation — see the spec

How a ROM's rule correlates against a fact a *peer* ROM owns, without either referencing the other,
is specified in [`../Specs/cross-rom-capability-spec.md`](../Specs/cross-rom-capability-spec.md).
The short version, because it constrains everything below: the matched type is a **contract type in a
shared interop assembly**, not the owner's concrete fact class — and because both peers then share
that type, ordinary NRules matching does the work with **no runtime broker call**. That property is
deliberate. Generated rule code has no constructor or DI seam, so any design that needs rule code to
*call* a broker reintroduces the static global `D20SystemContent.Configure` was.

**When to leave `Requires` undeclared: correlation vs. construction order.** `Capability.Requires`
exists to let `ComponentRegistry` answer one question — does the graph have a valid construction/load
order, with no component needing something that isn't there yet or that only exists at a higher tier.
It is not a general "these two things are related" annotation. Two peer ROMs that correlate on a
shared NRules fact type never have a construction-order problem to declare: both facts live in the
same session, `RuleEngine.Fire()` is synchronous, and neither peer's rule needs the other peer to have
constructed first — matching happens at fire time, against whatever facts are present, regardless of
which ROM's constructor ran first. So when two peers each read a contract the other provides —
GridMap's win-line rules matching `IChallengeSolved` (D20System-owned) while D20System's
challenge-prompt rules match `IPlayerLocation`/`IGameWon` (GridMap-owned) — declaring `Requires` on
**both** sides encodes a construction-order claim that isn't true, and `ComponentRegistry`'s cycle
detector correctly rejects it: from its point of view, that really is indistinguishable from two
components each waiting on the other to be built first, which has no valid order.

**The rule:** if a ROM's only relationship to a peer-provided contract is matching/querying it through
NRules (`Match<T>`, `rules.Query<T>()`) — no capability the peer's code must construct and hand over
before this ROM's own construction can complete — leave that side's `Requires` undeclared and say so
in a doc comment on the ROM (`GridMapRom.Requires`'s comment is the model to copy). Declare `Requires`
only for the direction that's a genuine capability the registry needs to size and order — typically
one side of the pair, not both, exactly as D20System's `PlayerLocation`/`PlayerMovement`/`GameWon`
requirements are real (it needs GridMap constructed to receive `IPlayerMovement`'s concrete instance)
while GridMap's mirror-image dependency on `IChallengeSolved` was never anything but a fact-match.
This isn't a special case invented for GridMap/D20System — it's `Requires`' actual scope, applied
consistently. First written down here 2026-08-03, ticket
[0071](../ClaudeWiki/Processes/Handoffs/0071-peer-rom-bidirectional-capability-cycle-detection-gap.md),
after the same shape (GridMap↔D20System) shipped correctly once already, undeclared, without this
reasoning being on record anywhere a second implementer could find it — see
[`grid-rom-d20-split-spec.md`'s capability table correction](../Specs/grid-rom-d20-split-spec.md#capability-declarations)
for the drift that surfaced it. `ComponentRegistry`'s cycle detector itself is not changed by this —
it is working exactly as designed; this is a modeling correction on the `Requires` side, not a
framework gap.

### ROM as the conduit to NRules

A ROM's pure calculations (`Dice.Roll()`, a Physics ROM's intersection test) stay ordinary method calls — a Rulebook Author references the ROM assembly and calls them directly, same as any library. What should go through the ROM rather than being called directly is anything touching the live NRules session: inserting, updating, or retracting a fact. A Rulebook shouldn't call `RuleEngine`/NRules directly — it goes through the ROM's API for whatever fact types that ROM owns, and the ROM is what actually calls `RuleEngine.Insert`/`Update`/`Retract` underneath. That keeps NRules an implementation detail of the ROM layer, consistent with `non-goals.md`'s decision not to abstract over multiple rule engines — NRules stays a committed dependency, just not one a Rulebook Author touches directly — and gives the ROM one place to eventually stamp fact provenance for the "why did this happen" tracing flagged as an open question in `rule-pipeline.md`.

Today, none of this indirection exists: Nine Rooms' hand-written `ARule`s call NRules facts directly (`Match<PlayerPosition>`, `ctx.Insert(new WinMarker(...))`), because there's no ROM-owned fact API to go through yet, and building one for a single disposable test rulebook isn't worth it. This section describes where that indirection goes once there's ROM content worth mediating.

### Registration: hardcoded now, config-driven later

Today, and for the near future, ROM (and rule) loading is exactly what `RuleEngineOptions.FromAssembly(...)` already is: one explicit call per assembly, written directly by whoever's configuring the engine (`Program.cs`, for Nine Rooms). No reflection-based discovery, no registry, no config file — deliberately, since there's no second real consumer yet to design a loader against, and a hardcoded call is trivially easy to change by hand while that's true.

The point at which this should change is when Rulebook work starts in earnest (Roadmap Phase 2+): a Rulebook is meant to be swappable — "play D&D or Monopoly with the same engine" — which argues for loading it from a config-defined package manifest (which ROM assemblies a shipped game needs, which Rulebook assembly to load) rather than a hardcoded reference in `Program.cs`. ROM registration can likely move onto the same manifest at that point too, once there's an actual reason to — a second Rulebook needing a different ROM combination is the concrete trigger, not a date. Building the config/package system before then would be solving "how do we ship different combinations to different end users" before that problem exists. See `non-goals.md` for this as an explicit, named deferral.

## `GameState`, `GameWorld`, `Session`

These three types in `LunaFlow.Engine.Game` are currently empty placeholders and are the obvious next things to design once the single-host consolidation above is done:

- `Session` — one connected client's context: input history, local state, which translator it's attached to.
- `GameState` — a snapshot of engine state at a point in time, with an eye toward rollback/replay.
- `GameWorld` — persistent, cross-session world data: locations, entities, and other domain-level constructs that outlive a single `Session`.

### Three kinds of "state" — don't conflate them

**Added 2026-07-20**, after a wiring question (how does a Rulebook's room/combatant data reach a ROM
handler at runtime) surfaced that this hadn't been written down anywhere. "Game state" gets used
loosely; in practice there are three distinct things, only one of which is actually still open:

1. **Mutable, in-play state** — `PlayerPosition`, `RoomSolved`, `CombatRound`, HP, anything that
   changes turn to turn. This already has a formal system: NRules facts, through `RuleEngine`
   (`Insert`/`Update`/`Retract`/`Query<T>()`), consistent since Phase 1.5. This is also the state
   that would actually need to cross a network in a multi-client future — and it's already
   centralized, not something each ROM invents its own answer for.
2. **Static seed/content data** — room definitions, combatant stat blocks, anything seeded once and
   read-only for the rest of the session (per `1-d20-spec.md`'s own framing: "static content, seeded
   once at session start"). This isn't game state in the sense above — it never changes, so it never
   needs network-sync machinery; every client already has it, compiled in. It reaches ROM handlers by
   plain constructor injection (same pattern as ticket 0019's `Random` seam) — a composition root
   (`Program.cs`/host-wiring) constructs ROM handler instances with whichever Rulebook/World Module
   data they need. No registry, no new mechanism; this is the existing pattern, just named.
3. **Engine-level bookkeeping** — `GameState`/`GameWorld`/`Session` above. This is the one genuinely
   still open, and it's deliberately deferred, not neglected: `non-goals.md` names no concurrent-
   session isolation and no persistence backend as explicit non-goals until a real interface layer
   (Roadmap Phase 5) exists to design against. Nothing about (1) or (2) forces that trigger — don't
   let a wiring question for static content talk you into building session/persistence
   infrastructure ahead of a real network interface to design it against.

Their current emptiness is a placeholder, not a design decision — no shape has been agreed for them yet beyond the doc comments already on the classes.
