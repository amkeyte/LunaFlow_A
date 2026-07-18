# Test Patterns — authoring concurrency-safe engine tests

Durable patterns for writing tests against `GameEngineCore` and other async/event-driven engine
code. These are the *how-to-write* conventions; for where tests live, how to run them, and the
parallelism model see [`testing.md`](testing.md); for run order and diagnostics see
[`test-runbook.md`](test-runbook.md).

## Completion gates, never `Thread.Sleep()`

The engine is event-driven and runs work on a background thread. **Never** assert after a fixed
delay — a `Thread.Sleep(500)` that passes on your machine flakes on a loaded or slow one, and
produces failures that look like engine bugs. Wait for the actual signal instead, with a
`ManualResetEventSlim` gate and a generous timeout.

```csharp
var completed = new ManualResetEventSlim(false);
var testCommand = new TestCommandWithCompletion(() => { /* action */ }, completed);
_engine.Submit(testCommand);
Assert.IsTrue(completed.Wait(TimeSpan.FromSeconds(1)), "Command should complete");
completed.Dispose();
```

If you find yourself writing `Thread.Sleep` in a test, stop — you want a gate.

## Wait for a *specific* event, subscribed *before* the action

Don't "do the thing, sleep, then check a bag of messages." Subscribe first, then wait for the
specific message/condition you care about:

```csharp
var startupFired = new ManualResetEventSlim(false);
EventHandler<GameEngineMessagePostedEventArgs> handler = (s, e) =>
{
    if (e.Message.Contains("started")) startupFired.Set();
};
SubscribeToMessagePosted(handler);         // see subscriber-cleanup pattern below
_engine.Start();
Assert.IsTrue(startupFired.Wait(TimeSpan.FromSeconds(1)), "Startup message should fire");
```

For a sequence of N events, gate each one in turn rather than sleeping between them.

## Subscriber cleanup via `TestInitialize` / `TestCleanup`

Manually pairing `+=` / `-=` inside a test method leaks the subscription if an assertion fails
before the `-=`, and the leaked handler then corrupts later tests. Track subscriptions and tear
them down in `[TestCleanup]`, which runs even when a test fails.

```csharp
[TestClass]
public class GameEngineCoreXxxTests
{
    private GameEngineCore _engine = null!;
    private readonly List<EventHandler> _turnTickSubs = new();
    private readonly List<EventHandler<GameEngineMessagePostedEventArgs>> _messageSubs = new();

    [TestInitialize]
    public void Setup()
    {
        _engine = GameEngineCore.Instance;
        if (_engine.Running) _engine.Stop(force: true);
        _turnTickSubs.Clear();
        _messageSubs.Clear();
    }

    [TestCleanup]
    public void Cleanup()
    {
        foreach (var h in _turnTickSubs) _engine.TurnTick -= h;
        foreach (var h in _messageSubs) _engine.MessagePosted -= h;
        _turnTickSubs.Clear();
        _messageSubs.Clear();
        if (_engine.Running) _engine.Stop(force: true);
    }

    private void SubscribeToTurnTick(EventHandler h)
    { _engine.TurnTick += h; _turnTickSubs.Add(h); }

    private void SubscribeToMessagePosted(EventHandler<GameEngineMessagePostedEventArgs> h)
    { _engine.MessagePosted += h; _messageSubs.Add(h); }
}
```

Test methods then call `SubscribeToTurnTick(handler)` / `SubscribeToMessagePosted(handler)` and
never write a manual `-=`.

## Singleton tests are `[DoNotParallelize]`

Any test using `GameEngineCore.Instance` (the pattern above does) is marked `[DoNotParallelize]`,
per the model in [`testing.md`](testing.md). `Setup()` force-stops a possibly-running engine so
each test starts from a known state — the serial equivalent of isolation. (The parallelism default
itself is tracked in [handoff 0005](Handoffs/0005-test-parallelism-default.md).)

## One behavior per test, descriptive name

`[Feature]_[Condition]_[ExpectedBehavior]` (e.g. `TurnCounter_StartsAtZero`,
`CommandException_DoesNotStopEngine`). One assertion target per test so a failure names the broken
behavior directly.

## Guard against tests that pass for the wrong reason

If a test passes because of an unstated implementation detail (e.g. `World` persisting across
`Start()` because it's a never-reset singleton field), add a comment saying the behavior is
*intentional and engine-guaranteed* — so a future engine change that breaks the assumption reads as
"assumption violated," not "engine broken."

## Related pages

- [Testing — conventions & workflow](testing.md)
- [Test runbook — execution order & diagnostics](test-runbook.md)
- [Wiki style](wiki-style.md)
