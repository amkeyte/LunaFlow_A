# 0026 — Reconcile 1-d20-spec.md against the shipped Phase 2.6 split

**Status:** resolved
**Opened by:** Supreme Leader (Kofi)
**Opened:** 2026-07-22
**For:** Game Designer (Juno)
**Origin:** [0021](0021-phase-2-6-spec-alignment.md) item 2, parked until the retrofit landed — it has
(impl [0022](0022-implement-phase-2-6.md), QA PASS [`phase-2-6-verdict.md`](../../../QA/phase-2-6-verdict.md),
2026-07-22). This is the ticket 0021 promised.

## Context

`1-d20-spec.md` is the spec Baraka builds 1.d20 work against, and it now **contradicts merged,
QA-passed code** — the erroneous-pickup risk. Found while scoping this ticket:

- It frames the five command handlers as Scenario-tier, hand-written `ICommandHandler`s "the same
  tier as Nine Rooms' checked-in `MoveCommandHandler`" (lines ~76, ~186, ~271). Phase 2.6 moved all
  five (`move`/`look`/`check`/`fight`/`flee`) into ROM as `LunaFlow.Rom.D20System` — ROM-tier, per
  2.6's sharpened dividing line ("writing an `ICommandHandler` at all *is* direct system access, so
  it's ROM-tier"). The spec's tier framing is now wrong.
- It points at the old flat path `src/LunaFlow.Rom/OneD20/` (line ~335). That namespace is gone —
  QA confirmed no `LunaFlow.Rom.OneD20` flat namespace remains in `src`.

Fact *shapes* are unchanged (0022 verified) — this is a reconcile-to-reality pass on
tier/ownership/path framing, **not a redesign**.

## Ask

Bring `1-d20-spec.md` into agreement with the 2.6-as-shipped structure. The settled facts to fix:

1. **Handler tier.** The five handlers are ROM-tier, in `LunaFlow.Rom.D20System` (`Handlers/`), not
   Scenario-tier hand-written `ICommandHandler`s. Rework the tier framing (lines ~76, ~186, ~271) so
   the spec stops describing them as "same tier as Nine Rooms' `Move`/`Look`."
2. **Paths/names.** `src/LunaFlow.Rom/OneD20/` → `src/LunaFlow.Rom/D20System/`; the flat `OneD20`
   namespace is gone (`D20SystemContent`, `D20SystemNarration`, `D20ResolutionMechanic`, etc. live
   under `D20System`). Line ~335 and anywhere else the old path/namespace appears.
3. **Declarative rule group.** Renamed `D20SystemRuleGroup` (`[RuleGroup("D20System")]`) — update any
   `OneD20RuleGroup` references.
4. **`IRuleBook` / Scenario data.** `IRuleBook` now carries real shape (`IHasLunaFlowReference` +
   `DependsOnRoms`); the Scenario content is `LunaFlow.Rulebook.OneD20`, pure data.

**Flag, don't pin — these are in flux and Phase 2.6.5 moves them.** Note them as transitional so you
don't hard-spec something about to change, and so you don't have to redo the settled parts above when
2.6.5 lands:

- `D20SystemContent.Configure(...)` exists today but is **deprecated as of 2.6.5** (superseded by
  `IInteropRom`). Mark it transitional, don't enshrine it as durable spec.
- `IRuleBook`'s home is `LunaFlow.Rules` today but **relocates to `LunaFlow.Rulebook` in 2.6.5**. Note
  the pending move rather than pinning the namespace.

## Boundaries

- **Spec doc only** — `Documents/Specs/1-d20/1-d20-spec.md`, your territory. Don't edit `src/`
  (Baraka's) or the design/framework docs (Amara's).
- Reconcile-to-reality, not redesign — you're fixing tier/ownership/path framing, not reopening the
  content design.
- **Out of scope, still parked:** `phase-2-7.md`'s content-split framing (the Nine-Rooms-retrofit
  language and the "no project reference between them" Done-When) is separately stale, but stays
  parked until Phase 2.7 resumes per Arryn — not this ticket. Flag anything you spot for that future
  pass; don't fix it here.

## Done when

`1-d20-spec.md` accurately describes the 2.6-as-shipped structure (ROM-tier handlers in `D20System`,
correct paths/names, `IRuleBook` shape), with the two 2.6.5-in-flux items flagged transitional rather
than pinned. No `src/` or design-doc edits. Baraka can read the spec end-to-end without hitting a
claim the merged code contradicts.

## Response

Reconciled `1-d20-spec.md` against the shipped, QA-passed Phase 2.6 split. Fact shapes untouched —
this was a tier/ownership/path framing pass, not a redesign, per the ticket's boundaries.

**1. Handler tier** — fixed at all three cited call sites (Purpose's "Rule vs. handler" intro, the
"Command handlers" subsection heading, and the "Commands & handlers" section intro). Each now states
explicitly: ROM-tier, living in `LunaFlow.Rom.D20System` (`Handlers/`), per Phase 2.6's boundary
retrofit. Kept the existing "same tier as Nine Rooms' `MoveCommandHandler`" comparison rather than
cutting it — that comparison is still true post-2.6 (Nine Rooms' handlers are `LunaFlow.Rom.NineRooms`,
also ROM-tier), it was just ambiguous about *which* tier that was before this edit made it explicit.

