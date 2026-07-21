# Nine Rooms — Fact Vocabulary & Room Content

**DEPRECATED as design guidance (2026-07-20)** — see [`nine-rooms.md`](nine-rooms.md)'s Status
line. Kept for the implemented engine-fixture test coverage; not current ROM/Rulebook design
guidance. See `LunaFlow.Rom.D20System` / `LunaFlow.Rulebook.OneD20` and
[Phase 2.6](../Roadmap/phase-2-6.md) for the current pattern.

**Reference chain:**
- [`nine-rooms-spec.md`](nine-rooms-spec.md) (main spec)
  - *Uses these facts to describe* Engine wiring item 3 (seeding the `Room` records)
  - *Uses these facts to describe* `GameEngineOptions` design (what gets seeded)
- [`nine-rooms-gameplay.md`](nine-rooms-gameplay.md) (handlers & rules)
  - *Operates on these facts* in the commands and rules

## Fact vocabulary (ROM layer)

Plain records, no behavior. These belong to the "invented bespoke ROM" `nine-rooms.md` describes — not split into a separate project, per that doc's reasoning.

```csharp
public sealed record Room(
    int Id,            // 1-9, row-major
    int Row,           // 0-2
    int Col,           // 0-2
    string Name,
    string Description,
    string? ChallengePrompt,  // null for the starting room only
    string? Answer);          // null for the starting room only

public sealed record PlayerPosition(int RoomId);

public sealed record RoomSolved(int RoomId);

public sealed record WinMarker(IReadOnlyList<int> Line);   // the 3 room ids that completed a line
```

### Notes

- `Room` facts are static content, inserted once at session start (one `Insert` per room, 9 total) — they never change during play. `PlayerPosition` and `RoomSolved` are the mutable facts.
- Exactly one `PlayerPosition` fact exists at a time. Moving retracts the old one and inserts a new one (not `Update` — simpler to reason about, and NRules re-evaluates `Not<>` guards correctly either way).
- `RoomSolved` facts accumulate — one inserted per solved room, never retracted.
- No `SessionId`/player-id field on any of these. `nine-rooms.md` explicitly rules out multiple simultaneous sessions for this test rulebook, so a bare global fact is enough. If that changes, these records gain a correlation id — deliberately not built now.

---

## Room content (Rulebook layer)

### Grid layout

3×3 grid, row-major ids 1–9, player starts in room 5:

```
1 2 3
4 5 6
7 8 9
```

### Adjacency rules

- `north` = `id - 3` (reject if result < 1)
- `south` = `id + 3` (reject if > 9)
- `east` = `id + 1` (reject if it would cross into the next row, i.e. `(id - 1) % 3 == 2`)
- `west` = `id - 1` (reject if `(id - 1) % 3 == 0`)

### Win lines

Eight hardcoded win lines (no generalized geometry):

```
rows: [1,2,3] [4,5,6] [7,8,9]
cols: [1,4,7] [2,5,8] [3,6,9]
diag: [1,5,9] [3,5,7]
```

### Room table

Finalized room content (carried over from `nine-rooms.md`'s draft — decision: ship these as-is):

| Room | Name | Challenge | Answer |
| --- | --- | --- | --- |
| 1 | The Gatehouse | "What is 7 + 5?" | `12` |
| 2 | The Long Hall | "I have keys but no locks, space but no room. What am I?" | `keyboard` |
| 3 | The Observatory | "What is 9 × 3?" | `27` |
| 4 | The Cellar | "The more you take, the more you leave behind. What am I?" | `footsteps` |
| 5 | The Atrium | "What is 6 × 6?" | `36` |
| 6 | The Armory | "What is 15 − 6?" | `9` |
| 7 | The Well | "What has a neck but no head?" | `bottle` |
| 8 | The Garden | "What is 4 squared?" | `16` |
| 9 | The Tower | "What comes once in a minute, twice in a moment, but never in a thousand years?" | `the letter m` |

**Answer matching:** trim + case-insensitive ordinal compare, per `nine-rooms.md`.

### Design notes: room 5 (start room)

Room 5 (the start room) now has a real challenge like every other room, so all 8 win lines are reachable (an earlier draft left room 5 challenge-less, which made the 4 lines through the center — `[4,5,6]`, `[2,5,8]`, `[1,5,9]`, `[3,5,7]` — mathematically unwinnable). One consequence: the challenge-prompt rule for room 5 is already satisfied the instant `PlayerPosition(5)` is seeded at startup, *before* any command runs — this is a general engine-startup concern, not specific to Nine Rooms, so it's tracked separately as the "initial settle" guarantee in [`../design/engine-core.md`](../design/engine-core.md#turn-loop-and-engine-guarantees).

---

## Win-line validation safeguard

**Boundary note:** the underlying risk — a win/completion condition defined over a set of cells silently going dead if one of those cells has no way to become "solved" — is a real failure mode, but it is **not** a ROM-level concern. Grids, adjacency, and win-lines are specific to this one disposable test game; `LunaFlow.Rom` itself defines no shared cell/grid/solvability contract, and shouldn't invent one on the strength of a single (hypothetical) second game. The safeguard below lives entirely inside the NineRooms module. If a second grid-shaped game actually gets built later, *that's* the point to look at what the two concrete implementations share and factor out only that — not before.

**Safeguard (NineRooms-internal only):** rather than trusting that every room in every declared line has a challenge, derive the reachable line set from the room table itself, so a future edit to room content can't silently reintroduce a dead win-line:

```csharp
namespace LunaFlow.Rom.NineRooms
{
    internal static class WinLineCatalog
    {
        private static readonly int[][] AllLines =
        {
            new[] { 1, 2, 3 }, new[] { 4, 5, 6 }, new[] { 7, 8, 9 },
            new[] { 1, 4, 7 }, new[] { 2, 5, 8 }, new[] { 3, 6, 9 },
            new[] { 1, 5, 9 }, new[] { 3, 5, 7 },
        };

        public static IReadOnlyList<int[]> ReachableLines(IReadOnlyDictionary<int, Room> rooms) =>
            AllLines.Where(line => line.All(id => rooms[id].Answer is not null)).ToList();
    }
}
```

A startup-time check (also NineRooms-internal, not engine-level) asserts `WinLineCatalog.ReachableLines(...).Count == AllLines.Length` and fails loudly otherwise — so if room content ever changes and a line becomes unwinnable again, it's caught at load time instead of silently shipping a dead goal. The 8 hand-written `WinLine*Rule` classes are exactly `WinLineCatalog.AllLines` today; the catalog exists as the thing to check new content against, not to replace the hand-written rules.
