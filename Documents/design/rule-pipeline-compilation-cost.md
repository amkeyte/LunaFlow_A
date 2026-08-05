# Rule-Compilation Cost & a Runtime Path That Isn't Roslyn

Implementation status: see [Roadmap](../Roadmap/roadmap.md).

> ⚠️ **FINDINGS + CANDIDATE OPTIONS — NOT RATIFIED.** Everything below is Baraka's writeup for
> Amara to rule on (ticket 0073), not a decision. Nothing in this repo depends on it yet. Don't
> build against any option below until it's ruled — same posture `gm-overlay.md`'s own draft
> section holds.

## Purpose

`GeneratorServices` compiles every Rulebook rule group through Roslyn (`RoslynRuleCompiler`,
in-memory `CSharpCompilation`), once per `[RuleGroup]`. Instrumenting `MyLunaFlowServer`'s startup
(ticket-adjacent work, 2026-08-04) surfaced a real number: for 1.d20's three rule groups (19 rules
total), Roslyn compilation costs **~800ms**; the actual NRules Rete-network build it feeds into costs
**~58ms**. Roslyn is roughly 14x the cost of the mechanism it exists to feed.

That's tolerable as a one-time startup cost for trusted, first-party Rulebook content. It is **not**
tolerable for anything that recompiles live, during play — and `gm-overlay.md`'s own source-verified
finding is that NRules has no incremental rule-mutation API at all: **every GM Overlay apply/revert
requires a full recompile of the complete current rule-definition set**, not just the overlay's own
rules. Whatever mechanism ends up compiling scripted/GM-Overlay content for Phase 4, running a
~800ms-class operation on every live edit is a non-starter — and if a future volatile rule source
(the hypothetical procedural-ROM case `non-goals.md` already names) ever needed something closer to
per-turn regeneration, the current mechanism would be worse, not better, at that scale. This doc
exists to lay out what the alternatives actually are, grounded in what NRules itself supports (verified
against the pinned 1.0.2 source, not assumed) rather than in the abstract.

## What was measured

`GameEngineCore.Start()` and `MyLunaFlowServer/Program.cs`'s `RunGame()` were instrumented with
`Stopwatch`s around three distinct phases, run against the real shipped 1.d20 content
(`DefaultOneD20WorldModule`, 3 ROMs, 19 rules across `GridMapRuleGroup`/`D20SystemRuleGroup`/
`LootRuleGroup`):

| Phase | What it does | Measured |
| --- | --- | --- |
| Bootstrap | ROM/Rulebook construction, `ComponentRegistry.Validate()`, §5.3/§5.4 probes | 17ms |
| Session setup (Roslyn) | 3x `RuleExport.Compile(Type)` — parse `[RuleTemplate]`s → generate C# source → `CSharpCompilation.Emit()` → `Assembly.Load()` | ~800ms |
| Rete compile (NRules) | `RuleRepository.Load(...)`/`Compile()` → `RuleCompiler.Compile(IEnumerable<IRuleDefinition>)` → `CreateSession()` | 58ms |
| Initial settle | `Rules.Fire()` against seeded facts | 1ms |

Roslyn dominates. It is also the only phase whose cost scales with something other than actual rule
complexity — see next section.

## Why it costs what it costs

Two compounding, independently-verified causes:

1. **One full Roslyn invocation per rule group, by a fork decision made during Phase 2's first
   implementation pass (ticket [0008](../ClaudeWiki/Processes/Handoffs/0008-rule-pipeline-implementation.md)),
   not by anything ticket 0007 ruled on.** `RuleParseContext.FromAssembly` can collect several
   `[RuleGroup]` types, but `IRuleParser.Parse` was built to throw if given more than one — "one
   group per pipeline run, explicitly," per `rule-pipeline.md`. `RuleExport.CompileAll(Assembly)`,
   the multi-group entry point this forces, still calls `Compile()` (one full Roslyn pass) once per
   group internally — it automates the loop, it doesn't reduce the invocation count. Every rule group
   (every ROM, eventually every GM Overlay) adds one more full compiler invocation. This was a solo
   implementation-time call, flagged inline to Kofi in conversation, never routed to Amara as its own
   design ticket — it has stood unquestioned since 2026-07-17.
