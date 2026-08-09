# 0009 — Reconcile rule-pipeline.md with the two Phase 2 implementation forks

**Status:** resolved 2026-07-17
**Opened by:** Supreme Leader (Kofi)
**Opened:** 2026-07-17
**For:** Architect (Amara)
**Blocks:** Nadia's B1–B5 test suite should not be written against a stale spec — do this first.

## Context

Phase 2 implementation ([0008](0008-rule-pipeline-implementation.md), resolved) surfaced two design
seams the [0007](0007-rule-pipeline-design-decisions.md) rulings didn't cover. Baraka resolved both
correctly in code, but `design/rule-pipeline.md` doesn't yet describe them — so the doc now lags the
code. This is **doc-catches-up-to-code**: the decisions are made and shipped; update the design doc
to match. No code changes.

## Ask

Update `design/rule-pipeline.md` to reflect what's in `LunaFlow.Rules.GeneratorServices` today:

1. **Parser cardinality + entry points.** `IRuleParser.Parse` produces **one `RuleIRGroup` per
   call** and `ReflectionRuleParser` **throws** if the context holds more than one `[RuleGroup]`
   type (silently merging distinct `GroupName`s was rejected). Multi-group assemblies go through the
   author-facing entry point `RuleExport`: `Compile(Type)` for one group, `CompileAll(Assembly)`
   which loops per group yielding one compiled `Assembly` each. The doc's "Pipeline stages" /
   author-facing sections currently imply a single `Compile` and don't mention this cardinality rule
   or the `RuleExport` surface — reconcile.

2. **`RuleCodeUnit.RequiredUsings`.** D1 said the generator "unions/dedups `RuleIR.RequiredUsings`
   across every rule in a group when it writes one file's using block," but the *output* DTO
   (`RuleCodeUnit`) had no slot for it — Baraka added `RuleCodeUnit.RequiredUsings` (`List<string>`),
   the same move D1 made on `RuleIR`. The doc's IR/output-conventions section should list this field
   and where the union lands (generator populates baseline framework usings + every rule's usings,
   deduped; compiler consumes it for the file's `using` block).

3. **Optional — record the reference-tracking MVP as a known limitation.** `RoslynRuleCompiler`
   references *every currently-loaded, file-backed assembly* as a pragmatic stand-in (the doc
   doesn't specify how a generated rule's domain-fact types get resolved). Worth a one-line "known
   MVP limitation / future refinement" note so it's a recorded decision, not a surprise. Your call
   whether it rises to that.

## Done when

`design/rule-pipeline.md` describes the pipeline as actually built — a reader (i.e. Nadia writing
B1–B5) can trust it without cross-checking the source for these two points.

Repo convention (`CLAUDE.md`): no state-changing git. This is a `Documents/` doc edit only.

## Response

Read all four implementation files (`ReflectionRuleParser`, `RuleCodeGenerator`,
`RoslynRuleCompiler`, `RuleExport`) plus the updated `RuleCodeUnit` before writing anything, so the
doc describes what's actually there rather than the ticket's summary of it. `design/rule-pipeline.md`
updated, doc-only, no code touched:

1. **Parser cardinality + entry points.** "Pipeline stages" now states the one-`RuleIRGroup`-per-call
   rule explicitly and documents `RuleExport.Compile(Type)`/`CompileAll(Assembly)` as the real
   compile-time entry points (compile-time mode isn't reached by constructing `RulePipeline`
   directly). Folded the *why* into a new "Implementation forks (Phase 2 build)" subsection
   alongside D1–D4, rather than burying it in prose — same treatment as the D-decisions, since this
   is the same kind of "the spec didn't survive contact, here's the resolved gap" record.
2. **`RuleCodeUnit.RequiredUsings`.** Added to the "Intermediate representation" bullet list and to
   the same "Implementation forks" subsection — what it holds (baseline framework usings ∪ every
   rule's `RequiredUsings`, deduped) and who populates/consumes it (`RuleCodeGenerator` /
   `RoslynRuleCompiler`).
3. **Reference-tracking MVP — included.** Added a "Known limitations (MVP)" section. Call: worth
   recording, not just a code comment — it's the kind of thing that'll look like a mystery bug
   ("why did this generated rule fail to compile") to whoever hits it, rather than a known,
   accepted MVP tradeoff, if it's not written down somewhere a doc reader would find it.

**Two more inconsistencies caught while cross-checking the doc against the real generator output**
(not in the ticket's ask, but the same doc-drift problem, so fixed in the same pass rather than
filing a fourth item back to Kofi for something already in scope):
- Status line and "Purpose"/"Pipeline stages" text still described the pipeline as unimplemented
  ("no concrete implementation yet," `RuleExport` as "an empty stub") — updated to reflect ticket
  0008's build, including the "zero maintained coverage until Nadia's suite" caveat so this doc
  doesn't overstate done-ness relative to 0008's own resolution notes.
- "Output conventions"' registry-class-name example said `CombatRules_Generated` — the real name is
  derived from the namespace's last segment (`GroupName`, e.g. `"Combat"`) plus `_Generated`, not
  from the author's C# class name (`CombatRules`). Fixed to `Combat_Generated` with a note on why.

Also touched `glossary.md`'s `RuleCodeUnit` entry (added "using block" to what it holds) — one
clause, same drift category.

## Resolution

Verified and closed by Kofi (opener), 2026-07-17. Doc now matches the code — spot-checked each
claim against the actual `GeneratorServices` source (read during 0008 review):

- Baseline usings in the doc == `RuleCodeGenerator`'s baseline array (exact match).
- `Combat_Generated` registry naming == `RoslynRuleCompiler.BuildSource`'s
  `Namespace.Split('.').Last() + "_Generated"`.
- Cardinality-throws, `RuleCodeUnit.RequiredUsings`, and the reference-tracking limitation all
  accurately describe the implementation.
- Status line honestly flags "no maintained coverage yet," consistent with 0008's resolution.

Amara stayed in-territory (doc-only, no `src/`) and caught two extra drift items beyond the ask
(stale "unimplemented" status text; the `CombatRules_Generated` example error). `glossary.md`
`RuleCodeUnit` entry also updated.

`rule-pipeline.md` is now trustworthy as the spec Nadia writes B1–B5 against. Next: Nadia's suite.
