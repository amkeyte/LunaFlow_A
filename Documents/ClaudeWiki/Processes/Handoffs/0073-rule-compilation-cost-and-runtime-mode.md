# 0073 — Rule-compilation cost: Roslyn's role, and what a runtime/scripted path should use instead

**Status:** in-review
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-08-04
**For:** Amara (Architect)
**[Arryn] - Scheduled: reight before Roadmap Phase 4

## Decision

**Ruled: Option B.** Roslyn stays, untouched, for compile-time Rulebook authoring. A second,
non-Roslyn runtime path — `RuleBuilder` + `System.Linq.Expressions`, no C# source text, no
`Assembly.Load` — is what Phase 4's `IRuleParser`/`IRuleCompiler` for scripted/GM-Overlay content
builds against. This resolves `gm-overlay.md`'s previously-vague "compile into the current session"
runtime-mode language into an actual mechanism; full reasoning in Response. Option C (merge per-group
Roslyn invocations into one) is endorsed as a separate, independently-worthwhile startup-cost fix —
not blocking, not part of this ruling, a candidate small ticket to Baraka whenever convenient.

## Details

### Context

Instrumenting `MyLunaFlowServer`'s startup with real timing (2026-08-04, requested alongside the
loading-metrics work) surfaced a concrete number: Roslyn compilation of 1.d20's three rule groups
(`RuleExport.Compile`, in-memory `CSharpCompilation`) costs **~800ms**; the NRules Rete-network build
it feeds into (`RuleRepository.Compile()`/`CreateSession()`) costs **~58ms**. Roslyn is ~14x the cost
of the mechanism it exists to feed.

That's a tolerable one-time startup tax for trusted, first-party Rulebook content. It stopped being
just a curiosity once we connected it to `gm-overlay.md`'s already-source-verified finding: NRules has
no incremental rule-mutation API at all, so **every GM Overlay apply/revert requires a full recompile
of the complete current rule-definition set**, not just the overlay's own rules. Running a
Roslyn-class operation on every live edit — or on anything approaching per-turn cadence, which
`non-goals.md`'s hypothetical procedural-ROM case already names as a real future risk — is not
something to inherit by default into Phase 4 without deciding it on purpose.

Separately, checked the pinned NRules 1.0.2 assemblies directly (not docs, not memory) and confirmed
Roslyn was never actually required by NRules itself: `RuleBuilder`/`PatternBuilder`/
`ActionGroupBuilder` build an `IRuleDefinition` from raw `System.Linq.Expressions.LambdaExpression`s,
and `RuleCompiler.Compile(IEnumerable<IRuleDefinition>)` turns that directly into a session factory —
no C# source text, no `Assembly.Load`, anywhere in that path. The class-per-rule-plus-Roslyn step is
entirely LunaFlow's own layer, chosen for reasons that were sound at the time (ticket 0007's D2:
same-run testability for Phase 2's B4 done-bar) but never weighed against a live-reload use case,
because GM Overlay didn't have a concrete design yet when that call was made.

