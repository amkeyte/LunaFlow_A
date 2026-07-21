# 0022 — Implement Phase 2.6 (ROM/Rulebook boundary fortification)

**Status:** resolved
**Opened by:** Supreme Leader (Kofi)
**Opened:** 2026-07-20
**For:** Dev — Implementation (Baraka)
**Depends on:** [Phase 2.6](../../../Roadmap/phase-2-6.md) (Amara, design complete),
[0021](0021-phase-2-6-spec-alignment.md) (Juno's split confirmation, item 1 — see note below, the
specific design it confirmed has since changed).

**Rewritten 2026-07-20, before any response landed — not a patch.** The original version of this
ticket was built off an earlier draft of `phase-2-6.md` that Amara has since revised after further
design discussion with Arryn. The old draft split by "is the logic generic" (`check`/`fight`
System-tier, `move`/`look`/`flee` Scenario-tier, with a new fact-API layer for the former to route
through). The current `phase-2-6.md` calls that distinction wrong and replaces it with a sharper
one: **direct system access, not genericness, is the dividing line.** All five command handlers
move to ROM. No fact-API layer is needed. If you started work off the old version of this ticket,
stop and re-read `phase-2-6.md` fresh before continuing — don't try to reconcile the two versions
yourself.

## Context

Bigger picture, so this doesn't read as a rules change for its own sake: talking through Phase 4
surfaced that scripted content can't be authored against a C# ecosystem that hasn't actually drawn
the boundaries it claims to have. This phase (and 2.7, 2.8 after it) aren't new architecture so
much as finally building the bones `architecture.md` has described since before any real content
existed — poorly captured until now, not changed. Phase 4 needs that foundation solid before a
World-Builder script has anything real to be scoped against.

1.d20 shipped flat: `D20ResolutionMechanic`, room content, the PC, and the rule group all live
undivided in `LunaFlow.Rom.OneD20`, and handlers call `LunaFlowEngine.Host.Rules` directly. Full
design and reasoning in [`phase-2-6.md`](../../../Roadmap/phase-2-6.md) — read that first, this
ticket doesn't restate the "why," only the "what."

**The dividing line:** a Rulebook never touches `LunaFlow.Engine` primitives directly — no
`ICommandHandler`, no `LunaFlowEngine.Host`, no `RuleEngine` — regardless of whether its specific
logic happens to be reusable. Writing a command handler at all *is* direct system access. All five
of 1.d20's handlers (`move`, `look`, `check`, `fight`, `flee`) are ROM-tier under this principle.

**Note on ticket 0021:** Juno reviewed and confirmed the *previous* System/Scenario split against
her intent in `1-d20.md`. That review is now against a superseded design — not wrong, just
overtaken. This is an architecture-authority correction, not a content-design one, so it likely
doesn't need her formal re-approval, but I'm looping her in separately so she's not carrying a
stale picture. Not blocking you — noted here for the record.

## Ask

1. **Move all five command handlers into the ROM project.** Working namespace
   `LunaFlow.Rom.D20System`, replacing today's flat `LunaFlow.Rom.OneD20`. Parameterize them to
   read Scenario-supplied data (room definitions, combatant stats, flavor-key lookups) rather than
   anything Department-7-specific — `check`/`fight` mostly already do this via `OneD20Content`;
   `move`/`look` need the same treatment relative to grid content. `Room`, `CombatantStats`,
   `CombatRound` — the fact *shapes* — move with the handlers as ROM vocabulary; Scenario only ever
   supplies instances of them.
2. **One ROM project, not two.** Grid/win-line navigation and d20 resolution both land in the same
   `LunaFlow.Rom` assembly for now — don't split them into separately-pluggable modules yet, no
   second consumer wants one without the other. Revisit only if that changes.
