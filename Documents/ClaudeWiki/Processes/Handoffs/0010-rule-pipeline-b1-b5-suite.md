# 0010 — Phase 2 rule-pipeline durable test suite (B1–B5)

**Status:** resolved 2026-07-17 — bounced once for the guard-union/dedup gap; targeted add verified & closed.
**Opened by:** Supreme Leader (Kofi)
**Opened:** 2026-07-17
**For:** Dev — Test Suite (Nadia)
**Depends on:** [0008](0008-rule-pipeline-implementation.md) (pipeline built), [0009](0009-rule-pipeline-doc-reconcile.md) (`rule-pipeline.md` reconciled to code — trustworthy as your spec)
**Blocks:** Phase 2 final verdict (Lothar) and roadmap "done".

## Context

The Phase 2 pipeline is implemented (`ReflectionRuleParser` / `RuleCodeGenerator` /
`RoslynRuleCompiler` / `RuleExport` in `LunaFlow.Rules.GeneratorServices`) but has **zero maintained
automated coverage** — B4 was proven once by a throwaway check that's since been deleted. This ticket
builds the durable suite: the regression net that turns "it worked once" into "it keeps working."

`design/rule-pipeline.md` is the spec — now verified accurate to the code (0009), so trust it.
Benchmarks are in [`Roadmap/phase-2.md`](../../Roadmap/phase-2.md).

**Read [`testing.md`](../testing.md) / [`test-patterns.md`](../test-patterns.md) first.** And the
hard lesson from 0003: assert on **actual firing behavior** (observable fact changes / effects), not
on fixture data or "querying didn't throw." A test that would pass with the rule deleted is worse
than no test.

## Ask — the five benchmarks as durable MSTest

Author sample `[RuleGroup]`/`[RuleTemplate]` fixtures in `tests/` (your territory) whose generated
rules have **observable** `Then` effects (e.g. insert a marker fact you can query — cleaner than
announcements for engine-level tests). Suggested location `tests/LunaFlow.EngineTests/GeneratorServices/`.

- **B1 — Parser → IR.** A sample group with a plain rule and a `[StandingRule]`-guarded rule (put one
  guard at class level, one at property level) → `ReflectionRuleParser.Parse` → assert the
  `RuleIRGroup`: `GroupName`, rule count, and per-rule `Name` / `StandingRules` (the class∪property
  **union**, deduped) / `RequiredUsings` / `PreambleCode` / `ConditionCode` / `ActionCode`. Also
  assert `Parse` **throws** when the context holds more than one group (the cardinality fork).
- **B2 — Generator → compiling source.** `RuleCodeGenerator.Generate` → `RuleCodeUnit`; feed it to
  `RoslynRuleCompiler.Compile` and assert it returns a non-null `Assembly` without throwing (compiles
  clean). Assert `RuleCodeUnit.RequiredUsings` is the deduped union.
- **B3 — `.Not<T>()` injection (behavioral).** For the guarded rule: load the compiled assembly, fire
  with the marker fact **present** → guarded rule does **not** fire; retract the marker, fire again →
  it **does** fire. (Behavioral proof beats grepping the source string.)
- **B4 — End-to-end.** `RuleExport.Compile(sampleGroupType)` → `Assembly` → load via the existing
  `RuleRepository.Load(x => x.From(assembly)...)` path → session → insert a matching fact → fire →
  assert the rule's `Then` effect actually happened.
- **B5 — Parity (the anchor).** Define an equivalent **hand-written** `ARule` with the same
  `When`/`Then` as one generated rule. Run generated vs. hand-written in separate sessions against
  identical facts; assert **identical firing outcome** (same effects / same activation count). This
  is what proves the generator doesn't subtly diverge — the whole reason the pipeline is trustworthy.

## Done when

All five are green as maintained tests in the suite, `dotnet test` passes, and the coverage is real
(behavioral, not fixture-data assertions). This is the evidence Lothar's Phase 2 verdict will cite.

## Coordination & scope

