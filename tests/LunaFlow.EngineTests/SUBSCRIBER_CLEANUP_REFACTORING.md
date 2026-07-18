# Subscriber Cleanup Refactoring Guide

## Problem

Tests manually pair `+=`/`-=` for event subscriptions inside test methods. If an assertion fails before the `-=`, the subscription leaks into subsequent tests, causing false failures or unexpected behavior.

## Solution

Move subscriber management to `[TestInitialize]`/`[TestCleanup]` using a tracking collection and helper methods.

---

## Pattern Template

### 1. Add fields and helper methods to test class

```csharp
[TestClass]
public class GameEngineCoreXxxTests
{
    private GameEngineCore _engine = null!;
    private List<EventHandler> _turnTickSubscribers = new();
    private List<EventHandler<GameEngineMessagePostedEventArgs>> _messagePostedSubscribers = new();

    [TestInitialize]
    public void Setup()
    {
        _engine = GameEngineCore.Instance;
        if (_engine.Running)
            _engine.Stop(force: true);
        _turnTickSubscribers.Clear();
        _messagePostedSubscribers.Clear();
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Unsubscribe all handlers (cleanup guaranteed even if test fails)
        foreach (var handler in _turnTickSubscribers)
        {
            _engine.TurnTick -= handler;
        }
        _turnTickSubscribers.Clear();

        foreach (var handler in _messagePostedSubscribers)
        {
            _engine.MessagePosted -= handler;
        }
        _messagePostedSubscribers.Clear();

        // Stop engine
        if (_engine.Running)
            _engine.Stop(force: true);
    }

    /// <summary>
    /// Helper to subscribe to TurnTick with automatic cleanup tracking.
    /// </summary>
    private void SubscribeToTurnTick(EventHandler handler)
    {
        _engine.TurnTick += handler;
        _turnTickSubscribers.Add(handler);
    }

    /// <summary>
    /// Helper to subscribe to MessagePosted with automatic cleanup tracking.
    /// </summary>
    private void SubscribeToMessagePosted(EventHandler<GameEngineMessagePostedEventArgs> handler)
    {
        _engine.MessagePosted += handler;
        _messagePostedSubscribers.Add(handler);
    }
}
```

### 2. Refactor test methods

**Before:**
```csharp
[TestMethod]
public void TurnTick_FiresWhenSubscriberAttached()
{
    _engine.Start();
    var turnsFired = 0;

    EventHandler handler = (s, e) => Interlocked.Increment(ref turnsFired);

    _engine.TurnTick += handler;  // ← Manual subscribe
    _engine.FinishTurn();
    Thread.Sleep(300);

    Assert.AreEqual(1, turnsFired);

    _engine.TurnTick -= handler;  // ← Manual unsubscribe (skipped if Assert fails!)
    _engine.Stop();
}
```

**After:**
```csharp
[TestMethod]
public void TurnTick_FiresWhenSubscriberAttached()
{
    _engine.Start();
    var turnsFired = 0;

    EventHandler handler = (s, e) => Interlocked.Increment(ref turnsFired);

    SubscribeToTurnTick(handler);  // ← Use helper (cleanup guaranteed)
    _engine.FinishTurn();
    Thread.Sleep(300);

    Assert.AreEqual(1, turnsFired);

    // No manual -= needed; Cleanup() handles it
}
```

### 3. Search and replace pattern

Use find-and-replace in your IDE:

**Find:** `_engine.TurnTick \+= ([a-zA-Z_][a-zA-Z0-9_]*);`
**Replace:** `SubscribeToTurnTick($1);`

**Find:** `_engine.MessagePosted \+= ([a-zA-Z_][a-zA-Z0-9_]*);`
**Replace:** `SubscribeToMessagePosted($1);`

Then remove all `_engine.TurnTick -=` and `_engine.MessagePosted -=` lines.

---

## Tests to Refactor

### GameEngineCoreTests
- **Subscriptions:** `MessagePosted`
- **Tests affected:** 3 (MessagePosted_EventFiresOnStartup, MessagePosted_EventFiresOnStop, MessagePosted_EventFiresOnRequestStop)
- **Helper needed:** `SubscribeToMessagePosted()`

### GameEngineCoreCommandDispatchTests
- **Subscriptions:** `MessagePosted`
- **Tests affected:** 1 (CommandException_IsCaughtAndPostedAsMessage)
- **Helper needed:** `SubscribeToMessagePosted()`

### GameEngineCoreeTurnLoopTests
- **Subscriptions:** `TurnTick`
- **Tests affected:** 9 (all tests that call `_engine.TurnTick +=`)
- **Helper needed:** `SubscribeToTurnTick()`
- **Status:** Already started refactoring; complete with find-and-replace

### GameEngineCoreThreadSafetyTests
- **Subscriptions:** `TurnTick`, `MessagePosted`
- **Tests affected:** 5 (multiple concurrent subscription tests)
- **Helpers needed:** `SubscribeToTurnTick()`, `SubscribeToMessagePosted()`

---

## Verification

After refactoring each class:

1. Build the project: `dotnet build tests/LunaFlow.EngineTests`
2. Run Phase 1: `dotnet test tests/LunaFlow.EngineTests --filter "Category=Phase1"`
3. Run Phase 2: `dotnet test tests/LunaFlow.EngineTests --filter "Category=Phase2"`
4. Verify no compiler errors or test failures

---

## Why This Matters

- **Guaranteed cleanup:** `[TestCleanup]` runs even if a test fails, preventing subscriber leaks
- **Test isolation:** Subscriptions don't bleed into subsequent tests
- **Cleaner test code:** No manual `+=`/`-=` pairs cluttering test logic
- **Matches best practices:** Standard pattern for resource management in MSTest

---

## Effort Estimate

- GameEngineCoreTests: ~10 mins (3 tests, simple pattern)
- GameEngineCoreCommandDispatchTests: ~5 mins (1 test)
- GameEngineCoreeTurnLoopTests: ~15 mins (already started, 8 more tests)
- GameEngineCoreThreadSafetyTests: ~20 mins (5 tests, more complex)

**Total:** ~50 minutes for all four classes.
