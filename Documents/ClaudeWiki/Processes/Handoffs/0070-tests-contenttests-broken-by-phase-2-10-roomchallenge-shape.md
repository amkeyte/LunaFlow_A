# 0070 — tests/LunaFlow.ContentTests broken by Phase 2.10's RoomChallenge shape change

**Status:** open
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-08-03
**For:** Nadia (Test Dev)

## Details

### Context

Building [`phase-2-10-content-coverage-spec.md`](../../../Specs/phase-2-10-content-coverage-spec.md)
per ticket [0069](0069-implement-phase-2-10-content-coverage-spec-md.md): D20System's `RoomChallenge`
record grew two new fields at the spec's own pinned positions (`CheckKind? Kind` right after
`ChallengeType`, `string? SaveType` right after `DC`) —

```csharp
public sealed record RoomChallenge(
    int Id, RoomChallengeType ChallengeType, CheckKind? Kind,
    string? Skill, string? SkillAbility, int? DC, string? SaveType,
    string? SuccessKey, string? FailureKey, string? PromptKey, string? MonsterId);
```

`MonsterTemplate` also grew a trailing optional `bool InflictsPoison = false`, which is source-compatible
with existing positional calls. `RoomChallenge`'s reorder is not — it breaks every positional
`new RoomChallenge(...)` call in `tests/`. `dotnet build LunaFlow_A.sln` confirms: `src/`, `example/`,
and `src/MyLunaFlowServer` all build clean; `tests/LunaFlow.ContentTests` fails with 4 `CS7036`
errors (missing `FailureKey` argument — the compiler's read of the shifted positions), all from direct
positional `RoomChallenge` construction. `tests/LunaFlow.EngineTests` is unaffected (doesn't reference
D20System content). Same shape of break as ticket 0062 (Phase 2.8's World Module extraction), not a new
kind of problem.

Two files, three call sites:
- `Rom/D20System/D20SystemHandlerTests.cs` — `ToRoomChallenge` local helper (line ~63) and the inline
  Fight-room `new RoomChallenge(...)` in `StartSession()` (line ~112).
- `Rom/ComponentRegistryContentTests.cs` — a `NewD20System()`-style fixture builder, two call sites
  (lines ~57, ~72).

### Ask

Repoint all `RoomChallenge` construction in the two files above to the new 11-argument shape (`Id`,
`ChallengeType`, `Kind`, `Skill`, `SkillAbility`, `DC`, `SaveType`, `SuccessKey`, `FailureKey`,
`PromptKey`, `MonsterId`). For every existing call these tests make, the room is Skill-kind
(`CheckKind.Skill`) or a Fight room (`Kind: null`) — no test currently needs a Save-kind room, so this
should be a pure insert-two-arguments repoint, not a logic change. `phase-2-10-content-coverage-spec.md`'s
own acceptance criteria (11 items, `Documents/Specs/phase-2-10-content-coverage-spec.md`) are **not**
in scope for this ticket — this is just getting the existing 62 tests compiling and green again against
the new shape, same as 0062 was for the World Module extraction. A separate ticket for maintained
coverage of Phase 2.10's five new items (Save-kind checks, `equip`/Backup Piece, `Poisoned` across a
room transition, the two new World Module validator checks) is Kofi's/your call to open once this one's
closed — ticket 0069's own Response has the implementation detail if useful context for scoping that.

`CheckKind` lives in `LunaFlow.Rom.D20System`, already `using`'d in both files (same namespace
`RoomChallengeType`/`RoomChallenge` already come from) — no new using or project reference needed.

Once `tests/LunaFlow.ContentTests` compiles, please also confirm the existing 62 tests still pass
unchanged (no assertion-value changes expected — this is a pure shape repoint) and report the count,
same bar 0062 held. `dotnet build LunaFlow_A.sln` full-solution green plus an Arryn-run `latest.trx`
is what phase-2-10's own Done-When bar is waiting on.

### Response

<Filled in by the assignee.>

### Resolution

<Filled in by the opener when closing.>
