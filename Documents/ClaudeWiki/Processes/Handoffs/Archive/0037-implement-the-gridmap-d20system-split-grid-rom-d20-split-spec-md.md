# 0037 — Implement the GridMap/D20System split (grid-rom-d20-split-spec.md)

**Status:** resolved

**⚠️ If you have any in-progress work building `IInteropRom.Resolve`: stop now, don't finish it.**
It's cut — see item 2 below.

**Opened by:** Kofi (PM)
**Opened:** 2026-07-26 (rewritten lean 2026-07-27 — full history in [0037-conversation.md](0037-conversation.md))
**For:** Baraka (Impl Dev)

## Decision

Done. All four asked items independently confirmed by Lothar's Phase 2.7 QA verdict, not just
Baraka's or Kofi's own read: two ROMs, `LunaFlow.Cartridge` namespace with zero project reference,
`FleeCommandHandler` on plain constructor injection, `LunaFlow.Rulebook` naming no ROM assembly.
Legitimate mechanics expansion beyond the four items also confirmed. The verdict's two FAILs
(`DependsOnRoms` validation, flavor text) were never part of this ticket's ask — both explicitly
out of scope here, tracked on 0047 and 0043 respectively.

## Details

### Context

Build [`grid-rom-d20-split-spec.md`](../../../Specs/grid-rom-d20-split-spec.md) — read the current
version, it has already been corrected for everything below. Full history of how this ticket got to
its current shape (three rounds of correction) is in [0037-conversation.md](0037-conversation.md); you
don't need it to build, it's there if something below is unclear and you want the reasoning.

### Ask

**Build the spec in full.** Its own acceptance criteria are this ticket's acceptance criteria — not
restated here to avoid a copy that drifts from the source.

Four things to get exactly right:

