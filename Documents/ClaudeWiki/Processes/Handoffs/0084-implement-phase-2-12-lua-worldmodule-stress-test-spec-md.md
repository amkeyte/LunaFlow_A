# 0084 — Implement phase-2-12-lua-worldmodule-stress-test-spec.md

**Status:** resolved
**Opened by:** Kofi (PM)
**Opened:** 2026-08-09
**For:** Baraka (Impl Dev)

## Decision

Built in full — all 12 acceptance criteria met, verified via a checked-in runnable spike
(`example/Phase2_12Spike`), including a real full-session playthrough driven through the actual
`move`/`check`/`fight`/`flee` command handlers (not just rule-level fact manipulation) with
deterministic scripted dice. Two real bugs the spike caught before they'd have shipped as false
confidence: a missing `move` between winning room 1 and checking room 2 (masked nothing — the
`check` would have silently no-op'd against room 1's already-solved challenge), and a validator probe
whose "reject" assertions were passing for the wrong reason (a structurally-invalid fake World Module
tripped an unrelated roster-size check before ever reaching the Enrage check under test) — both fixed
in the spike itself, documented inline. Item 11's Phase-4 finding is recorded below, plainly, per the
ticket's own ask. Full suite green locally (93 EngineTests + 88 ContentTests, 0 regressions); Arryn-run
`latest.trx` still needed to close. Maintained test coverage routed to Nadia as
[0085](0085-phase-2-12-maintained-test-coverage.md).

## Details

### Context

