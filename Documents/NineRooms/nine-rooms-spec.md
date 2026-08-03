# Nine Rooms — Implementation Spec

**DEPRECATED as design guidance (2026-07-20)** — see [`nine-rooms.md`](nine-rooms.md)'s Status
line. Kept for the implemented engine-fixture test coverage; not current ROM/Rulebook design
guidance. See `LunaFlow.Rom.D20System` / `LunaFlow.Rulebook.OneD20` and
[Phase 2.6](../Roadmap/phase-2-6.md) for the current pattern.

Supersedes nothing — this is the concrete companion to
[`nine-rooms.md`](nine-rooms.md) (the concept doc) and
[`../Roadmap/roadmap.md`](../Roadmap/roadmap.md) (Phase 1.5). Where this spec makes a call the
concept doc left open, that's noted explicitly in [`nine-rooms-gameplay.md`](nine-rooms-gameplay.md).
Its own "Engine wiring" section, item 1 below, is itself superseded by
[`../Specs/engine-host-spec.md`](../Specs/engine-host-spec.md) — referenced inline to use `IEngineHost` rather
than re-deriving the surface.

## Purpose

Nine Rooms is the test game the rest of the engine gets built against — not a rulebook worth
keeping. This spec exists to turn `nine-rooms.md`'s concept into something buildable: exact fact
shapes, exact rules, exact commands, and — because the concept doc didn't cover it — exactly
where this content plugs into the running engine.

This spec **defines interfaces and boundaries** where the current code doesn't have a place for
this content to attach. It does **not** implement those internals. It's fine for the solution not
to build until that follow-up work lands — this doc is the target those changes are aimed at.

## Related documents

- **[`nine-rooms-facts.md`](nine-rooms-facts.md)** — Fact shapes, room layout, room table, grid geometry
- **[`nine-rooms-gameplay.md`](nine-rooms-gameplay.md)** — Commands, handlers, rules, gameplay decisions, acceptance criteria
- **[`engine-host-spec.md`](../Specs/engine-host-spec.md)** — `IEngineHost` interface and access model

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

## Engine wiring

This section defines what the engine needs to support Nine Rooms' handlers and rules. See [`nine-rooms-gameplay.md`](nine-rooms-gameplay.md) for how handlers and rules **use** this surface; see [`nine-rooms-facts.md`](nine-rooms-facts.md) for the fact shapes being operated on.

Nothing today runs a `RuleEngine` inside `GameEngineCore`, and nothing lets a rule's `Then().Do(...)` produce player-visible output. Additions needed — declared here as the contract, left unimplemented:

1. **Fact operations and player-visible output go through `IEngineHost`, not `GameEngineCore`
   directly.** `GameEngineCore` is `internal sealed` — widening individual members to `public`
   (the original plan here) doesn't help, because an internal *class* can't be named from
   `LunaFlow.Rom`, a separate assembly with no `InternalsVisibleTo`, regardless of member
   accessibility. [`engine-host-spec.md`](../Specs/engine-host-spec.md) resolves this with a narrow public
   interface reached through a static gateway:

   ```csharp
   public interface IEngineHost
   {
       RuleEngine Rules { get; }
       void Announce(string message);
   }

   public static class LunaFlowEngine
   {
       public static IEngineHost Host => GameEngineCore.Instance;
   }
   ```

   **In Nine Rooms, this surface is used by:**
   - `MoveCommandHandler`, `AnswerCommandHandler`, `LookCommandHandler` — they call `LunaFlowEngine.Host.Rules.Query<T>()` to read facts (see [`nine-rooms-gameplay.md`](nine-rooms-gameplay.md#commands--handlers))
   - Rule `Then()` blocks — they call `LunaFlowEngine.Host.Announce(...)` and `LunaFlowEngine.Host.Rules.Insert/Retract/Fire()` (see [`nine-rooms-gameplay.md`](nine-rooms-gameplay.md#rules-hand-written-arules))

   See [`engine-host-spec.md`](../Specs/engine-host-spec.md) for the full design, including why
   `FinishTurn`/`Stop`/`RequestStop` are deliberately excluded from this surface.

2. **`Fire()` runs after every dispatched command**, unconditionally, inside
   `DispatchGameCommand.Execute` right after `_dispatcher.Dispatch(_command)`. This is
   deliberately not wired through `FinishTurn()`/`TurnTick` — that gate is aimed at
   multi-client turn pacing (`engine-core.md`), which is explicitly out of scope
   (`nine-rooms.md`'s non-goals: no multiple simultaneous sessions). One command = one turn's
   worth of fact changes = fire immediately. `TurnCounter`/`TurnTick` stay unused by Nine Rooms.
   Firing unconditionally after `quit`/`echo`/`help` too is harmless (no new fact activations,
   no-op).

   `Start()` also needs to fire once after initial seeding, before any command exists — but
   that's a generic engine-core guarantee, not something Nine Rooms wires up itself. Documented as
   the "initial settle" guarantee in
   [`../design/engine-core.md`](../design/engine-core.md#turn-loop-and-engine-guarantees).

3. **Project references:** `LunaFlow.Rom` needs references to both `LunaFlow.Rules` (for
   `ARule`, used by the rule classes in [`nine-rooms-gameplay.md`](nine-rooms-gameplay.md#rules-hand-written-arules)) and `LunaFlow.Engine` (for `IEngineHost`/`LunaFlowEngine.Host`, used by those same rules and handlers). `MyLunaFlowServer` currently
   references only `LunaFlow.Engine` and needs a new reference to `LunaFlow.Rom` so
   `Program.cs` can point `GameEngineOptions.Rules` at its assembly (see section below).

   **Where the content lives:** `src/LunaFlow.Rom/NineRooms/`
   - Fact records from [`nine-rooms-facts.md`](nine-rooms-facts.md)
   - Command handlers and rule classes from [`nine-rooms-gameplay.md`](nine-rooms-gameplay.md)

   Namespace: flat `LunaFlow.Rom.NineRooms` for now rather than the eventual `LunaFlow.Rom.[NameOfRom]` split — a deliberate simplification, not a naming-convention change.

## `GameEngineOptions` design (mockup)

The knob `GameEngineCore.Start()` needs: which rule types to load, from potentially more than
one assembly (a Rulebook can depend on several ROMs — `glossary.md`'s ROM entry is explicit that
ROMs are meant to be pluggable, not singular), plus a way to seed initial facts.

In Nine Rooms' case, this means loading the rule classes from [`nine-rooms-gameplay.md`](nine-rooms-gameplay.md#rules-hand-written-arules) and seeding the 9 `Room` facts from [`nine-rooms-facts.md`](nine-rooms-facts.md) that aren't rules themselves.

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
        .Seed(() => NineRoomsContent.Rooms.Values)         // the 9 Room facts
        .Seed(() => new object[] { new PlayerPosition(NineRoomsContent.StartRoomId) }); // initial player position
})
```

Two separate `.Seed(...)` calls, not one — `RuleEngineOptions` already supports chaining multiple, and
`Room` content and the player's starting `PlayerPosition` are conceptually different seed sources. Without
the second call, the room-5 "initial settle" story (`../design/engine-core.md#turn-loop-and-engine-guarantees`)
has nothing to match against: the room-5 challenge-prompt rule needs a seeded `PlayerPosition(5)` to be present
before `Start()`'s first `Fire()` for it to fire on startup at all.

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

---

## See also

- **Gameplay decisions and acceptance criteria:** [`nine-rooms-gameplay.md`](nine-rooms-gameplay.md)
