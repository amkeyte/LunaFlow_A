# 0066 — Lua World Module maintained test coverage

**Status:** resolved
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-08-03
**For:** Nadia (Test Dev)

## Decision

Built all three asks as `LuaOneD20WorldModuleTests.cs` in `tests/LunaFlow.ContentTests/Rulebook/OneD20/`
(15 tests): 4 malformed-script cases, 2 sandbox-escape cases, and a 9-test full diff of
`FromEmbeddedResource()`'s output against `DefaultOneD20WorldModule` (validator pass + exact
per-field equality across Rooms/Challenges/Monsters/loot table/PC/ability scores/all 62 flavor-text
entries) — record value-equality made the diff a straightforward `Assert.AreEqual` per entry rather
than a new fixture, as 0066 anticipated. Full suite green, self-run:
`LunaFlow.EngineTests` 93/93, `LunaFlow.ContentTests` 77/77 (62 + 15 new). Not yet an Arryn-run
`latest.trx` — routing to Kofi/Lothar per the same handoff shape as 0065.

## Details

### Context

Ticket [0065](0065-implement-lua-world-module-spec-md.md) built `LunaFlow.Scripting`,
`LuaOneD20WorldModule`, and the demonstration Lua script per
[`lua-world-module-spec.md`](../../../Specs/lua-world-module-spec.md). Its acceptance criteria require
demonstrating malformed-script rejection, a sandbox-escape rejection, and a full session running
correctly against a `LuaOneD20WorldModule`-backed session. Per the plan reviewed with Amara
([`CodePlans/0065-lua-world-module-implementation-plan.md`](../../../Roadmap/CodePlans/0065-lua-world-module-implementation-plan.md),
Decision 5), those three proofs were done ad hoc during implementation (exception text captured in
0065's Response) rather than checked into `tests/` — that's your territory, not mine, per `dev.md`. Same
Baraka→Nadia handoff shape as [0052](0052-implement-loot-rom-content-expansion-spec-md.md)→
[0054](0054-loot-rom-test-suite.md) and [0060](0060-implement-world-module-extraction-spec-md.md)→
[0062](0062-tests-lunaflow-contenttests-broken-by-phase-2-8-world-module-extraction.md).

### Ask

Add maintained coverage for `LuaOneD20WorldModule`/`SandboxedScriptHost` to
`tests/LunaFlow.ContentTests` (or `LunaFlow.EngineTests`, whichever fits the existing split better —
your call):

1. **Malformed-script rejection** — at minimum: a Lua syntax error, a missing required top-level key
   (e.g. `rooms`), a field of the wrong type (e.g. `dc` as a string), and an unrecognized `challengeType`
   string. Each should throw `InvalidOperationException` naming the script source and the specific field.
2. **Sandbox-escape rejection** — a script attempting `io.open(...)` or `os.execute(...)` (or equivalent)
   should fail, proving `SandboxedScriptHost`'s `Preset_HardSandbox` configuration is actually
   restrictive.
3. **Full-session integration test** — a session assembled against
   `LuaOneD20WorldModule.FromEmbeddedResource()` behaves identically to one assembled against
   `DefaultOneD20WorldModule` for the same fixed inputs (same bar `OneD20SessionAssemblerTests`/
   `OneD20WorldModuleValidatorTests` already hold for the C#-authored modules) — at minimum, confirm
   `OneD20WorldModuleValidator.Validate()` accepts it without modification and that `Rooms`/`Challenges`/
   `Monsters`/etc. match `DefaultOneD20WorldModule`'s values exactly (the demo script is a verbatim
   transcription, so this should be a straightforward equality/diff assertion, not a new fixture).

The demo script lives at `src/LunaFlow.Rulebook/OneD20/Content/DefaultOneD20WorldModule.lua`, embedded
as a resource in `LunaFlow.Rulebook.csproj`. `LuaOneD20WorldModule`'s constructor takes a raw Lua
`string` directly (no filesystem dependency needed for malformed/hostile-script tests — inline the Lua
source as a string literal, same as the throwaway probe 0065 used).

### Response

Added `tests/LunaFlow.ContentTests/Rulebook/OneD20/LuaOneD20WorldModuleTests.cs`
(`[TestCategory("Rom_OneD20")]`/`[TestCategory("Phase2.9")]`, matching the tag convention the sibling
`OneD20SessionAssemblerTests`/`OneD20WorldModuleValidatorTests` already use for this namespace; no
`GameEngineCore` touch, so no `[DoNotParallelize]` needed beyond the assembly-level default).

**1. Malformed-script rejection** (4 tests, each asserting `InvalidOperationException` naming both
`sourceName` and the specific field, driven through `new LuaOneD20WorldModule(source, sourceName)`
directly — per the ticket's note, no filesystem dependency needed):
- `Construct_SyntaxError_ThrowsNamingSourceAndFailsToExecute`
- `Construct_MissingRoomsKey_ThrowsNamingTopLevelAndRooms`
- `Construct_ChallengeDcAsString_ThrowsNamingChallengeAndField`
- `Construct_UnrecognizedChallengeType_ThrowsNamingRoomAndValue`

**2. Sandbox-escape rejection** (2 tests), same construction path — `io.open`/`os.execute` calls
fail during `SandboxedScriptHost.Execute`'s `DoString` (wrapped as `InvalidOperationException` by the
constructor) because `io`/`os` are absent globals under `Preset_HardSandbox`, not present-but-guarded:
- `Construct_ScriptCallsIoOpen_RejectedBecauseIoIsNotSandboxed`
- `Construct_ScriptCallsOsExecute_RejectedBecauseOsIsNotSandboxed`

**3. Full-session integration coverage** (9 tests) against `LuaOneD20WorldModule.FromEmbeddedResource()`:
- `FromEmbeddedResource_DemoScript_PassesOneD20WorldModuleValidatorUnmodified` — confirms
  `OneD20WorldModuleValidator.Validate()` accepts the Lua-marshaled module unmodified.
- 8 exact-match tests (`StartRoomId`, `Rooms`, `Challenges`, `Monsters`, `ItemIdByMonsterId`,
  `PlayerCharacter`, `PlayerAbilityScores`, `FlavorText`) diffing every entry against
  `DefaultOneD20WorldModule`. `OneD20RoomData`/`OneD20ChallengeTemplate`/`OneD20MonsterTemplate`/
  `OneD20CombatantStats` are all `sealed record`s, so `Assert.AreEqual` gives exact structural
  equality per entry — no need for HollowKeep-style field-by-field zipping, since the Lua script is a
  verbatim transcription (identical keys, not a reskin with renamed keys).

Did not re-run a full-session command-by-command playthrough (move/look/check/fight/flee/inventory) —
that was already demonstrated ad hoc in 0065 against the real `GameEngineCore`/`Program.cs` swap-line,
and re-driving it here would duplicate `D20SystemHandlerTests`' existing handler-level coverage
against the *shape* this module produces rather than testing anything Lua-specific. The validator
pass + exact-value diff against `DefaultOneD20WorldModule` (which `D20SystemHandlerTests` already
exercises end-to-end) covers the same ground without a second copy of that fixture.

Verified: `dotnet build LunaFlow_A.sln` 0 warnings/0 errors;
`dotnet test tests/LunaFlow.ContentTests --filter "FullyQualifiedName~LuaOneD20WorldModuleTests"` →
15/15; full `dotnet test tests/LunaFlow.ContentTests` → 77/77 (62 pre-existing + 15 new); full
`dotnet test tests/LunaFlow.EngineTests` → 93/93, unaffected. All self-run, not yet an Arryn-run
`latest.trx`.

### Resolution

Independently re-verified, not just read: `dotnet build LunaFlow_A.sln` clean (0/0); the new
`LuaOneD20WorldModuleTests` filtered in isolation → 15/15; full `dotnet test tests/LunaFlow.ContentTests`
→ 77/77; full `dotnet test tests/LunaFlow.EngineTests` → 93/93 — all match Nadia's reported numbers
exactly. Read the test file itself, not just the Response summary: real construction path throughout
(`new LuaOneD20WorldModule(source, sourceName)` / `LuaOneD20WorldModule.FromEmbeddedResource()`), no
tautologies or simulated logic, exception-message assertions check both the `sourceName` and the
specific-field text rather than just "an exception was thrown." The full-session-replay scope call
(validator pass + exact per-field diff against `DefaultOneD20WorldModule`, instead of re-driving
move/look/check/fight/flee/inventory through `GameEngineCore`) is sound — 0065 already proved that path
end-to-end against the real engine, and `D20SystemHandlerTests` already covers the handler behavior a
second command-by-command replay here would just be duplicating. All three asks from this ticket are
met. Closing.
