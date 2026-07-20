# 0007 — Phase 2 rule-pipeline design decisions (pre-implementation)

**Status:** resolved 2026-07-17
**Opened by:** Supreme Leader (Kofi)
**Opened:** 2026-07-17
**For:** Architect (Amara)
**Blocks:** Phase 2 implementation (parser/generator/compiler) — Baraka should not start until these are ruled.

## Context

Phase 2 (Rule pipeline MVP — [`Roadmap/phase-2.md`](../../Roadmap/phase-2.md),
[`design/rule-pipeline.md`](../../design/rule-pipeline.md)) fills in the three empty
`GeneratorServices` interface bodies (`IRuleParser`, `IRuleIRGenerator`, `IRuleCompiler`) and
`RuleExport`. Verified current state: `RulePipeline`, `RuleParseContext.FromAssembly`, and all DTOs
(`RuleIR`, `RuleIRGroup`, `RuleCodeUnit`, `RuleCodeClass`, `RulePrototype`) already exist; only the
impls + entry point are missing.

Four design seams surfaced during the breakdown that are the Architect's call and must be settled
before implementation, because they change the shape of the code Baraka writes. Rule on each and
record the decisions in `design/rule-pipeline.md` so implementation has a coherent target.

## Decisions needed

**D1 — IR ↔ Prototype impedance mismatch (blocks the parser).**
`RulePrototype` (author surface) models a rule as **statement lists**: `PreambleStatements`,
`WhenStatements`, `ThenStatements`, plus `RequiredUsings`. `RuleIR` (parsed IR) models it as **two
opaque strings** — `ConditionCode` / `ActionCode` — with **no slot for preamble or usings**. So
`RulePrototype → RuleIR` is lossy today: the preamble variable declarations and required usings have
nowhere to land. Decide the `RuleIR` shape — grow it to carry preamble/usings (and possibly make
condition/action lists to match the prototype), or another mapping. This is the first thing the
parser trips on.

**D2 — Compiler MVP semantics (materially sizes the phase).**
The roadmap says the MVP compiler may "emit `.cs` at minimum; Roslyn `.dll` later." But the done-bar
(B4) says the generated rule "loads and fires" — you can't load-and-fire a `.cs` file written to disk
within the same test run. So the MVP compiler must either (a) do **in-memory Roslyn compilation** to a
loadable assembly, or (b) the phase is redefined as "emit `.cs` → separate build step → a later test
loads the built assembly." These are very different amounts of work; "Roslyn later" as written
contradicts the done-bar. Pick one and correct the roadmap/design wording to match. (Arryn was offered
this call and routed it here.)

**D3 — Reference assignment for generated rules.**
Generated `ARule`s must set `public override ILunaFlowReference Reference`. Nine Rooms did this by hand
(`new NineRoomsReference("Room1Challenge")`). Decide how the generator assigns it — e.g. a dotted path
from `GroupName` + rule `Name` — and what reference type it instantiates.

**D4 — Generated namespace convention.**
`RuleCodeUnit.Namespace` defaults to `LunaFlow.GeneratedRules`, but `design/rule-pipeline.md`'s "Output
conventions" says `LunaFlow.Rules.Generated.[ModuleName]`. Reconcile to one.

## Target the decisions must support

The Phase 2 benchmarks (see `phase-2.md`): B1 parser→correct IR (incl. `[StandingRule]` capture),
B2 generator→compiling source, B3 `.Not<T>()` guard injected, B4 end-to-end load+fire, **B5 parity**
(generated rule fires identically to an equivalent hand-written rule). D1–D4 should each be resolvable
in a way that keeps B5 achievable.

## Out of scope (do not design here)

Runtime/script mode, the ROM/Rulebook component model, config-driven loading, and all five of
`rule-pipeline.md`'s "open design questions" (rule scoping, prioritization, versioning, validation
pass, `RuleTester`). Compile-time C# path only.

## Response

