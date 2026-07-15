# Rule Authoring & Generation Pipeline

**Status:** interfaces and DTOs exist in `LunaFlow.Rules.GeneratorServices`; no concrete implementation. This is the most fleshed-out piece of design work carried over from planning, and the closest to being buildable as-is.

## Purpose

Rulebook authors write rules in C#, close to the game logic they're describing. World-builders and GM tooling need to add or change rules without a C# compile step. Both cases should ultimately produce the same thing — classes derived from `NRules.Fluent.Dsl.Rule` (aliased as `NRule`, see `LunaFlow.Rules.Globals`) that can be loaded into an NRules session — so the engine only ever deals with one kind of rule, regardless of how it was authored.

`GeneratorServices` is a single generation pipeline that can run in two modes against the same intermediate representation:

| Mode | Input | Output |
| --- | --- | --- |
| Compile-time | Rulebook author's C# (`[RuleGroup]`/`[RuleTemplate]`-attributed static classes) | Generated `.cs` files, optionally compiled to a `.dll` |
| Runtime | World-builder / GM scripted content (JSON/YAML/DSL, not yet designed — see [`scripting-content.md`](scripting-content.md)) | An in-memory rule definition, compiled directly into the running session |

Unifying these onto one pipeline is a deliberate choice: it means a validator, a test harness, or a debugger built for one mode works for the other for free, and there's exactly one place that understands "what a rule is."

## Pipeline stages

```
Rule Source (C# attributes or script)
        │
        ▼
   IRuleParser          — Parse(RuleParseContext) → RuleIRGroup
        │
        ▼
 IRuleIRGenerator        — Generate(RuleIRGroup) → RuleCodeUnit
        │
        ▼
   IRuleCompiler          — Compile(RuleCodeUnit) → emitted .cs / compiled .dll / live session
```

`RulePipeline` (`LunaFlow.Rules.GeneratorServices.RulePipeline`) is the orchestrator: it's constructed with one implementation of each of the three interfaces and its `Compile(RuleParseContext)` method just calls them in sequence. Swapping the compiler implementation (emit-`.dll` vs. compile-into-current-session) is what distinguishes compile-time from runtime mode — parsing and IR generation are the same either way as long as the source is C#.

None of `IRuleParser`, `IRuleIRGenerator`, or `IRuleCompiler` has a concrete implementation yet. `RuleExport`, the intended author-facing static entry point for triggering the pipeline, is currently an empty stub.

## Intermediate representation