3. **Wire up `src/LunaFlow.Rulebook/`.** The project already exists on disk
   (`LunaFlow.Rulebook.csproj`, empty net9.0 class library) but **is not in `LunaFlow_A.sln`** —
   confirmed by grep, zero matches. Add it to the solution as part of this work, don't assume it's
   already wired. Scenario content becomes `LunaFlow.Rulebook.OneD20` there: the 9 rooms' specific
   skill/DC/flavor assignments, the monster's stats, the pregenerated PC's sheet, flavor text — data
   only. References `LunaFlow.Rom` (for the fact shapes and ROM types it instantiates) but no
   `LunaFlow.Rulebook` **source** uses a `LunaFlow.Engine` primitive — no `ICommandHandler`, no
   `LunaFlowEngine.Host`, no `RuleEngine`.

   **Correction (Kofi, on close):** the original wording here said "never `LunaFlow.Engine`, directly
   or transitively." That clause is wrong and unsatisfiable. ROM legitimately references Engine — it
   *is* the conduit, that's the whole point of the tier — so anything depending on ROM pulls
   `LunaFlow.Engine.dll` transitively by construction, and the retrofit correctly does exactly that
   (`LunaFlow.Rulebook.csproj` → `LunaFlow.Rom` → `LunaFlow.Engine`; `Engine.dll` lands in Rulebook's
   `bin/` as a transitive copy). The real bar — and what `phase-2-6.md` and this ticket's own Done-when
   actually state — is source-level: no Rulebook source touches an Engine primitive. That is met.
4. **No separate fact-API-mediation layer.** Once handlers live entirely in ROM, Scenario has no
   code path that touches facts at all — this falls out of item 1, it isn't its own piece of work.
   If you find yourself building one, stop — that's a sign item 1 isn't actually done.
5. **`IRuleBook` grows minimal real shape** — likely just `IHasLunaFlowReference` plus a declared
   list of which ROM(s) it depends on (static data, not full dependency-ordering machinery — that
   stays deferred per `non-goals.md`). Same "smallest interface that does the job" discipline
   ticket 0015 applied to `IMechanic`.
6. **Reconcile the declarative rule group** (`OneD20RuleGroup.cs`) against the new split — it moves
   to the ROM project too, still pipeline-authored, still subject to Phase 2.5's `GeneratorServices`
   condition. Code only. `1-d20-spec.md` itself is **not your call to edit** — separate ticket to
   Juno, filed once this lands. If the retrofit changes fact shapes enough that the spec will
   clearly be wrong, note it in your response so that ticket is accurate, but don't touch the doc.

**Explicitly not asked of you, don't build it:**

- A generic command-authoring mechanism for Scenarios that want a command no ROM provides. Per
  `phase-2-6.md`: that's the Rulebook author's problem to solve by writing a small ROM, not an
  engine concern.
- Correcting `IEngineHost`'s doc comment or recording the "no Rulebook touches engine primitives"
  rule in `architecture.md`/`engine-core.md` — those are Amara's docs to correct, not yours to edit.
- Host-wiring changes beyond what's needed to run 1.d20 — `Program.cs` referencing `LunaFlow.Engine`
  freely is fine, it's composition-root code, not Rulebook content.

## Done when

1.d20's existing acceptance criteria and the full test suite (96/96 as of
[0020](0020-phase-2-5-final-verdict.md), including 0017's 25 Phase2.5 tests) still pass unchanged.
`LunaFlow.Rulebook` contains **zero** references to `LunaFlow.Engine` — no `ICommandHandler`, no
`LunaFlowEngine.Host`, no `RuleEngine` — and is pure data plus an `IRuleBook` declaration. All
command handlers and declarative rules live in `LunaFlow.Rom` instead.

If any test needs to change to keep passing, that's a red flag worth explaining, not a routine
update — this is a structural retrofit, not a behavior change.

**If anything in `phase-2-6.md` turns out unbuildable or ambiguous**, that's a ticket to Amara, same
channel as always — not a silent workaround.

Repo convention (`CLAUDE.md`): no state-changing git commands.

## Response

Implemented per the rewritten ticket. Summary, then the details worth flagging.

**Structural split, as asked:**

- All five handlers (`move`/`look`/`check`/`fight`/`flee`) moved to `LunaFlow.Rom.D20System`
  (replacing flat `LunaFlow.Rom.OneD20`), along with the fact shapes (`Room`, `CombatantStats`,
  `PlayerCharacter`, `MonsterTemplate`, `PlayerPosition`, `RoomSolved`, `WinMarker`, `CombatRound`,
  `RoomFailed`, `PreviousRoomId`), `D20ResolutionMechanic`, and the declarative rule group (renamed
  `D20SystemRuleGroup`, `[RuleGroup("D20System")]`).
- `src/LunaFlow.Rulebook/` is wired into real use: `LunaFlow.Rulebook.OneD20` holds the 9 room
  instances, the pregenerated PC/monster stat blocks, ability scores, and all flavor text
  (`OneD20RulebookContent`/`OneD20FlavorText`) — pure data, zero logic, zero `LunaFlow.Engine`
  references (verified by grep: the only match in the whole project is a doc-comment that names the
  assembly, not a using/reference). `OneD20RuleBook : IRuleBook` declares its one ROM dependency
  (`D20System`).
