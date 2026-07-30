# 0045 — Challenge-prompt rule missing Not&lt;RoomSolved&gt;/Not&lt;RoomFailed&gt;/Not&lt;WinMarker&gt; guards

**Status:** resolved
**Opened by:** Nadia (Test Dev)
**Opened:** 2026-07-29
**For:** Baraka (Impl Dev)

## Decision

Fixed in two passes. `Not<RoomSolved>`/`Not<RoomFailed>` restored immediately (D20System-owned, no
boundary question). The `Not<WinMarker>` third guard genuinely conflicted with the split spec's
"WinMarker crosses no boundary" statement and the zero-project-reference acceptance bar, so it was
held pending an Architect ruling rather than guessed at. **Amara's ruling:** promote a fifth,
empty-marker Cartridge contract, `IGameWon` — same mechanism as the other four, `WinMarker : IGameWon`
in GridMap, `GameWon` capability added to GridMap's `Provides`/D20System's `Requires`, rule guards
`Not<IGameWon>()` rather than the concrete fact. Implemented and verified: all 25/25
`TestCategory=Rom_OneD20` tests pass, zero-peer-reference bar between `ExampleA`/`ExampleB` still
holds (verified against the `.csproj` files).

## Details

### Context

Found while fixing ticket [0044](0044-fix-d20systemhandlertests-after-0037.md) (getting `tests/`
compiling again after 0037). Once compiling, a narrow `--filter "TestCategory=Rom_OneD20"` run
(one-off mid-work check, not a doneness pass) turned up 3 real failures, all tracing to the same
cause — none of them related to 0044's compile-fix or to the disclosed flavor-text gap in
[0043](0043-generated-rules-have-no-seam-for-rulebook-flavor-text.md).

`ChallengePromptRule` in
`example/D20SystemExampleA/LunaFlow.Rom/D20System/Rules/D20SystemRuleGroup.cs` currently has a
single `WhenStatements` entry:

```csharp
WhenStatements = new()
{
    $"Match<IPlayerLocation>(pl => pl.RoomId == {roomId})",
},
```

No `Not<RoomSolved>()`, `Not<RoomFailed>()`, or `Not<WinMarker>()` guard — the rule fires purely on
room entry, regardless of whether that room is already solved/failed or the game has already been
won. `grid-rom-d20-split-spec.md` (line 225) says the challenge-prompt rules are "relocated:
unchanged shape," naming only two intentional changes (the `Match<Room>`→`Match<RoomChallenge>` and
`Match<PlayerPosition>`→`Match<IPlayerLocation>` swaps); line 229 explicitly says `Not<RoomSolved>`/
`Not<RoomFailed>` "stay concrete... no cross-ROM read needed." The guards are spec-required and
presumably present pre-split — they're just missing from what shipped.

Three pre-existing tests in `D20SystemRuleFixtureTests.cs` (not written by me, not touched by 0044's
fix pass) pin exactly this and now fail against the real rule:

- `ChallengePrompt_RoomSolved_StopsAnnouncing` — expects 0 messages once `RoomSolved` exists; gets 1.
- `ChallengePrompt_RoomFailed_StopsAnnouncing` — expects 0 messages once `RoomFailed` exists; gets 1.
- `ChallengePrompt_AfterWinMarker_StopsAnnouncing_EvenForUnrelatedUnsolvedRoom` — expects the
  placeholder text absent once any `WinMarker` exists; it's present.

Left these three tests as-is rather than rewriting them to match the broken behavior — unlike the
flavor-text gap (0043, a disclosed and deliberately accepted placeholder), nobody decided the guards
should be dropped; this reads as an oversight in translating the `RulePrototype`, not a ruling.

### Ask

Restore the three `Not<>` guards on `ChallengePromptRule`'s `WhenStatements` (D20System owns
`RoomSolved`/`RoomFailed` concretely per the spec; `WinMarker` is GridMap's, so that guard is the one
cross-ROM read this rule needs — same contract-typed pattern already used for
`Match<IPlayerLocation>`). Once fixed, the three tests above should pass unchanged.

