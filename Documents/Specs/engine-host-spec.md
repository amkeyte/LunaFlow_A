# Engine Host Interface — Implementation Spec

**Status:** draft, for review. Concrete companion to [`../design/engine-core.md`](../design/engine-core.md), specifically the "ROM as the conduit to NRules" subsection of its Component model section. Triggered by a real, present finding (see [Purpose](#purpose)), not by the general ROM/Rulebook component model — this spec deliberately does **not** build `AEngineComponent`/`EngineTier`/dependency resolution. Those stay exactly as deferred in [`../design/non-goals.md`](../design/non-goals.md) until a second real ROM exists.

## Purpose

`GameEngineCore` is `internal sealed class` (`src/LunaFlow.Engine/Game/GameEngineCore.cs`). [`nine-rooms-spec.md`](nine-rooms-spec.md)'s "Engine wiring" section (items 1–3) calls for content code in `LunaFlow.Rom` — a separate assembly, no `InternalsVisibleTo` — to reach a `Rules` property, a `Query<T>()` passthrough, and a widened `Post`. It only proposed widening `Post` to `public`. That's insufficient: the class itself is unreachable from outside `LunaFlow.Engine` regardless of individual member accessibility, since C# doesn't let another assembly name an internal type at all — so none of the four wiring items in that section actually work without a separate fix.

The obvious fix — flip `GameEngineCore` itself to `public` — is worse than the problem it solves. Verified against the actual class today, nearly everything on it is already `public`; only the class declaration is `internal`:

```
public static GameEngineCore Instance
public GameEngineOptions Options
public GameState State
public GameWorld World
public bool Running
public int TurnCounter
public volatile bool TurnFinished
public event EventHandler<GameEngineMessagePostedEventArgs>? MessagePosted
public GameEngineOptions SetOptions(...)
public void Start()
public bool Stop(bool force = false)
public void RequestStop()
public void Submit(IGameCommand cmd)
public void FinishTurn()
public event EventHandler TurnTick
```

Widening the class exposes all of it, including `FinishTurn()`, `Stop()`, and `RequestStop()` — letting any content assembly end a turn or shut the engine down unilaterally. That directly contradicts "Turn-ending: ordering, not arbitration" in `engine-core.md`: ending a turn is Rulebook/command-layer authority, and a ROM has no more business calling `FinishTurn()` than `Dice` has a say in when it gets rolled. This spec defines a narrower, purpose-built surface instead, so the access boundary is drawn by what content code should be able to do, not by what happened to already be `public` on the concrete class.

## `IEngineHost`

```csharp
namespace LunaFlow.Engine
{
    /// <summary>
    /// The engine surface exposed to content assemblies (ROM today, Rulebook once that tier
    /// exists). Deliberately smaller than GameEngineCore's full surface — content code should
    /// never reach GameEngineCore itself, which stays internal.
    /// </summary>
    public interface IEngineHost
    {
        RuleEngine Rules { get; }
        void Announce(string message);
    }
}
```

Two members, matching exactly what `nine-rooms-spec.md`'s handlers and rules actually call: `Rules` for fact operations (`Query<T>`, `Insert`, `Update`, `Retract`, `Fire` — `Query<T>()` itself is `nine-rooms-spec.md`'s addition to `RuleEngine`, not redefined here), and `Announce` for a rule's `Then()` to produce player-visible output.

`Rules` is exposed as the concrete `RuleEngine` type, not a further `IRuleEngine` abstraction — consistent with `non-goals.md`'s decision not to abstract over multiple rule engines. One rule engine, one wrapper class, no interface needed between this spec and it.

`Announce(string)` is the content-facing name for the channel `nine-rooms-spec.md` already decided to reuse rather than duplicate: `GameEngineCore`'s existing internal `Post(string)` → `MessagePosted` event → `ConsoleEngine`'s `MessagePostedHandler` → `Console.WriteLine`. `GameEngineCore` implements `IEngineHost.Announce` as an explicit interface implementation that calls its existing `Post`, so `Post`'s current internal call sites (engine status messages like `"GameEngine initialized."`) don't change at all:

```csharp
internal sealed class GameEngineCore : IEngineHost
{
    // ...existing members unchanged...

    public RuleEngine Rules { get; private set; } = null!; // populated in Start(), per nine-rooms-spec.md item 1

    void IEngineHost.Announce(string message) => Post(message);
}
```

## Reaching it from content code

`GameEngineCore.Instance` can't be named from `LunaFlow.Rom` even with `Instance` marked `public`, because the class itself is internal — so a genuinely public gateway is needed. A small dedicated static class, in `LunaFlow.Engine`:

```csharp
namespace LunaFlow.Engine
{
    public static class Engine
    {
        public static IEngineHost Host => GameEngineCore.Instance;
    }
}
```

Content code calls `Engine.Host.Rules.Query<Room>()` / `Engine.Host.Announce("...")`, never touching `GameEngineCore` by name. (Naming bikeshed: `Engine`/`Host` is a placeholder pairing, not load-bearing — rename freely if something reads better once it's actually being written against.)

## Deliberately excluded, and why

Everything on `GameEngineCore` not listed above is left off `IEngineHost` on purpose, not by oversight:

- **`FinishTurn`, `Stop`, `RequestStop`** — lifecycle and turn-ending control. Never content code's call; see Purpose above.
- **`Submit(IGameCommand)`** — `IGameCommand` is itself `internal`, so exposing `Submit` would require widening that too. Nothing in `nine-rooms-spec.md` has content code submitting commands (handlers react to commands already dispatched, they don't dispatch new ones), so there's no present consumer to design this against. Revisit if one shows up.
- **`Options`, `State`, `World`, `Running`, `TurnCounter`** — no current consumer needs to read these from content code. `TurnCounter` in particular is moot for Nine Rooms, which bypasses `TurnTick` entirely per `nine-rooms-spec.md` item 4.
- **`TurnFinished` (raw field), `TurnTick` (event)** — subscribing to turn-advance notification is what the future `IEngineEventSubscriber.OnTurnAdvance` (`engine-core.md`) is for, once there's a real second ROM to justify building it. `IEngineHost` is not that mechanism and shouldn't grow into it by accretion.
- **`MessagePosted` (event, subscribe side)** — `Announce` covers *raising* a message; nothing today needs content code *subscribing* to the engine's own output stream. Same deferral as `TurnTick`.

## Relationship to the future component model

`IEngineHost` isn't a competing design against `engine-core.md`'s `AEngineComponent`/`EngineTier` — it's the grounded, minimal seed of the same idea, sized to what one real consumer needs today instead of speculative future ones. When the general component model does get built, the likely shape is `IEngineHost` (or an equivalent) becoming part of what an `AEngineComponent` receives as its event-handling context, not something this spec's `IEngineHost` needs to anticipate now. No changes here are wasted by that later work; nothing here blocks it either.

## Project references (correction to `nine-rooms-spec.md`)

`nine-rooms-spec.md` item 5 lists `LunaFlow.Rom → LunaFlow.Rules` (for `ARule`) and `MyLunaFlowServer → LunaFlow.Rom`, but never states `LunaFlow.Rom → LunaFlow.Engine` — even though that same spec's handlers and rules call `GameEngineCore`/`RuleEngine` members directly, which requires it. That reference was implicitly needed already; this spec makes it explicit: **`LunaFlow.Rom` needs a `ProjectReference` to `LunaFlow.Engine`, in addition to `LunaFlow.Rules`.**

## Impact on `nine-rooms-spec.md`

Items 1–3 of that spec's "Engine wiring" section (the `Rules` property, `Query<T>()` passthrough, and public `Post`/`Announce`) are superseded by this spec — `IEngineHost` is now the authoritative shape for that surface. Items 4 (unconditional `Fire()` after every dispatched command) and 5 (project references, now corrected above) remain Nine-Rooms-specific and unaffected. The Nine Rooms session should update its own spec to reference `IEngineHost`/`Engine.Host` rather than re-deriving the interface shape.

## Acceptance criteria

- `LunaFlow.Rom` compiles against `Engine.Host.Rules.Query<T>()` and `Engine.Host.Announce(string)` without referencing `GameEngineCore` by name (it can't — the type isn't visible outside `LunaFlow.Engine`).
- `GameEngineCore` remains `internal sealed`; nothing outside `LunaFlow.Engine` can construct it, call `FinishTurn`/`Stop`/`RequestStop`/`Submit`, or read `TurnFinished`/`TurnCounter`/`Options`/`State`/`World` directly.
- `ConsoleEngine` and other in-assembly translators are unaffected — they still use `GameEngineCore.Instance` directly, since they're in `LunaFlow.Engine` and were never blocked by this in the first place.
