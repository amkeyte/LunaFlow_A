# 0044 — Fix `D20SystemHandlerTests.cs` after ticket 0037

**Status:** resolved
**Opened by:** Kofi (PM)
**Opened:** 2026-07-29
**For:** Nadia (Test Dev)

## Decision

`tests/LunaFlow.EngineTests` compiles again (0 errors) — the literal Cartridge/interop swap plus
several more compile-breaking changes from 0037's shipped shape that the ticket text didn't name
(handler constructors now require `flavorText`; ROM constructors want ROM-typed data, not
Rulebook types directly; `RoomChallengeType` needs translation). `D20SystemRuleFixtureTests.cs`
was fixed in the same pass (in scope per user decision) — it had its own undiscovered compile gap
(missing `GridMap` using, never compiled `GridMapRuleGroup` so `WinLine`/`GameEnd` rules were never
loaded). Assertions that depended on real Rulebook flavor text for *rule*-fired announcements were
rewritten to characterize the actual placeholder text those rules emit today, per the disclosed gap
in [0043](0043-generated-rules-have-no-seam-for-rulebook-flavor-text.md). A narrow `--filter
"TestCategory=Rom_OneD20"` run surfaced 3 real failures unrelated to any of the above — a
missing-guard defect in `ChallengePromptRule`, filed separately as
[0045](0045-challenge-prompt-rule-missing-solved-failed-winmarker-guards.md) to Baraka. Those 3
tests were left failing on purpose (evidence, not silently rewritten). **Update 2026-07-29: 0045 is
now resolved** (guards restored, Amara ruled on the `WinMarker` boundary question via a new
`IGameWon` Cartridge contract) — re-ran `TestCategory=Rom_OneD20` myself, 25/25 pass. Full-suite
Arryn-run pass is the next step; nothing known is red.

## Details

### Context

Anticipated back in ticket [0038](0038-phase-2-7-test-suite-blocked-on-rom-implementations-in-src.md)'s
reconciliation note — the moment's arrived. Baraka's build of ticket
[0037](0037-implement-the-gridmap-d20system-split-grid-rom-d20-split-spec-md.md) deleted
`LunaFlow.Rom.D20SystemInterop`/`D20SystemInteropRom` entirely (superseded by the `LunaFlow.Cartridge`
namespace, per the Cartridge pivot). `tests/LunaFlow.EngineTests/Rom/D20System/D20SystemHandlerTests.cs`
still references both — the solution no longer compiles.

`grid-rom-d20-split-spec.md`'s acceptance criteria (which 0037 inherits) end on "Full suite green,
read from an Arryn-run `latest.trx`." That can't happen until this compiles again, so this ticket is
what's blocking 0037 from closing.

### Ask

Get `tests/LunaFlow.EngineTests` compiling and green again against the shipped 0037 shape:

- Replace any reference to `LunaFlow.Rom.D20SystemInterop`/`D20SystemInteropRom` with the
  `LunaFlow.Cartridge` namespace's bare interfaces (`IPlayerLocation`/`IChallengeSolved`/
  `ICombatStatus`/`IPlayerMovement`) — there's no broker instance to construct or register anymore;
  `FleeCommandHandler` takes `IPlayerMovement` via constructor injection (`GridMovementService`,
  from GridMap's project).
- `StartSession()`'s wiring (per 0038's Response) needs the same correction — it currently constructs
  and registers a `D20SystemInteropRom` that no longer exists.
- Confirm `ComponentRegistry` setup in the test harness matches `Program.cs`'s real registration:
  two `IRom`s (`GridMap`, `D20System`), not three.

Once it compiles: full suite run, Arryn-run per the standing hard rule (`agent-test-workflow.md`),
`.trx` read from `tests/LunaFlow.EngineTests/TestResults/latest.trx`. Report back here — this is
what 0037 is waiting on before it can close, and what a QA pass will want as evidence.

### Response

Confirmed with the PM upfront that the scope was wider than the ticket text (see Decision) before
touching any file — three questions, all answered "yes, fold it in": (1) fix the extra
compile-breaking changes found beyond the literal interop swap, (2) rewrite flavor-text assertions
to characterize the real placeholder text rather than leaving them `[Ignore]`d, (3) fix
`D20SystemRuleFixtureTests.cs` in the same pass even though it wasn't named in the ticket and
wasn't itself a compile-blocker.

**`D20SystemHandlerTests.cs`:**
- Swapped `using LunaFlow.Rom.D20SystemInterop;` for `using LunaFlow.Cartridge;`; dropped the
  `D20SystemInteropRom _interop` field entirely — `FleeCommandHandler` now takes a bare
  `GridMovementService` (which implements `IPlayerMovement`) via direct constructor injection, no
  broker/`Resolve<T>` call.
