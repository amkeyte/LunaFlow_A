# 0050 — Test harness needs `RoomChallengePrompt`/`GameEndAnnouncement` seeding

**Status:** in-review
**Opened by:** Baraka (Impl Dev)
**Opened:** 2026-07-30
**For:** Nadia (Test Dev)

## Decision

Both asks done. `D20SystemHandlerTests.cs`'s `StartSession()` gained a third `.Seed(() => ...)`
block mirroring `Program.cs`'s exactly (one `RoomChallengePrompt` per room, one
`GameEndAnnouncement`), and its 5 silenced tests' assertions were restored to check real flavor
text. `D20SystemRuleFixtureTests.cs`'s `Setup()` got the same seeding, and its 3 placeholder-text
tests were rewritten (and two renamed back, since they're no longer describing a placeholder) to
check the real content. `TestCategory=Rom_OneD20` — 25/25 pass.

## Details

### Context

Built ticket [0043](0043-generated-rules-have-no-seam-for-rulebook-flavor-text.md) per Amara's
ruling: the challenge-prompt rules and `GameEndRule` now match two new facts —
`RoomChallengePrompt(int RoomId, string Text)` (D20System) and `GameEndAnnouncement(string Text)`
(GridMap) — carrying already-resolved Rulebook flavor text instead of announcing generic placeholder
strings. `Program.cs` seeds one `RoomChallengePrompt` per room plus one `GameEndAnnouncement` in a
new `.Seed(() => ...)` block. Verified correct via manual playthrough (real flavor text end to end:
room 5's `monster-desc`, room 6's `PromptKey`, the `win-announce` win text).

Full suite went from 119/119 to 111/119. All 8 failures traced and confirmed as expected fallout, not
a regression:

- `D20SystemRuleFixtureTests.cs`: `ChallengePrompt_SkillCheckRoom_AnnouncesGenericPlaceholder_*`,
  `ChallengePrompt_FightRoom_AnnouncesGenericPlaceholder_*`, `GameEnd_AnnouncesOnAnyWinMarker` — these
  assert the placeholder text ticket 0044 characterized while explicitly tracking this ticket as
  still open. Now moot; the placeholder is gone.
- `D20SystemHandlerTests.cs`: `EnterSkillCheckRoom_AnnouncesDescriptionOnce_AndPromptOnce_NeverDuplicated`,
  `SessionStart_AnnouncesRoom5Description_BeforeChallengePrompt`,
  `Check_RoomSpecificSuccess_AnnouncesTextAndReadout_SolvesOnce_NoRePrompt`,
  `Win_CombatFreeColumn_FiresWinMarkerAndGameEnd`,
  `Win_LineIncludingCombatRoom_FiresWinMarker_WhenSolvedViaFight` — these don't just see different
  text, the rules go **fully silent**: `StartSession()` wires `GridMapRom`/`D20SystemRom`/handlers
  directly, independently of `Program.cs`, and never seeds the two new facts — so
  `Match<RoomChallengePrompt>(...)`/`Match<GameEndAnnouncement>(...)` never has anything to match,
  and the challenge-prompt/`GameEndRule` rules never fire at all in this harness.

### Ask

Two things, both in `tests/LunaFlow.EngineTests/Rom/D20System/`:

1. **`D20SystemHandlerTests.cs`'s `StartSession()`**: seed one `RoomChallengePrompt` per room and one
   `GameEndAnnouncement`, mirroring `Program.cs`'s new `.Seed(() => ...)` block exactly (same
   `flavorText["monster-desc"]`/`flavorText[r.PromptKey!]` branching, same `flavorText["win-announce"]`
   key) — so the challenge-prompt/`GameEndRule` rules fire in tests the same way they do in
   production.
2. **`D20SystemRuleFixtureTests.cs`**: rewrite the three placeholder-text assertions
   (`ChallengePrompt_SkillCheckRoom_AnnouncesGenericPlaceholder_*`,
   `ChallengePrompt_FightRoom_AnnouncesGenericPlaceholder_*`, `GameEnd_AnnouncesOnAnyWinMarker`) to
   assert the real flavor content instead — same rewrite pattern ticket 0044 already used once for
   the handler-level assertions, applied here to the rule-level ones that were explicitly left
   tracking this now-closed ticket.

Acceptance: full suite green again, read from an Arryn-run `latest.trx`.

### Response

**`D20SystemHandlerTests.cs`:**
- `StartSession()` gained a third `.Seed(() => ...)` block, copied from `Program.cs`'s new one:
  builds one `RoomChallengePrompt` per room (`monster-desc` for Fight rooms,
  `flavorText[r.PromptKey!]` for SkillCheck) plus a `GameEndAnnouncement(flavorText["win-announce"])`.
- Restored the 5 assertions the silence broke, back to checking real content instead of placeholder
  text: `EnterSkillCheckRoom_...` (room-2-prompt), `SessionStart_AnnouncesRoom5Description_BeforeMonsterDesc`
  (renamed back from `BeforeChallengePrompt` — monster-desc is real content now, not a generic
  placeholder), `Check_RoomSpecificSuccess_...` (room-2-prompt re-prompt check), and both
  `Win_*FiresWinMarker*` tests (`win-announce` instead of the literal `"You win!\n"`).

**`D20SystemRuleFixtureTests.cs`:**
- `Setup()` now seeds the same two fact kinds directly into the bare `RuleEngine`, alongside the
  existing room inserts — same resolution logic as `Program.cs`/`StartSession()`.
- Rewrote the 3 tests still tracking the old placeholder: `ChallengePrompt_SkillCheckRoom_...` and
  `ChallengePrompt_FightRoom_...` renamed back to `AnnouncesPromptKey`/`AnnouncesMonsterDesc` (their
  0044-era names, since the placeholder these described is gone) and now assert
  `OneD20FlavorText.Text["room-2-prompt"]`/`["monster-desc"]`; `ChallengePrompt_AfterWinMarker_...`
  and `GameEnd_AnnouncesOnAnyWinMarker` swapped their literal placeholder checks for the real
  `room-4-prompt`/`win-announce` keys.
- Left `D20SystemNarration.CheckNarration`/`CombatStatus` `// TODO` comments untouched — a
  pre-existing, unrelated coverage gap (never-built helper), not part of this ticket's break
  surface.

**Verification:** clean rebuild of `tests/LunaFlow.EngineTests` (0 errors). Ran
`--filter "TestCategory=Rom_OneD20"` myself as a one-off mid-work check: 25/25 pass, all 8
previously-broken tests among them. Full-suite doneness pass is still Arryn's to run per house
rule.

### Resolution

<Filled in by the opener when closing — what was confirmed, and how.>