Phase 2.12 ([`phase-2-12.md`](../../../Roadmap/phase-2-12.md)) is ratified into `roadmap.md` — the
last of the three 2.10–2.12 stress-test phases. Juno's spec is drafted and reviewed clean: Amara's
read (ticket [0081](0081-phase-2-12-spec-ready-for-ratification-read.md)) confirmed item 11's
`OneD20MonsterTemplate`/`MonsterTemplate` contract extension is sound and framework-light, found one
Mechanics-section defect (Enrage's Defense effect misdescribed), which Juno fixed and Amara
independently re-verified against the live spec file (ticket
[0083](0083-phase-2-12-item-11-mechanics-section-misdescribes-enrage-s-defense-effect.md), resolved).
Items 9/10/12 are clean as drafted, no framework dependency anticipated.

**Sequencing note, your call:** ticket [0082](0082-implement-phase-2-11-1-rulebook-content-extraction-spec-md.md)
(Phase 2.11.1, Rulebook content extraction) is also open and assigned to you. It's a smaller,
purely-mechanical structural move (no behavior change) versus this ticket's real new content —
whichever order costs you less is fine; flag back if you'd rather I make the call.

### Ask

Build [`phase-2-12-lua-worldmodule-stress-test-spec.md`](../../../Specs/phase-2-12-lua-worldmodule-stress-test-spec.md)
in full. Four items, twelve acceptance criteria — work each one, don't summarize around it:

1. **Item 9 — "Signal Static," a new Lua-only World Module.** No C# equivalent to diff against
   (unlike Hollow Keep's verbatim transcription in 2.9). Full session playthrough: a win via a
   combat-free line, a win via a line touching combat, at least one `RoomFailed`. At least one
   Save-kind challenge and one `InflictsPoison` monster round-tripping correctly through the Lua
   marshal path — verify against actual constructed field values, not just "it didn't crash." Zero
   changes to `IOneD20WorldModule`, `OneD20WorldModuleValidator`, or `OneD20SessionAssembler`.
2. **Item 10 — procedural generation, not a table literal.** A five-monster roster generated via a
   Lua loop/conditional inside "Signal Static"'s source; stats must match the same base-plus-step
   formula computed by hand for all five monsters.
3. **Item 11 — Enrage.** `OneD20MonsterTemplate`/ROM-tier `MonsterTemplate` grow two nullable fields:
   `EnrageThresholdFraction` (fraction of MaxHp, `(0, 1]`) and `EnrageDefenseBonus` (flat). A new
   generic D20System rule inserts `MonsterEnraged(RoomId)` once, when `MonsterHp <= Threshold *
   MaxHp`; while present, the monster's effective Defense is `Stats.Defense + EnrageDefenseBonus` —
   **purely defensive, per ticket 0083's ruling: it must not touch the monster's own attack roll.**
   `MonsterEnraged` retracts when the `CombatRound` ends (win/loss/timeout/flee) — a retry fight
   against the same monster starts un-enraged. `OneD20WorldModuleValidator` rejects a template with
   only one of the two fields set, or `EnrageThresholdFraction` outside `(0, 1]`. Verify the fields
   survive the full chain: Lua source → `LuaOneD20WorldModule` marshaling → `Program.cs` translation
   → the ROM-tier fact `FightCommandHandler.cs` actually reads. Every existing monster (Phase 2.7.1's
   five, Hollow Keep's roster) must keep constructing/fighting correctly with both fields absent —
   no migration.
4. **Item 12 — Lua-path failure probes.** A marshal-failure probe that throws at
   `LuaOneD20WorldModule` construction, naming the offending field, never reaching
   `OneD20WorldModuleValidator` (verify via call-count or equivalent). A validator-failure probe that
   marshals successfully but is rejected by the existing pool-size check, naming the shortfall.

Framework dependencies: none anticipated for the mechanism itself (item 11 is ROM/Rulebook-tier only,
no `ComponentRegistry`/`Capability`/`EngineTier` involvement, confirmed by Amara). **Item 11's result
is a real, standing dependency for Phase 4's kickoff pass** (`design/gm-overlay.md`'s "effect fact"
draft needs to read it first, per ticket 0067's ruling) — record what you find plainly in your
Response so it's easy for whoever runs that kickoff to locate later, not buried in acceptance-test
prose.

Also verify: no existing criterion from `lua-world-module-spec.md`, `1-d20-spec.md`,
`loot-rom-content-expansion-spec.md`, or `phase-2-10-content-coverage-spec.md` regresses. Full suite
green from an Arryn-run `latest.trx` before this closes.

### Response

**Sequencing:** built 2.12 first (ticket 0082/Phase 2.11.1 left untouched, per Arryn's direction to
let it sit).

**Items 9/10 — "Signal Static."** New `.lua` file,
`src/LunaFlow.Rulebook/OneD20/Content/SignalStaticWorldModule.lua` (embedded resource, same glob
that already picks up `DefaultOneD20WorldModule.lua`/`HollowKeepWorldModule` content), no C#
equivalent. Same 9-room grid topology as every OneD20 module (only content differs, per the spec).
9-challenge pool (7 skill, 2 save: `signal-arc`/Reflex, `signal-haze`/Fortitude). 5-monster roster
generated by a Lua loop using the spec's own worked example almost verbatim (`defense = 11 + i`,
`maxHp = 6 + i*2`, `attackBonus` constant), with a post-loop conditional assigning item 11's Enrage
fields to monster 1 and `inflictsPoison` to monster 5 — kept as post-loop assignment specifically so
the loop itself stays the pure arithmetic proof item 10 asks for, not entangled with the one-off
flags. Full flavor text, own fiction (a radio-relay station, not the warehouse or Hollow Keep).
`OneD20WorldModuleValidator.Validate(signalStatic)` passes unmodified — spike-verified.

**Item 11 — Enrage, the framework-adjacent one.**
- `OneD20MonsterTemplate` (Rulebook) and `MonsterTemplate` (ROM-tier, D20System) both grow
  `EnrageThresholdFraction`/`EnrageDefenseBonus`, nullable, trailing, default null — no migration,
  confirmed by the full suite staying green (every existing monster in Default/Hollow Keep/the Phase
  2.9 Lua demo keeps constructing and fighting with both absent).
- New fact `MonsterEnraged(RoomId)`, D20System-owned. New `EnrageEffect` static helper (mirrors
  `PoisonEffect`'s shape) shared by `FightCommandHandler` (three terminal branches: win, loss,
  timeout) and `FleeCommandHandler` (the fourth) — one retraction call at each of `CombatRound`'s
  four exit points, so a retry always starts un-enraged.
- `FightCommandHandler`: reads `MonsterEnraged` once, up front each round, into `effectiveDefense` —
  used only for the PC's attack check, never the monster's own attack roll, per ticket 0083's ruling.
  Threshold-crossing check sits right after a non-lethal hit, guarded by the same up-front read (not
  a fresh query) so it can't double-insert within one round.
- `OneD20WorldModuleValidator` gains the pairing/range check (both-or-neither, threshold in `(0,1]`).
- `LuaOneD20WorldModule` gains `OptionalNumber` and marshals both fields the same optional-defaults-
  null way `inflictsPoison` already does.
- Live, spike-verified: a fight against Signal Static's Enrage monster (Defense 12, MaxHp 8,
  threshold 0.5, bonus 3) shows the *same PC roll* (8+4=12) hit at baseline Defense and miss once
  enraged (effective Defense 15) — the purely-defensive effect demonstrated directly, not inferred.
  Retraction confirmed on all three `fight`-side terminal outcomes plus flee; a flee-then-retry
  starts un-enraged (same roll hits again at baseline) and can re-enrage; final kill happens while
  still enraged, confirming retraction fires on the win path too.

**Item 11's Phase-4 finding, recorded plainly per the ticket's ask (not buried in acceptance-test
prose):** building a real threshold-triggered conditional stat effect surfaced three structural
properties worth reading before finalizing `gm-overlay.md`'s "effect fact"/`MonsterBehaviorOverride`
draft:
1. **The effect has to be read from inside the handler that resolves the roll, not just matched by a
   declarative rule** — `FightCommandHandler` computes `effectiveDefense` inline, because "which
   Defense value applies to this specific roll" is a decision the attack-resolution flow itself needs
   to make, not something a separate rule can inject after the fact. If GM Overlay wants live,
   improvised effects to modify in-flight resolution the same way, the mechanism needs a handler-side
   read hook, not only a rule-side one — a plain queryable fact (which is what `MonsterEnraged` is)
   satisfies both, and that seems like the right default shape to carry forward.
2. **Lifecycle bookkeeping had to be threaded through every exit point by hand** — `CombatRound` has
   four ways to end (win/loss/timeout/flee), living across two handler classes, and `MonsterEnraged`'s
   retraction needed a call at each one. A shared static helper (`EnrageEffect`) kept that from being
   four independent, driftable copies, but nothing enforces a *fifth* future exit point remembering to
   call it too — an "effect fact" mechanism with more than one lifecycle-scope shape (this session,
   this room, this combat, this turn) will need that enforcement built in, not left to author
   discipline, or it'll silently leak state the way this would have if I'd missed one branch.
3. **Pairing/range validity needed a new, hand-written validator check** — a C# record can't express
   "both of these nullable fields or neither, and this one's bounded (0,1]" at the type level.
   Whatever "effect fact" configuration shape GM Overlay lands on will very likely need the same kind
   of load-time structural validation this item needed, not just C#'s type system.

No `ComponentRegistry`/`Capability`/`EngineTier` involvement anywhere, confirmed — this stayed exactly
the ROM/Rulebook-tier change the spec's Framework dependencies section predicted.

**Item 12 — Lua-path failure probes.** Both inline Lua literals in the spike (no separate `.lua`
files needed, same convention `LuaOneD20WorldModuleTests.cs`'s Phase 2.9 malformed-script tests
already use). Marshal probe (`dc = "ten"`): `LuaOneD20WorldModule`'s constructor throws, message
names the script, the entry (`'broken'`), and the field (`'dc'`); a boolean flag proves
`OneD20WorldModuleValidator.Validate` was never reached (can only be set past a successful
construction — structurally impossible to set otherwise, not just "wasn't observed to run").
Validator probe (9 valid rooms, 4 challenges): constructs cleanly, then
`OneD20WorldModuleValidator.Validate` throws the existing pool-size message naming the shortfall.

**Two real bugs the spike caught, both fixed before this ticket closed, not shipped and found
later:**
1. Scenario B's command sequence checked room 2's challenge immediately after winning room 1's fight
   — without moving there first. Winning a fight doesn't move the PC; the `check` would have silently
   resolved against room 1 (already solved, so it's a no-op) and never touched room 2 at all, meaning
   the "combat-touching win" assertion would have failed for a completely different, confusing
   reason. Fixed by adding the missing `move east`.
2. `RunValidatorRejectionChecks`' first draft used a `SingleMonsterProbeWorldModule` with only 1
   monster, 9 all-SkillCheck rooms, and 6 challenges — which trips `OneD20WorldModuleValidator`'s
   roster-size (`Monsters.Count < 3`) and pool-size (`skillCheckCount > Challenges.Count`) checks
   *before* the Enrage-specific check under test is ever reached. All four "reject" assertions still
   passed (some `InvalidOperationException` fired), for entirely the wrong reason — a green check that
   proved nothing about Enrage specifically. Caught by reading which check actually fired for the
   fifth ("accept threshold == 1") case, which failed loudly instead of silently passing for the wrong
   reason. Fixed by making the probe module otherwise fully valid (3 monsters, 9 challenges) and
   asserting the exception message names "Enrage" specifically, not just its type.

**Build/test status:**
- `dotnet build LunaFlow_A.sln` — clean, 0 errors.
- `dotnet run --project example/Phase2_12Spike` — all checks pass (28 individual assertions across
  marshal/field/procedural-roster/three live scenarios/validator-rejections/item-12-probes).
- `dotnet test LunaFlow_A.sln` — `LunaFlow.EngineTests` 93/93, `LunaFlow.ContentTests` 88/88 — **zero
  regressions**, no test fallout this time (unlike 2.11's `DependsOnRoms`/`Requires`-count breaks —
  this phase's contract changes are additive-nullable, so nothing existing needed repointing).
- Full-suite-green criterion's Arryn-run `latest.trx` bar is still open — Kofi/Arryn, over to you.
- Maintained test coverage (fixture-level `MonsterEnraged` lifecycle, the corrected validator probe
  shape, Lua marshaling, "Signal Static" itself, item 12's probes) routed to Nadia as
  [0085](0085-phase-2-12-maintained-test-coverage.md).

### Resolution

**Resolved 2026-08-09 (Kofi, opener).** The bar this ticket was waiting on — an Arryn-run full-suite
`.trx` — is in: `tests/LunaFlow.EngineTests/TestResults/latest.trx`, run `amkey@ARRYNSTATION
2026-08-09 01:47:37`, `runUser="ARRYNSTATION\amkey"`. **200/200 passed, 0 failed** — and 200 is
exactly 93 (EngineTests, untouched by this phase) + 107 (ContentTests, per 0085's independently
re-verified count: 88 baseline + 19 new), so this run covers both projects post-0085, same merged-trx
shape established for 2.10/2.11.

Everything else was already independently re-verified before this closed: 0085 (Baraka, against the
actual test files — hand-walked the scripted-roll arithmetic for the two trickiest cases himself
rather than trusting the inline comments, confirmed the validator-probe shape correctly avoids the
false-pass trap the spike's first draft fell into). Nothing here is my own read of `/src`; it's the
Arryn-run `.trx` plus that independent chain of evidence, same discipline every prior gate has held to.

Build ticket closed. Phase 2.12's QA gate cut to Lothar as ticket
[0086](0086-phase-2-12-qa-gate.md).
