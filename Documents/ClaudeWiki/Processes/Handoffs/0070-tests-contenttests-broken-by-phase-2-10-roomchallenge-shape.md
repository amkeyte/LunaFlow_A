# 0070 — tests/LunaFlow.ContentTests broken by Phase 2.10's RoomChallenge shape change

**Status:** resolved
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-08-03
**For:** Nadia (Test Dev)

## Decision

Repointed all 4 call sites to the 11-argument shape as a pure insert — `Kind: CheckKind.Skill`/`null`
and `SaveType: null` per the Ask, no logic change. `dotnet build LunaFlow_A.sln` is full-solution
green. Both repointed classes are 19/19 green in isolation. A full `LunaFlow.ContentTests` run shows
5 failures unrelated to this ticket's scope — see Response for detail and routing.

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

Repointed the 4 call sites to the new 11-arg shape, pure insert as scoped:

- `D20SystemHandlerTests.cs` — `ToRoomChallenge` helper: inserted `CheckKind.Skill` after
  `ChallengeType` and `null` (`SaveType`) after `DC`. Inline Fight-room `new RoomChallenge(...)` in
  `StartSession()`: two more `null`s (`Kind`, `SaveType`) alongside the existing six.
- `ComponentRegistryContentTests.cs` — same two patterns, in `NewD20System()`'s Fight-room branch and
  its own `ToRoomChallenge` helper.

No assertion values touched. `dotnet build LunaFlow_A.sln` — full solution green, 0 errors (12
pre-existing warnings, none from these files). `dotnet test tests/LunaFlow.ContentTests --filter
"FullyQualifiedName~D20SystemHandlerTests|FullyQualifiedName~ComponentRegistryContentTests"` —
19/19 pass.

Note: a full `LunaFlow.ContentTests` run (no filter) shows **5 failures outside this ticket's scope**,
all pre-existing and unrelated to `RoomChallenge` construction:
- `LuaOneD20WorldModuleTests.FromEmbeddedResource_DemoScript_ChallengesMatchDefaultExactly` (11 vs 9
  challenges)
- `LuaOneD20WorldModuleTests.FromEmbeddedResource_DemoScript_MonstersMatchDefaultExactly` (scrapper's
  `InflictsPoison` True vs False)
- `LuaOneD20WorldModuleTests.FromEmbeddedResource_DemoScript_FlavorTextMatchesDefaultExactly` (68 vs
  62 flavor-text entries)
- `OneD20WorldModuleValidatorTests.HollowKeepWorldModule_ChallengeMechanics_MatchDefaultsSkillAbilityAndDcPerPoolEntry`
  (11 vs 9)
- `LootRomTests.Provides_IsEmpty_FirstPureSinkRomShape` (`Provides.Count` 1 vs expected 0)

These read as the Lua/Hollow-Keep World Module and `LootRom` not yet reflecting Phase 2.10's new
shape (`CheckKind`/`SaveType`, the Scrapper's `InflictsPoison`) — a content/data gap in `src/`,
not a test defect, and outside this ticket's ask (which was scoped to the compile break only).
Not filing a new ticket myself since 0069 (Baraka, in-review, Phase 2.10 implementation) already
covers this surface — flagging here for Baraka/Kofi to route. Total suite: 72/77 pass (up from the
62 baseline — Phase 2.10 already added test files ahead of this ticket).

### Resolution

**Resolved 2026-08-04 (Baraka, opener).** The ask (repoint 4 `RoomChallenge` call sites to the new
11-arg shape, no logic change) is done exactly as scoped — verified the diff myself: two files, both
patterns are pure argument inserts (`Kind`/`SaveType` slotted in, no existing argument touched), no
assertion values changed anywhere. `19/19` on the two repointed classes, confirmed independently.

The 5 out-of-scope failures Nadia flagged and routed back are also closed now, though that work
happened against [0069](0069-implement-phase-2-10-content-coverage-spec-md.md), not this ticket:
`LootRomTests.Provides_IsEmpty_FirstPureSinkRomShape` was already repointed by Nadia for 0072's
`EquippedWeapon`/`IEquippedWeapon` shape before the flagged trx even ran; the remaining 4
(HollowKeep/Lua parity against the new save checks and Scrapper's `InflictsPoison`) were fixed
directly against 0069 — see its 2026-08-04 addendum. `tests/LunaFlow.ContentTests` is **77/77**,
confirmed by rerunning the full suite, not just trusting the addendum's own count.
