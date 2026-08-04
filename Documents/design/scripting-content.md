# World Content & Scripting

Implementation status: see [`../Roadmap/roadmap.md`](../Roadmap/roadmap.md).

## Why content needs its own path

[`architecture.md`](architecture.md)'s role table draws a hard line between Rulebook Authors (C#, compiled, statically checked) and World-Builders (data + light scripting, no compile step). The rule-generation pipeline in [`rule-pipeline.md`](rule-pipeline.md) already anticipates this: it's designed to run in a "runtime" mode where a script is parsed straight into the same `RuleIRGroup`/`RuleCodeUnit` IR that C# rule groups produce, so a world-builder's content and a rulebook author's C# both end up as ordinary NRules rules. This document is about the missing left-hand side of that pipeline — the parser and script format that would feed it.

## Two authoring surfaces, two technologies

| Surface | Technology | Used for |
| --- | --- | --- |
| World/content data | JSON | Characters, items, locations, encounter definitions — structured data with clear schema |
| Embedded/event behavior | Lua via MoonSharp | Logic that's awkward to express as pure data: conditionals, sequencing, one-off event scripts |

MoonSharp was chosen specifically because it's a sandboxed, embeddable Lua implementation for .NET — world-builder and GM scripts run inside the engine process without the ability to reach outside their sandbox, which matters because these authors are explicitly less trusted than rulebook authors (see the role/access table in `architecture.md`).

Three patterns for combining the two, in increasing order of preference as scripts grow:

1. **Inline Lua strings in JSON fields** — smallest footprint, but string escaping makes anything beyond a one-liner painful.
2. **JSON fields that reference external `.lua` files** — cleaner separation, preferred for anything more than a trivial action.
3. **Pure Lua config files (`return {}`)** — skips JSON deserialization entirely; most useful for developer/rulebook-adjacent tooling rather than typical world-builder content.

## Refinement — the World Module tier collapses this split to Lua-only

**Added 2026-08-03.** The table above frames JSON and Lua as two technologies for two different
jobs — structured data vs. awkward-as-data logic. For the [World Module](world-module.md) tier
specifically, that split doesn't hold: Lua table literals are already a structural superset of
JSON's data shape, so a Lua chunk that's pure `return {...}` with no logic *is* the JSON case, and a
chunk with real computation is the same execution path with more of it exercised. One technology
covers both ends of that spectrum for World Module content, so
[`world-module.md`'s Lua-hosted authoring path](world-module.md#two-authoring-paths-into-one-contract)
uses Lua exclusively there rather than standing up a separate JSON deserializer alongside it.

This refinement is scoped to World Module content authoring only. It does **not** change anything
below — the script-driven rule adapter immediately below this section is a separate mechanism
(compiling scripted *behavior* into the live rule engine, not marshaling scripted *data* into a
Rulebook-owned shape) and isn't affected by which format World Module content happens to use.

## Script-driven rule adapter

**Tier, added 2026-08-03:** this mechanism belongs to the GM-Overlay tier, not World Module or
Rulebook — see [`gm-overlay.md`](gm-overlay.md#ruling-phase-4s-startcombat-example-is-gm-overlay-territory)
for the ruling and why. "World-builder content" below is kept as this section's original framing but
should be read as GM-Overlay-authored; a World Module never produces live rule *behavior*, only data
(see `world-module.md`).

The concrete mechanism for turning a GM's scripted rule into a live one is a small DSL compiled at runtime through the same pipeline used for compiled rules:

```yaml
- name: StartCombat
  when:
    - player.state == "ready"
    - area.hasEnemies == true
  then:
    - set session.inCombat = true
    - insert Event("CombatStarted")
```

The intended flow:

1. A parser (an `IRuleParser` implementation targeting this DSL rather than C# reflection) reads the script and produces the same `RuleIRGroup` IR that `RuleParseContext.FromAssembly` produces for compiled rule groups.
2. Engine-level guards — the same standing markers described in [`rule-pipeline.md`](rule-pipeline.md#author-facing-conventions), e.g. `Not<ExitGameMarker>` — get injected at this stage exactly as they would for a C# `[StandingRule]`, so a world-builder's script can't accidentally bypass an engine guarantee simply because it wasn't written in C#.
3. The rest of the pipeline (IR → generator → compiler) is unchanged; only the compiler target differs — scripted content compiles into the live session rather than emitting a `.dll`.

A first cut at where this would live in the source tree:

```
LunaFlow.Engine/
  RuleRuntimeLoader.cs   # loads & composes rules at runtime
  MarkerFacts.cs         # guard types like ExitGame
  RuleAdapter.cs         # turns scripts into NRules rules

LunaFlow.Scripting/
  rules.yml              # example game-module rulebook
  <parser + validator>
```

`RuleAdapter`/`RuleRuntimeLoader` as named here would likely become the concrete `IRuleParser`/`IRuleCompiler` pair referenced from `LunaFlow.Rules.GeneratorServices` in [`rule-pipeline.md`](rule-pipeline.md), living in `LunaFlow.Scripting` rather than `LunaFlow.Rules` so that `LunaFlow.Rules` doesn't need a MoonSharp/YAML dependency just to support the pipeline's interfaces.

## Open design questions

- Whether the script format is YAML, JSON, or a small custom DSL — the example above is YAML-shaped but nothing has committed to it.
- **Answered for the GM-Overlay case, 2026-08-03:** the declarative `when`/`then` shape stays as the
  top-level rule structure (it's what lets standing-guard injection separate condition from action);
  Lua, if used at all, would live *inside* a condition/action as an expression evaluator, not replace
  the declarative shape. See [`gm-overlay.md`](gm-overlay.md#ruling-gm-overlay-does-not-collapse-to-lua-only-the-way-world-module-did).
  Whether that inner expression language is a custom grammar or MoonSharp-backed remains open.
- **Answered at the mechanism level, 2026-08-03:** NRules 1.0.2 (the pinned version) has no API to
  mutate a running session's rule set — reload can only mean recompile a new `ISessionFactory` from
  the full active rule set, create a new session, replay current facts into it, and swap. See
  [`gm-overlay.md`"How reload actually works"](gm-overlay.md#how-reload-actually-works-verified-against-nrules-102-source),
  checked against NRules source per `CLAUDE.md`'s convention. Still open: the concrete reload-primitive
  API shape, its `GameEngineCore` integration point, and its measured cost at realistic fact counts.
- Where script validation errors surface to a GM or world-builder who isn't watching a compiler — this needs to integrate with whatever GM/world-builder tooling eventually exists, not just log to a console.
