# 0018 — Recode 1.d20 per revised spec (playtest fixes)

**Status:** resolved
**Opened by:** Game Designer (Juno)
**Opened:** 2026-07-20
**For:** Dev — Implementation (Baraka)

## Context

Arryn ran ticket 0016's build against the actual console host — first real playtest of 1.d20.
Surfaced four content-spec gaps (not implementation bugs; confirmed against source before
concluding that) plus one design gap found on review rather than reported directly. All five are
now fixed in [`1-d20-spec.md`](../../../Specs/1-d20/1-d20-spec.md) and
[`1-d20-flavor.md`](../../../Specs/1-d20/1-d20-flavor.md) — see the spec's two dated revision
notes at the top for the full reasoning. This ticket is the recode ask against that revision.

0016 isn't wrong to have built what it built — it matched the spec as written at the time, and
Baraka's disclosed judgment calls (silent no-op on a solved/failed room; `flee` from room 5 with
no prior move) and the disclosed cosmetic double-announce quirk were exactly the right way to
surface this. Two of those disclosures are now formalized into the spec proper (see its Open
questions section); the double-announce is what triggered the rest of this revision.

## What changed, in short

1. **`Room` gains `PromptKey`, `SuccessKey`, `FailureKey`** alongside the existing
   `DescriptionKey`. The double-announce bug is gone because `DescriptionKey` (announced by
   `MoveCommandHandler`, always) and `PromptKey` (announced by the challenge-prompt rule, only
   while unsolved) are now different text, the way Nine Rooms' `Description`/`ChallengePrompt`
   always were — 1.d20 had collapsed them into one field that both call sites announced.
2. **Every `fight` roll now announces something.** Hit/miss plus damage and remaining HP, both
   directions, every round — not just the three terminal outcomes.
3. **`look` reports both combatants' HP while a `CombatRound` is active.**
4. **`check` announces a room-specific success/failure pair**, not the old shared
   `check-success`/`check-failure`, plus a mechanical roll/DC readout.
5. **`PlayerHp`/`PlayerDown` are gone.** `CombatRound` now carries `PlayerHp` alongside
   `MonsterHp`, seeded fresh (both combatants at full HP) every time a fight starts — no
   persistence across separate fights, including after a `flee`. Losing a fight now inserts
   `RoomFailed` (same as the 3-round timeout) instead of ending the session. No more
   session-wide `PlayerDown` guard on `check`/`fight`.

`D20ResolutionMechanic`/`IMechanic` are unaffected by this revision — ticket 0015's ruling and
0016's build of it stand as-is; the spec's Framework Dependencies section now just cites the
concrete type instead of describing it as an open question.

## Ask

Recode `LunaFlow.Rom.OneD20` against the revised spec. Concretely, starting from what 0016 built:

1. Add `PromptKey`/`SuccessKey`/`FailureKey` to the `Room`/`OneD20Content` shape; wire the new
   flavor keys from `1-d20-flavor.md` (6 `-prompt` keys, 12 room-specific check keys, 4 new combat
   narration keys, `pc-lost` replacing `pc-down`).
2. Fix `OneD20RuleGroup`'s challenge-prompt rule to announce `PromptKey` (skill rooms) instead of
   `DescriptionKey` — leave `MoveCommandHandler`'s existing `DescriptionKey` announce as the sole
   source of that text. Combat rooms keep announcing `monster-desc`, unchanged.
3. Rework `FightCommandHandler`: per-round hit/miss narration with damage/HP; fold `PlayerHp` into
   `CombatRound`, seeded fresh per fight; PC HP ≤ 0 inserts `RoomFailed` (not `PlayerDown`) and
   announces `pc-lost`; remove the `PlayerDown` guard entirely (nothing left to guard against).
4. Rework `CheckCommandHandler`: room-specific `SuccessKey`/`FailureKey` plus the mechanical
   roll/DC readout.
5. Extend `LookCommandHandler`: report both combatants' HP when a `CombatRound` is active for the
   current room.
6. Delete `PlayerHp`/`PlayerDown` from `Facts.cs`; delete whatever guard logic referenced them.

Full behavior detail is the spec's own [Mechanics / rules](../../../Specs/1-d20/1-d20-spec.md#mechanics--rules)
section — this list is a summary, not the source of truth.

**Acceptance criteria:** the (revised) checklist in `1-d20-spec.md`'s Acceptance criteria section.
Several items are new (per-round narration, `look`-shows-HP, fresh-HP-per-fight, the
`RoomFailed`-not-session-ending behavior) — don't assume 0016's manual coverage still applies
without re-checking each one against the new behavior.

**If anything turns out unbuildable or ambiguous**, that's a ticket to Amara, same channel as
before — not a silent workaround.

## Downstream note, not part of this ask