2. **Each invocation references every loaded, file-backed assembly in the AppDomain, not a scoped
   set.** `rule-pipeline.md`'s own "Known limitations (MVP)" section names this directly: "a
   pragmatic stand-in, not real reference tracking... the fix is giving `RuleCodeUnit`/
   `RuleParseContext` an explicit slot for reference assemblies rather than scanning the AppDomain,
   but that's speculative scope for a problem nothing has hit yet." Something has now hit it: as the
   solution grows (more ROMs, more NuGet dependencies), every one of the group-scoped Roslyn
   invocations above has to resolve a larger reference set, so per-invocation cost climbs
   independently of rule count too.

Neither cause is a Roslyn limitation. Both are choices LunaFlow's own pipeline made, on top of an
NRules dependency that — see below — never required Roslyn in the first place.

## Why Roslyn was chosen at all (reconstructed from the record)

Worth stating plainly so the options below don't read as second-guessing a mistake — the original
call was reasonable for the problem it was solving, which was narrower than "compile a rule":

- **The generator's actual purpose was authoring-surface unification, not compilation.**
  `rule-pipeline.md`'s Purpose: Rulebook authors write C#; world-builders/GM tooling need to add rules
  "without a C# compile step"; both should produce the same thing so "a validator, a test harness, or
  a debugger built for one mode works for the other for free." NRules already turns rule definitions
  into a Rete network on its own; the generator's job was giving every authoring surface one shared IR.
- **In-memory Roslyn specifically was a testability constraint.** Ticket 0007's D2, verbatim: "the
  done-bar (B4) says the generated rule 'loads and fires' — you can't load-and-fire a `.cs` file
  written to disk within the same test run." Emit-to-disk-then-external-build couldn't satisfy
  same-run provability; in-memory `CSharpCompilation.Emit()` → `Assembly.Load()` could. This was a
  2026-07-17 testability call, made before GM Overlay had a concrete design (`gm-overlay.md` is dated
  2026-08-03) — nobody was weighing Roslyn's cost against a live-reload use case, because that use
  case didn't exist in the design yet.
- **Opaque C# source text (not a structured condition/action model) was already settled before ticket
  0007**, which explicitly declines to reopen it: "`ConditionCode`/`ActionCode` stay opaque strings —
  this doesn't reopen the already-settled 'no structured expression tree' call." `rule-pipeline.md`
  notes this "supersed[ed] an earlier, more structured `RuleModel`-with-`Condition`/`Action`-object
  sketch from planning." Reasoning inferred, not cited: opaque strings keep the generator near-trivial
  (string templating) while giving a Rulebook author unrestricted C# inside `When()`/`Then()` — any
  ROM mechanic call, any LINQ — rather than boxing them into whatever a hand-built expression model
  could express, for a single real consumer at the time.
- **Reuse.** The compiled output loads through the exact path `RuleEngine.LoadRulesFromAssembly`
  already used for Nine Rooms' hand-written rules — zero new loading machinery needed.

None of these reasons argue for Roslyn on a *live-reloaded* path. They argue for it on a
*compile-once-at-startup, fully-trusted, wants-arbitrary-C#* path — which is what Rulebook authoring
still is today.

[Arryn] A question I have is... if a Rulebook is C#, it shouldn't need to be generated? At some point we were
creating C# content for the purpose of testing the generator. We should vberify that ROM/Rulebook
actually needs generation at all. If so, it would be for procedurally generated rules, which a) aren't the
normal case and b) don't warrant the use of generation as a go-to processing method. Therefore
based on what we're finding we might still want to support the generator, but only for specific use cases
and not the go-to pathway for adding content. But I don't want to simply remove it - I have a fear
that this is something that came up in my pre-Claude design time as a solution to a potentially
hypothetical problem.
Also, we currently aren't making use of the IMechanic concept, which may be why I did this originally.\
Needs explored.

## What NRules actually requires (verified against the pinned 1.0.2 source)

