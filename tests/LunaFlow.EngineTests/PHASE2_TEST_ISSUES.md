# Phase 2 Test Issues Found During Review

Based on lessons learned from Phase 1 test failures, the Phase 2 tests (GameEngineCoreTests and GameEngineCoreCommandDispatchTests) have several fragile synchronization patterns that could fail under load or slow systems.

## Issue 1: Hardcoded Thread.Sleep() Timeouts Instead of Proper Gates

**Affected tests:**
- `Submit_AcceptsCommandWhileEngineRunning()` — line 66, uses `Thread.Sleep(500)`
- `Submit_QueuedCommandsExecuteInOrder()` — line 106, uses `Thread.Sleep(500)`
- `MessagePosted_EventFiresOnStartup()` — line 277, uses `Thread.Sleep(100)` then checks bag
- `MessagePosted_EventFiresOnStop()` — line 301, uses `Thread.Sleep(100)`
- `MessagePosted_EventFiresOnRequestStop()` — line 317, uses `Thread.Sleep(100)` then waits while loop

**Problem:** These rely on fixed delays instead of waiting for actual completion. Under system load or slow machines, 500ms or 100ms may not be enough, causing flaky failures. The Phase 1 tests learned this lesson: use `ManualResetEventSlim` gates that signal when the actual event fires, not guesses about how long things take.

**Pattern from Phase 1 that works:**
```csharp
var completed = new ManualResetEventSlim(false);
EventHandler handler = (s, e) =>
{
    // Do work
    completed.Set();
};
SubscribeToMessagePosted(handler);
_engine.Start();
var signaled = completed.Wait(TimeSpan.FromSeconds(1));
Assert.IsTrue(signaled, "Event should fire");
```

**Recommendation:** Replace all hardcoded sleeps with proper completion gates.

---

## Issue 2: Message Tests Assume Timely Event Posting

**Affected tests:**
- `MessagePosted_EventFiresOnStartup()` — Assumes message posted within 100ms of Start()
- `MessagePosted_EventFiresOnStop()` — Assumes message posted within 100ms of Stop()
- `MessagePosted_EventFiresOnRequestStop()` — Uses while-loop polling instead of event gate

**Problem:** The tests wait a fixed time then check if messages were captured, rather than waiting for the specific message to arrive. If the engine posts the message slightly slower than expected, the test might see zero messages despite them eventually arriving.

**Better approach:** Subscribe *before* the action, then wait for a specific message pattern to appear:
```csharp
var startupMessageFired = new ManualResetEventSlim(false);
EventHandler<GameEngineMessagePostedEventArgs> handler = (s, e) =>
{
    if (e.Message.Contains("started"))
        startupMessageFired.Set();
};
SubscribeToMessagePosted(handler);
_engine.Start();
Assert.IsTrue(startupMessageFired.Wait(TimeSpan.FromSeconds(1)), "Startup message should fire");
```

---

## Issue 3: Command Execution Tests Don't Verify Completion

**Affected tests:**
- `Submit_AcceptsCommandWhileEngineRunning()` — Sets a flag but doesn't gate on it
- `Submit_QueuedCommandsExecuteInOrder()` — Uses `Thread.Sleep(500)` then checks a bag

**Problem:** These tests check the result *after* a sleep, but don't verify that all commands actually completed. If a command is slow, the sleep might be insufficient. Also, the Phase 1 tests learned that `ConcurrentBag` order isn't guaranteed to match execution order reliably over time.

**Better approach:** Use completion signals (already exist in some tests like `CommandCompletion_SignalFires()`) to verify each command actually completed:
```csharp
var completed = new ManualResetEventSlim(false);
var commandExecuted = false;
var testCommand = new TestCommandWithCompletion(
    () => { commandExecuted = true; },
    completed
);
_engine.Submit(testCommand);
Assert.IsTrue(completed.Wait(TimeSpan.FromSeconds(1)), "Command should complete");
Assert.IsTrue(commandExecuted, "Command code should have run");
```

---

## Issue 4: World_PersistsBetweenStarts Assertion is Valid But Fragile

**Affected test:**
- `World_PersistsBetweenStarts()` — line 385

**Status:** This test is **actually correct** — World is a singleton field (`public GameWorld World { get; } = new();`) and is never reset in `Start()`, so it does persist. The test accurately reflects this behavior.

**Why flagging it:** The test passes because of an implementation detail (singleton field, never reset). If the engine design changes to reset World on Start() (which would be reasonable for a "clean slate" restart), this test would silently fail in a way that looks like engine breakage rather than a test assumption violation. Consider adding a comment explaining that World persistence across restarts is intentional and engine-guaranteed.

---

## Issue 5: Stop() Polling in MessagePosted_EventFiresOnRequestStop

**Affected test:**
- `MessagePosted_EventFiresOnRequestStop()` — line 308-327

**Problem:** The test uses a while-loop with `Thread.Sleep(10)` to poll `_engine.Running`:
```csharp
var timeout = System.DateTime.UtcNow.AddMilliseconds(2000);
while (_engine.Running && System.DateTime.UtcNow < timeout)
{
    System.Threading.Thread.Sleep(10);
}
```

This is closer to a proper gate than the hardcoded sleeps, but still fragile — it relies on polling. Better to have `RequestStop()` signal completion or to use the already-captured `MessagePosted` event as the gate.

---

## Summary of Fixes Needed

| Test | Issue | Fix | Priority |
|------|-------|-----|----------|
| `Submit_AcceptsCommandWhileEngineRunning` | Sleep-based timeout | Use completion gate | High |
| `Submit_QueuedCommandsExecuteInOrder` | Sleep-based timeout | Use completion gates per command | High |
| `MessagePosted_EventFiresOnStartup` | Sleep + bag check | Wait for message event | High |
| `MessagePosted_EventFiresOnStop` | Sleep + bag check | Wait for message event | High |
| `MessagePosted_EventFiresOnRequestStop` | Polling + sleep | Wait for message event | Medium |
| `World_PersistsBetweenStarts` | Valid but implicit | Add clarifying comment | Low |

---

## Pattern to Use (from Phase 1 working tests)

For **command completion:**
```csharp
var completed = new ManualResetEventSlim(false);
var testCommand = new TestCommandWithCompletion(() => { /* action */ }, completed);
_engine.Submit(testCommand);
Assert.IsTrue(completed.Wait(TimeSpan.FromSeconds(1)), "Command should complete");
completed.Dispose();
```

For **event waiting:**
```csharp
var eventFired = new ManualResetEventSlim(false);
EventHandler handler = (s, e) =>
{
    if (/* condition met */)
        eventFired.Set();
};
SubscribeToEvent(handler);
// Trigger action
Assert.IsTrue(eventFired.Wait(TimeSpan.FromSeconds(1)), "Event should fire");
eventFired.Dispose();
```

For **multiple sequential events:**
```csharp
for (int i = 0; i < N; i++)
{
    var gate = new ManualResetEventSlim(false);
    EventHandler handler = (s, e) =>
    {
        if (/* condition for turn i */)
            gate.Set();
    };
    Subscribe(handler);
    TriggerEvent();
    gate.Wait(TimeSpan.FromSeconds(1));
    Unsubscribe(handler);
    gate.Dispose();
}
```

---

These patterns eliminate timing assumptions and make tests robust to system load variations.
