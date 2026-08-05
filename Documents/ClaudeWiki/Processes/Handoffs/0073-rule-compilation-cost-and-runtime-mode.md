# 0073 — Rule-compilation cost: Roslyn's role, and what a runtime/scripted path should use instead

**Status:** open
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-08-04
**For:** Amara (Architect)
**[Arryn] - Scheduled: reight before Roadmap Phase 4

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

<Filled in by the assignee.>

### Resolution

<Filled in by the opener when closing.>