Reflected directly over `NRules.RuleModel.dll`/`NRules.dll` at the version pinned in
`LunaFlow.Engine.csproj`/`LunaFlow.Rules.csproj` (`nuget.org` package cache, not docs, not memory):

```
NRules.RuleModel.Builders.PatternBuilder.Condition(System.Linq.Expressions.LambdaExpression)
NRules.RuleModel.Builders.ActionGroupBuilder.Action(System.Linq.Expressions.LambdaExpression, ActionTrigger)
NRules.RuleModel.Builders.GroupBuilder.Pattern(System.Type, System.String) -> PatternBuilder
NRules.RuleModel.Builders.RuleBuilder.LeftHandSide() -> GroupBuilder
NRules.RuleModel.Builders.RuleBuilder.RightHandSide() -> ActionGroupBuilder
NRules.RuleModel.Builders.RuleBuilder.Build() -> IRuleDefinition
NRules.RuleCompiler.Compile(IEnumerable<IRuleDefinition>) -> ISessionFactory
```

`RuleBuilder` takes conditions and actions as raw `System.Linq.Expressions.LambdaExpression`s, builds
an `IRuleDefinition`, and `RuleCompiler.Compile(IEnumerable<IRuleDefinition>)` turns that directly
into a session factory — no C# source text, no `Assembly`, no `Assembly.Load`, anywhere in that path.
`RuleRepository.Load(x => x.From(assembly)...)` — what `RuleEngine.LoadRulesFromAssembly` uses today
— is a convenience wrapper: it reflects `Rule` subclasses, whose `Define()` calls into this exact
same `RuleBuilder` API, and lands on this exact same `Compile(IEnumerable<IRuleDefinition>)` call.
The class-per-rule-plus-Roslyn step is entirely LunaFlow's own layer on top; NRules was never the
reason it exists.

One caveat, also verified: `PatternBuilder.Condition`/`ActionGroupBuilder.Action` take real
`LambdaExpression`s, meaning action bodies are subject to ordinary .NET expression-tree limits
(confirmed independently — ticket 0008's build log records a bug where `.Do(ctx => x = value)` failed
because bare assignment isn't legal inside an expression tree; the fix was a method call instead, "the
same restriction the hand-written Nine Rooms rules already respect"). Any runtime path — Roslyn-based
or Expression-tree-based — inherits this same shape constraint; it is not specific to either option
below.

`NRules.RuleCompiler` also exposes a swappable `IExpressionCompiler` (`ExpressionCompiler` property,
public getter/setter) — NRules' own compilation of a `LambdaExpression` into a runnable delegate is
itself a pluggable seam, separate from anything discussed here, not investigated further.

## Options

### A — Eliminate the Roslyn generator entirely; author everything (Rulebook and scripted) as data, built into `IRuleDefinition` via `RuleBuilder`

| Pros | Cons |
| --- | --- |
| One mechanism, one cost profile, everywhere — no "two paths" to maintain or keep in sync. | Rulebook authors lose arbitrary-C# expressiveness inside `When()`/`Then()` — every ROM-mechanic call, every LINQ expression, has to be re-expressible in whatever structured condition/action model replaces `RulePrototype`'s statement lists. |
| Removes the `Microsoft.CodeAnalysis.CSharp` dependency (flagged as real added cost at D2, 2026-07-17) entirely. | Building a structured model general enough for *today's* Rulebook content (not just future scripted content) is a substantial, speculative engineering investment — effectively reinventing a slice of what Roslyn already does well, to replace a cost (~800ms, paid once, by trusted first-party content at startup) that isn't actually the load-bearing problem. |
| Smallest total surface area long-term. | Directly reopens a call `rule-pipeline.md`/ticket 0007 already made deliberately ("no structured expression tree") for reasons that were sound for the compile-time case specifically. |
| | `RuleCodeGenerator`/`RoslynRuleCompiler`/`RuleExport`'s existing, tested, B1–B5-proven pipeline gets thrown away rather than narrowed. |

**Read:** solves a problem this path doesn't actually have (compile-time cost is a one-time,
acceptable tax) at the cost of rebuilding something Roslyn already provides well, for the tier that
most wants it.

