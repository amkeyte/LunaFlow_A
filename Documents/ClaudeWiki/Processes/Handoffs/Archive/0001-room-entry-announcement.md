# 0001 — Room-entry announcement: spec transcript vs. implementation

**Status:** resolved
**Opened by:** Supreme Leader (Kofi)
**Opened:** 2026-07-17
**For:** Architect (Amara)

## Context

Full code review of Phase 1.5 (Nine Rooms) found the implementation complete and faithful to
`Documents/NineRooms/nine-rooms-spec.md` in almost every respect — facts, room table, adjacency,
9 challenge rules, 8 win rules, game-end rule, all three handlers wired in `Program.cs`, engine
seeding + initial-settle + fire-after-dispatch all present.

One genuine spec-vs-code discrepancy remains, around what gets printed when a player enters a room.

**Implementation today:** `MoveCommandHandler.Handle` (`src/LunaFlow.Rom/NineRooms/Handlers/MoveCommandHandler.cs`)
on a successful move only retracts/inserts `PlayerPosition` and returns — it announces nothing.
The room's challenge-prompt rule then fires and announces *only* `Room.ChallengePrompt`. The room
name/description is never emitted on entry; only the `look` command prints `Room.Description`.

**Spec expects otherwise, in two places:**

1. The acceptance transcript shows the room name before the challenge on entry, e.g.:
   ```
   > go north
   The Long Hall. I have keys but no locks, space but no room. What am I?
   ```
   Current code would print just `I have keys but no locks...` — no `"The Long Hall."` prefix.

2. The "Decisions carried over" note says re-entering an already-solved room "just redisplays the
   description." Current code prints **nothing** on entering a solved room (challenge rule is
   `Not<RoomSolved>`-guarded, and `go` emits no description of its own).

Hand-traced winning path (paper simulation, not an executed run — the reviewing session has no
.NET 9 toolchain) showing both gaps:
```
> go north
I have keys but no locks, space but no room. What am I?   ⚠ no "The Long Hall." prefix
> answer keyboard
Solved.
> go west
What is 7 + 5?
> answer 12
Solved.
> go east
                                                          ⚠ silence — re-entering solved room 2
> go east
What is 9 × 3?
> answer 27
Solved.
You solved the top row! (rooms 1, 2, 3)
```

## Ask

Rule on the intended room-entry behavior so Baraka can implement one way or the other:

- Should a successful `go` announce the entered room's name/description (matching the spec
  transcript and the re-entry decision)? If so, name + description, or description only, and does
  it fire before or after the challenge prompt?
- Or is the spec transcript illustrative and the thinner "challenge-only on entry" output
  acceptable — in which case the spec transcript and the re-entry decision should be corrected to
  match the code?

Either resolution is fine; the point is that code and spec currently disagree and shouldn't.
Whichever way it goes, the fix lands in `MoveCommandHandler` and/or the spec, and this is the last
open item before Phase 1.5 can go to Nadia (tests) and Lothar (verification build).

## Response

Ruling: the spec transcript is correct, the code was short. A successful `go` should announce the
destination room on entry — same content `look` shows — unconditionally, whether the room is
solved or not. That's a thin addition to `MoveCommandHandler`, not a spec correction.

**Implemented** in `src/LunaFlow.Rom/NineRooms/Handlers/MoveCommandHandler.cs`: after retracting
the old `PlayerPosition` and inserting the new one, the handler now calls
`LunaFlowEngine.Host.Announce(NineRoomsContent.Rooms[toId].Description)` before returning. `Fire()`
still runs after, centrally, in `DispatchGameCommand` (per `nine-rooms-spec.md` item 2) — so the
ordering falls out for free:

- **Unsolved room:** handler announces `Description`, then `Fire()` runs the challenge-prompt rule,
  which announces `ChallengePrompt` right after — matches the transcript's `"The Long Hall. I have
  keys but no locks..."` two-part line exactly.
- **Solved room:** handler announces `Description`; the challenge rule's `Not<RoomSolved>()` guard
  keeps it from firing at all, so only the description prints — matches the "just redisplays the
  description" decision.

No handler-side branching on solved state was needed — the existing rule guard already does that
work; the handler just needed to stop staying silent on `go`.

Docs updated to match: `nine-rooms-gameplay.md`'s command table and re-entry decision bullet. While
in those files, also cleared the four other still-open items from `passdown.md`'s "Phase 1.5
implementation" entry (`Engine.Host` → `LunaFlowEngine.Host` naming, 9-not-8 challenge rules, the
`Reference` requirement, and `Fire()` centralization wording) plus the matching `Engine.Host` fix
and missing `PlayerPosition` seed call in `nine-rooms-spec.md` — all doc-only corrections, same
finding, no reason to leave them for a second pass.

## Resolution

Verified and closed by Kofi (opener).

- **Code:** confirmed `MoveCommandHandler` now announces `NineRoomsContent.Rooms[toId].Description`
  after the move and before the centralized `Fire()`. Ordering is correct by inspection: unsolved
  room prints description then challenge-prompt (rule fires on `Fire()`); solved room prints
  description only (`Not<RoomSolved>` guard suppresses the prompt). Matches the ruling.
- **Docs (Architect territory):** `nine-rooms-gameplay.md` command table + re-entry decision
  updated and cite this ticket; `nine-rooms-spec.md` / `engine-host-spec.md` naming correct. All
  verified.
- **Roadmap territory (PM):** Amara's `Engine.Host` → `LunaFlowEngine.Host` sweep was complete
  within design/spec docs. The roadmap phase files (`phase-1-1.md`, `phase-1-5.md`) still carried
  the old placeholder name — those are PM-owned, so Kofi fixed them on close rather than bouncing
  the ticket back. No stragglers remain (`grep` for bare `Engine.Host` outside the QA report's
  intentional spec-vs-actual note comes back clean).

**Not part of this ticket, tracked separately:** the behavior change means the Phase 1.5 done-bar
transcript now includes room descriptions on entry — Nadia's tests and Lothar's real build/verify
need to reflect that. A pre-existing `Documents/QA/Phase_1.5_QA_Report.md` predates this fix and
asserts "fully playable end-to-end" without an actual build; that claim needs Lothar to
re-substantiate against a real run before Phase 1.5 is marked done.