[Ticket 0017](0017-1-d20-test-suite.md) (Nadia's automated suite) was filed against 0016's
pre-revision behavior and will need to move with this recode — flagging so Kofi can loop her in
once this lands, not asking Baraka to touch her ticket.

## Response

Recoded against the revised spec, all six ask items:

1. **`Room` gains `SuccessKey`/`FailureKey`/`PromptKey`** (`Facts.cs`), null for Fight rooms.
   `OneD20Content.cs`'s room table wired to the new `1-d20-flavor.md` keys (6 `-prompt` keys, 12
   room-specific check keys). `OneD20Flavor.cs` rewritten: shorter `-desc` text matching the
   revised table, `pc-down`→`pc-lost`, `check-success`/`check-failure` replaced by 12 room-specific
   keys, 4 new combat keys (`pc-hit`/`pc-miss`/`monster-hit`/`monster-miss`).
2. **`OneD20RuleGroup`'s `ChallengeRule`** now announces `room.PromptKey!` for skill-check rooms
   (was `DescriptionKey`) and only `monster-desc` for combat rooms — no longer chains a second
   `.Do()` for the description, since `DescriptionKey` isn't this rule's job anymore.
   `MoveCommandHandler` was already the sole `DescriptionKey` announcer (unchanged) — that's what
   makes the double-announce actually gone, not just the rule's half of it.
3. **`FightCommandHandler` reworked**: per-round hit/miss narration on both sides via two new
   `OneD20Flavor` helpers (`HitNarration`, plain `Text[...]` for misses); `CombatRound` gained a
   `PlayerHp` field, seeded fresh from `PlayerCharacter.Stats.MaxHp` alongside `MonsterHp` every
   time a round starts (fresh fight, fresh HP, no carryover — including after `flee` or a loss in a
   different room); PC HP ≤ 0 now inserts `RoomFailed` + announces `pc-lost` instead of
   `PlayerDown`; the `PlayerDown` guard is gone (checked at the top of every handler in 0016 — none
   of them need it anymore).
4. **`CheckCommandHandler` reworked**: `D20ResolutionMechanic.ResolveCheck` now returns a
   `CheckResult` struct (`D20`/`Modifier`/`Total`/`TargetNumber`/`Success`) instead of a bare
   `bool` — the only way to get the actual roll for the readout without re-rolling. Announces
   `room.SuccessKey!`/`room.FailureKey!` via a new `OneD20Flavor.CheckNarration` helper combining
   the room-specific text with "`{Skill}: rolled {Total} vs. DC {DC}.`" I used the *total*
   (roll+modifier), not the raw d20, since the raw value alone would misreport a Str-keyed check
   (e.g. Climb) as a near-miss when the modifier actually carried it — flagging the call since the
   spec just says "roll" without specifying which.
5. **`LookCommandHandler` extended**: announces `OneD20Flavor.CombatStatus(...)` (both HP over
   max) when a `CombatRound` is active for the current room, via `CombatRound.PlayerHp`/`MonsterHp`
   directly — no separate PC-HP fact to look up anymore.
6. **`PlayerHp`/`PlayerDown` deleted** from `Facts.cs`; the seed in `Program.cs` no longer inserts
   a `PlayerHp` fact (removed, not replaced — `CombatRound` seeds its own HP per-fight).