**2. Paths/names** — `src/LunaFlow.Rom/OneD20/` → `src/LunaFlow.Rom/D20System/` in the `IMechanic`
Framework-dependencies bullet (the only place the spec named a concrete `src/` path). Verified against
real source (`Glob` on `src/LunaFlow.Rom/D20System/`): `Handlers/` holds all five handlers,
`Rules/D20SystemRuleGroup.cs` holds the rule group, `D20ResolutionMechanic.cs`/`D20SystemContent.cs`/
`D20SystemNarration.cs`/`D20SystemReference.cs`/`Facts.cs` sit alongside — matches the ticket's
description exactly. One `OneD20RuleGroup` reference remains, in the dated 2026-07-20 playtest
revision note (line ~19) — left as-is; it's historically accurate (that's the class ticket 0016's
playtest build actually read against, pre-2.6) and revision notes are a dated log, not current-state
claims, so overwriting it would falsify the history rather than fix a stale fact.

**3. Declarative rule group rename** — checked: the spec had no other current-tense reference to
`OneD20RuleGroup`/`D20SystemRuleGroup` outside the historical note above, so there was nothing else to
rename. Added the current name (`D20SystemRuleGroup`, `[RuleGroup("D20System")]`) into the new
Framework-dependencies bullet below so it's stated somewhere current, not just implied.

**4. `IRuleBook` / Scenario data** — the spec previously made no claim at all about `IRuleBook`'s shape
or where Scenario data lives, so there was nothing contradicted to fix. Added a new Framework-
dependencies bullet stating the real 2.6-as-shipped structure (`LunaFlow.Rulebook.OneD20` holds room/
PC/monster/flavor data, pure data, zero `LunaFlow.Engine` references; `IRuleBook` — still in
`LunaFlow.Rules` today, verified by `Grep` — declares `DependsOnRoms`) plus the two 2.6.5-in-flux
items flagged transitional per the ticket's explicit ask: `IRuleBook`'s pending move to
`LunaFlow.Rulebook`, and `D20SystemContent.Configure(...)`'s deprecation in favor of `IInteropRom`.
Neither is pinned as durable spec — both point at `phase-2-6-5.md` instead of asserting a namespace
that's about to change.

Also added a dated reconcile note at the top of the doc (matching its existing revision-history
convention) and two entries to Related documents (tickets 0022, 0026), so a reader lands on the "why"
before hitting the fixed claims.

**Verified against real source, not just the ticket's word:** `Glob`'d `src/LunaFlow.Rom/D20System/`
(confirms `Handlers/`, `Rules/D20SystemRuleGroup.cs`, and the five ROM-tier support files) and
`Grep`'d `IRuleBook`'s definition in `src/LunaFlow.Rules/IRuleBook.cs` (confirms it's still there
today, not yet moved — matches the "flag, don't pin" instruction rather than jumping ahead of 2.6.5).
`src/LunaFlow.Rulebook/` and any pre-split `OneD20/` folder under `LunaFlow.Rom` were not independently
re-verified beyond ticket 0022's own disclosed grep/build evidence, which already confirmed both
(zero `LunaFlow.Engine` references in Rulebook source; no `OneD20/` directory remaining in `Rom`) —
re-deriving that here would have been redundant, not a gap.

Per the ticket's boundaries: no `src/` edits, no design-doc edits, `phase-2-7.md`'s separately-stale
content-split framing left parked as instructed.

## Resolution

Closed by Kofi 2026-07-22. Verified against the actual spec, not Juno's summary:

- **Handler tier** — fixed at all three sites: the Purpose "Rule vs. handler" intro (line ~86), the
  "Command handlers" subsection heading (~197), and the "Commands & handlers" intro (~282) all now
  read ROM-tier, `LunaFlow.Rom.D20System` (`Handlers/`). ✓
- **Path/name** — `src/LunaFlow.Rom/OneD20/` → `src/LunaFlow.Rom/D20System/` (~347, ~354);
  `D20SystemRuleGroup` / `[RuleGroup("D20System")]` now stated current-tense (~353). ✓
- **`IRuleBook`/Scenario** — new Framework-dependencies bullet (~356–362) describes
  `LunaFlow.Rulebook.OneD20` (pure data) + `IRuleBook`'s `DependsOnRoms`, with the two 2.6.5-in-flux
  items flagged transitional and pointing at `phase-2-6-5.md` rather than pinned. ✓
- **Dated reconcile note** (~65–73), in the doc's own revision-history block. ✓

Three judgment calls, all endorsed: (1) she kept the "same tier as Nine Rooms' `MoveCommandHandler`"
comparison rather than cutting it — verified sound, Nine Rooms' handlers are `LunaFlow.Rom.NineRooms`,
also ROM-tier, so the comparison is true once disambiguated; (2) she left the one surviving
`OneD20RuleGroup` reference inside the dated 2026-07-20 playtest note untouched — correct, that's a
historical log of what was checked then, and overwriting it would falsify the record, not fix a stale
fact; (3) flag-don't-pin honored exactly. Done-bar met: no `src/` or design-doc edits, `phase-2-7.md`
framing left parked, and Baraka can now read the spec without hitting a claim the merged code
contradicts.

**Separate observation, not part of this close (flagged to Juno/Arryn):** the spec's own header still
carries content-spec status `in review`, with the bar "moves to `active` once implemented and
QA-verified against the Acceptance criteria." 1.d20's acceptance criteria were implemented and
QA-passed at Phase 2.5 ([0020](0020-phase-2-5-final-verdict.md)), so that bar may already be met —
whether to advance the spec to `active` is Juno's call on her own doc's lifecycle, out of scope here.