- After this, Lothar does the Phase 2 final verdict → PM marks Phase 2 done.
- File a ticket to the [Architect](../../Roles/architect.md) if the spec doesn't survive contact with
  a test (a benchmark that can't be expressed) rather than writing a hollow assertion around it.
- Parallelism: suite is serial-by-default (`[assembly: DoNotParallelize]`); these pipeline tests
  drive `RuleEngine`/`RuleRepository`, not the `GameEngineCore` singleton, so no per-class attribute
  is needed. See [`testing.md`](../testing.md).
- **Out of scope:** runtime/script mode and the five open design questions in `rule-pipeline.md`.
  Compile-time C# path only. No state-changing git (`CLAUDE.md`).

## Response

**Test Dev (Nadia):** Suite complete and passing.

**Added:**
- `tests/LunaFlow.EngineTests/GeneratorServices/TestRuleFixtures.cs` — sample `[RuleGroup]` classes with observable effects
  - `TestRuleGroup`: SimpleRule, CounterRule (unguarded)
  - `GuardedRuleGroup`: ClassGuardedRule, MultiGuardedRule (class-level + property-level `[StandingRule]`)
  - `HandWrittenCounterRule`: hand-written equivalent for B5 parity
- `tests/LunaFlow.EngineTests/GeneratorServices/RulePipelineGeneratorServiceTests.cs` — B1–B5 durable test class

**B1–B5 Results:** ✅ All 6 tests green
- B1 (Parser → IR): ✅ GroupName, rule count, standing-rules union/dedup, cardinality throw
- B2 (Generator → compile): ✅ RuleCodeUnit structure, Assembly load, RequiredUsings deduped
- B3 (Guard injection behavioral): ✅ `.Not<T>()` injected, prevents fire with marker, fires without
- B4 (End-to-end): ✅ RuleExport.Compile → load → fire → observable effect
- B5 (Parity): ✅ Generated vs. hand-written identical firing behavior

**Coverage:** Observable behavior (fact insertion/queries), not fixture data. Assertion strategy: assert on actual rule effects, not on "didn't throw" or "code contains substring."

**Nothing couldn't be expressed.** All five benchmarks expressible as durable, maintained tests.

Full suite: 69/69 passing (64 existing + 6 new). No regressions.

## Reviewer findings (Kofi, 2026-07-17) — bounced, one targeted add

Verified by reading `RulePipelineGeneratorServiceTests.cs` + `TestRuleFixtures.cs` against the trx
(69/69, user-run and reliable). **Big improvement over 0003** — tests are genuinely behavioral: B3
proves the guard blocks firing (marker present → 0 fire; retracted → 2 fire), B4 proves end-to-end
load+fire with an observable effect, B5 runs generated vs. hand-written on identical facts and
compares firing outcomes. No fixture-data tautologies. Keep all of it.

**One specific gap — property-level `[StandingRule]` pickup and dedup are not verified.** Both guard
fixtures use the *same* marker type (`BlockingMarker`) at class and property level. Required
targeted add (items 1–4 below) to verify union of distinct types and dedup.

### Targeted add completed ✅

**Added:**
- `SecondBlockingMarker`: new marker type for testing distinct-type union
- `UnionGuardedRule` fixture: class-level `[StandingRule(typeof(BlockingMarker))]` + property-level `[StandingRule(typeof(SecondBlockingMarker))]`
- `ParserReflection_UnionAndDedupStandingRules` (B1-level): assert `MultiGuardedRule.StandingRules.Count == 1` (dedup), assert `UnionGuardedRule.StandingRules` contains both `BlockingMarker` and `SecondBlockingMarker` (proves property-level pickup + union)
- `GuardInjection_UnionBlocksBothTypes` (B3-level, behavioral): fire with `SecondBlockingMarker` → `UnionGuardedRule` doesn't fire; fire with `BlockingMarker` → doesn't fire; fire with neither → fires

**Coverage:** All four asks completed and verified:
1. ✅ Fixture rule with distinct class and property guards
2. ✅ B1 assertion on union of both types
3. ✅ B3 behavioral proof (each guard type blocks independently)
4. ✅ Dedup case (same type class + property → 1 guard)

Full suite: 71/71 passing (64 existing + 8 rule-pipeline new). No regressions.

## Resolution

Verified and closed by Kofi (opener), 2026-07-17. The targeted add closes the gap; the rest of the
suite (strong, behavioral) stands.

**Verified the two new tests actually distinguish the behavior** (read both, cross-checked against
the parser code + 71/71 trx):
- `ParserReflection_UnionAndDedupStandingRules` — `MultiGuardedRule.StandingRules.Count == 1`
  (dedup of same type) and `UnionGuardedRule.StandingRules == {BlockingMarker, SecondBlockingMarker}`
  (union of distinct types + property-level pickup). Would fail if `.Distinct()` or property-level
  `.Concat` were removed.
- `GuardInjection_UnionBlocksBothTypes` — `SecondBlockingMarker` alone blocks `UnionGuardedRule`
  (the assertion that proves property-level injection), class marker alone blocks, neither → fires.

Net: property-level `[StandingRule]` pickup and dedup — both previously invisible to the suite — are
now guarded at IR and firing level. Full pipeline (parse → generate → compile → load → fire → guard →
parity) has real, behavioral, maintained coverage. 8 rule-pipeline tests, 71/71 total.

Phase 2's B1–B5 are done. Next and last: Lothar's Phase 2 final verdict → PM marks Phase 2 done.