The IR types already exist as plain data-transfer objects and are the settled shape for this stage (superseding an earlier, more structured `RuleModel`-with-`Condition`/`Action`-object sketch from planning — the current design keeps conditions and actions as opaque C# source strings rather than modeling them as a structured expression tree):

- `RuleParseContext` — input to parsing. `RuleParseContext.FromAssembly(assembly)` reflects over an assembly and collects every `static`, `abstract`, `sealed` class marked `[RuleGroup]`.
- `RuleIRGroup` — one parsed rule group: a `GroupName` plus a list of `RuleIR`.
- `RuleIR` — one parsed rule: `Name`, optional `Description`, `StandingRules` (marker types the rule must guard against — see below), `ConditionCode` and `ActionCode` (raw C# snippets to be emitted verbatim into the generated `When()`/`Then()` blocks).
- `RuleCodeUnit` — the generator's output: a `FileName`, target `Namespace` (defaults to `LunaFlow.GeneratedRules`), a list of `RuleCodeClass`, and a `RegistryMethodBody` for the generated group-registry class's `GetRules()`-style method.
- `RuleCodeClass` — one generated class: `ClassName` plus its full `SourceCode`.

## Author-facing conventions

A rulebook author writes a static partial class per rule group:

```csharp
[RuleGroup("Combat")]
[StandingRule(typeof(GameOverMarker))]
public static partial class CombatRules
{
    [RuleTemplate]
    public static RulePrototype LowHealth => new()
    {
        Name = "LowHealth",
        Description = "Player is critically low on health.",
        // Condition/Action are expressed as WhenStatements/ThenStatements on RulePrototype,
        // not as delegates — see RulePrototype below.
    };
}
```

- `[RuleGroup(name)]` marks the class itself as a group; `RuleParseContext.FromAssembly` looks for exactly this attribute.
- `[RuleTemplate]` marks a static property returning a `RulePrototype` as one rule to generate.
- `[StandingRule(typeof(T))]` can be applied at class or property level and is intended to inject a `.Not<T>()` guard automatically, so a rule author never has to remember to write `.Not<GameOverMarker>()` by hand on every rule. This is the mechanism [`engine-core.md`](engine-core.md) refers to as a "settling marker" — enforced by the generator, not by convention.

`RulePrototype` (already implemented as a plain DTO, not yet consumed by anything) is the author's DSL surface:

- `Name`, `Description`
- `RequiredUsings` — namespaces the generated file needs
- `PreambleStatements` — C# lines before the `When()`/`Then()` blocks (typically variable declarations for `Match<T>(() => x, ...)`)
- `WhenStatements` / `ThenStatements` — DSL lines to insert inside `When()`/`Then()`, e.g. `Match<Character>(() => c, c => c.Wounds > 5)` and `Do(ctx => c.Status = "injured")`

## Output conventions

Generated rule classes:

- Derive from `NRules.Fluent.Dsl.Rule` — in practice this means from `LunaFlow.Rules.ARule`, which already carries an `ILunaFlowReference` alongside the NRules base, so every generated rule is traceable to a dotted path (`MyRuleBook.Chapter(A).Subsection(s)`) as well as loadable by NRules.
- Live in the `LunaFlow.Rules.Generated.[ModuleName]` namespace (or `RuleCodeUnit.Namespace` if the generator overrides it).
- Must be `public` with a public parameterless constructor, per NRules' own loading requirements (`RuleRepository.Load(x => x.From(assembly).Where(t => typeof(Rule).IsAssignableFrom(t.RuleType)))` — the exact call already used by `LunaFlow.Engine.RuleEngine` and by the test suite).
- Override `public override void Define()`.

Example of the intended generated shape:

```csharp
public class LowHealthRule : ARule
{
    public override ILunaFlowReference Reference => /* assigned reference */;

    public override void Define()
    {
        Character c = null;

        When()
            .Match<Character>(() => c, c => c.HP < 10)
            .Not<GameOverMarker>();   // injected by [StandingRule]

        Then()
            .Do(ctx => ctx.Emit("You feel faint."));
    }
}
```

A group's generated file also emits a registry class (`RuleCodeUnit.RegistryMethodBody`) exposing something like `CombatRules_Generated.GetRules()`, so a rulebook can assemble its full rule set without reflecting over an assembly at runtime if it doesn't want to.

## Loading generated rules

No new loading mechanism is needed — generated rules are ordinary NRules `Rule` subclasses, so they load through the exact same path `LunaFlow.Engine.RuleEngine` already uses:

```csharp
var repository = new RuleRepository();
repository.Load(x => x.From(assembly).Where(t => typeof(Rule).IsAssignableFrom(t.RuleType)));
var session = repository.Compile().CreateSession();
```

This is also how `tests/LunaFlow.EngineTests/Rules/RuleTests.cs` loads its hand-written test rules today — those tests don't exercise `GeneratorServices` at all, but they exercise the exact loading contract generated rules will need to satisfy.

## Open design questions

These are flagged in planning but not resolved:

- **Rule scoping** — grouping by `rulebook` / `module` / `agenda` isn't represented in the current IR types (`RuleIRGroup` only has a flat `GroupName`).
- **Prioritization and phase control** — exposing NRules agenda groups / rule priority through `RulePrototype` hasn't been designed.
- **Versioned rule schemas** — a `version:` tag for forward-compatibility of runtime/scripted rule sources, relevant once [`scripting-content.md`](scripting-content.md)'s script format exists.
- **Validation pass** — warning on rules with no `Then`, duplicate names, or missing preconditions before compilation.
- **Test tooling** — a `RuleTester`-style utility for firing a rule in isolation with injected facts, built on the same IR so it works for both compile-time and runtime rules.
