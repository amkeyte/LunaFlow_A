# Rule Authoring & Generation Pipeline

Implementation status: see [`../Roadmap/roadmap.md`](../Roadmap/roadmap.md).

## Purpose

Rulebook authors write rules in C#, close to the game logic they're describing. World-builders and GM tooling need to add or change rules without a C# compile step. Both cases should ultimately produce the same thing — classes derived from `NRules.Fluent.Dsl.Rule` (aliased as `NRule`, see `LunaFlow.Rules.Globals`) that can be loaded into an NRules session — so the engine only ever deals with one kind of rule, regardless of how it was authored.

`GeneratorServices` is a single generation pipeline that can run in two modes against the same intermediate representation:

| Mode | Input | Output |
| --- | --- | --- |
| Compile-time | Rulebook author's C# (`[RuleGroup]`/`[RuleTemplate]`-attributed static classes) | In-memory-compiled assembly (Roslyn) — see D2 in [Design decisions](#design-decisions-phase-2-pre-implementation) |
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
   IRuleCompiler          — Compile(RuleCodeUnit) → Assembly (in-memory Roslyn compile — see D2 below)
```

`RulePipeline` (`LunaFlow.Rules.GeneratorServices.RulePipeline`) is the orchestrator: it's constructed with one implementation of each of the three interfaces and its `Compile(RuleParseContext) → Assembly` method calls them in sequence and returns the compiled assembly. Swapping the compiler implementation (in-memory Roslyn vs. compile-into-current-session) is what distinguishes compile-time from runtime mode — parsing and IR generation are the same either way as long as the source is C#.

**`IRuleParser.Parse` produces exactly one `RuleIRGroup` per call.** `RuleParseContext.FromAssembly` can collect several `[RuleGroup]` types from one assembly, but `ReflectionRuleParser.Parse` throws if the context holds more than one — silently merging distinct `GroupName`s into a single `RuleIRGroup` was rejected as the wrong default. One pipeline run = one group, explicitly.

Compile-time mode is reached through `RuleExport` (`LunaFlow.Rules.GeneratorServices.RuleExport`), the author-facing entry point, not by constructing `RulePipeline` directly:

- `RuleExport.Compile(Type ruleGroupType) → Assembly` — one `[RuleGroup]` class in, one compiled assembly out. Builds a single-group `RuleParseContext` and runs it through a `ReflectionRuleParser` + `RuleCodeGenerator` + `RoslynRuleCompiler` pipeline.
- `RuleExport.CompileAll(Assembly source) → IEnumerable<Assembly>` — the multi-group entry point the one-group-per-call rule above requires: finds every `[RuleGroup]` type via `RuleParseContext.FromAssembly`, then calls `Compile` once per group, yielding one compiled assembly per group (not one merged assembly for the whole source).

`IRuleParser`, `IRuleIRGenerator`, and `IRuleCompiler` are implemented by `ReflectionRuleParser`, `RuleCodeGenerator`, and `RoslynRuleCompiler` respectively (all in `LunaFlow.Rules.GeneratorServices`) — see [Design decisions](#design-decisions-phase-2-pre-implementation) and [Implementation forks](#implementation-forks-phase-2-build) below for what each one does and why.

## Intermediate representation

The IR types already exist as plain data-transfer objects and are the settled shape for this stage (superseding an earlier, more structured `RuleModel`-with-`Condition`/`Action`-object sketch from planning — the current design keeps conditions and actions as opaque C# source strings rather than modeling them as a structured expression tree):

- `RuleParseContext` — input to parsing. `RuleParseContext.FromAssembly(assembly)` reflects over an assembly and collects every `static`, `abstract`, `sealed` class marked `[RuleGroup]`.
- `RuleIRGroup` — one parsed rule group: a `GroupName` plus a list of `RuleIR`.
- `RuleIR` — one parsed rule: `Name`, optional `Description`, `StandingRules` (marker types the rule must guard against — see below), `RequiredUsings` and `PreambleCode` (see D1 below), `ConditionCode` and `ActionCode` (raw C# snippets to be emitted verbatim into the generated `When()`/`Then()` blocks).
- `RuleCodeUnit` — the generator's output: a `FileName`, target `Namespace` (generator-assigned per group — see D4 below), `RequiredUsings` (the file's using block — see [Implementation forks](#implementation-forks-phase-2-build) below), a list of `RuleCodeClass`, and a `RegistryMethodBody` for the generated group-registry class's `GetRules()`-style method.
- `RuleCodeClass` — one generated class: `ClassName` plus its full `SourceCode`.

### Design decisions (Phase 2 pre-implementation)

Four seams surfaced while breaking Phase 2 down for implementation — ruled here per
[ticket 0007](../ClaudeWiki/Processes/Handoffs/0007-rule-pipeline-design-decisions.md), each
reflected in the DTOs/interfaces already:

**D1 — `RuleIR` grows to close the `RulePrototype` mapping gap.** `RulePrototype` (the author
surface) carries `RequiredUsings` and `PreambleStatements` that `RuleIR` had no slot for — the
`RulePrototype → RuleIR` mapping was lossy. `RuleIR` now carries `RequiredUsings` (`List<string>`,
copied straight across — the generator unions/dedups these across every rule in a group to build
one file's `using` block) and `PreambleCode` (`string`, `PreambleStatements` joined one statement
per line). `ConditionCode`/`ActionCode` stay opaque strings — the parser's job is to join
`WhenStatements`/`ThenStatements` into them (each statement prefixed with `.` and a newline, so the
result reads as a fluent chain following `When()`/`Then()`). This keeps the already-settled
"opaque C# source, not a structured expression tree" call intact (see above) while giving the two
real gaps somewhere to land — it doesn't reopen the conditions/actions shape question.

**D2 — Compiler MVP is in-memory Roslyn compilation, not "emit `.cs`, Roslyn later."** The
roadmap's original wording (`.cs` at minimum, `.dll` later) contradicts Phase 2's own done-bar
(B4: a generated rule "loads and fires," provable as a same-run test) — a `.cs`-to-disk-only
compiler needs an out-of-process build step to produce something loadable, which can't happen
inside a single test run. `IRuleCompiler.Compile(RuleCodeUnit)` now returns `Assembly`
(previously `void` — a real gap regardless of which compiler strategy got picked, since nothing
gave the caller a way to reach the compiled output at all) using `Microsoft.CodeAnalysis.CSharp`
(`CSharpCompilation.Create(...).Emit(memoryStream)` → `Assembly.Load(bytes)`), throwing on
compilation failure with diagnostics in the message. `RulePipeline.Compile(RuleParseContext)`
propagates that return value. The resulting assembly loads through the exact path
`LunaFlow.Engine.RuleEngine` already uses (`RuleRepository.Load(x => x.From(assembly)...)`) — see
[Loading generated rules](#loading-generated-rules) below. This is a real added cost: implementing
`IRuleCompiler` needs a new `Microsoft.CodeAnalysis.CSharp` package reference on `LunaFlow.Rules`,
which isn't there today. Writing the generated `.cs` to disk in addition (for author
debugging/inspection) is a fine, cheap addition on top of this — the source string already exists
in memory — but it is not what satisfies B4; the in-memory compile is.

**D3 — Generated rules get a flat, generator-assigned `Reference`.** New type
`GeneratedRuleReference` (`LunaFlow.Rules.GeneratorServices`, `public` — has to be, since it's
constructed from source that gets compiled into a separate in-memory assembly, unlike Nine Rooms'
`internal NineRoomsReference` which only needs same-assembly visibility). `FullPath` is a dotted
`GroupName.RuleName` path, `Parent` is `null`, same simplicity tier as `NineRoomsReference` — not
a real object graph. Generated source emits `public override ILunaFlowReference Reference { get; }
= new GeneratedRuleReference("Combat", "LowHealth");`. A genuine parent/child hierarchy (group
reference as `Parent`, its rules as `Subreferences`) is explicitly deferred to `non-goals.md` until
`IRuleBook`/`IRuleGroup` have real bodies to hang it on.

**D4 — Generated namespace is `LunaFlow.Generated.[GroupName]`, generator-assigned.** Reconciles
`RuleCodeUnit.Namespace`'s code default (previously `LunaFlow.GeneratedRules`, a flat bucket with
no per-group segmentation — two rulebooks each naming a group "Combat" would collide) against this
doc's older text (`LunaFlow.Rules.Generated.[ModuleName]`, never implemented). Landed in between:
drops the `.Rules.` segment — generated content is the *consuming rulebook's* generated artifact,
not part of the `LunaFlow.Rules` library itself, and a namespace rooted there reads otherwise — and
renames `ModuleName` to `GroupName`, the field that actually exists on `RuleIRGroup` (there's no
module concept in the IR; inventing one now would be exactly the premature generalization
`non-goals.md` warns against elsewhere). `RuleCodeUnit.Namespace`'s field default is now
`LunaFlow.Generated` as a bare fallback for direct construction (e.g. in a test) —
`IRuleIRGenerator` implementations must always set it explicitly to
`$"LunaFlow.Generated.{ir.GroupName}"`.

### Implementation forks (Phase 2 build)

Two seams the D1–D4 rulings above didn't cover, surfaced while building `IRuleParser`,
`IRuleIRGenerator`, and `IRuleCompiler` ([ticket 0008](../ClaudeWiki/Processes/Handoffs/0008-rule-pipeline-implementation.md)).
Both resolved in code first, reconciled into this doc after
([ticket 0009](../ClaudeWiki/Processes/Handoffs/0009-rule-pipeline-doc-reconcile.md)):

**Parser cardinality.** `RuleParseContext` can hold multiple `[RuleGroup]` types (that's what
`FromAssembly` is for), but nothing in D1–D4 said what `IRuleParser.Parse` should do with more
than one — and its return type, one `RuleIRGroup`, can't represent more than one group's rules
without merging their `GroupName`s. Resolved as: `Parse` throws on more than one group type in the
context. `RuleExport.CompileAll` is the multi-group answer instead — see
[Pipeline stages](#pipeline-stages) above.

**`RuleCodeUnit.RequiredUsings`.** D1 said the generator "unions/dedups `RuleIR.RequiredUsings`
across every rule in a group when it writes one file's using block," but that's a statement about
behavior, not a DTO field — `RuleCodeUnit`, the generator's *output*, had nowhere to carry the
unioned result forward to the compiler. Added `RuleCodeUnit.RequiredUsings` (`List<string>`), the
same kind of strict addition D1 made to `RuleIR` for the identical reason. `RuleCodeGenerator`
populates it as the baseline namespaces every generated file needs (`System`,
`System.Collections.Generic`, `NRules.Fluent.Dsl`, `LunaFlow.Rules`,
`LunaFlow.Rules.GeneratorServices`) unioned with every rule's own `RequiredUsings`, deduped;
`RoslynRuleCompiler` consumes it to write the file's `using` block before compiling.

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

`RulePrototype` (a plain DTO, consumed by `ReflectionRuleParser`) is the author's DSL surface:

- `Name`, `Description`
- `RequiredUsings` — namespaces the generated file needs
- `PreambleStatements` — C# lines before the `When()`/`Then()` blocks (typically variable declarations for `Match<T>(() => x, ...)`)
- `WhenStatements` / `ThenStatements` — DSL lines to insert inside `When()`/`Then()`, e.g. `Match<Character>(() => c, c => c.Wounds > 5)` and `Do(ctx => c.Status = "injured")`

## Output conventions

Generated rule classes:

- Derive from `NRules.Fluent.Dsl.Rule` — in practice this means from `LunaFlow.Rules.ARule`, which already carries an `ILunaFlowReference` alongside the NRules base, so every generated rule is traceable to a dotted path (`MyRuleBook.Chapter(A).Subsection(s)`) as well as loadable by NRules.
- Live in the `LunaFlow.Generated.[GroupName]` namespace — `RuleCodeUnit.Namespace`, always generator-assigned per D4 above.
- Get a `GeneratedRuleReference` per D3 above — not a hand-picked type per rule the way Nine Rooms did it.
- Must be `public` with a public parameterless constructor, per NRules' own loading requirements (`RuleRepository.Load(x => x.From(assembly).Where(t => typeof(Rule).IsAssignableFrom(t.RuleType)))` — the exact call already used by `LunaFlow.Engine.RuleEngine` and by the test suite).
- Override `public override void Define()`.

Example of the intended generated shape:

```csharp
namespace LunaFlow.Generated.Combat
{
    public class LowHealthRule : ARule
    {
        public override ILunaFlowReference Reference { get; } =
            new GeneratedRuleReference("Combat", "LowHealth");

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
}
```

A group's generated file also emits a registry class (`RuleCodeUnit.RegistryMethodBody`) exposing something like `Combat_Generated.GetRules()` — named from the namespace's last segment (the `GroupName`) plus `_Generated`, not from the author's C# class name (`CombatRules` above) — so a rulebook can assemble its full rule set without reflecting over an assembly at runtime if it doesn't want to.

## Loading generated rules

No new loading mechanism is needed — generated rules are ordinary NRules `Rule` subclasses, so
they load through the exact same path `LunaFlow.Engine.RuleEngine` already uses, once
`IRuleCompiler.Compile(...)` has handed back the in-memory `Assembly` (per D2 above) rather than a
path on disk:

```csharp
var repository = new RuleRepository();
repository.Load(x => x.From(assembly).Where(t => typeof(Rule).IsAssignableFrom(t.RuleType)));
var session = repository.Compile().CreateSession();
```

This is also how `tests/LunaFlow.EngineTests/Rules/RuleTests.cs` loads its hand-written test rules today — those tests don't exercise `GeneratorServices` at all, but they exercise the exact loading contract generated rules will need to satisfy.

## Known limitations (MVP)

**Compiler reference resolution.** `RoslynRuleCompiler` references *every currently loaded,
file-backed assembly in the `AppDomain`* when compiling generated source — a pragmatic stand-in,
not real reference tracking, because nothing above specifies how a generated rule's domain-fact
types (a rulebook's own `Character`, `GameOverMarker`, etc.) are supposed to get resolved.
Works today because those types are necessarily already loaded by whatever assembly is calling the
pipeline. Would break for a fact type defined in an assembly that hasn't been loaded yet at compile
time. *Revisit if that ever happens* — the fix is giving `RuleCodeUnit`/`RuleParseContext` an
explicit slot for reference assemblies rather than scanning the AppDomain, but that's speculative
scope for a problem nothing has hit yet.

## Open design questions

These are flagged in planning but not resolved:

- **Rule scoping** — grouping by `rulebook` / `module` / `agenda` isn't represented in the current IR types (`RuleIRGroup` only has a flat `GroupName`).
- **Prioritization and phase control** — exposing NRules agenda groups / rule priority through `RulePrototype` hasn't been designed.
- **Versioned rule schemas** — a `version:` tag for forward-compatibility of runtime/scripted rule sources, relevant once [`scripting-content.md`](scripting-content.md)'s script format exists.
- **Validation pass** — warning on rules with no `Then`, duplicate names, or missing preconditions before compilation.
- **Test tooling** — a `RuleTester`-style utility for firing a rule in isolation with injected facts, built on the same IR so it works for both compile-time and runtime rules.