`D20ResolutionMechanic`/`IMechanic` themselves are otherwise unchanged (per the spec's own note) —
only `ResolveCheck`'s return type grew from `bool` to `CheckResult`, an internal signature change
to my own class, not a framework or `IMechanic`-shape change.

**Verification:** `dotnet build` — 0 errors (8 pre-existing warnings, none from this change,
confirmed via a `--no-incremental` full rebuild diffed against 0016's baseline). `dotnet test
tests/LunaFlow.EngineTests` — 71/71 green, unaffected. Manual transcripts against the running
console host re-verified every acceptance-criteria item against the new behavior specifically (not
assuming 0016's coverage carried over, per the ticket's own instruction):

- No double-announce: room 2's desc/prompt print as distinct text; room 1's desc/`monster-desc`
  likewise. Confirmed by inspection of a full transcript, not just absence of an exact duplicate.
- Room-specific `check` text + mechanical readout observed directly (e.g. "You spot the latch..."
  vs. a shared generic message, with "`Search: rolled 13 vs. DC 10.`" appended).
- Every `fight` round narrates — hit, miss, and the terminal outcomes all produced output across
  repeated trials; no silent rounds observed.
- `look` mid-fight reports both HPs (e.g. "You're at 9/11 HP. He's at 2/8 HP.") — verified via
  transcripts calling `look` between `fight` rounds.
- Fresh `CombatRound` seeds both combatants at full HP — confirmed via `look` immediately after a
  fresh `fight` start, including after a `flee`-and-retry.
- Lethal hit skips the counterattack (verified: damage line immediately followed by
  `monster-defeated`, no monster-attack line in between).
- PC HP reaching 0 inserts `RoomFailed` + `pc-lost`, and the session demonstrably continues — ran a
  transcript where room 1 was lost via `fight`, then rooms 2/3/6/9 were solved via `check`
  afterward with no disruption.
- **`RoomFailed` permanently kills a win line, both causes checked**: forced room 1 to `RoomFailed`
  via a lost fight (not just the timeout), then solved rooms 2 and 3 — confirmed the top-row line
  `[1,2,3]` never fires (no `WinMarker`/`GameEndRule` output for it) while the unrelated column-3
  line `[3,6,9]` still fires normally once solved. The timeout-cause path is structurally identical
  code (same `RoomFailed` insert, same win-line guard mechanism) and was already exercised in
  0016's testing; the loss-cause path is what's new here and is what I specifically re-verified.
- `flee` mid-fight retains no partial state; a subsequent `fight` in the same room starts over.
- Fleeing room 5 with no prior `move` still doesn't move the PC and doesn't throw (fallback logic
  untouched by this recode).
- Combat-free column-3 win and the top-row line (`[1,2,3]`, which requires room 1 solved via
  `fight`) both fire `WinMarker`/`GameEndRule` correctly.

One thing worth flagging back: `1-d20-flavor.md`'s worked room-desc/prompt table doesn't give
combat rooms a prompt row at all (marked "no prompt text — `monster-desc` covers it"), which is
exactly what got implemented — flagging only because it means combat rooms have no room-specific
prompt text distinct from the shared monster line, unlike every skill-check room. Not a gap, just
confirming that's deliberate before treating it as an oversight later.

No changes needed to `move`/`look`'s reused adjacency logic, `WinLineRule`/`GameEndRule` in
`OneD20RuleGroup.cs`, or `D20ResolutionMechanic.Roll` — none were touched by this revision's ask.

Ticket [0017](0017-1-d20-test-suite.md) (Nadia's suite) still needs to move with this recode per
your own downstream note — I haven't touched it, leaving that hand-off to you as flagged.

---

**Update — second playtest's two fixes (spec's third revision note):**

1. **Lethal-hit contradiction, `FightCommandHandler.cs`.** Reordered both attack blocks so damage
   is applied and lethality checked *before* choosing which line to announce — a killing PC hit now
   announces `monster-defeated` only (no `pc-hit` first), and a killing monster hit announces
   `pc-lost` only (no `monster-hit` first). Same structural fix both directions, matching the
   spec's steps 2–3 exactly. Verified across ~30 combat trials: every `monster-defeated`/`pc-lost`
   line was checked for an immediately-preceding hit-narration line on the *same* round — none
   found. (Trials with an earlier non-lethal `pc-hit` in a prior round, followed by a later lethal
   round showing only `monster-defeated`, confirm non-lethal narration is untouched — only the
   killing round itself skips its own hit line.)
2. **Room 5's missing startup description, `Program.cs`.** This one took a bit more than "add an
   `Announce()` call" — `LunaFlowEngine.Host.Announce()` is a no-op with nobody listening until
   `ConsoleEngine.Run()` wires up `MessagePosted`, and that wiring happens *inside* `Run()`, which
   Program.cs's own top-level code runs entirely *before* calling. A bare `Announce()` call placed
   anywhere in `Program.cs` before `engine.Run()` would just fire into the void. The one place
   content code executes *after* that subscription and *before* the initial-settle `Fire()` is a
   `RuleEngineOptions.Seed()` provider — those are invoked lazily inside `GameEngineCore.Start()`,
   which itself runs from inside `Run()`, after the subscription, and completes seeding before
   `Fire()` runs. So the fix is the seed lambda that already provides `PlayerPosition`/
   `PlayerCharacter`/`Monster` also announcing `OneD20Content.Rooms[StartRoomId].DescriptionKey` as
   a side effect first. Not an engine change — reusing existing plumbing, per the spec's own
   framing ("content seeding... wherever the initial `Announce()` calls already happen"). Verified:
   a fresh transcript now shows the room-5 description line immediately before `monster-desc`, not
   `monster-desc` alone.

**Verification:** `dotnet build` — 0 errors, 0 warnings. `dotnet test tests/LunaFlow.EngineTests` —
71/71 green, unaffected (neither fix touches anything the fixture tests exercise). Both new
acceptance-criteria items manually re-verified against running transcripts as above.

## Resolution

Closed by Juno (opener). Confirmed against source directly, not taken on the writeup alone:

- `FightCommandHandler.cs`: damage is applied and lethality checked before either announce branch
  runs — a killing hit `return`s immediately after `monster-defeated`/`pc-lost`, with no code path
  that can print `pc-hit`/`monster-hit` first. Matches the spec's steps 2–3 exactly.
- `Program.cs`: the room-5 `DescriptionKey` announce lives in the `Seed()` lambda, with a comment
  explaining why that's the one safe spot (runs after `MessagePosted` is subscribed, before the
  initial-settle `Fire()`) — a real fix, not a workaround bolted on to satisfy the letter of the ask.
- Combat rooms having no dedicated `PromptKey` (Baraka's flagged question): confirmed deliberate,
  per the spec's own "PromptKey is null for these rooms; unused" — not a gap.

Both playtest rounds' findings are addressed; build green, 71/71 unaffected, manual coverage
specifically re-verified the things that broke rather than re-asserting old coverage. Phase 2.5's
overall done-bar (a Lothar verdict, same shape as prior phases) is a separate, later step — not
blocked on anything left open here. [Ticket 0017](0017-1-d20-test-suite.md) still needs to move to
this revision; unchanged status, still Kofi's to route to Nadia per 0018's own downstream note.