### B — Keep the Roslyn generator for compile-time Rulebook authoring, unchanged; add a second, non-Roslyn runtime path for scripted/GM-Overlay content via `RuleBuilder` + `System.Linq.Expressions`

| Pros | Cons |
| --- | --- |
| Rulebook authoring is untouched — no regression, no re-litigating a settled call, no migration. | Two mechanisms to maintain, not one — `rule-pipeline.md`'s "one place that understands what a rule is" goal only holds at the IR level, not the compiler level (though this was already true in the doc's own "two modes, same IR, different compiler target" framing — this *finishes* that design rather than departing from it). |
| Directly matches what was already planned: `rule-pipeline.md`'s two-mode IR framing anticipated a distinct runtime `IRuleCompiler`; this just picks its actual mechanism. | Needs a structured condition/action representation for the *runtime* path specifically — new work, not free, though scoped to exactly what `gm-overlay.md` already ruled scripted content needs (see next row). |
| Scope is already bounded by an existing ruling: `gm-overlay.md` — "the top-level rule structure stays a declarative `when`/`then` shape... what Lua might still reasonably own is narrower: the expression language inside a condition or action line." Scripted content was never meant to need arbitrary C#, so the structured model this needs is smaller than what Option A would need to cover Rulebook content too. | Rulebook content and scripted content produce rules through genuinely different code paths now — a subtle behavioral difference (e.g. in how `[StandingRule]` guard injection works) could drift between them if not deliberately kept in parity. |
| **Trust/security, not just performance:** a narrow structured-AST-to-`Expression`-tree translator can only ever emit the operations it explicitly implements (comparisons, field access, boolean combinators). Assembling untrusted script content into literal C# source text and handing it to a full compiler — even in-memory — is a strictly larger attack surface for the least-trusted tier in the stack (`architecture.md`'s trust gradient; `cartridge.md`'s push-down-never-pull-up framing). This isn't a new requirement, just a reason B is a better fit than any option that keeps text-based codegen for scripted content specifically. | |
| Eliminates the ~800ms-class cost from exactly the place it's actually load-bearing (live reload), while leaving it where it's tolerable (startup). | |

**Read:** the option that maps most directly onto findings already on record — it doesn't invent a
new direction, it builds the runtime mode the pipeline's own two-mode design already sketched, using
the mechanism NRules itself actually exposes for it.

### C — Keep Roslyn everywhere; merge the three (eventually N) per-group compiles into one invocation

| Pros | Cons |
| --- | --- |
| Cheapest possible change — doesn't reopen D1–D4, D2, or the "no structured tree" ruling at all. | Doesn't touch the actual load-bearing problem. This only reduces *startup* cost (fewer fixed-overhead Roslyn invocations); it does nothing for GM Overlay's per-edit full-recompile cost, which is the more urgent concern per `gm-overlay.md`'s own finding. |
| Directly addresses cause #1 above (one invocation per group) without touching cause #2 (AppDomain-wide references) or the deeper "why Roslyn at all for scripted content" question. | Still reopens `IRuleParser.Parse`'s "exactly one `RuleIRGroup` per call" ruling from ticket 0008 — a real, if small, design change, not a config flag. |
| | Scripted/GM-Overlay content still has nowhere non-Roslyn to go — this doesn't unblock Phase 4 at all. |

**Read:** a real, low-risk improvement to *today's* startup number, but orthogonal to the question
that actually motivated this doc. Worth doing regardless of A/B/D/E, not a substitute for any of them.

### D — Keep Roslyn for both paths; use Roslyn *Scripting* (`Microsoft.CodeAnalysis.CSharp.Scripting`) instead of full `CSharpCompilation` for the runtime path