- No fact-API-mediation layer was built — confirmed by construction, since Rulebook has no
  fact-touching code path at all once the handlers moved.
- `IRuleBook` grew from an empty marker to `IHasLunaFlowReference` + `IReadOnlyList<string>
  DependsOnRoms`, mirroring `IMechanic`'s ticket-0015 treatment.

**One real design decision beyond a mechanical move, flagging per the ticket's own "if anything's
ambiguous, don't build around it silently" instruction:** NRules' declarative rule group is compiled
and instantiated by NRules itself (via `RuleExport.Compile`), not by application code — it can't
take constructor arguments the way a hand-written `ICommandHandler` can. So Scenario data reaches
ROM two different ways depending on what's structurally possible:

- The five handlers take optional constructor parameters (`Room` dictionary, PC, monster,
  mechanic, ability scores), falling back to a new registry when omitted — exact mirror of the
  existing `D20ResolutionMechanic? mechanic = null` seam from ticket 0019.
- The rule group's generated code, and the handlers' fallback, both read from a new
  `D20SystemContent` static class in ROM — populated once by the composition root
  (`Program.cs`/tests' `ClassSetup`) via `D20SystemContent.Configure(...)` before rules compile or
  fire. This isn't a new mechanism: `OneD20Content`/`OneD20Flavor` were already static content
  holders that the generated rule code reached into directly; this just relocates the data source
  to the Rulebook side and adds an explicit registration step, rather than inventing a DI container
  NRules-compiled code has no way to use.

**Also done, smaller items:**

- `D20SystemNarration` (new, ROM) holds the four narration-formatting helpers
  (`CheckNarration`/`HitNarration`/`CombatStatus`/`WinAnnouncement`) lifted out of the old
  `OneD20Flavor` — pure format shape, no prose; they read the actual strings via
  `D20SystemContent.FlavorText[key]`. The fixed event-slot keys (`"pc-hit"`, `"monster-defeated"`,
  etc.) stay as literals in ROM code — same key-indirection pattern `Room.DescriptionKey`/
  `PromptKey`/`SuccessKey`/`FailureKey` already used between ROM structure and Rulebook text.
- Renamed the internal `OneD20Reference` → `D20SystemReference` (internal-only, not requested
  verbatim but kept consistent with the namespace move — flagging since it wasn't literally asked
  for, though low-risk).
- `CombatRoomIds`/`WinLines` inside `D20SystemRuleGroup` were left as-is (not re-derived from
  `Room.ChallengeType`) — the win-line room-id combinations are already-established grid-geometry
  ROM content (per `phase-2-7.md`'s own "grid/win-line navigation" framing), and `CombatRoomIds` is
  a private generation-time detail, redundant with but not contradicting `Room.ChallengeType`.
  Touching it risked turning this into a logic rewrite instead of a structural move, against the
  "if a test needs to change, that's a red flag" bar.

**Fact shapes unchanged** — nothing here should require touching `1-d20-spec.md`'s documented fact
vocabulary; only namespaces/ownership moved, not shapes. Flagging that conclusion here per the
ticket's ask, for whoever files Juno's follow-up reconciliation ticket.

**Verification:** `dotnet build LunaFlow_A.sln` — 0 errors. `grep -rn "LunaFlow.Engine"
src/LunaFlow.Rulebook` — one match, a doc-comment, not a reference. `dotnet test
tests/LunaFlow.EngineTests` — fresh run, 96/96 (including the 25 handler/fixture tests, relocated
to `Rom/D20System/` and renamed `D20SystemHandlerTests`/`D20SystemRuleFixtureTests`, logic/assertions
untouched — only namespaces, usings, and a `D20SystemContent.Configure(...)` setup call changed).
Also ran a live manual transcript through `MyLunaFlowServer` (`move`/`look`/`check`/`move`/`fight`)
confirming the runtime wiring — not just tests — produces the same room descriptions, roll/DC
readouts, and combat narration as before the split.

**Not touched, per the ticket's explicit exclusions:** `IEngineHost`'s doc comment,
`architecture.md`/`engine-core.md`'s "no Rulebook touches engine primitives" rule, and
`1-d20-spec.md` — all Amara's/Juno's.

**Addendum, same day:** `D20SystemContent.Configure(...)`'s 7-parameter signature retriggered the
`DefaultDocumentation` MAX_PATH failure first hit in ticket 0012 — VS2022 errored on the
`Documents` project again. Fixed (excluded the method as composition-root plumbing via
`LunaFlow.Rom`'s `DefaultDocumentation.json`, plus a real bug found in the process: that project's
doc target was missing `--ConfigurationFilePath` entirely) and written up properly this time at
[`api-doc-generation.md`](../api-doc-generation.md) so it's a lookup, not a re-derivation, next
time. Rebuilt clean and re-ran the full suite after — still 96/96.

## Resolution

Closed by Kofi 2026-07-20. Structural retrofit verified against source; behavioral regression green;
formal QA verdict deliberately deferred (see below).

**Verified against evidence, not the response's word:**

- **Handlers moved to ROM.** All five (`move`/`look`/`check`/`fight`/`flee`) live under
  `src/LunaFlow.Rom/D20System/` (`Handlers/`), alongside `Facts.cs`, `D20ResolutionMechanic`,
  `D20SystemContent`, `D20SystemNarration`, `D20SystemReference`, and `Rules/`. No `OneD20/`
  directory remains in `LunaFlow.Rom` — the flat namespace is gone, as asked.
- **Rulebook is source-clean of Engine.** `src/LunaFlow.Rulebook/` has exactly four `.cs` files
  (`OneD20RuleBook`, `OneD20RulebookContent`, `OneD20FlavorText`, `OneD20RulebookReference`); the only
  `LunaFlow.Engine` string in Rulebook source is a doc-comment asserting the absence of a reference.
  `.csproj` references only `LunaFlow.Rom`. Transitive `Engine.dll` presence via ROM is expected and
  correct — see the Ask item-3 correction above; it is **not** a violation.
- **`IRuleBook` grew minimal real shape.** `IRuleBook : IHasLunaFlowReference` + `IReadOnlyList<string>
  DependsOnRoms`, exactly the "smallest interface that does the job" discipline ticket 0015 set for
  `IMechanic`. Read directly, matches the response.
- **`LunaFlow.Rulebook` is in the solution.** `LunaFlow_A.sln` now names it (was zero matches when the
  ticket was filed).
- **No fact-API layer** — confirmed by construction; Rulebook has no fact-touching code path.
- **Build:** fresh full rebuild 7/7, 0 errors (Arryn-run, 2026-07-20 23:22). Pre-existing nullable
  warnings in `RuleEngine.cs` and unused-variable warnings in test files are unrelated to this work.
- **Regression:** `latest.trx` from the same run — **96/96 passed, 0 failed**. The done-bar's
  "96/96 unchanged" is met *today*; no test needed changing to pass (only namespace/using/setup
  relocations, per the response).

**One design decision flagged, not blocking close — `D20SystemContent.Configure(...)`.** Baraka
introduced a static, mutable content holder in ROM that the composition root populates before rules
compile/fire, because NRules-compiled rule code can't take constructor args. He disclosed it honestly.
It sits adjacent to the "avoid global mutable state" reasoning ticket 0019 used when it chose
constructor injection — but it's a defensible answer to a real constraint (generated rules have no DI
seam), and it relocates an already-existing static-holder pattern (`OneD20Content`) rather than
inventing one. **Not ticketed to Amara now, on purpose:** this is exactly the ROM/Rulebook
data-plumbing seam Phase 2.7 reshapes when a second ROM enters. Opening an architecture ticket against
a seam about to move would be churn. Recorded as a **2.7 watch item** instead (also noted in
`phase-2-7.md`). Revisit then; if 2.7 doesn't touch it, ticket it to Amara at that point.

**Spec follow-up:** fact shapes unchanged, so `1-d20-spec.md` needs no reconciliation for shape — only
namespace/ownership moved. Juno's spec-reconcile ticket (deferred per 0021 item 2) can proceed on that
basis when filed.

**QA status:** formal Lothar final-verdict for the Phase 2.6 done-bar is **deferred to post-2.7 per
Arryn** (2026-07-20) — verifying a one-ROM boundary proves less than verifying the two-ROM shape 2.7
introduces, so the QA spend is held until the boundary firms. This close rests on structural
verification + the green build/regression above, not on a QA verdict. Phase 2.6 is marked
**implementation-complete, QA-deferred** on the roadmap accordingly — not "done" in the fully-signed
sense phases 1.5/2/2.5 carry.