Also traced *why* the cost is as high as it is, beyond "Roslyn is Roslyn": `IRuleParser.Parse`'s
"exactly one `RuleIRGroup` per call" shape — a fork decision made solo during ticket
[0008](0008-rule-pipeline-implementation.md)'s implementation, flagged inline to Kofi at the time but
never routed to Amara as its own design question — means every rule group pays a full, separate Roslyn
invocation, and each of those invocations references every loaded assembly in the AppDomain
(`rule-pipeline.md`'s own documented "Known limitations," previously filed as "a problem nothing has
hit yet" — something now has).

### Ask

Full findings, methodology, and a charted set of options (A: eliminate Roslyn entirely; B: keep it for
compile-time Rulebook authoring, add a separate non-Roslyn runtime path via `RuleBuilder`+
`Expression`s for scripted/GM-Overlay content; C: merge per-group Roslyn invocations into one, startup
cost only; D: Roslyn Scripting API instead of full `CSharpCompilation` for the runtime path; E:
compiled-assembly caching) are written up in
[`rule-pipeline-compilation-cost.md`](../../../design/rule-pipeline-compilation-cost.md), including pros
and cons for each and how each interacts with `gm-overlay.md`'s separate, already-drafted
session-isolation axis. That doc is explicitly findings-plus-options, not a decision — flagged as
unratified the same way `gm-overlay.md`'s own draft section is.

Nothing here is blocking today's work. It's worth ruling before Phase 4 kickoff finalizes anything
about the GM Overlay rule-compilation mechanism, since whichever option gets picked determines what
`gm-overlay.md`'s still-open `IRuleParser`/`IRuleCompiler` runtime-mode question actually builds
against. No urgency beyond that — routing it now while the context and the real numbers are fresh,
rather than letting it wait for Phase 4 kickoff to rediscover.

### Response

Read all five options and the evidence doc in full before ruling, not just the "where the evidence
points" summary — the summary's read holds up independently.

**Why B and not the alternatives, in my own words rather than just endorsing Baraka's:**

- **A** solves a cost problem that isn't actually load-bearing (Rulebook's ~800ms is a one-time,
  trusted-content startup tax, not a live-reload cost) by reopening a settled call
  (`rule-pipeline.md`/ticket 0007's "no structured expression tree," made deliberately for the
  compile-time case) and rebuilding, badly, something Roslyn already does well for the tier that
  actually wants arbitrary C#. Rejected for the same reason `non-goals.md`'s discipline rejects
  building the general thing on the strength of one pressure point.
- **B** is the option that was already half-decided without anyone naming it: `gm-overlay.md`'s own
  ruling ("the top-level rule structure stays a declarative `when`/`then` shape... what Lua might
  still reasonably own is narrower: the expression language inside a condition or action line")
  already committed scripted content to a structured, bounded shape, not arbitrary C#. `RuleBuilder`
  + `Expression` is the mechanism that shape was implicitly asking for — a translator that can only
  ever emit the operations it explicitly implements is a *better* fit for the least-trusted tier in
  the stack than "assemble untrusted content into C# source text and hand it to a general compiler,"
  which is what the runtime path would otherwise have inherited by default from the Rulebook path
  without anyone deciding it on purpose. It also costs nothing Rulebook authoring has today — that
  path is untouched.
- **C** is real and worth doing, but it's a startup-number improvement, not an answer to the question
  this ticket was actually opened over (GM Overlay's per-edit full-recompile cost). Endorsed as
  independent, uncoupled follow-up work, not folded into this ruling.
- **D** keeps the same trust-surface problem A/B differ on (untrusted content still becomes compiled
  C#) while adding nothing B doesn't already give scripted authors, given the shape is already
  bounded to declarative `when`/`then` plus a narrow expression grammar rather than "write C#."
  Rejected.
- **E** is orthogonal and worth layering on regardless of A–D, but doesn't answer this ticket's
  question — GM Overlay content is precisely the case that doesn't repeat. Noted, not ruled on here
  since nothing about B depends on it.

**Direct corollary this ruling forces, not previously settled:** `gm-overlay.md`'s still-open
"expression grammar for condition/action bodies (custom vs. Lua-via-MoonSharp)" question is resolved
by the same logic that picked B over D — a narrow, explicit AST-to-`Expression`-tree translator is the
whole point; reaching for MoonSharp as the inner expression evaluator would reintroduce a general
embedded interpreter (broader trust surface, per-evaluation interpretation cost) for exactly the case
B was chosen to avoid that for. **Ruled: the expression grammar is a small custom grammar, translated
directly to `System.Linq.Expressions`, not Lua.** Folding both this and the mechanism ruling into
`gm-overlay.md` directly — see that doc's changelog for where.

**On Arryn's embedded question (whether Rulebook authoring needs `GeneratorServices`/Roslyn at all):**
real question, genuinely still open, and deliberately **not** answered by this ruling — Option B keeps
Rulebook's path untouched specifically so this ticket doesn't have to answer it. If the generator
pipeline is ever dropped for Rulebook content too, B's runtime path is unaffected either way; it never
depended on Roslyn to begin with. Worth its own investigation on its own timeline, not a Phase 4
blocker — flagging so it doesn't get lost, not picking it up here.

`rule-pipeline-compilation-cost.md`'s draft banner updated to record this ruling rather than left
reading "not ratified" indefinitely.

### Resolution

<Filled in by the opener when closing.>