### Response

Two passes, per the user's own sequencing request (fix the safe two now, hold the third):

1. **`Not<RoomSolved>(rs => rs.RoomId == roomId)` / `Not<RoomFailed>(rf => rf.RoomId == roomId)`**
   added to `ChallengePromptRule`'s `WhenStatements` in
   `example/D20SystemExampleA/LunaFlow.Rom/D20System/Rules/D20SystemRuleGroup.cs`. D20System owns
   both facts concretely, exactly as the split spec says — no boundary question, no ticket needed.
   Verified: the narrow `TestCategory=Rom_OneD20` filter went from 3 failures to 1 (only the
   `WinMarker` test still red).
2. **`Not<WinMarker>()` — held rather than guessed at**, since `WinMarker` is GridMap's and the split
   spec states it "crosses no boundary"; writing it directly into `D20SystemRuleGroup` would need a
   project reference from `ExampleA` to `ExampleB`, breaking the zero-peer-reference acceptance
   criterion the whole split is built on. Raised the tension rather than silently adding the
   reference or inventing a new contract unilaterally.

**Amara's ruling, relayed by the user:** promote a fifth Cartridge contract, `IGameWon` — an empty
marker interface (`Not<T>()` never reads a property, so nothing to carry across, not even
`WinMarker`'s own `Line`). Implemented:
- `IGameWon` (no members) added to `LunaFlow.Cartridge` (`example/D20SystemExampleC`).
- `WinMarker : IGameWon` in GridMap's `Facts.cs`.
- `GridMapRom.Provides` gains `("GameWon", typeof(IGameWon))`; `D20SystemRom.Requires` gains the
  same.
- `ChallengePromptRule`'s guard is `Not<IGameWon>()` — the contract type, never `WinMarker` by name,
  same pattern `Match<IPlayerLocation>` already uses.

**Verification:** `dotnet build LunaFlow_A.sln` clean; `dotnet test tests/LunaFlow.EngineTests
--filter "TestCategory=Rom_OneD20"` — 25/25 pass. Re-checked `.csproj` files by hand: `ExampleA`/
`ExampleB` still reference only `ExampleC`, never each other — the zero-peer-reference bar survives
the new contract.

### Resolution

Closed 2026-07-29. Independently re-verified rather than taken on the Response section's word:

- `Not<RoomSolved>`, `Not<RoomFailed>`, `Not<IGameWon>` all present in `ChallengePromptRule`'s
  `WhenStatements` (`example/D20SystemExampleA/LunaFlow.Rom/D20System/Rules/D20SystemRuleGroup.cs:29-31`).
- `IGameWon` exists at `example/D20SystemExampleC/LunaFlow.Cartridge/IGameWon.cs`; `WinMarker : IGameWon`
  in GridMap's `Facts.cs`.
- `GameWon` capability present on both `GridMapRom.Provides` and `D20SystemRom.Requires`.
- `ExampleA`/`ExampleB` `.csproj` files checked by hand — neither references the other, only
  `ExampleC` and the `src/` trio — zero-peer-reference bar intact.
- Clean rebuild of `tests/LunaFlow.EngineTests` (0 errors). Ran the full `TestCategory=Rom_OneD20`
  filter myself: 25/25 pass, including all three originally-failing tests
  (`ChallengePrompt_RoomSolved_StopsAnnouncing`, `ChallengePrompt_RoomFailed_StopsAnnouncing`,
  `ChallengePrompt_AfterWinMarker_StopsAnnouncing_EvenForUnrelatedUnsolvedRoom`).

No further action here. This clears the last blocker [0044](0044-fix-d20systemhandlertests-after-0037.md)
flagged — full-suite doneness verification is still Arryn's to run per house rule, but nothing known
is red.
