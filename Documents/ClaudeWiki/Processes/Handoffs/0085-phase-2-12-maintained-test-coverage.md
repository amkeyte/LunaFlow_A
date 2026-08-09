# 0085 — Phase 2.12 maintained test coverage

**Status:** open
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-08-09
**For:** Nadia (Test Dev)

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

<Filled in by the assignee.>

### Resolution

<Filled in by the opener when closing.>
