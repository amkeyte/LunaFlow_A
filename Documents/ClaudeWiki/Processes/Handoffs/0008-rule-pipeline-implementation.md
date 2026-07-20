# 0008 — Implement the Phase 2 rule pipeline

**Status:** resolved 2026-07-17
**Opened by:** Supreme Leader (Kofi)
**Opened:** 2026-07-17
**For:** Dev — Implementation (Baraka)
**Depends on:** [0007](0007-rule-pipeline-design-decisions.md) — resolved (design settled, DTOs/interfaces shaped)

## Context

Phase 2 (Rule pipeline MVP — [`Roadmap/phase-2.md`](../../Roadmap/phase-2.md)). The design is
settled and the DTOs/interfaces are shaped per [0007](0007-rule-pipeline-design-decisions.md);
[`design/rule-pipeline.md`](../../design/rule-pipeline.md) is the authoritative spec, including its
"Design decisions (Phase 2 pre-implementation)" section (D1–D4). Your job is to fill in the three
empty interface bodies + `RuleExport` so an author-written `[RuleGroup]` class becomes a loadable,
firing NRules rule.

**First step — confirm the build.** 0007's shape-only edits to `src/` (RuleIR, IRuleCompiler,
RulePipeline, RuleCodeUnit, GeneratedRuleReference) were verified by reading, not building — the PM
has no toolchain. Build the solution before you start; if it's red, that's a defect to fix or bounce
to the Architect, not something to build on top of.

## Ask

Implement against `design/rule-pipeline.md` (D1–D4 are binding):

1. **`IRuleParser`** — reflect each `[RuleGroup]` class from `RuleParseContext` → its `[RuleTemplate]`
   static properties → invoke the getter for each `RulePrototype` → map to `RuleIR`:
   `RequiredUsings` → `RuleIR.RequiredUsings`, `PreambleStatements` → `RuleIR.PreambleCode`,
   `When`/`Then` statements → `ConditionCode`/`ActionCode`; capture `[StandingRule(typeof(T))]` at
   **class and property level** into `RuleIR.StandingRules`.
2. **`IRuleIRGenerator`** — `RuleIRGroup` → `RuleCodeUnit`: emit `ARule`-derived source per rule with
   `public override ILunaFlowReference Reference { get; } = new GeneratedRuleReference(group, rule);`,
   inject a `.Not<T>()` guard for **each** `StandingRules` type, set namespace
   `LunaFlow.Generated.[GroupName]`, and emit the group registry (`RegistryMethodBody`). Generated
   classes must be `public` with a public parameterless ctor and override `Define()`.
3. **`IRuleCompiler`** — in-memory Roslyn compile of `RuleCodeUnit` → `Assembly`; add the
   `Microsoft.CodeAnalysis.CSharp` package reference to `LunaFlow.Rules` (the added cost D2 flagged);
   throw on compilation failure with diagnostics in the message.
4. **`RuleExport`** — wire the author-facing entry point that runs the pipeline.

## Done when (B4)

A hand-written sample `[RuleGroup]` class → `RulePipeline.Compile(...)` → an `Assembly` whose
generated rule loads via the existing `RuleRepository.Load(x => x.From(assembly)...)` path and
**fires** in a session. Prove it with a **throwaway end-to-end check** (dev-territory quick check per
[`Roles/dev.md`](../../Roles/dev.md) — the *maintained* B1–B5 suite incl. B5 parity is Nadia's
follow-on, not this ticket). Build green, no `NotImplementedException` left in the pipeline.

## Coordination & scope

- After this lands, Nadia writes the durable **B1–B5** suite (parser IR shape, generator compiles,
  `.Not<T>()` injection, end-to-end load+fire, and B5 parity vs. an equivalent hand-written rule);
  Lothar does final verification. Don't build the maintained suite yourself.
- File a ticket to the [Architect](../../Roles/architect.md) if the spec doesn't survive contact with
  the compiler, rather than diverging from it silently.
- **Out of scope:** runtime/script mode, the ROM/Rulebook component model, config-driven loading, and
  all five of `rule-pipeline.md`'s open design questions (scoping, prioritization, versioning,
  validation pass, `RuleTester`). Compile-time C# path only.
- Conventions: follow [`testing.md`](../testing.md)/[`test-patterns.md`](../test-patterns.md) for any
  check you write; no state-changing git (`CLAUDE.md`).

## Response

Build confirmed green first, per the ticket's own instruction (0 errors, only pre-existing
unrelated warnings). All four asks implemented against `design/rule-pipeline.md`:

1. **`ReflectionRuleParser`** (`GeneratorServices/ReflectionRuleParser.cs`) — reflects
   `RuleParseContext.RuleGroupTypes` → `[RuleTemplate]` static properties → `RulePrototype` →
   `RuleIR`, per D1's exact join rules (`WhenStatements`/`ThenStatements` joined into
   `ConditionCode`/`ActionCode` with a leading `.` per line). `[StandingRule]` captured at both
   class and property level, unioned and deduped per rule.
2. **`RuleCodeGenerator`** (`GeneratorServices/RuleCodeGenerator.cs`) — emits one `{Name}Rule :
   ARule` class per `RuleIR` into `LunaFlow.Generated.{GroupName}` (D4), `GeneratedRuleReference`
   per D3, a `.Not<T>()` appended per `StandingRules` entry, plus the group's `RegistryMethodBody`.