- `StartSession()`: translated `OneD20RulebookContent`'s Rulebook-owned data into the ROM's own
  fact/constructor shapes, mirroring `Program.cs` exactly — `OneD20ChallengeType` → `RoomChallengeType`
  via the same ternary, `OneD20CombatantStats` → `PlayerCharacter`/`MonsterTemplate` wrapping
  `CombatantStats`. Added `flavorText` (`OneD20FlavorText.Text`) to all five handler constructors
  (`Move`/`Look`/`Check`/`Fight`/`Flee`), including the ~10 mid-test re-registrations that swap in a
  seeded mechanic.
- Removed the `ClassInitialize` call to `D20SystemContent.Configure(...)` — that class is now an
  `[Obsolete]`, object-typed legacy stub nothing in the shipped rule/handler path reads from; the
  call compiled but did nothing.
- Rewrote 5 assertions that checked real Rulebook flavor text against *rule*-fired output (the
  challenge-prompt announcement, the win announcement) to check the actual placeholder text those
  generated rules emit (`"[Room N] Challenge prompt."`, `"You win!\n"`), each with a one-line comment
  pointing at 0043. Left 3 pre-existing `// TODO: Pending Phase 2.7 test refactor` comments alone —
  they reference a `D20SystemNarration.CheckNarration`/`CombatStatus` helper that was never built;
  unrelated to this ticket's break surface, not touched.

**`D20SystemRuleFixtureTests.cs`:**
- Added the missing `using LunaFlow.Rom.GridMap;` (it referenced `WinMarker`, defined there, with no
  import — a second, previously undiscovered compile gap).
- `ClassSetup`/`Setup` only ever compiled and loaded `D20SystemRuleGroup`. Win-line rules and
  `GameEndRule` live in `GridMapRuleGroup` now (per 0037's split) — every `WinLine_*`/`GameEnd_*` test
  was firing against an engine that never loaded those rules. Now compiles and loads both rule-group
  assemblies.
- Removed the same dead `D20SystemContent.Configure(...)` call.
- Renamed and rewrote the two `ChallengePrompt_*` tests whose names/assertions claimed
  Rulebook-specific text (`AnnouncesPromptKey`, `AnnouncesMonsterDesc`) to describe and assert the
  real generic placeholder; fixed the `AfterWinMarker` test's placeholder check and
  `GameEnd_AnnouncesOnAnyWinMarker`'s `D20SystemNarration.WinAnnouncement(...)` call (that method
  only takes a line argument the real rule never uses — replaced with the literal `"You win!\n"`).

**Verification:** `dotnet build tests/LunaFlow.EngineTests/LunaFlow.EngineTests.csproj` — clean
rebuild after clearing `obj`/`bin` — 0 errors. One-off narrow `--filter
"TestCategory=Rom_OneD20"` (25 tests): 22 pass, 3 fail. All 3 failures are the *same* real defect —
`ChallengePromptRule` ships with no `Not<RoomSolved>`/`Not<RoomFailed>`/`Not<WinMarker>` guard at
all, contradicting `grid-rom-d20-split-spec.md`'s explicit statement that these rules are
"relocated: unchanged shape" and that the two `Not<>` guards "stay concrete." Filed as
[0045](0045-challenge-prompt-rule-missing-solved-failed-winmarker-guards.md) to Baraka — left the 3
tests failing rather than rewriting them to match broken behavior, since (unlike 0043) nobody
decided the guards should be gone.

**Not done, and out of my lane:** fixing `D20SystemRuleGroup`'s missing guards (`example/`, Baraka's
territory) and ruling on 0043's flavor-text-seam question (Amara's). Full-suite doneness run is
Arryn's per house rule — I only ran the narrow filter above as a mid-work sanity check.

### Resolution

Closed 2026-07-29. The full-suite Arryn-run pass this ticket was waiting on happened, and it's
independently confirmed — not just Nadia's own narrow-filter runs. Lothar's Phase 2.7 QA verdict
(`phase-2-7-verdict.md`) cites `tests/LunaFlow.EngineTests/TestResults/latest.trx`, timestamped
2026-07-29 22:49:44, explicitly noted as **Arryn-run, not solo-run by QA**: 119/119 passed, 0 failed,
with `D20SystemHandlerTests`/`D20SystemRuleFixtureTests` confirmed present in that run (not a stale
snapshot). That's this ticket's own acceptance bar, met and evidenced by someone other than the
assignee. Compile break fixed, 0045's guard defect resolved and folded in, full suite green — nothing
left open here.

This also clears the blocker I'd noted on [0037](0037-implement-the-gridmap-d20system-split-grid-rom-d20-split-spec-md.md)'s
review — same evidence applies. Not closing 0037 as part of this ticket, but flagging it's likely
ready too if you want to take that up separately.
