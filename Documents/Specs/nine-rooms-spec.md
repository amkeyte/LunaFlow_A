# Nine Rooms — Implementation Spec

**Status:** draft, for review. Supersedes nothing — this is the concrete companion to
[`../design/nine-rooms.md`](../design/nine-rooms.md) (the concept doc) and
[`../design/roadmap.md`](../design/roadmap.md) (Phase 1.5). Where this spec makes a call the
concept doc left open, that's noted explicitly under [Decisions](#decisions-carried-over-from-nine-roomsmd).

## Purpose

Nine Rooms is the test game the rest of the engine gets built against — not a rulebook worth
keeping. This spec exists to turn `nine-rooms.md`'s concept into something buildable: exact fact
shapes, exact rules, exact commands, and — because the concept doc didn't cover it — exactly
where this content plugs into the running engine.

This spec **defines interfaces and boundaries** where the current code doesn't have a place for
this content to attach (see [Engine wiring](#engine-wiring-new-boundary)). It does **not**
implement those internals. It's fine for the solution not to build until that follow-up work
lands — this doc is the target those changes are aimed at.

## Baseline: what Phase 1 already provides

Verified against code as of this writing, not assumed from the design docs:

- `EngineHost`/`GameEngine` are gone. `ConsoleEngine` + `GameEngineCore` is the sole host,
  already wired up in `MyLunaFlowServer/Program.cs`.
- `DefaultCommandParser.Parse` → `DefaultCommand` already splits `"go north"` into
  `Name="go"`, `Arguments=["north"]` (first token lowercased, rest split on spaces).
- `DefaultCommandDispatcher` and `ICommandHandler` work today (`quit`, `echo`, `help` are
  registered and functional in `Program.cs`).
- `GameEngineCore.RunLoop` drains submitted commands and calls `ICommandHandler.Handle` per
  command; it does **not** touch `RuleEngine` anywhere — there is currently no rule session
  running inside the loop at all.
- `GameState` (just `TurnNumber`), `GameWorld` (just a session list), and `Session` (id,
  translator name, connect time) are all still empty placeholders — no player-position or
  room-content shape exists yet.

## Fact vocabulary (ROM layer)

Plain records, no behavior. These belong to the "invented bespoke ROM" `nine-rooms.md` describes
— not split into a separate project, per that doc's reasoning.

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

Notes:

- `Room` facts are static content, inserted once at session start (one `Insert` per room, 9
  total) — they never change during play. `PlayerPosition` and `RoomSolved` are the mutable
  facts.
- Exactly one `PlayerPosition` fact exists at a time. Moving retracts the old one and inserts a
  new one (not `Update` — simpler to reason about, and NRules re-evaluates `Not<>` guards
  correctly either way).
- `RoomSolved` facts accumulate — one inserted per solved room, never retracted.
- No `SessionId`/player-id field on any of these. `nine-rooms.md` explicitly rules out multiple
  simultaneous sessions for this test rulebook, so a bare global fact is enough. If that
  changes, these records gain a correlation id — deliberately not built now.

## Room content (Rulebook layer)

3×3 grid, row-major ids 1–9, player starts in room 5:

```
1 2 3
4 5 6
7 8 9
```

Adjacency: `north` = `id - 3` (reject if result < 1), `south` = `id + 3` (reject if > 9),
`east` = `id + 1` (reject if it would cross into the next row, i.e. `(id - 1) % 3 == 2`),
`west` = `id - 1` (reject if `(id - 1) % 3 == 0`).

Win lines (8, hardcoded, no generalized geometry):

```
rows: [1,2,3] [4,5,6] [7,8,9]
cols: [1,4,7] [2,5,8] [3,6,9]
diag: [1,5,9] [3,5,7]
```

Finalized room table (carried over from `nine-rooms.md`'s draft — decision: ship these as-is,
see [Decisions](#decisions-carried-over-from-nine-roomsmd)):

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

Answer matching: trim + case-insensitive ordinal compare, per `nine-rooms.md`.

**Revision:** room 5 (the start room) now has a real challenge like every other room, so all 8
win lines are reachable (an earlier draft of this spec left room 5 challenge-less, which made
the 4 lines through the center — `[4,5,6]`, `[2,5,8]`, `[1,5,9]`, `[3,5,7]` — mathematically
unwinnable; that's fixed now). One consequence: the challenge-prompt rule for room 5 is already
satisfied the instant `PlayerPosition(5)` is seeded at startup, *before* any command runs — this
is a general engine-startup concern, not specific to Nine Rooms, so it's tracked separately in
[`../passdown.md`](../passdown.md) rather than as content-specific wiring here.

**Boundary note:** the underlying risk here — a win/completion condition defined over a set of
cells silently going dead if one of those cells has no way to become "solved" — is a real
failure mode, but it is **not** a ROM-level concern. Grids, adjacency, and win-lines are specific
to this one disposable test game; `LunaFlow.Rom` itself defines no shared cell/grid/solvability
contract, and shouldn't invent one on the strength of a single (hypothetical) second game. The
safeguard below lives entirely inside the NineRooms module. If a second grid-shaped game
actually gets built later, *that's* the point to look at what the two concrete implementations
share and factor out only that — not before.

**Safeguard (NineRooms-internal only):** rather than trusting that every room in every declared
line has a challenge, derive the reachable line set from the room table itself, so a future edit
to room content can't silently reintroduce a dead win-line the way room 5 almost did:

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

A startup-time check (also NineRooms-internal, not engine-level) asserts
`WinLineCatalog.ReachableLines(...).Count == AllLines.Length` and fails loudly otherwise — so if
room content ever changes and a line becomes unwinnable again, it's caught at load time instead
of silently shipping a dead goal. The 8 hand-written `WinLine*Rule` classes below are exactly
`WinLineCatalog.AllLines` today; the catalog exists as the thing to check new content against,
not to replace the hand-written rules.

## Commands & handlers

All registered the same way `quit`/`echo`/`help` are today, in `Program.cs`'s
`ConfigureCommand` block.

| Command name | Argument shape | Handler | Behavior |
| --- | --- | --- | --- |
| `go` | one of `north`/`south`/`east`/`west` | `MoveCommandHandler` | Computes target room id from current `PlayerPosition`. Off-grid or invalid direction → print rejection message, no fact change. Otherwise retract old `PlayerPosition`, insert new one, `Fire()`. |
| `answer` | free text (remaining tokens rejoined with spaces) | `AnswerCommandHandler` | Reads current room via `PlayerPosition`, looks up its `Room` fact, compares trimmed/case-insensitive against `Room.Answer`. Match → insert `RoomSolved(roomId)`, `Fire()`. No match → print re-prompt, no fact change. |
| `look` | none | `LookCommandHandler` | Reads current `PlayerPosition`, prints that room's `Name` + `Description`. Never touches facts, never re-triggers the challenge-prompt rule (see [Decisions](#decisions-carried-over-from-nine-roomsmd)). |

`go`/`answer`/`look` are plain `ICommandHandler` implementations — no interface change needed
there; they reach the rule session the same way `QuitCommandHandler` already reaches
`GameEngineCore.Instance` directly (see next section for what they call on it).

## Rules (hand-written `ARule`s)

Loaded once at startup via `RuleRepository.Load(...).Compile().CreateSession()`, exactly the
pattern `tests/LunaFlow.EngineTests/Rules` already proves out. Three rule shapes:

**Challenge-prompt rule** — one rule per non-start room (8 rules), e.g.:

```csharp
public sealed class Room1ChallengeRule : ARule
{
    public override void Define()
    {
        Room room = null!;
        When()
            .Match<PlayerPosition>(p => p.RoomId == 1)
            .Match<Room>(() => room, r => r.Id == 1)
            .Not<RoomSolved>(rs => rs.RoomId == 1)
            .Not<WinMarker>();
        Then()
            .Do(ctx => Announce(room.ChallengePrompt!));
    }
}
```

(8 near-identical classes, not a generalized/parameterized rule — matches the "hand-written,
disposable content" ethos `nine-rooms.md` sets for this rulebook.)

**Win-detection rule** — one rule per line, all 8, each hardcoded to its 3 room ids, guarded so
it only fires once:

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

**Game-end announcement** — one rule, fires once when `WinMarker` first appears, prints the
win message (which line, per the decision below).

`Announce(...)` above is new surface, not existing code — see next section.

## Engine wiring (new boundary)

This is the actual gap: nothing today runs a `RuleEngine` inside `GameEngineCore`, and nothing
lets a rule's `Then().Do(...)` produce player-visible output. Additions needed — declared here
as the contract, left unimplemented per your steer that we're defining boundaries, not building
the internals yet:

1. **`GameEngineCore` gains a `RuleEngine Rules` property**, populated at `Start()` from
   `GameEngineOptions.Rules` — see [`GameEngineOptions` design](#gameengineoptions-design-mockup)
   below for the shape.

2. **`RuleEngine` gains a `Query<T>()` passthrough** (`_session.Query<T>()`), so handlers like
   `LookCommandHandler`/`AnswerCommandHandler` can read the current `PlayerPosition`/`Room`
   facts instead of only blind-inserting. Today `RuleEngine` only exposes
   `Insert`/`Update`/`Retract`/`Fire` — none of them let a caller read current state back out.

3. **An `Announce(string)` path from inside a rule's `Then()` back out to the player.**
   `GameEngineCore` already has exactly this channel for engine status text
   (`private void Post(string)` → `MessagePosted` event → `ConsoleEngine`'s
   `MessagePostedHandler` → `Console.WriteLine`). Decision: reuse it rather than build a second
   one. Now that content lives in `LunaFlow.Rom` — a separate assembly from `LunaFlow.Engine` —
   `internal` isn't reachable (no `InternalsVisibleTo` today), so `Post` needs to widen all the
   way to `public`: `GameEngineCore.Instance.Post(...)` called from a rule's
   `Then().Do(ctx => ...)` lambda.

4. **`Fire()` runs after every dispatched command**, unconditionally, inside
   `DispatchGameCommand.Execute` right after `_dispatcher.Dispatch(_command)`. This is
   deliberately not wired through `FinishTurn()`/`TurnTick` — that gate is aimed at
   multi-client turn pacing (`engine-core.md`), which is explicitly out of scope
   (`nine-rooms.md`'s non-goals: no multiple simultaneous sessions). One command = one turn's
   worth of fact changes = fire immediately. `TurnCounter`/`TurnTick` stay unused by Nine Rooms.
   Firing unconditionally after `quit`/`echo`/`help` too is harmless (no new fact activations,
   no-op).

   `Start()` also needs to fire once after initial seeding, before any command exists — but
   that's a generic engine-core guarantee, not something Nine Rooms wires up itself. Tracked in
   [`../passdown.md`](../passdown.md) as its own engine-level finding ("initial settle").

5. **Project references:** `LunaFlow.Rom` needs a reference to `LunaFlow.Rules` (for `ARule`).
   `MyLunaFlowServer` currently references only `LunaFlow.Engine` (`CLAUDE.md`) and needs a new
   reference to `LunaFlow.Rom` so `Program.cs` can point `GameEngineOptions.Rules` at its
   assembly.

**Where the content lives:** `src/LunaFlow.Rom/NineRooms/` (room table, fact records, rule
classes, handler classes), per your new `LunaFlow.Rom` project. Namespace: flat
`LunaFlow.Rom.NineRooms` for now rather than the eventual `LunaFlow.Rom.[NameOfRom]` split — you
called this out as a deliberate simplification, not a naming-convention change, so nothing here
blocks giving Nine Rooms its own sub-namespace/folder later if a second ROM shows up needing the
convention for real.

## `GameEngineOptions` design (mockup)

The knob `GameEngineCore.Start()` needs: which rule types to load, from potentially more than
one assembly (a Rulebook can depend on several ROMs — `glossary.md`'s ROM entry is explicit that
ROMs are meant to be pluggable, not singular), plus a way to seed initial facts (the 9 `Room`
records) that isn't itself a rule.

Recommendation: a small options object mirroring the loading surface `RuleEngine` **already**
exposes (`LoadRulesFromAssembly`/`LoadRulesFromTypes`) rather than inventing a new abstraction,
plus one new knob for fact seeding:

```csharp
public sealed class GameEngineOptions
{
    public string GameName { get; set; } = "Unnamed";
    public bool EnableRulesEngine { get; set; } = true;
    public RuleEngineOptions Rules { get; } = new();
}

public sealed class RuleEngineOptions
{
    private readonly List<Assembly> _assemblies = new();
    private readonly List<Type> _types = new();
    private readonly List<Func<IEnumerable<object>>> _seedProviders = new();

    public RuleEngineOptions FromAssembly(Assembly assembly)
    {
        _assemblies.Add(assembly);
        return this;
    }

    public RuleEngineOptions FromTypes(params Type[] types)
    {
        _types.AddRange(types);
        return this;
    }

    public RuleEngineOptions Seed(Func<IEnumerable<object>> factsProvider)
    {
        _seedProviders.Add(factsProvider);
        return this;
    }

    internal void ApplyTo(RuleEngine engine)
    {
        foreach (var assembly in _assemblies)
            engine.LoadRulesFromAssembly(assembly);
        if (_types.Count > 0)
            engine.LoadRulesFromTypes(_types);
        foreach (var provider in _seedProviders)
            foreach (var fact in provider())
                engine.Insert(fact);
    }
}
```

`Program.cs` usage for Nine Rooms:

```csharp
.ConfigureGameEngine(opts =>
{
    opts.GameName = "Nine Rooms";
    opts.Rules
        .FromAssembly(typeof(NineRoomsRules).Assembly)   // one call per ROM assembly if more show up later
        .Seed(() => NineRoomsContent.Rooms);              // the 9 Room facts
})
```

Why this shape over a bare `IEnumerable<Type>` field or a heavier builder:

- **Calling `FromAssembly` more than once** is exactly what a Rulebook depending on multiple
  ROM modules needs, without inventing multi-ROM composition machinery up front — it's just a
  list.
- **Seeding is explicitly separate from rule loading.** Rules are types (`ARule` subclasses);
  initial facts are data. Conflating them (e.g. a `Room` fact accidentally implementing some
  marker interface just to get loaded) would be the wrong kind of cleverness for disposable
  test content.
- **No new abstraction over `RuleEngine`'s existing methods** — `RuleEngineOptions.ApplyTo` is a
  thin adapter, not a reimplementation. When Phase 2's `GeneratorServices` starts emitting
  compiled `ARule` classes, they're still just types — `FromTypes`/`FromAssembly` don't care
  whether a rule was hand-written or generated, so this doesn't need to change shape then.
- Deliberately **not** a fluent builder with per-ROM metadata, priority ordering, or conditional
  loading — nothing in Nine Rooms or the roadmap's next few phases needs that yet, and it's
  easy to add later without breaking this shape (new methods on `RuleEngineOptions`, not a
  rewrite).

## Decisions carried over from nine-rooms.md

`nine-rooms.md`'s four open questions, resolved here:

- **Re-entering an already-solved room:** just redisplays the description. No handler-side
  special case needed — the challenge-prompt rule's own `Not<RoomSolved>()` guard already
  prevents re-prompting, and `look`/`go` never emit the challenge prompt directly.
- **`WinMarker` records which line:** yes, store the 3 room ids (see record shape above) so the
  end-of-game message can name it (e.g. "You solved the top row!").
- **Riddle/math set:** ship the draft table as final. No gameplay reason to change it for a
  disposable smoke-test rulebook.
- **`look`/`status` shape:** a bare `look` command, no arguments, prints current room's name +
  description only — never the challenge prompt (that only ever comes from room-entry via the
  rule, per above).

## Acceptance criteria

Playable transcript proving the roadmap's Phase 1.5 "done" bar, e.g.:

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

(Path: 5 → 2 → 1 → 2 → 3, completing the top row `[1,2,3]` — verified against the adjacency
rules above.) No `NotImplementedException` anywhere in the path, matching Phase 1's own done
bar.