All four ruled. Full reasoning lives in `design/rule-pipeline.md`'s new
["Design decisions (Phase 2 pre-implementation)"](../../design/rule-pipeline.md#design-decisions-phase-2-pre-implementation)
section; summary here:

- **D1:** `RuleIR` grows `RequiredUsings` (`List<string>`) and `PreambleCode` (`string`), closing
  the lossy `RulePrototype → RuleIR` gap. `ConditionCode`/`ActionCode` stay opaque strings — this
  doesn't reopen the already-settled "no structured expression tree" call.
- **D2:** in-memory Roslyn compilation, not "emit `.cs`, Roslyn later" — the roadmap wording
  contradicted its own B4 done-bar (same-run load+fire). Caught a second, sharper gap along the
  way: `IRuleCompiler.Compile`/`RulePipeline.Compile` were both `void` — no return value could get
  a compiled assembly back to a caller *regardless* of which compiler strategy got picked. Both
  now return `Assembly`. Needs a new `Microsoft.CodeAnalysis.CSharp` package reference on
  `LunaFlow.Rules` when implemented — flagged as real added cost, not hidden.
- **D3:** new `GeneratedRuleReference` (`LunaFlow.Rules.GeneratorServices`, `public` — has to be,
  since generated source compiles into a separate in-memory assembly, unlike same-assembly
  `NineRoomsReference`). Flat `GroupName.RuleName` dotted path, `Parent => null`. Real hierarchy
  deferred to `non-goals.md` until `IRuleBook`/`IRuleGroup` have real bodies.
- **D4:** `LunaFlow.Generated.[GroupName]`, generator-assigned. Splits the difference between the
  code's flat `LunaFlow.GeneratedRules` default (namespace collision risk across rulebooks) and the
  doc's unimplemented `LunaFlow.Rules.Generated.[ModuleName]` (misleadingly reads as framework-owned
  code; "ModuleName" isn't a field that exists on `RuleIRGroup`).

Also touched, since D1/D3/D4 pin down exact shapes rather than leaving them as prose: updated the
actual DTOs/interfaces (`RuleIR.cs`, `IRuleCompiler.cs`, `RulePipeline.cs`, `RuleCodeUnit.cs`) and
added `GeneratedRuleReference.cs` — all shape-only, zero pipeline logic, so `rule-pipeline.md` and
the code it describes don't drift apart before Baraka even starts. Did **not** touch
`IRuleParser`/`IRuleIRGenerator`/`IRuleCompiler` implementations themselves — that's the
Phase 2 work this ticket blocks, not this ticket's job. `Roadmap/phase-2.md` updated to match (D2
wording, status line). Added one `non-goals.md` entry (D3's deferred hierarchy) and one
`glossary.md` entry (`GeneratedRuleReference`).

## Resolution

Verified and closed by Kofi (opener), 2026-07-17. All four rulings sound; code and
`design/rule-pipeline.md` are consistent (verified by reading — PM has no toolchain, did not build).

- **D1** — `RuleIR.cs` grew `RequiredUsings` (`List<string>`) + `PreambleCode` (`string`); opaque
  condition/action strings retained. Docstrings correctly assign parser-copies / generator-unions
  responsibility.
- **D2** — `IRuleCompiler.Compile` and `RulePipeline.Compile` now return `Assembly` (both were
  `void` — a real blocker Amara caught beyond the original ask). In-memory Roslyn; Roslyn package
  cost flagged for implementation time, not now.
- **D3** — `GeneratedRuleReference` added, `public` (justified: generated source compiles to a
  separate assembly), flat `GroupName.RuleName`, `Parent => null`; real hierarchy deferred to
  `non-goals.md`.
- **D4** — `RuleCodeUnit.Namespace` default now `LunaFlow.Generated`, generator sets
  `LunaFlow.Generated.[GroupName]`; design doc's output conventions + example match.
- **B5 unaffected:** parity depends on `When`/`Then`, not `Reference`, so the generated-vs-handwritten
  comparison stays achievable under D3.

**Boundary note:** these edits touched `src/` DTOs/interfaces — the architect-doesn't-edit-`src/`
rule was made explicit *after* this ticket and grandfathers 0007 as the one-time exception (see
`Roles/architect.md`). No implementations were written (correctly left for Baraka). Build not
independently confirmed here; shape changes have no existing implementors, so no break is expected —
Baraka confirms green as the first implementation step.

Phase 2 implementation (parser/generator/compiler + `RuleExport`, against B1–B5) is now unblocked.
