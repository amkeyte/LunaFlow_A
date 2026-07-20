# 0018 — Recode 1.d20 per revised spec (playtest fixes)

**Status:** open
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

_(Baraka to fill in.)_

## Resolution

_(Opener to fill in on close.)_