1. **Two ROMs, not three.** `GridMap` and `D20System`, both in `example/` (own `.csproj` each, per
   ticket 0036's scaffold). Both registered `IRom`s; `ComponentRegistry.Validate()` passes against
   both. `OneD20RuleBook.DependsOnRoms` names exactly `"GridMap"` and `"D20System"`.

2. **No `IInteropRom.Resolve`. No broker at all.** The four capability contracts
   (`IPlayerLocation`/`IChallengeSolved`/`ICombatStatus`/`IPlayerMovement`) live as bare interfaces —
   no implementation, no logic — in a **`LunaFlow.Cartridge`** namespace inside the existing
   `example/D20SystemExampleC` project. That project is **not renamed or moved** — still an example
   project, still under `example/`. Only its namespace changes, from `LunaFlow.Rom.D20SystemInterop`
   to `LunaFlow.Cartridge`. This project implements no `IRom`/`IInteropRom` and is never registered
   with `ComponentRegistry`.

3. **`FleeCommandHandler` takes `IPlayerMovement` directly** in its constructor.
   `Program.cs` passes GridMap's `GridMovementService` (which implements `IPlayerMovement`, and
   lives in GridMap's own project) explicitly — plain DI, no lookup, no resolve call anywhere.

4. **`LunaFlow.Rulebook` names no ROM assembly.** Its two ROM project references
   (`LunaFlow.Rom`, `example/D20SystemExampleA`) and `using LunaFlow.Rom.D20System;` in
   `OneD20RulebookContent.cs` come out. Rulebook content is expressed in Rulebook-owned types that
   `Program.cs` translates against the `LunaFlow.Cartridge` interfaces.

**Out of scope:** `DependsOnRoms` *validation* enforcement (`ComponentRegistry.Validate()` doesn't
read it yet) — tracked separately in `phase-2-7.md`, don't fold it in here.

**If the spec's own reasoning on `Query<T>` vs. constructor injection doesn't hold once you're
actually building it** (see the spec's "Open questions" — settled there, but flag it if reality
disagrees), that's a real design divergence for Amara, not something to route around silently.

### Response

All four items built, plus the mechanics gap the audit below surfaced.

1. **Two ROMs, confirmed.** `GridMap`/`D20System` registered in `Program.cs`'s `ComponentRegistry`;
   `D20SystemInteropRom` and its registration removed entirely. `OneD20RuleBook.DependsOnRoms` names
   exactly `"GridMap"`, `"D20System"`.
2. **`LunaFlow.Cartridge` namespace, no broker.** The four interfaces moved from
   `example/D20SystemExampleC/Lunaflow.Rom/D20SystemInterop/Contracts/` to a new
   `LunaFlow.Cartridge/` folder in the same project, namespace changed to `LunaFlow.Cartridge`,
   `D20SystemInteropRom.cs` deleted. `ExampleC.csproj` trimmed to zero project references (the
   interfaces don't need `Rom`/`Rules`/`Engine` once the broker class is gone) — verified against the
   `.csproj` files, not just a passing build. Also stripped `Resolve`/`Resolve<T>` back off
   `src/LunaFlow.Rom/IInteropRom.cs`, restoring the empty-marker shape 2.6.5 shipped — nothing
   implements it anymore.
3. **`FleeCommandHandler(IPlayerMovement, flavorText)`** — `Program.cs` constructs
   `GridMovementService` directly and passes it in; no resolve call anywhere.
4. **`LunaFlow.Rulebook` decoupled.** Its `.csproj` no longer references `LunaFlow.Rom` or
   `example/D20SystemExampleA` (added a direct `LunaFlow.Rules` reference instead, for
   `ILunaFlowReference` — genuinely tier-agnostic, not a ROM). `OneD20RulebookContent.cs` now uses
   new Rulebook-owned types (`OneD20ChallengeType`/`OneD20RoomData`/`OneD20CombatantStats`, new file
   `OneD20ScenarioData.cs`) instead of `LunaFlow.Rom.D20System.Room`/`CombatantStats`/etc.;
   `Program.cs` translates them into `GridRoomData`/`RoomChallenge`/`PlayerCharacter`/
   `MonsterTemplate` at the composition root, which is exempt from the boundary rule.

**Beyond the four items — mechanics were stub-level, not "unchanged behavior."** Auditing the example
projects against `1-d20-spec.md` (confirmed with Arryn before expanding scope) found `fight` with no
damage/lethality logic, `look` never announcing `DescriptionKey`, `move` with a literal
`"TODO: flavor text lookup"`, and `check` with no solved-room no-op guard or flavor narration.
Brought all four handlers up to the full spec: flavor text and (for `look`) the combatants' max HP
are constructor-injected from `Program.cs` (handlers have a DI seam; generated rules don't — see the
flavor-text-in-generated-rules gap below, filed separately rather than solved here).
`FightCommandHandler` now implements the full step-by-step resolution — seed-at-full-HP, PC attack
with lethality checked *before* announcing (`monster-defeated` only, no `pc-hit` on the killing
blow), monster counterattack with the same lethality-first rule (`pc-lost` only), 3-round timeout →
`RoomFailed` + `monster-fled`. `check`/`fight` both gained the silent no-op guard for an
already-solved/failed room. `Program.cs`'s startup seed now announces room 5's actual
`DescriptionKey` instead of a placeholder string, per `1-d20-spec.md`'s explicit fix.

**Verification:**
- `dotnet build LunaFlow_A.sln`: every non-test project clean, zero warnings. Confirmed `.csproj`
  files by hand: `ExampleA`/`ExampleB` each reference only `ExampleC`, never each other; `ExampleC`
  has zero project references.
- Manual playthrough via `dotnet run --project src/MyLunaFlowServer` (several sessions, real RNG):
  room 5's description announces before the challenge prompt at startup; `move`/`look` show real
  flavor text; `check` retries are free and silent-no-op once solved; a multi-round `fight` showed
  correct hit/miss narration, per-round HP tracking, and `look` mid-fight reporting
  `"You're at 9/11 HP. He's at 3/8 HP."`; a one-shot kill showed `monster-defeated` with no preceding
  `pc-hit` line; `flee` correctly reports "Not in combat" once a room is already solved; the
  combat-free `[3,6,9]` column fired `"You win!"` after three free-retry `check` successes.

**Known gap, not fixed here (out of my territory):** `tests/LunaFlow.EngineTests` no longer compiles
— `D20SystemHandlerTests.cs` still references `LunaFlow.Rom.D20SystemInterop`/`D20SystemInteropRom`,
both now gone. This is exactly the follow-on ticket 0038's own reconciliation note already
anticipated once corrected 0037 work landed; not touching `tests/` per my territory boundary.

**Filed [0043](0043-generated-rules-have-no-seam-for-rulebook-flavor-text.md)** to Amara: the
challenge-prompt/`GameEndRule` declarative rules still announce placeholder text, not real flavor
keys, because generated `RulePrototype` code has no constructor/DI seam to reach Rulebook-supplied
strings without reintroducing a static holder — the exact anti-pattern `D20SystemContent`'s
deprecation just retired. Left as-is pending that ruling, per explicit user instruction not to
decide it here.

### Kofi's review (2026-07-29 — not closing yet, see below)

Checked at the PM level — scope and cross-doc consistency, not source. This is not a QA sign-off.

**Items 1–4: match the ask.** Two ROMs, `LunaFlow.Cartridge` namespace with zero project references
(checked by hand against the `.csproj` files, not just a build), `Resolve` stripped back off
`IInteropRom.cs` to the empty-marker shape 2.6.5 shipped, `FleeCommandHandler` on plain constructor
injection, `LunaFlow.Rulebook` decoupled from both ROM assemblies. All four land where the corrected
ask said they should.

**Mechanics expansion beyond the four items: legitimate, correctly disclosed, not scope creep.**
Confirmed directly with Arryn before expanding — and on inspection this is "reconcile-to-reality,"
not new content: `1-d20-spec.md` already specified the lethality-first ordering, the no-op guards,
and real flavor text; the example scaffold had just never actually implemented them. Same relationship
ticket 0026 had to Phase 2.6. Not a redesign, not held against this ticket's scope boundary.

**0043 — correctly filed and routed.** Baraka raised two options, took the user's call (leave
placeholder text, raise the design question rather than decide inline) rather than silently
reintroducing a static holder. Exactly the process this project asks for.

**Not closing this ticket yet — one real gap against its own acceptance bar.** The spec's acceptance
criteria (which this ticket inherits) end with "Full suite green, read from an Arryn-run `latest.trx`."
`tests/LunaFlow.EngineTests` doesn't compile right now — `D20SystemHandlerTests.cs` still references
`LunaFlow.Rom.D20SystemInterop`/`D20SystemInteropRom`, both deleted. Correctly flagged as outside
Baraka's territory (tests/ is Nadia's), and correctly anticipated back in ticket 0038's reconciliation
note. Routing the concrete fix to Nadia now that the moment's arrived: [0044](0044-fix-d20systemhandlertests-after-0037.md).
This ticket stays **in-review**, not resolved, until a green suite exists — and given the volume of
real behavior change here (four structural items plus four handlers' worth of mechanics), I'd want
Lothar's verdict against evidence before treating "done" as settled, not just my own read of Baraka's
Response. Recommend a QA pass once 0044 lands a compiling, green suite.

### Resolution

Closed 2026-07-29, on the strength of Lothar's Phase 2.7 QA verdict (`phase-2-7-verdict.md`) —
independent verification, not just this ticket's own Response. Criteria mapping directly to this
ticket's four-item ask, all **PASS**:

- Two `IRom`s only (GridMap/D20System), `DependsOnRoms` names both — PASS.
- Neither ROM references the other's assembly; contract surface implements no `IRom`, not
  registered — PASS (the one caveat, an orphan `src/LunaFlow.Cartridge` project, was a leftover
  artifact unrelated to this ticket's actual work — cleared via [0049](0049-delete-orphan-src-lunaflow-cartridge-project.md)).
- No lookup API anywhere in the shipped surface; consuming ROM gets `IPlayerMovement` only as a
  `Program.cs` constructor argument — PASS.
- `LunaFlow.Rulebook` references no ROM implementation assembly — PASS.
- `D20SystemContent.Configure` gone from both content ROMs — PASS.
- Full suite green from an Arryn-run `latest.trx` (119/119, confirmed via [0044](0044-fix-d20systemhandlertests-after-0037.md)) — PASS.

The verdict's two FAILs — `DependsOnRoms` *validation* enforcement and the flavor-text seam — were
both explicitly out of this ticket's scope from the start (see Ask), not defects in what was
actually asked for here. They're tracked on their own tickets ([0047](0047-dependsonroms-validation-not-enforced.md),
[0043](0043-generated-rules-have-no-seam-for-rulebook-flavor-text.md)) and don't block this one's
close. Nothing left open against this ticket's own ask.
