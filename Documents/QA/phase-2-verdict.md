# Phase 2 (Rule Pipeline MVP) — Final QA Verdict

**Verdict:** PASS — no manual-only gap this phase, done-bar fully automated and verified
**Date:** 2026-07-19
**Ticket:** [handoff 0011](../ClaudeWiki/Processes/Handoffs/0011-phase-2-final-verdict.md)
**Build:** `dotnet build LunaFlow_A.sln` — **Build succeeded, 0 Errors, 4 Warnings** (same pre-existing `CS0219` unused-variable warnings as Phase 1.5's verdict, unrelated to the rule pipeline).
**Suite:** **71 Passed, 0 Failed, 0 Skipped** — from a full run performed this session
(`dotnet test tests/LunaFlow.EngineTests --settings LunaFlow_A.runsettings`), read directly from
the regenerated `latest.trx`'s `<Counters>` element, not inherited from ticket 0010's text.
`ParserReflection_ThrowsOnMultipleGroups` (the B1 cardinality test) is present and passing.

## Done-bar confirmation (`phase-2.md` B4 + B1–B5)

Read `tests/LunaFlow.EngineTests/GeneratorServices/RulePipelineGeneratorServiceTests.cs` and
`TestRuleFixtures.cs` directly, and cross-checked the assertions against the actual pipeline source
(`ReflectionRuleParser.cs`, `RuleCodeGenerator.cs`, `RoslynRuleCompiler.cs`, `RuleExport.cs`) rather
than trusting the ticket's description of what the tests do.

| Benchmark | What the test actually asserts | Behavioral or fixture-data? | Result |
| --- | --- | --- | --- |
| **B1** — Parser → IR | `ParserReflection_ProducesCorrectRuleIRGroup` + `ParserReflection_UnionAndDedupStandingRules`: `GroupName`, rule count, per-rule fields; `MultiGuardedRule.StandingRules.Count == 1` (same type, class+property → deduped) vs. `UnionGuardedRule.StandingRules` contains both `BlockingMarker` and `SecondBlockingMarker` (distinct types → union). Matches `ReflectionRuleParser.Parse`'s actual `.Concat(...).Distinct()` line by line. | Behavioral (asserts real parser output) | PASS |
| **B1 cardinality** | `ParserReflection_ThrowsOnMultipleGroups` — `[ExpectedException(ArgumentException)]` on a 2-group context. Matches `ReflectionRuleParser`'s explicit `if (RuleGroupTypes.Count > 1) throw`. | Behavioral | PASS |
| **B2** — Generator → compiling source | `GeneratorAndCompiler_ProducesLoadableAssembly`: non-null `Assembly`, correct `Namespace`/`FileName`/class names, `RequiredUsings` deduped. | Mostly structural/behavioral (compiles real source, checks real output) | PASS |
| **B3** — `.Not<T>()` injection | `GuardInjection_PreventsFireWhenMarkerPresent` + `GuardInjection_UnionBlocksBothTypes`: inserts the marker fact, fires, asserts the guarded rule produced **zero** observable effects; retracts, fires again, asserts it **does** fire. Tests both guard types independently. | Behavioral (fires real compiled rules, checks real fact effects) | PASS |
| **B4** — End-to-end | `EndToEnd_CompiledRuleFires`: `RuleExport.Compile` → `RuleEngine.LoadRulesFromAssembly` → insert `Counter` → `Fire()` → asserts both `RuleFiredMarker`s exist. Exercises the real author-facing entry point, not a test-only shortcut. | Behavioral | PASS |
| **B5** — Parity | `Parity_GeneratedVsHandWritten`: generated `CounterRule` vs. a genuinely separate hand-written `ARule` (`HandWrittenCounterRule`), same facts, same-count firing outcome asserted for both, independently. | Behavioral | PASS |

No fixture-data tautologies found in this file — every assertion above traces to an actual fired
rule's effect or actual parser/generator output, not to re-asserting input data. This is a real
improvement over the first two rounds of the Phase 1.5 fixture (ticket 0003), and this session's
read confirms the 0010 reviewer findings were accurate, not just claimed.

## No `NotImplementedException`

`grep -rn NotImplementedException src/` — no matches, checked this session.

## Known limitation — recorded, not re-litigated

`rule-pipeline.md`'s "Known limitations (MVP)" section states `RoslynRuleCompiler` references
*every currently loaded, file-backed assembly in the `AppDomain`* rather than doing real reference
tracking. Read `RoslynRuleCompiler.cs` directly — `GetReferences()` does exactly that
(`AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic && ...)`), matching the doc's
description precisely. Disclosed and accepted per the doc; not treated as a defect here.

## Deviations from spec

None found. `RuleExport.Compile`/`CompileAll`, `RuleCodeGenerator`'s namespace
(`LunaFlow.Generated.{GroupName}`, per D4) and `GeneratedRuleReference` usage (per D3), and the
`RequiredUsings` baseline-union-dedup behavior (per the 0008/0009 implementation fork) all match
`rule-pipeline.md` as read against the actual source.

## Superseded reports

None — this is the first Phase 2 verdict.

## Tickets filed

None. No failing or unproven criteria.

## Recommendation to PM

Phase 2 done-bar is met, fully automated, freshly re-verified (not inherited). Kofi may mark it
done on the roadmap citing this verdict.
