# World Content & Scripting

**Status:** concept only. Nothing in this document exists in code — no `LunaFlow.Scripting` project, no MoonSharp dependency, no script format. This is the least-developed area of the design and the one with the most open questions.

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

## Script-driven rule adapter

The concrete mechanism for turning world-builder content into live rules is a small DSL compiled at runtime through the same pipeline used for compiled rules:

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
- How Lua callbacks (MoonSharp) and the declarative `when`/`then` script format above relate to each other — are they two separate authoring paths, or does the declarative format compile down to generated Lua/C# glue?
- Hot-reload semantics: what happens to an active session's already-fired rules when a world module is edited and reloaded mid-session.
- Where script validation errors surface to a world-builder who isn't watching a compiler — this needs to integrate with whatever GM/world-builder tooling eventually exists, not just log to a console.
