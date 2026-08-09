# 0085 — Phase 2.12 maintained test coverage

**Status:** resolved
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-08-09
**For:** Nadia (Test Dev)

## Decision

All 6 items built. Re-ran `example/Phase2_12Spike` first to confirm its baseline still passes, then
reused its already-verified roll sequences where the maintained tests cover the same ground (items
1's threshold/defensive/flee/retry chain), deriving only the loss and timeout-while-enraged sequences
fresh (the spike doesn't cover those). `dotnet build` full-solution green, `dotnet test
tests/LunaFlow.ContentTests` **107/107** (88 baseline + 19 new).

## Details

### Context

Phase 2.12 ([ticket 0084](0084-implement-phase-2-12-lua-worldmodule-stress-test-spec-md.md), build
complete, in-review) shipped "Signal Static" (a new Lua-only World Module), procedural monster
generation, item 11's Enrage mechanic (`MonsterEnraged`, two new nullable fields on
`OneD20MonsterTemplate`/`MonsterTemplate`, a validator pairing/range check), and item 12's two
Lua-path failure probes. Same split as every prior content phase (0052→0054, 0069→0070,
0076→0079): implementation build plus a checked-in verification spike is Baraka's; the maintained
suite in `tests/` is yours.

A checked-in, runnable spike at `example/Phase2_12Spike` (`dotnet run --project
example/Phase2_12Spike`) already exercises everything below once, deterministically (via a
`ScriptedRandom` that returns a pre-set queue of roll values, not a seeded PRNG), and prints
PASS/FAIL per check — worth reading before writing the maintained equivalents, not as a substitute
for them. It also needed `[assembly: InternalsVisibleTo("Phase2_12Spike")]` added to
`src/LunaFlow.Engine/GlobalSuppressions.cs` (same grant `LunaFlow.EngineTests`/`LunaFlow.ContentTests`
already have) to drive `GameEngineCore` directly the way `D20SystemHandlerTests.cs` does — flagging
in case that affects how you scope any new fixture project references.

### Ask

1. **Fixture-level coverage for `MonsterEnraged`'s lifecycle** — same shape as
   `D20SystemHandlerTests.cs` (drives real `move`/`fight`/`flee` handlers through
   `GameEngineCore.Instance`, deterministic via `D20ResolutionMechanic`'s injectable RNG):
   - Threshold crossing inserts `MonsterEnraged` exactly once, at `MonsterHp <= Threshold * MaxHp`,
     never before.
   - While present, a PC attack roll that would have hit at baseline `Defense` misses against
     `Defense + EnrageDefenseBonus` — the same roll, demonstrating the effect is real, not just that
     *some* roll misses.
   - Enrage never affects the monster's own attack roll (a monster miss/hit pattern is unchanged
     whether enraged or not, for identical monster rolls) — pin this as a named regression per
     ticket 0083's ruling, not just inferred from the handler not reading `Defense` there.
   - `MonsterEnraged` retracts on all four `CombatRound` terminal outcomes: win, loss (PC HP <= 0),
     timeout (3 rounds), and flee — four separate assertions, not one generic "retracts" test.
   - **The retroactive-safety property**: a fresh fight against the same monster after a flee starts
     un-enraged (an identical roll that missed while enraged hits again at baseline) — mirrors the
     retroactive-grant regression `example/Phase2_11Spike`/ticket 0079 pinned for Loot's bonus rule;
     same "verify it doesn't leak state across a retry" instinct applies here too.
2. **`OneD20WorldModuleValidator`'s Enrage pairing/range check** — four rejection cases (threshold-
   only, bonus-only, threshold > 1, threshold == 0) plus the inclusive-upper-bound accept case
   (threshold == 1). **Use a properly-valid surrounding World Module for the probe** — the spike's
   first draft used a single-monster, under-populated fake module and every "reject" assertion
   passed for the wrong reason (roster-size/pool-size checks fired first, not the Enrage check) until
   caught by inspecting the actual exception message; `example/Phase2_12Spike/Program.cs`'s
   `SingleMonsterProbeWorldModule` shows the fixed shape (3 monsters, 9 challenges, so only the
   Enrage-specific check is what's under test) — worth reading that class's doc comment before
   writing the maintained version so the same mistake doesn't repeat.
3. **The full field-carry chain** — a monster template with Enrage fields set, translated through
   `Program.cs`-equivalent construction into ROM-tier `MonsterTemplate`, actually read by
   `FightCommandHandler`. The live scenarios above already exercise this end to end; a maintained
   version proves it's pinned, not just spike-demonstrated once.
4. **`LuaOneD20WorldModule` marshaling for the two new optional fields** — present-and-correct,
   absent-defaults-to-null (existing scripts predating this phase must keep marshaling unchanged),
   same shape as the existing `inflictsPoison`/save-kind marshaling tests in
   `LuaOneD20WorldModuleTests.cs`.
5. **"Signal Static" itself** — `FromEmbeddedResource_SignalStatic_PassesOneD20WorldModuleValidatorUnmodified`
   at minimum (mirrors the existing Default-script test); Baraka's read is the live-session
   playthrough coverage is better owned by a `SpikeSession`-shaped fixture if you want it maintained
   rather than re-deriving the whole harness — your call on how much of `example/Phase2_12Spike/SpikeSession.cs`
   is worth promoting into `tests/` verbatim vs. rebuilding to fit existing fixture conventions.
6. **Item 12's two probes** — marshal-failure (message names the script, entry, and field; validator
   never reached — a boolean flag that can only be set past a successful construction, same technique
   the spike uses) and validator-failure (constructs cleanly, then the exact pool-size message).

Framework note: no `ComponentRegistry`/`Capability`/`EngineTier` involvement anywhere in this phase —
purely ROM/Rulebook-tier fixture work, ordinary `RuleEngine`/`GameEngineCore` test technique already
established.

### Response

**Items 1 & 3 — new file `tests/LunaFlow.ContentTests/Rom/D20System/SignalStaticEnrageHandlerTests.cs`
(9 tests).** Same `StartSession`-inline shape as `D20SystemHandlerTests.cs`, driven live through
`GameEngineCore.Instance`, but against "Signal Static" (loaded via
`LuaOneD20WorldModule.FromEmbeddedResource`) instead of `DefaultOneD20WorldModule` — the latter has
no Enrage-capable monster. Pinned room/monster assignment matches
`example/Phase2_12Spike/Program.cs`'s exactly. A small test-owned `ScriptedRolls : Random` (returns a
scripted value queue, exhausts loudly) replaces `D20SystemHandlerTests`' seed-searched `Random(seed)`
approach — same technique the spike's own `ScriptedRandom` introduced, re-declared here rather than
referencing the spike project (spikes are explicitly throwaway/unmaintained by their own convention).

- `StartSession_TranslatesEnrageFieldsFromWorldModuleIntoMonsterTemplate_Unchanged` (item 3): asserts
  `EnrageThresholdFraction`/`EnrageDefenseBonus` carry unchanged into the ROM-tier `MonsterTemplate`
  `FightCommandHandler` actually reads, and that a plain monster carries both as null — structural,
  no fight needed.
- `Fight_FirstHitCrossingThreshold_InsertsMonsterEnragedExactlyOnce_AtMonsterHpEqualToThreshold`:
  threshold crossing, exactly once, at `MonsterHp == Threshold*MaxHp` exactly (not just "below").
- `Fight_WhileEnraged_IdenticalPcRollThatHitBefore_NowMissesAgainstBoostedDefense`: the same roll
  that hit at baseline now misses at the enraged effective Defense — the defensive-effect claim
  pinned against one concrete roll, not "some roll misses."
- `Fight_MonsterCounterattack_UnaffectedByEnrage_IdenticalRollSameOutcomeBeforeAndAfter`: the named
  regression per ticket 0083's ruling — the same `monsterRoll` deals the identical damage once
  pre-enrage and once post-enrage (8→5 both times), proving Enrage never touches the monster's own
  attack calculation.
- `MonsterEnraged_RetractsOnWin` / `RetractsOnLoss` / `RetractsOnThreeRoundTimeout` / `RetractsOnFlee`
  — four separate tests, one per `CombatRound` terminal outcome, as asked (not one generic
  "retracts" test). Win/flee reuse the spike's Scenario B numbers directly (re-ran the spike first to
  confirm it still passes); loss and timeout-while-enraged are new sequences of my own derivation
  — verified empirically via `dotnet test`, same discipline `D20SystemHandlerTests.cs`'s own seeded
  scenarios document ("verified deterministic... not hand-computed"), both passed on the first run.
- `Fight_RetryAfterFlee_StartsUnenraged_IdenticalRollThatMissedNowHitsAgain`: the retroactive-safety
  property — reuses the spike's full Scenario B roll sequence (already spike-verified), a roll that
  missed while enraged connects again at baseline after a flee+retry.

**Item 2 — extended `OneD20WorldModuleValidatorTests.cs` (5 tests).** Reused this file's existing
`TestWorldModule.FromDefault()` double (seeded from a real, fully-valid `DefaultOneD20WorldModule`)
rather than building a new minimal single-monster probe module — mutating just one monster
("duelist")'s Enrage fields keeps every other structural check (room/challenge/monster counts)
satisfied, avoiding the exact mistake `example/Phase2_12Spike/Program.cs`'s
`SingleMonsterProbeWorldModule` doc comment warns about (an under-populated probe module trips the
roster/pool-size floor checks first, so "reject" assertions pass for the wrong reason). Four
rejection cases (threshold-only, bonus-only, threshold>1, threshold==0) plus the inclusive-upper-bound
accept case (threshold==1) — read `OneD20WorldModuleValidator.cs` directly for the exact thrown-message
wording rather than guessing.

**Item 4 — extended `LuaOneD20WorldModuleTests.cs` (2 tests).** `Construct_MonsterWithEnrageFieldsSet_MarshalsThresholdAndBonusExactly`
and `Construct_MonsterWithoutEnrageFields_MarshalsBothFieldsAsNull`, same inline-minimal-script shape
as this file's existing `Construct_*` tests. (The full `FromEmbeddedResource_DemoScript_MonstersMatchDefaultExactly`
round-trip test already incidentally covers the absent-defaults-to-null path for
`DefaultOneD20WorldModule.lua`, since it predates this phase and has no Enrage fields — these two
new tests make that claim explicit and direct rather than relying on an incidental proof.)

**Item 5 — `FromEmbeddedResource_SignalStatic_PassesOneD20WorldModuleValidatorUnmodified`** in
`LuaOneD20WorldModuleTests.cs`, mirroring the existing Default-script test exactly, plus items 1/3
above (which exercise Signal Static live, end to end). **SpikeSession promotion call:** rebuilt
rather than promoted verbatim — `D20SystemHandlerTests.cs`'s own convention is an inline private
`StartSession` method on the test class, not a separate reusable session helper class; mixing the two
conventions in one test project felt like the wrong trade for reuse this size (one new test class).

**Item 6 — extended `LuaOneD20WorldModuleTests.cs` (2 tests)** using the spike's exact Lua source
snippets for both probes: `Construct_ChallengeDcAsString_ThrowsBeforeValidatorIsEverReached`
(marshal-failure — asserts the exception names the script/entry/field, and a flag provably never
set past construction proves the validator was never reached) and
`Construct_FourChallengesAgainstSixSlotFloor_ConstructsCleanly_ThenRejectedByValidator`
(validator-failure — marshal-clean construction, then rejected at the pool-size floor specifically).

**Verification:** `dotnet build LunaFlow_A.sln` — full solution green, 0 warnings, 0 errors.
`dotnet test tests/LunaFlow.ContentTests` (full suite, no filter) — **107/107 pass** (88 baseline +
19 new: 9 Enrage lifecycle/field-carry + 5 validator + 2 Lua marshaling + 1 Signal Static + 2 item-12
probes). Updated 2 `.playlist` files (`ContentTestsD20System.playlist`,
`ContentTestsOneD20Rulebook.playlist`) and `ContentTestsAll.playlist`, cross-checked every entry
against `dotnet test --list-tests` discovery in both directions (no phantom entries, no discovered
test missing from `ContentTestsAll`) and confirmed `ContentTestsAll` still equals the exact sum of
the 5 split playlists.

### Resolution

**Resolved 2026-08-09 (Baraka, opener).** Independently ran the suite rather than trusting the
Response: `dotnet build LunaFlow_A.sln` clean, `dotnet test tests/LunaFlow.ContentTests` **107/107**,
`--list-tests` discovery also reports 107 — matches.

Read `SignalStaticEnrageHandlerTests.cs` directly and re-derived the roll arithmetic by hand for the
two trickiest cases rather than trusting the inline comments:
- `Fight_MonsterCounterattack_UnaffectedByEnrage_...`: walked the scripted queue
  `(1, 9, 3, 8, 1, 9, 3)` against the handler's actual call order — round 1 (PC miss, monster hits for
  3, PlayerHp 11→8) consumes exactly 3 rolls, round 2 (PC hits and crosses the threshold mid-round,
  *then* the identical `monsterRoll=9`/damage=3 fires again, PlayerHp 8→5) consumes exactly 4 more.
  The math checks out and genuinely isolates the claim: the counterattack's outcome and damage are
  byte-for-byte identical before and after Enrage triggers, because the branch never reads the
  monster's Defense at all — not asserted, demonstrated.
- `MonsterEnraged_RetractsOnLoss`: walked `(8,1,1, 10,9,6, 10,9,6)` — PlayerHp goes 11→5→-1 across
  three rounds, correctly triggering the loss branch on round 3 while enraged (effective Defense 15
  throughout rounds 2-3, confirmed by the miss/hit pattern matching that Defense value specifically,
  not the baseline 12).

Also confirmed: the corrected validator-probe shape (mutating one monster in a fully-valid
`TestWorldModule.FromDefault()`, not a new under-populated fake module) correctly avoids the exact
false-pass trap `example/Phase2_12Spike`'s first draft fell into — read `OneD20WorldModuleValidatorTests.cs`
directly, all five assertions check the message contains `"Enrage"` and `"'duelist'"` specifically,
not just that *some* exception fired. Item 12's two probes mirror the spike's exact Lua source and
the "validator never reached" flag technique.

No defects found. No fix requested. Phase 2.12's full-suite-green bar (0084) is now unblocked on the
test side — still needs an Arryn-run `latest.trx` before 0084 itself can close.
