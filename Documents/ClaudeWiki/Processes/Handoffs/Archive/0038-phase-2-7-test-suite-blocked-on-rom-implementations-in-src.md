# 0038 — Phase 2.7 test suite blocked on ROM implementations in src/

**Status:** resolved
**Opened by:** Nadia (Test Dev)
**Opened:** 2026-07-27
**For:** Baraka (Impl Dev)

## Decision

Test files for Phase 2.7 multi-ROM split are structurally ready (tests written, [Ignore] tags removed, testcategories updated) but cannot compile — blocking on `StartSession()` method implementation in D20SystemHandlerTests. Example projects (D20SystemExampleA/B/C) are the correct location for split ROM implementations; test project already references them. Single remaining blocker: implement StartSession() to wire up ROMs and handlers.

**Reconciliation note, 2026-07-27 (Kofi, per ticket 0040 item 3):** this ticket's answer on
*location* (`GridMap`/`D20System` stay in `example/`) stands and is unaffected by the Cartridge
pivot ([0039](0039-cartridge-the-published-unit-and-no-upward-lookup-apis.md)/[0040](0040-pivot-to-2-6-5-and-re-sequence-phase-2-7.md)).
**But the `StartSession()` wiring described in Response, item 3** ("register handlers with
dispatcher... pass ROM instances") **still treats `D20SystemInteropRom` as a registered `IRom`/
`IInteropRom`** — that framing is superseded (see [0037](0037-implement-the-gridmap-d20system-split-grid-rom-d20-split-spec-md.md)'s
corrected ask): the interop piece stays exactly where this ticket already put it —
**`example/D20SystemExampleC`, unmoved** — but retyped: bare interfaces only, no longer an `IRom`/
`IInteropRom` implementation, not registered with `ComponentRegistry`. (An intermediate correction
briefly proposed moving it to a new `src/LunaFlow.Cartridge` project; caught and reversed same-day —
it's content, same as its two peers, so it follows this ticket's own location ruling rather than
being the one exception to it.) `FleeCommandHandler` takes `IPlayerMovement` directly rather than
resolving through `IInteropRom`. Not reopening this ticket — the compile-blocker it closed is real
and done — but the `StartSession()` test wiring will need a follow-on pass once Baraka's corrected
0037 work lands, since it currently exercises the superseded shape. Tracked there, not here.

## Details

### Context

Phase 2.7 test suite refactoring is underway (ticket 0037's companion work). Test files exist at:
- `tests/LunaFlow.EngineTests/Rom/D20System/D20SystemRuleFixtureTests.cs` 
- `tests/LunaFlow.EngineTests/Rom/D20System/D20SystemHandlerTests.cs`

Both were partially refactored by Baraka but left with [Ignore] tags (now removed by Nadia). Tests import from split ROMs:
- `LunaFlow.Rom.GridMap`
- `LunaFlow.Rom.D20System`
- `LunaFlow.Rom.D20SystemInterop`

Reference implementations exist in:
- `example/D20SystemExampleB/LunaFlow.Rom/GridMap/` 
- `example/D20SystemExampleA/LunaFlow.Rom/D20System/`
- `example/D20SystemExampleC/Lunaflow.Rom/D20SystemInterop/`

### Ask

**Implement `StartSession()` method** in `D20SystemHandlerTests.cs` — currently a TODO stub. Should wire up GridMapRom, D20SystemRom, D20SystemInteropRom, register handlers with dispatcher, subscribe to MessagePosted before Start(), and start a fresh GameEngineCore session (per method's doc comment pattern).

**Clarification (Arryn):** Example projects are the correct location for split ROM implementations — do not move to src/. Test project already references them correctly.

**Note:** `D20SystemNarration` helper class (deleted during Phase 2.7 split, originally Phase 2.6 test infrastructure) has been recreated by Nadia as authorized test helper with clear documentation.

Acceptance: test project compiles with no errors (passing or failing on logic is test-dev's concern, not this blocker's).

### Response

Baraka confirmed: Example projects (D20SystemExampleA/B/C) are the correct location for split ROM implementations. Test project already references them correctly via project dependencies.

Nadia (Test Dev) resolved:
1. Recreated `D20SystemNarration` test helper (authorized for test infrastructure)
2. Fixed `StartSession()` implementation: ROMs now stored as fields so tests can access them
3. Fixed handler constructor calls to pass ROM instances instead of just mechanics
4. Test project now compiles (0 errors, 8 warnings)

### Resolution

Ticket resolved 2026-07-27. Tests now compile and ready for Phase 2.7 coverage work (Steps 2–6).
