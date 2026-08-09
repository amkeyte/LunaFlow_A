# GM Overlay — the live-session tier above World Module

Implementation status: see [`../Roadmap/roadmap.md`](../Roadmap/roadmap.md).

> **Ruled, 2026-08-09: single session for v1.** The two-network split proposed and drafted here on
> 2026-08-03 is **considered, not adopted** — kept in place below for the reasoning trail, same
> convention `glossary.md` uses for a rejected design worth keeping ("kept for lineage"). See
> ["Ruled: single session, not two"](#ruled-single-session-not-two) for why the isolation problem
> that motivated it turned out to already be solved a different way, and what would still bring it
> back.

A **GM Overlay** is a live, session-scoped rule or world-state edit a GM (Storyteller) makes during
play — "Goblin 3 is unkillable," "go directly to the Wizard, do not pass Go." `architecture.md`'s
four-tier diagram and `glossary.md` have named this tier since before either had a line of code
behind it. This document gives it the same treatment `world-module.md` gave World Module at Phase
2.8: a real boundary, not just a diagram label. It exists now because Phase 4's kickoff gate
(ticket [0064](../ClaudeWiki/Processes/Handoffs/0064-phase-2-9-ready-and-phase-4-scope-correction.md))
surfaced a concrete question this tier has to answer before a buildable spec exists — where does a
scripted, session-live rule (`scripting-content.md`'s `StartCombat` example) actually belong?

**`EngineTier.GmOverlay` already exists in shipped code** (value 3, between `WorldModule` and
`Player` — see the [API Reference](../ApiReference/LunaFlow.Engine/LunaFlow/Engine/Components/EngineTier/index.md)).
What doesn't exist is anything tagged with it: no contract type, no `ComponentRegistry`
registration, no concrete mechanism. Same starting position `WorldModule` was in before Phase 2.8 —
the enum value was sized for the full stack years before a real consumer justified building around
it (`engine-core.md`).

## Boundary against World Module

World Module and GM Overlay are **independent siblings on top of Rulebook**
(`architecture.md#quick-reference`), not a chain — neither depends on the other. But they answer
different questions, and the difference is load-bearing for how each is built, not just a labeling
distinction:

| | World Module | GM Overlay |
| --- | --- | --- |
| Authored | Ahead of time, offline | Live, during a session |
| Reviewable | Yes — a `.lua`/`.cs` file that exists before play starts | No — a GM's in-the-moment call |
| What it produces | **Data**: populates a Rulebook-owned static shape (`IOneD20WorldModule`'s properties) | **Behavior**: a rule that fires inside the running NRules session |
| When it's checked | Once, at construction/assembly (`OneD20WorldModuleValidator`) | N/A in the same sense — there's no pre-play file to validate; the check (if any) happens at the moment the GM applies it |
| Lifecycle | Fixed for the session once assembled (`world-module.md`'s "executes once, at construction") | Ephemeral — scoped to the session it was made in, possibly to less than that |
| Consumed by | Rulebook-tier assembly code (`OneD20SessionAssembler`) | The live rule engine directly — inserted into the running session, not marshaled into a shape another layer reads later |

The practical consequence: a World Module is a **populator** for a contract the Rulebook already
defines and validates before anything runs. A GM Overlay is not populating anything — it is
**adding to or overriding the running rule set**, which is a fundamentally different mechanism, even
though both sit at the same trust distance from Rulebook in the tier diagram.

## Boundary against Rulebook

**History note, resolved 2026-08-09 — see ["Ruled: single session, not two"](#ruled-single-session-not-two)
below for the final word.** This section originally (2026-08-03) claimed GM Overlay rules compile
into and fire inside the *same* session as the Rulebook, then got flagged as wrong the same day: a
rule's RHS actions run with full `ISession` reach once compiled in, regardless of what LHS guards
gated whether it fired. A two-network split (a separate, isolated overlay session) was drafted as the
fix and sat unratified for a stretch. **It's since been superseded again, the other direction: single
session is correct after all, but not for the reason the original text below assumed** — the
isolation gap is closed a different way (a narrow, closed expression grammar, ruled via ticket 0073,
rather than a session boundary). Full reasoning below the two-network draft. The paragraph
immediately following is kept as originally written for the history, same as the draft that corrected
it is kept below rather than deleted.

Rulebook rules and GM Overlay rules can both end up as ordinary NRules rules loaded into the same
session — that's the point of `rule-pipeline.md`'s IR being format-agnostic (`RuleParseContext` →
`RuleIRGroup` → `RuleCodeUnit`, regardless of whether the source was C# reflection or a script). What
differs is trust and compile time, not the destination shape:

- **Rulebook**: C#, statically checked at build time, fully trusted, compiled ahead of play via
  `ReflectionRuleParser` + `RoslynRuleCompiler` (compile-time mode, `rule-pipeline.md`).
- **GM Overlay**: scripted, unreviewed, produced during play, compiled through the same pipeline's
  **runtime mode** — the concrete slot Phase 4's `IRuleParser`/`IRuleCompiler` pair for a script DSL
  fills. **Mechanism ruled, 2026-08-09 (ticket [0073](../ClaudeWiki/Processes/Handoffs/0073-rule-compilation-cost-and-runtime-mode.md),
  full evidence in [`rule-pipeline-compilation-cost.md`](rule-pipeline-compilation-cost.md)):** not
  Roslyn. Rulebook's compile-time path stays exactly as it is (~800ms of in-memory `CSharpCompilation`,
  tolerable as a one-time startup cost for trusted content); the runtime path instead builds
  `IRuleDefinition`s directly via `NRules.RuleModel.Builders.RuleBuilder` and
  `System.Linq.Expressions.LambdaExpression` — no C# source text generated, nothing handed to a
  compiler frontend at all. Two independent reasons this is right for this tier specifically, not
  just cheaper: it's the mechanism NRules itself actually exposes for building an `IRuleDefinition`
  (`RuleCompiler.Compile(IEnumerable<IRuleDefinition>)` never required Roslyn — verified against the
  pinned 1.0.2 source), and a narrow AST-to-`Expression`-tree translator can only ever emit the
  operations it explicitly implements, which is a tighter trust boundary for the least-trusted tier
  in the stack than assembling untrusted content into C# source and handing it to a general compiler.

Because a GM Overlay author is the least trusted contributor in the stack short of a Player, the same
standing-guard injection that protects a C# `[StandingRule]` has to apply automatically here too —
`scripting-content.md`'s script-driven rule adapter already names this
(`Not<ExitGameMarker>`-style guards injected at parse time, "so a world-builder's script can't
accidentally bypass an engine guarantee simply because it wasn't written in C#"). A GM Overlay rule
gets the same treatment; nothing about being GM-authored rather than world-builder-authored earns an
exemption.

## Ruling: Phase 4's `StartCombat` example is GM-Overlay territory

`scripting-content.md`'s `when`/`then` example —

```yaml
- name: StartCombat
  when:
    - player.state == "ready"
    - area.hasEnemies == true
  then:
    - set session.inCombat = true
    - insert Event("CombatStarted")
```

— mutates live session state and inserts a live event. That's not World Module content (which
populates a fixed data shape, once, before the session assembles) and it's not ordinary Rulebook
content either (it's not part of the reviewable, compiled ruleset shipped with the game). It's a
rule a GM introduces or triggers during play. **This example belongs at the GM-Overlay tier.**

This resolves the open call in 0064 point 4: the rule-DSL pipeline Phase 4 builds
(`IRuleParser`/`IRuleCompiler` for the script format, `RuleRuntimeLoader`/`RuleAdapter`) is the GM
Overlay tier's mechanism, not a free-floating capability with no tier owner. The `IScript` capability
type Phase 4 introduces (`Capability.ContractType` value, per [ticket
0024](../ClaudeWiki/Processes/Handoffs/0024-phase-2-6-5-scope.md)) should be understood as tagging
GM-Overlay-tier participation specifically.

## Ruling: GM Overlay does not collapse to Lua-only the way World Module did

`world-module.md`'s refinement (2026-08-03) collapsed World Module's JSON/Lua split into Lua-only,
because a Lua table literal is a structural superset of JSON and World Module's whole job is
producing **data** that matches a known shape. It is tempting to reach for the same move here — reuse
`LunaFlow.Scripting`'s sandboxed Lua host, skip a bespoke DSL entirely. **That precedent doesn't
transfer, and applying it anyway would be building the general thing on the strength of one
example — from the wrong example.**

The two problems aren't the same shape. World Module marshals an opaque table into properties a
Rulebook already declared — there's no LHS/RHS to extract, just field-by-field assignment. A GM
Overlay rule has to decompose into `RuleIR`'s actual shape: `ConditionCode` and `ActionCode` as
separate slots, because that's what lets the pipeline inject a standing guard into the condition
side without touching the action side, and what lets the generator emit an ordinary NRules
`When()`/`Then()` block downstream (`rule-pipeline.md`'s IR section). A raw Lua chunk is an
undifferentiated script — nothing marks where "the condition" ends and "the action" begins short of
parsing Lua's own control flow, which reopens exactly the structural question the declarative
`when`/`then` shape exists to avoid answering generically.

**Ruling:** the top-level rule structure stays a declarative `when`/`then` shape (format — YAML vs.
something else — still `scripting-content.md`'s open question, not resolved by this doc).

**Expression grammar, ruled 2026-08-09 (direct corollary of ticket 0073's compile-mechanism ruling
above — not a separate decision):** custom, not Lua. The condition/action bodies
(`player.state == "ready"`, `set session.inCombat = true`) get parsed into a small, purpose-built AST
and translated directly to `System.Linq.Expressions`, the same mechanism the rule structure itself now
compiles through. Reaching for MoonSharp here would reintroduce a general embedded interpreter —
broader trust surface, per-evaluation interpretation cost — for exactly the case a narrow,
explicit-operations-only translator was chosen to avoid that for one level up. This was a live open
question until 0073 forced the answer; noted here so it doesn't read as independently re-litigated.

This also answers `scripting-content.md`'s second open question ("are declarative `when`/`then` and
Lua two separate authoring paths, or does the declarative format compile down to generated Lua/C#
glue?") for the GM-Overlay case specifically, now definitively rather than provisionally: they are not
two competing top-level paths, and Lua isn't the answer for either layer. The declarative shape is
structural and stays; the expression grammar inside it is the small custom grammar ruled above, not
Lua living inside it as an evaluator — that possibility, left open in this doc's first pass, is closed.

## Placement in the tier stack, and what stays deferred

- `EngineTier.GmOverlay` (already shipped, value 3) is the tag; nothing is registered under it yet.
  Whether a GM Overlay ever becomes an `IHasDependencies` component with declared `Capability`
  requirements is not decided by this doc — same as World Module at 2.8, there is exactly one kind
  of consumer in view (a script compiled into the live session) and no peer-correlation problem yet
  that would justify `ComponentRegistry` machinery. *Revisit when a GM Overlay rule needs to declare
  a dependency on something a specific ROM or Rulebook provides, not preemptively.*
- **Push-down-never-pull-up still holds.** A GM Overlay rule may read/insert facts the same way any
  NRules rule does, but it does not get a lookup API into World Module or Rulebook internals it
  wasn't handed — same trust-gradient rule `engine-core.md` states for every tier above the core.
- **No validation-pass analog to `OneD20WorldModuleValidator` is specified here.** World Module has
  one because there's a fixed shape to check content against before a session assembles. A GM
  Overlay rule's "validation" is closer to the standing-guard injection described above (an engine
  invariant it cannot violate) than to a content-shape check — whether anything more is needed is a
  Phase 4 spec question, not ruled here.
- **Hot-reload / mid-session application is GM Overlay's defining case, not an edge case** — a GM
  applies an overlay *because* something needs to change mid-session. See "How reload actually
  works" below for the mechanism, checked against NRules source rather than assumed.

## How reload actually works (verified against NRules 1.0.2 source)

**Finding, checked against `NRules`/`NRules.Fluent` source and API docs at the pinned version
(1.0.2 — `LunaFlow.Engine.csproj`/`LunaFlow.Rules.csproj`), per `CLAUDE.md`'s "upstream source before
inference":** neither `ISession` nor `ISessionFactory` exposes any operation to add, remove, or
replace a rule after a session (or its factory) has been created. `ISession`'s only mutating
operations are on **facts** (`Insert`/`Update`/`Retract`, their `Try`/`All` variants) and `Fire()`;
`ISessionFactory.CreateSession()` takes no rule-set argument, because the rule set was already baked
in when `RuleCompiler.Compile(IEnumerable<IRuleDefinition>)` (or the `IEnumerable<IRuleSet>` overload)
built the factory. NRules' own remark on `ISessionFactory` is blunt about the cost model this implies:
"session factory is expensive to create... there needs to be only a single instance... for the
lifetime of the application" — the Rete network is fixed at compile time, full stop.

**"Reload" therefore cannot mean "patch the running network."** It can only mean: recompile a new
`ISessionFactory` from the complete current rule-definition set (Rulebook's rules plus every
currently-active GM Overlay's rules — `RuleCompiler.Compile` takes the whole collection each time,
there's no incremental "add one more rule" call), create a new `ISession` from it, replay the old
session's current facts into the new one (`Query<object>()` off the old session, `InsertAll` into the
new one), and swap the engine's reference to "the current session" from old to new. The old session is
discarded.

**Consequence worth stating plainly: Phase 4's two-tier framing ("restart-level now, a live-reload
version maybe later") likely collapses into one tier.** There is no NRules-native mechanism that would
make a future "live" version structurally different from a well-executed restart — both are the same
recompile-and-replay operation; the only question is how much of it is hidden from the GM. Given a
tabletop session's fact count (dozens, not millions), a full replay should be cheap in wall-clock
terms even though NRules' own docs flag compilation as the "expensive" step — that's an assumption to
confirm empirically (a timing acceptance criterion for Phase 4's spec), not assert here, but there's
no reason to expect a hypothetical later "live" version to look different from this one.

### What survives the swap and what doesn't

- **Facts survive** — that's the point of the replay step, and it's the state a GM actually cares
  about (current HP, position, inventory, flags).
- **Agenda does not survive, and doesn't need to** — pending rule matches waiting to fire are
  recomputed from scratch against the new (superset) rule set once facts are re-inserted; nothing is
  lost that `Fire()` wouldn't have recomputed anyway between GM commands.
- **Anything set directly on the old `ISessionFactory`/`ISession`** — `ActionInterceptor`,
  `DependencyResolver`, `Events` subscriptions — has to be re-attached to the new factory/session by
  whatever owns the swap. Ordinary composition-root bookkeeping, but a concrete checklist Baraka's
  spec needs, not an afterthought.
- **The swap must happen between `Fire()` cycles, never during one.** `Fire()` blocks until nothing
  more matches; recompiling mid-cycle isn't coherent. Applying (or reverting) a GM Overlay is a
  distinct engine-level operation, gated to run only when the engine isn't mid-resolution — a
  scheduling constraint on wherever this lives in `GameEngineCore`, not a new concept.

### Applying and reverting an overlay are the same primitive

Because there's no incremental rule-set mutation, "add an overlay" and "remove/revert a previously
applied one" aren't two mechanisms — both are "recompile from a target rule-definition set, replay
facts, swap." The only difference is whether the target set includes the overlay's rule definition or
not. This simplifies scope: whatever owns the reload operation needs one operation (something like
`ReloadWith(IEnumerable<RuleDefinition> activeOverlays)` — Baraka's naming call), not a separate
add/remove pair. An overlay that expires on its own (a GM's "just for this encounter") is a
content-authoring concern layered on top of this primitive — a marker fact plus a turn-count guard,
the same convention `[StandingRule]` guards already use — not a second reload mechanism.

**That convention is now verified, not just assumed.** When this was first written, "the same
convention `[StandingRule]` guards already use" was an untested extrapolation — nothing had ever made
a marker survive past the scope that created it. Phase 2.10 item 3 (Poisoned — a status marker
inflicted mid-fight, required to persist across a room transition and keep applying a penalty until it
expires) shipped and passed QA specifically to test whether the Marker/`StandingRule` suppression
pattern holds across a boundary it had never crossed. It does — confirmed end-to-end, not just
unit-tested (see [`phase-2-10.md`](../Roadmap/phase-2-10.md)). The auto-expiry design above rests on
solid ground now, not an extrapolation from a pattern that had only ever operated within a single
`CombatRound`.

### Where this lives, and what it doesn't decide

- **The swap is `LunaFlow.Engine`-tier machinery, not GM-Overlay-authored code.** A GM Overlay script
  supplies a rule definition; recompiling and swapping the live session is core engine composition,
  same trust tier as everything else in `GameEngineCore` — consistent with push-down/never-pull-up:
  the risky operation (rebuilding the running session) is core-trusted code acting *on behalf of* a
  request, not a capability exposed to script-level trust.
- **Not decided here:** exactly where in `GameEngineCore` "the current session" becomes a swappable
  reference rather than whatever it is today (a Phase 4 spec question — this doc doesn't touch
  `src/`), the exact `IRuleParser`/`IRuleCompiler` wiring that turns a GM's script into an
  `IRuleDefinition` the reload primitive consumes, and the empirical cost of a full replay at
  realistic fact counts (flagged above as something to measure, not assume).

This resolves `scripting-content.md`'s hot-reload open question **at the mechanism level** — what a
reload structurally *is* — while leaving the concrete API shape and its measured cost for Phase 4's
buildable spec.

## Two-network split — considered, not adopted (kept for lineage)

> ⚠️ **CONSIDERED AND NOT ADOPTED, ruled 2026-08-09.** This section is a 2026-08-03 draft, kept as
> written rather than deleted — same "rejected design, kept for lineage" convention `glossary.md`
> uses. Nothing in this repo should be built against it. See
> ["Ruled: single session, not two"](#ruled-single-session-not-two) at the end of this section for
> why, and what would legitimately bring it back.

### The problem this is trying to solve

The "How reload actually works" ruling above is still correct as far as it goes: NRules has no API
to mutate a running session's rule set, so any reload is recompile-a-factory-and-replay-facts. But it
inherited an assumption from "Boundary against Rulebook" that turns out to be load-bearing and wrong:
that GM Overlay rules compile into the *same* session as the Rulebook. Two problems follow from that
assumption, surfaced in conversation rather than by this doc's original pass:

1. **Isolation.** A rule's RHS actions have full `ISession` reach once compiled into a session,
   independent of whatever LHS guard gated whether it fired. Standing-guard injection stops an
   overlay rule from firing when a marker says it shouldn't — it does nothing to stop the rule's
   *actions* from retracting or inserting facts the guard never mentioned, once it does fire. For the
   least-trusted author in the stack short of Player, that's a real gap, not a theoretical one.
2. **Cost.** "Recompile and replay" against the same session as the Rulebook means replaying *every*
   fact in the game's current state on every overlay edit, regardless of how much of that state the
   overlay rule actually cares about — the replay cost grows with total session history, not with
   overlay complexity.

### The candidate shape

Two separate `ISessionFactory`/`ISession` pairs instead of one:

- **Main session** — Rulebook + ROM, compiled once at bootstrap via `RuleCompiler`, the same as
  today. Nothing above the GM-Overlay tier ever asks this to recompile.
- **Overlay session** — a small, separate session holding only the GM's currently-active overlay
  rules, compiled and recompiled freely because it's cheap: its own rule count and its own working
  memory both stay small by construction, not by hope.

Neither session is ever handed to the other or to anything at GM-Overlay trust level. Only
`GameEngineCore` (or whatever ends up owning this — an engine-core question, not decided here) holds
both, and it is the sole channel between them, in both directions:

- **Main → overlay**, before the overlay session fires: `GameEngineCore` mirrors in whatever subset
  of main-session facts the overlay's own rule conditions reference (`player.state`,
  `area.hasEnemies` in the `StartCombat` example) — not the whole game state, just what's declared as
  relevant.
- **Overlay → main**, after the overlay session fires: the overlay session's actions produce **effect
  facts** — a small, closed vocabulary the Rulebook declares ahead of time (a `MonsterBehaviorOverride`,
  a `StatModifier`, a `ForceRoomTransition` — names TBD, Baraka/Rulebook-author's call), which
  `GameEngineCore` copies into the main session as ordinary facts. Main-session rules that already
  know to check for a given effect type (a join against an optional override fact, the same shape as
  any other NRules pattern) act on it; rules that don't check for it are unaffected.

This makes the trust boundary structural rather than conventional: an overlay rule's blast radius is
bounded by its own session's working memory plus whatever narrow effect-fact vocabulary the Rulebook
chose to expose, not by "whatever `ISession` lets you do." It also makes the reload cost bounded:
replay-on-recompile only ever touches the overlay session's own small, curated fact set, never the
main game's accumulated state — the empirical-cost question flagged in "How reload actually works"
above stops mattering for the part that could actually get expensive.

### What this does and doesn't let a GM do

The concrete test case that surfaced this: "these goblins shape-shift and need to act like
elephants." That example is doing real work, not just illustrating — it's the case that distinguishes
"GM sets a flag" from "GM improvises actual decision logic," and the distinction matters for whether a
second session is needed at all:

- **A static toggle** ("Goblin 3 is unkillable") doesn't need any of this — an `Invulnerable(id)`
  effect fact, inserted once, that main-session combat rules already check for, is enough. No overlay
  session required for a one-shot flag.
- **Improvised conditional behavior** ("if damaged below half, flee") is a rule — a LHS/RHS pair that
  has to live somewhere capable of matching conditions and firing actions. That's what the overlay
  session is actually for: not a data pipe, a small place for the GM's own reactive logic to run,
  whose output becomes effect facts the same way a one-shot toggle would.

**Real evidence now exists on one half of this, and it's worth being precise about which half.**
Phase 2.12 item 11 (Enrage — a threshold-triggered Defense boost, the first extension to
`IOneD20WorldModule`'s fixed shape since Phase 2.8) shipped and passed QA; the finding was routed
here directly (ticket [0087](../ClaudeWiki/Processes/Handoffs/0087-phase-2-12-item-11-finding-for-gm-overlay-draft.md)).
**What it proves:** extending a Rulebook-owned content shape with a conditional, threshold-triggered
stat modifier is cheap and needs zero `ComponentRegistry`/`Capability`/`EngineTier` involvement — real
evidence for the "rebind or toggle a pre-declared extension point" half of this section's claim.
**What it does not prove, in Lothar's own words, worth keeping verbatim:** "extending the monster
shape is cheap" is proven; "the runtime-composable effect mechanism is proven" is not. Enrage is
authored ahead of time, in content, by whoever writes the World Module — threshold and bonus fixed at
Lua-authoring time, interpreted by a rule hand-written for exactly that one field pair. This section's
actual target — a GM improvising a stat change mid-session via a runtime-declared, *generically*
interpreted effect fact, not one bespoke rule per concept — is a fundamentally different shape that
Enrage says nothing about either way. Don't read Enrage as de-risking the effect-fact idea; it only
de-risks the much narrower claim that `IOneD20WorldModule` itself can grow without framework cost.

**The hard ceiling, stated plainly so it doesn't get oversold later:** an overlay can rebind or toggle
whatever extension points the Rulebook already declared as override-able. It cannot introduce a fact
type or behavior category the compiled main network has zero rules watching for — NRules resolves
match types at compile time, and the main network is deliberately not being recompiled anymore under
this design. A literal `Elephant` type nothing in `OneD20` was ever written to match against doesn't
become real because an overlay script mentions it. "Goblins act like elephants" only works if the
Rulebook shipped an override surface general enough to express elephant-like behavior as a
*parameterization* of something it already understands (a behavior-template override, a stat-block
swap) — that's a real authoring commitment on the Rulebook side this mechanism doesn't grant for
free, and whether `OneD20` (or any Rulebook) is willing to make that commitment is exactly the kind of
question a Phase 4 kickoff pass would need to work through, not this doc.

### The real axis is volatility, not trust

**Added 2026-08-03, same conversation.** Stress-testing this shape against a hypothetical — a ROM
that procedurally generates a large rule set (chemistry-simulation-scale, not Nine-Rooms-scale) —
surfaced a correction to how this section frames the split. GM Overlay needed its own session for two
independent reasons: it's **untrusted** (unreviewed, least-trusted author short of Player) and it's
**volatile** (its rule set changes during play). This draft was written as if isolation follows from
trust. It doesn't — it follows from volatility, and trust is only *one* reason a rule source can be
volatile.

A procedurally-generating ROM is the counter-example that makes this visible: fully trusted,
first-party C#, reviewed the normal way — and yet, if it regenerates its rule set during play rather
than once at bootstrap, it hits the exact same recompile-cost wall GM Overlay hits, for a completely
unrelated reason. Bundling a volatile-but-trusted rule source into the same session as the stable core
would reproduce the cost problem this split exists to solve, even though it wouldn't reproduce the
isolation problem (a trusted ROM's actions don't need to be sandboxed from the rest of the engine the
way a GM's script does).

**So: two independent questions, not one, whenever a rule source is being placed.** Does it need
**isolation** (is it untrusted)? Does it need **cost containment** (does its rule set change during
play, and how large is it)? GM Overlay answers yes to both, which is why one session covers both
needs for it specifically — that's a coincidence of this particular tier, not evidence the two
questions are the same question. A future rule source could need only one, or neither, and should be
sized accordingly rather than assumed to need "the GM Overlay treatment" wholesale.

This doesn't add a third tier to this draft — there's no real consumer proposing rule-count-at-scale
today, and building one on the strength of a hypothetical is exactly the mistake `non-goals.md`'s
discipline exists to catch (see its "Not building rule-scale guardrails yet" entry, pinned from the
same conversation, for where the concrete guardrails against this landed). What changes is how this
draft should be read if a second real case ever does show up: as a session boundary motivated by
volatility with trust as one contributing factor, not a session boundary *for* the trust gradient that
volatility happens to ride along with.

### What's still completely open, even if this shape is adopted

- Whether `GameEngineCore` is really the right owner of both sessions and the mirror/effect-fact pump,
  or whether that's its own small component — not decided, not even sketched.
- The effect-fact vocabulary itself — this doc names three illustrative examples, not a real list.
  Designing that vocabulary is close to designing what "GM improv" is allowed to mean for a given
  Rulebook, which is real content-design-adjacent work, not a one-line addition.
- Identity and mutation semantics for mirrored facts — same CLR object reference in both sessions'
  working memories vs. a translated copy, and who's responsible for calling `Update()` on which
  session when the underlying data changes. Not even a candidate answer yet.
- Timing: when exactly the mirror-in/fire/mirror-out cycle runs relative to `OnTurnAdvance` and the
  main session's own `Fire()` — sequenced before, after, interleaved, gated on GM action rather than
  every turn. Not decided.
- Whether this is worth the added machinery at all versus a simpler same-session model with a
  narrower RHS-action safety mechanism instead (e.g., an interceptor limiting what an overlay rule's
  actions may do, rather than a whole second Rete network) — that alternative hasn't been evaluated
  against this one and might turn out to be cheaper for the isolation problem specifically, even if it
  doesn't help the cost problem.

### Ruled: single session, not two

**2026-08-09, following directly from ticket 0073's compile-mechanism ruling above.** The last open
item in the list just above — "versus a simpler same-session model" — turned out to be the answer,
but not for the reason it was floated as a maybe. Checked `IActionInterceptor` against NRules source
(`Intercept(IContext context, IReadOnlyCollection<IActionInvocation> actions)` —
`context` carries "information about the firing rule," confirmed against
[the API source](https://nrules.net/api/NRules.Extensibility.IActionInterceptor.html)) expecting it
might be the fix. It isn't the load-bearing one — something else already ruled today is.

**The isolation problem this section opened with is already closed, by a different ruling, made the
same day.** The isolation gap — "a rule's RHS actions have full `ISession` reach once compiled,
independent of what LHS guards gated" — was true under the assumption GM-Overlay content compiles the
way Rulebook content does: opaque C# text handed to a general compiler, capable of expressing
anything. Ticket 0073 ruled that assumption out for the runtime path specifically: GM-Overlay rules
(both structure and expression grammar) are built through a narrow, purpose-built AST-to-`Expression`
translator that **can only ever emit the operations it explicitly implements.** A GM-Overlay rule
cannot construct a call to retract an arbitrary Rulebook-internal fact, because nothing in a closed
grammar gives it the vocabulary to say so — not because a session boundary stopped it, because the
capability to express it was never compiled into existence in the first place. That's a stronger
guarantee than session isolation gives, not a weaker one: a second session bounds *where* a
misbehaving action can reach; a closed grammar prevents the action from being constructible at all.

**What residual risk this leaves, and where it actually lives now:** whether the grammar's own
primitive operations (`set <field> = <value>`, `insert Event(<name>)`, eventually an effect-fact
insertion) are themselves scoped to a reviewed, closed allowlist — a field or fact type the grammar
can target has to be one the Rulebook explicitly declared settable/insertable, the same discipline the
still-useful "effect fact" vocabulary idea above already wanted. That's a grammar-design and
Rulebook-authoring review question now, not a session-architecture one. `IActionInterceptor` remains a
real, available, source-verified mechanism if a belt-and-suspenders runtime audit is ever wanted later
— it can distinguish which rule is firing via `IContext` and choose not to invoke an action — but
nothing in this ruling requires it; the grammar closure is what's actually load-bearing.

**What the two-network draft's other rationale (cost containment) still means:** unaffected by the
above, and still real, but ["The real axis is volatility, not trust"](#the-real-axis-is-volatility-not-trust)
already correctly identified that this is a scale problem, not a trust problem — and at GM-Overlay's
actual scale (a tabletop session's fact count, dozens not millions), full-session replay is expected
to be cheap, per "How reload actually works" above. That's still an assumption to confirm empirically
in Phase 4's spec, not proven here, but it's the same assumption either way, single- or two-session —
a second session only pays for itself once total fact count is large enough for full replay to
actually hurt, which nothing in view today (GM-Overlay as scoped) is close to.

**Ruling:** GM-Overlay rules compile into and fire inside the **same session** as the Rulebook,
exactly as "How reload actually works" already described (recompile the complete current rule set —
Rulebook plus every active overlay — replay facts, swap) — that section needed no correction, only
"Boundary against Rulebook" did, twice, and this is the second and final correction. The two-network
split stays on record above, not deleted, and stays the right answer **if** a genuinely volatile,
large-scale rule source shows up — the hypothetical procedural-ROM case `non-goals.md`'s "Not building
rule-scale guardrails yet" entry already names — where cost containment, not isolation, would be the
actual driver. Not GM-Overlay, not today.

## What this doc does not settle

- The script format itself (YAML vs. something else) — still `scripting-content.md`'s open question.
- ~~The expression grammar for condition/action bodies~~ — **ruled 2026-08-09**: custom, translated
  to `System.Linq.Expressions`, per ticket 0073's corollary above.
- ~~The compile mechanism for the runtime path~~ — **ruled 2026-08-09**: `RuleBuilder` + `Expression`,
  not Roslyn, per ticket 0073 (`rule-pipeline-compilation-cost.md`).
- The concrete reload-primitive API shape and its `GameEngineCore` integration point — mechanism
  (recompile-and-replay) ruled, now with a ruled compiler target too; exact API shape still left to a
  Phase 4 spec.
- Any concrete GM-facing authoring surface (a live console command, a text box, a hot-key) — that's
  interface-layer/tooling work, not this tier's contract.
- Whether GM Overlay ever becomes a registered, dependency-declaring component — deferred per above.
- ~~Whether the two-network split happens at all~~ — **ruled 2026-08-09**: no, single session for v1.
  See ["Ruled: single session, not two"](#ruled-single-session-not-two). The draft stays on record as
  the answer if a genuinely volatile, large-scale rule source ever shows up — not GM-Overlay as
  scoped.
- **The grammar's own allowlist** — which fields a `set` operation may target and which fact types an
  `insert` operation may construct, reviewed and closed per Rulebook. This is the real safety surface
  now that session isolation isn't carrying that weight; not designed yet, just named as where the
  design work actually needs to go.

These, plus the rule-DSL format ruling and the grammar allowlist, are what a Phase 4 kickoff spec
still needs before Baraka has something buildable — narrower than before this doc (the tier question,
the reload mechanism, the compile mechanism, the expression grammar, and the session-architecture
question are all answered now), not fully closed.

## Related documents

- [`architecture.md`](architecture.md#quick-reference) — the four-tier diagram naming this tier as a
  sibling of World Module
- [`world-module.md`](world-module.md) — the sibling tier this doc contrasts against throughout, and
  the precedent (Lua-collapse) this doc declines to reuse
- [`scripting-content.md`](scripting-content.md) — the script-driven rule adapter mechanism this tier
  owns, and the open questions this doc narrows
- [`rule-pipeline.md`](rule-pipeline.md) — `RuleIR`'s `ConditionCode`/`ActionCode` shape and the
  compile-time-vs-runtime compiler-target distinction this doc leans on
- [`engine-core.md`](engine-core.md) — `EngineTier`, and the push-down/never-pull-up trust rule
- [`non-goals.md`](non-goals.md) — the "don't generalize past one real consumer" discipline this doc
  applies when declining the Lua-collapse precedent
- [`../Roadmap/phase-4.md`](../Roadmap/phase-4.md) — the phase this document is kickoff work for
- [`../ClaudeWiki/Processes/Handoffs/0064-phase-2-9-ready-and-phase-4-scope-correction.md`](../ClaudeWiki/Processes/Handoffs/0064-phase-2-9-ready-and-phase-4-scope-correction.md) —
  the ticket that surfaced the tier question this doc answers
- [NRules API docs](https://nrules.net/api/NRules.ISession.html) (`ISession`),
  [`ISessionFactory`](https://nrules.net/api/NRules.ISessionFactory.html),
  [`RuleCompiler`](https://nrules.net/api/NRules.RuleCompiler.html) — the upstream source "How reload
  actually works" is checked against, at NRules 1.0.2, the version pinned in `LunaFlow.Engine.csproj`
  and `LunaFlow.Rules.csproj`
- [`rule-pipeline-compilation-cost.md`](rule-pipeline-compilation-cost.md) and [ticket
  0073](../ClaudeWiki/Processes/Handoffs/0073-rule-compilation-cost-and-runtime-mode.md) — the
  compile-mechanism ruling (`RuleBuilder`+`Expression`, not Roslyn) this doc's "Boundary against
  Rulebook" and expression-grammar ruling build on
- [Ticket 0087](../ClaudeWiki/Processes/Handoffs/0087-phase-2-12-item-11-finding-for-gm-overlay-draft.md) —
  the Enrage finding folded into "What this does and doesn't let a GM do" above
- [`../Roadmap/phase-2-10.md`](../Roadmap/phase-2-10.md) — item 3 (Poisoned), verifying the
  marker-across-a-boundary assumption this doc's overlay-expiry design rests on
- [`../Roadmap/phase-2-12.md`](../Roadmap/phase-2-12.md) — Enrage's full build/QA record
- [`IActionInterceptor`](https://nrules.net/api/NRules.Extensibility.IActionInterceptor.html) — checked
  against source while ruling out the two-network split; a real, available defense-in-depth mechanism,
  not required by the final ruling