3. **`RoslynRuleCompiler`** (`GeneratorServices/RoslynRuleCompiler.cs`) — in-memory
   `CSharpCompilation` → `MemoryStream` → `Assembly.Load`, per D2. Throws with Roslyn diagnostics
   + the generated source in the exception message on failure. References every currently loaded,
   file-backed assembly (pragmatic MVP stand-in — see fork #2 below).
4. **`RuleExport`** (`GeneratorServices/RuleExport.cs`) — `Compile(Type)` wires the three through
   `RulePipeline`; added `CompileAll(Assembly)` as a convenience loop since `RuleParseContext.
   FromAssembly` already existed and multi-group assemblies needed *some* entry point (see fork #1).

Added `Microsoft.CodeAnalysis.CSharp` 5.6.0 to `LunaFlow.Rules.csproj` (the flagged D2 cost).

**Two fork decisions the spec didn't pin down** (per this ticket's own "file a ticket rather than
diverge silently" instruction — surfaced directly in-conversation rather than a separate 0007
follow-up ticket, since the PM was driving this session live):

1. **`RuleParseContext`/`IRuleParser` cardinality mismatch.** `FromAssembly` can collect multiple
   `[RuleGroup]` types, but `Parse()` returns one `RuleIRGroup`. Resolved as: `Parse()` throws if
   given more than one group type — one group per pipeline run, explicit rather than silently
   merging distinct `GroupName`s. `RuleExport.CompileAll` loops per group as the multi-group entry
   point instead.
2. **`RuleCodeUnit` had no field for the D1-mandated using-union.** D1 says the generator "unions/
   dedups [`RuleIR.RequiredUsings`] across every rule in a group when it writes one file's using
   block," but `RuleCodeUnit` (the generator's *output* DTO) had no slot to carry that. Added
   `RuleCodeUnit.RequiredUsings` (`List<string>`) — a strict addition, same move D1 made to `RuleIR`
   for the identical reason. `RuleCodeGenerator` populates it (baseline framework usings + every
   rule's `RequiredUsings`, deduped); `RoslynRuleCompiler` consumes it for the file's `using` block.

**B4 proving check** (throwaway, `tests/LunaFlow.EngineTests/Scratch/RulePipelineProvingCheck.cs`,
deleted after confirming — not part of the maintained suite per `Roles/dev.md`): a hand-written
`[RuleGroup]` class with two rules — one plain, one `[StandingRule]`-guarded — compiled through
`RuleExport.Compile` → loaded via the existing `RuleRepository.Load(x => x.From(assembly)...)`
path → fired in a session. Confirmed: the plain rule fires on a matching fact; the guarded rule
does **not** fire while its marker fact is present and **does** fire once the marker is retracted
(exercises ask #1's `[StandingRule]` capture and ask #2's `.Not<T>()` injection together, not just
the minimal load+fire bar). Full suite re-run after: 64/64 green, solution build clean (0 errors).

No `NotImplementedException` remains anywhere in the pipeline.

## Resolution

Verified by inspection and closed by Kofi (opener), 2026-07-17. Implementation is real and faithful
to `rule-pipeline.md` D1–D4; both fork calls are sound.

**What I verified:** read all four impls (`ReflectionRuleParser`, `RuleCodeGenerator`,
`RoslynRuleCompiler`, `RuleExport`) + the DTO changes. Parser cardinality guard, `[StandingRule]`
class∪property union, D1 joins, D3 `GeneratedRuleReference`, D4 namespace, `.Not<T>()` injection,
in-memory Roslyn→`Assembly`, and `RuleCodeUnit.RequiredUsings` (fork #2) all present and consistent.
`StandingRuleAttribute.NotType` confirmed real (parser compiles).

**Two honest caveats (verification limits, not defects):**
- **Build/64-green claimed, not independently run** — PM has no toolchain. Code is internally
  consistent and uses real Roslyn/NRules APIs, but the green build rests on Baraka's run.
- **B4 was proven by a throwaway that's now deleted** — so neither I nor Lothar can re-run *that*
  check. Per the plan this is fine: durable B4 (plus B1/B2/B3/B5) is Nadia's follow-on suite, and
  Lothar's verdict re-establishes it. **Net: the pipeline currently has zero maintained automated
  coverage** until Nadia's suite lands. That's expected, but Phase 2 is NOT done on the strength of
  this ticket alone.

**Flag — doc drift owed to the Architect (not blocking this close):** the two forks changed shapes
`rule-pipeline.md` doesn't yet describe — Parse() is one-group-per-call (with `RuleExport.Compile`/
`CompileAll` as the entry points) and `RuleCodeUnit.RequiredUsings` now exists. Those belong in the
design doc so code and spec don't drift; that's Amara's territory. Tracked as a small reconciliation.

**Minor, non-blocking:** `RoslynRuleCompiler.GetReferences()` references every loaded file-backed
assembly — Baraka flagged it as a pragmatic MVP stand-in for real reference tracking; fine for now,
worth revisiting if fact types ever live in an unloaded assembly.

Next: Nadia writes the durable **B1–B5** suite; then Lothar's Phase 2 verdict.