| Pros | Cons |
| --- | --- |
| Scripted content authors could write near-identical syntax to Rulebook authors (still C#-shaped), preserving `rule-pipeline.md`'s "one authoring feel" goal more literally than A/B. | Still invokes the Roslyn compiler frontend — parsing, binding, IL emission — for every script evaluated. Lighter than full assembly-emitting compilation for small snippets, but not free, and not verified against this project's own numbers. |
| No new structured condition/action model needed. | Same trust-surface concern as Option A/text-codegen generally: untrusted script content still becomes compiled C#, at the least-trusted tier in the stack. |
| | Doesn't obviously help the "full rule-set recompile on every overlay edit" cost `gm-overlay.md` found — that's an NRules-level constraint (no incremental `ISession` mutation), not something a different C#-to-delegate mechanism changes. |

**Read:** a plausible middle ground on authoring ergonomics, weaker than B on both performance and
trust grounds, and doesn't address the deeper NRules-level recompile constraint either way.

### E — Cache/pool compiled rule assemblies by content hash

| Pros | Cons |
| --- | --- |
| Helps the compile-time dev loop (repeated process restarts against unchanged Rulebook content) and any case where identical scripted content recurs. | Doesn't help a GM's live overlay edit — by definition novel content each time, so cache misses every time it would matter most. |
| Orthogonal to A–D; could layer on top of any of them. | Cache invalidation/versioning is its own small design problem (per-rule-group content hash, staleness against a changed World Module, etc.) — real but bounded scope. |

**Read:** worth doing regardless of what else is decided, but doesn't answer this doc's actual
question — it optimizes repetition, and GM Overlay content is precisely the case that doesn't repeat.

## An orthogonal, already-drafted axis: session isolation

Whichever compile mechanism is chosen for scripted/GM-Overlay content, `gm-overlay.md`'s own
unratified "two-network split" draft addresses a *different* part of the same problem: bounding
**blast radius and replay cost**, not compile mechanism. A small, isolated overlay session — recompiled
freely because its own rule count and working memory stay small by construction — means a GM editing
one overlay never pays a cost that scales with the whole game's accumulated state, regardless of
whether the overlay's own compile step is Roslyn, Expression-tree-direct, or anything else. These are
genuinely two separate axes (**what compiles the rule** vs. **what session it lands in**) that
combine, not compete — worth reading together with whichever option above gets ruled on, not instead
of it.

## Where the evidence points (not a ruling)

Option B is the only one that (a) fixes the cost where it's actually load-bearing — live reload, not
startup — (b) doesn't ask Rulebook authors to give up anything they have today, (c) doesn't reopen a
ruling made for good reasons at the time, and (d) is scoped by a requirement (`gm-overlay.md`'s narrow
declarative-`when`/`then`-plus-expression-grammar ruling) that already exists on the record, not one
invented for this doc. C is worth doing regardless, independently, as a small startup-cost win. A and
D both trade something real (Rulebook expressiveness, or a clean trust boundary) for a benefit that
doesn't address the actual finding. This is Baraka's read of the evidence gathered, not a decision —
see ticket [0073](../ClaudeWiki/Processes/Handoffs/0073-rule-compilation-cost-and-runtime-mode.md).

## Related documents

- [`rule-pipeline.md`](rule-pipeline.md) — the pipeline this doc's findings are about; its Purpose
  section, D1–D4 design decisions, and "Known limitations (MVP)" section are cited throughout above
- [`gm-overlay.md`](gm-overlay.md) — "How reload actually works" (the source-verified full-recompile
  finding motivating this doc) and the draft two-network split (the complementary isolation axis)
- [`non-goals.md`](non-goals.md) — "Not building rule-scale guardrails yet," the hypothetical
  procedural-ROM case this doc's findings now give a real, measured data point against
- [`cartridge.md`](cartridge.md), [`architecture.md`](architecture.md) — the trust-gradient framing
  behind Option B's security argument
- [Ticket 0007](../ClaudeWiki/Processes/Handoffs/0007-rule-pipeline-design-decisions.md) — D2's
  in-memory-Roslyn ruling and its stated reasoning (same-run testability, per B4)
- [Ticket 0008](../ClaudeWiki/Processes/Handoffs/0008-rule-pipeline-implementation.md) — the
  one-Roslyn-compile-per-rule-group fork decision, made solo, never escalated
- [Ticket 0073](../ClaudeWiki/Processes/Handoffs/0073-rule-compilation-cost-and-runtime-mode.md) —
  this doc's companion ticket, opened to Amara
