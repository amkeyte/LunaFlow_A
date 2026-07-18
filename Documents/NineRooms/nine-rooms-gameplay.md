# Nine Rooms — Gameplay: Commands, Rules & Decisions

**Reference chain:**
- [`nine-rooms-spec.md`](nine-rooms-spec.md) (main spec)
  - *Describes the engine surface* (Engine wiring item 1) that these handlers and rules use
  - *References these handlers and rules* to explain what the `IEngineHost` surface must support
- [`nine-rooms-facts.md`](nine-rooms-facts.md) (facts & layout)
  - *Defines the data structures* that these commands and rules operate on

## Commands & handlers

All registered the same way `quit`/`echo`/`help` are today, in `Program.cs`'s `ConfigureCommand` block.

| Command name | Argument shape | Handler | Behavior |
| --- | --- | --- | --- |
| `go` | one of `north`/`south`/`east`/`west` | `MoveCommandHandler` | Computes target room id from current `PlayerPosition`. Off-grid or invalid direction → print rejection message, no fact change. Otherwise retract old `PlayerPosition`, insert new one, announce the destination room's `Description` (same text `look` would show — unconditional, solved or not). |
| `answer` | free text (remaining tokens rejoined with spaces) | `AnswerCommandHandler` | Reads current room via `PlayerPosition`, looks up its `Room` fact, compares trimmed/case-insensitive against `Room.Answer`. Match → insert `RoomSolved(roomId)`. No match → print re-prompt, no fact change. |
| `look` | none | `LookCommandHandler` | Reads current `PlayerPosition`, prints that room's `Description`. Never touches facts, never re-triggers the challenge-prompt rule. |

None of these handlers call `Fire()` themselves — it's centralized in `DispatchGameCommand.Execute`, right after dispatch, per [`nine-rooms-spec.md`](nine-rooms-spec.md#engine-wiring) item 2. So a `go` into an unsolved room prints the destination's `Description` (from the handler, above) immediately followed by that room's `ChallengePrompt` (from the challenge-prompt rule firing right after) — matching the acceptance transcript below. A `go` into an already-solved room prints only the `Description`, since the challenge rule's `Not<RoomSolved>()` guard keeps it from firing.

`go`/`answer`/`look` are plain `ICommandHandler` implementations — no interface change needed there; they reach the rule session via `LunaFlowEngine.Host`/`IEngineHost` — not the way `QuitCommandHandler` reaches `GameEngineCore.Instance` directly, since that handler lives in `LunaFlow.Engine.Commands` (the same assembly as `GameEngineCore`), while NineRooms' handlers live in `LunaFlow.Rom.NineRooms`, a separate assembly that can't name `GameEngineCore` at all (see `engine-host-spec.md`).

---

## Rules (hand-written `ARule`s)

Loaded once at startup via `RuleRepository.Load(...).Compile().CreateSession()`, exactly the pattern `tests/LunaFlow.EngineTests/Rules` already proves out. Three rule shapes:

### Challenge-prompt rule

One rule per room, including the start room (9 rules — room 5 needs one too, same as every
other room, now that it has a real challenge), e.g.:

```csharp
public sealed class Room1ChallengeRule : ARule
{
    public override ILunaFlowReference Reference { get; } = new NineRoomsReference("Room1Challenge");

    public override void Define()
    {
        Room room = null!;
        When()
            .Match<PlayerPosition>(p => p.RoomId == 1)
            .Match<Room>(() => room, r => r.Id == 1)
            .Not<RoomSolved>(rs => rs.RoomId == 1)
            .Not<WinMarker>();
        Then()
            .Do(ctx => LunaFlowEngine.Host.Announce(room.ChallengePrompt!));
    }
}
```

(9 near-identical classes, not a generalized/parameterized rule — matches the "hand-written,
disposable content" ethos. `Reference` is required by `ARule`'s abstract member — every rule
class needs one; `NineRoomsReference` is a minimal flat implementation, not a shared ROM
abstraction.)

### Win-detection rule

One rule per line, all 8, each hardcoded to its 3 room ids, guarded so it only fires once:

```csharp
public sealed class WinLine123Rule : ARule
{
    public override void Define()
    {
        When()
            .Match<RoomSolved>(rs => rs.RoomId == 1)
            .Match<RoomSolved>(rs => rs.RoomId == 2)
            .Match<RoomSolved>(rs => rs.RoomId == 3)
            .Not<WinMarker>();
        Then()
            .Do(ctx => ctx.Insert(new WinMarker(new[] { 1, 2, 3 })));
    }
}
```

### Game-end announcement

One rule, fires once when `WinMarker` first appears, prints the win message (which line won).

---

## Decisions carried over from nine-rooms.md

`nine-rooms.md`'s four open questions, resolved here:

- **Re-entering an already-solved room:** just redisplays the description. No solved-room-specific
  handler code — `MoveCommandHandler` announces the destination room's `Description`
  unconditionally on every successful move, solved or not, and the challenge-prompt rule's own
  `Not<RoomSolved>()` guard already prevents re-prompting; `look` shows the same `Description` on
  demand. (Resolves [ClaudeWiki ticket 0001](../ClaudeWiki/Processes/Handoffs/0001-room-entry-announcement.md).)

- **`WinMarker` records which line:** yes, store the 3 room ids so the end-of-game message can name it (e.g. "You solved the top row!").

- **Riddle/math set:** ship the draft table as final (see [`nine-rooms-facts.md`](nine-rooms-facts.md)). No gameplay reason to change it for a disposable smoke-test rulebook.

- **`look`/`status` shape:** a bare `look` command, no arguments, prints current room's name + description only — never the challenge prompt (that only ever comes from room-entry via the rule, per above).

---

## Acceptance criteria

Playable transcript proving the roadmap's Phase 1.5 "done" bar:

```
[startup] What is 6 × 6?
> look
The Atrium.
> go north
The Long Hall. I have keys but no locks, space but no room. What am I?
> answer keyboard
Solved.
> go west
The Gatehouse. What is 7 + 5?
> answer 12
Solved.
> go east
> go east
The Observatory. What is 9 × 3?
> answer 27
Solved.
You solved the top row! (rooms 1, 2, 3)
```

(Path: 5 → 2 → 1 → 2 → 3, completing the top row `[1,2,3]` — verified against the adjacency rules in [`nine-rooms-facts.md`](nine-rooms-facts.md).) No `NotImplementedException` anywhere in the path, matching Phase 1's own done bar.
