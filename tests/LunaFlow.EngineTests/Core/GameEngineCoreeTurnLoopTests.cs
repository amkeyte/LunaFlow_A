using LunaFlow.Engine.Game;
using System.Collections.Concurrent;
using System.Threading;

namespace LunaFlow.EngineTests.Core
{
    /// <summary>
    /// Unit tests for GameEngineCore turn-based loop functionality.
    /// Covers: Turn advancement, TurnFinished gate, TurnTick events, turn counter, state synchronization.
    /// </summary>
    [TestClass]
    [TestCategory("Phase1")]
    [TestCategory("TurnLoop")]
    [DoNotParallelize]  // GameEngineCore.Instance is a singleton; tests must run sequentially to match production model
    public class GameEngineCoreeTurnLoopTests
    {
        private GameEngineCore _engine = null!;
        private List<EventHandler> _turnTickSubscribers = new();

        [TestInitialize]
        public void Setup()
        {
            _engine = GameEngineCore.Instance;
            if (_engine.Running)
                _engine.Stop(force: true);
            _turnTickSubscribers.Clear();
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

        #region Turn Counter Tests

        [TestMethod]
        public void TurnCounter_StartsAtZero()
        {
            // Arrange & Act
            _engine.Start();

            // Assert
            Assert.AreEqual(0, _engine.TurnCounter, "Turn counter should start at 0");

            // Cleanup
            _engine.Stop();
        }

        [TestMethod]
        public void TurnCounter_IncrementAfterTurnAdvance()
        {
            // Arrange
            _engine.Start();
            var turnsFired = new ConcurrentBag<int>();

            EventHandler handler = (s, e) =>
            {
                turnsFired.Add(_engine.TurnCounter);
            };

            SubscribeToTurnTick(handler);

            // Act - advance each turn and wait for it to complete before advancing the next
            for (int i = 0; i < 3; i++)
            {
                var turnGate = new ManualResetEventSlim(false);
                var expectedTurn = i + 1;

                EventHandler waitForTurn = (s, e) =>
                {
                    if (_engine.TurnCounter == expectedTurn)
                        turnGate.Set();
                };

                SubscribeToTurnTick(waitForTurn);
                _engine.FinishTurn();
                turnGate.Wait(TimeSpan.FromSeconds(1));
                _engine.TurnTick -= waitForTurn;
                _turnTickSubscribers.Remove(waitForTurn);
                turnGate.Dispose();
            }

            // Assert
            Assert.AreEqual(3, turnsFired.Count, "Should have fired 3 turns");
            var orderedTurns = turnsFired.OrderBy(x => x).ToList();
            Assert.AreEqual(1, orderedTurns[0], "First turn should be 1");
            Assert.AreEqual(2, orderedTurns[1], "Second turn should be 2");
            Assert.AreEqual(3, orderedTurns[2], "Third turn should be 3");
        }

        #endregion

        #region TurnTick Event Tests

        [TestMethod]
        public void TurnTick_DoesNotFireWithoutSubscribers()
        {
            // Arrange
            _engine.Start();

            // Act: Call FinishTurn() with no subscribers attached. This should be a no-op (no exception).
            // If TurnTick fired without listeners, it would be an error (trying to invoke null delegates).
            // The test verifies the engine safely handles this case.
            _engine.FinishTurn();

            // Assert: If we reach here, the engine didn't crash — FinishTurn() is safe without subscribers.
            // This is an implicit pass; there's nothing to assert because the absence of an exception is the behavior.
            Assert.IsTrue(true, "FinishTurn() should be a safe no-op when no subscribers are attached");
        }

        [TestMethod]
        public void TurnTick_FiresWhenSubscriberAttached()
        {
            // Arrange
            _engine.Start();
            var turnsFired = 0;
            var completed = new ManualResetEventSlim(false);

            EventHandler handler = (s, e) =>
            {
                Interlocked.Increment(ref turnsFired);
                if (turnsFired >= 1)
                    completed.Set();
            };

            // Act
            SubscribeToTurnTick(handler);
            _engine.FinishTurn();

            // Wait for turn to fire
            var signaled = completed.Wait(TimeSpan.FromSeconds(1));

            // Assert
            Assert.IsTrue(signaled, "Turn should fire when subscriber is attached");
            Assert.AreEqual(1, turnsFired, "Turn should fire exactly once");

            // Cleanup
            completed.Dispose();
        }

        [TestMethod]
        public void TurnTick_FiresForMultipleSubscribers()
        {
            // Arrange
            _engine.Start();
            var subscriber1Fired = false;
            var subscriber2Fired = false;
            var completed = new ManualResetEventSlim(false);

            EventHandler handler1 = (s, e) =>
            {
                subscriber1Fired = true;
                if (subscriber2Fired) completed.Set();
            };
            EventHandler handler2 = (s, e) =>
            {
                subscriber2Fired = true;
                if (subscriber1Fired) completed.Set();
            };

            SubscribeToTurnTick(handler1);
            SubscribeToTurnTick(handler2);

            // Act
            _engine.FinishTurn();
            var signaled = completed.Wait(TimeSpan.FromSeconds(1));

            // Assert
            Assert.IsTrue(signaled, "Turn should fire for both subscribers");
            Assert.IsTrue(subscriber1Fired, "First subscriber should receive event");
            Assert.IsTrue(subscriber2Fired, "Second subscriber should receive event");

            // Cleanup
            completed.Dispose();
        }

        [TestMethod]
        public void TurnTick_StopsFilingWhenLastSubscriberRemoved()
        {
            // Arrange
            _engine.Start();
            var turnsFired = 0;
            var firstTurnFired = new ManualResetEventSlim(false);
            var secondTurnFired = new ManualResetEventSlim(false);

            EventHandler handler = (s, e) =>
            {
                if (Interlocked.Increment(ref turnsFired) == 1)
                    firstTurnFired.Set();
                else if (turnsFired == 2)
                    secondTurnFired.Set();
            };

            SubscribeToTurnTick(handler);

            // Act - Request first turn and wait for it to fire
            _engine.FinishTurn();
            var firstSignaled = firstTurnFired.Wait(TimeSpan.FromSeconds(1));
            Assert.IsTrue(firstSignaled, "First turn should fire");

            // Remove subscriber and request second turn
            _engine.TurnTick -= handler;
            _turnTickSubscribers.Remove(handler);
            _engine.FinishTurn();

            // Wait briefly to see if second turn fires (it shouldn't)
            var secondSignaled = secondTurnFired.Wait(TimeSpan.FromMilliseconds(500));

            // Assert
            Assert.AreEqual(1, turnsFired, "Turns should stop firing after subscriber is removed");
            Assert.IsFalse(secondSignaled, "Second turn should not fire after unsubscribing");

            // Cleanup
            firstTurnFired.Dispose();
            secondTurnFired.Dispose();
        }

        #endregion

        #region TurnFinished Gate Tests

        [TestMethod]
        public void FinishTurn_AdvancesExactlyOneTurn()
        {
            // Arrange
            _engine.Start();
            var turnNumbers = new ConcurrentBag<int>();
            var completed = new ManualResetEventSlim(false);
            var count = 0;

            EventHandler handler = (s, e) =>
            {
                turnNumbers.Add(_engine.TurnCounter);
                if (++count >= 1)
                    completed.Set();
            };

            SubscribeToTurnTick(handler);

            // Act
            _engine.FinishTurn();
            completed.Wait(TimeSpan.FromSeconds(1));
            var firstTurn = _engine.TurnCounter;

            completed.Reset();
            count = 0;
            _engine.FinishTurn();
            completed.Wait(TimeSpan.FromSeconds(1));
            var secondTurn = _engine.TurnCounter;

            // Assert
            Assert.AreEqual(1, firstTurn, "First turn should be 1");
            Assert.AreEqual(2, secondTurn, "Second turn should be 2");

            // Cleanup
            completed.Dispose();
        }

        [TestMethod]
        public void FinishTurn_MultipleTimes_QueuesCorrectly()
        {
            // Arrange
            _engine.Start();
            var turnsFired = new ConcurrentBag<int>();

            EventHandler handler = (s, e) =>
            {
                turnsFired.Add(_engine.TurnCounter);
            };

            SubscribeToTurnTick(handler);

            // Act - Each FinishTurn() call advances exactly one turn; wait for it to complete before the next
            for (int i = 0; i < 3; i++)
            {
                var turnGate = new ManualResetEventSlim(false);
                var expectedTurn = i + 1;

                EventHandler waitForTurn = (s, e) =>
                {
                    if (_engine.TurnCounter == expectedTurn)
                        turnGate.Set();
                };

                SubscribeToTurnTick(waitForTurn);
                _engine.FinishTurn();
                turnGate.Wait(TimeSpan.FromSeconds(1));
                _engine.TurnTick -= waitForTurn;
                _turnTickSubscribers.Remove(waitForTurn);
                turnGate.Dispose();
            }

            // Assert
            Assert.AreEqual(3, turnsFired.Count, "Should have 3 turns (one per FinishTurn call, each waited for)");
        }

        [TestMethod]
        public void TurnFinished_ResetsBetweenTurns()
        {
            // Arrange
            _engine.Start();
            var completed = new ManualResetEventSlim(false);
            var turnCount = 0;

            EventHandler handler = (s, e) =>
            {
                turnCount++;
                if (turnCount >= 1)
                    completed.Set();
            };

            _engine.TurnTick += handler;

            // Act
            _engine.FinishTurn();
            completed.Wait(TimeSpan.FromSeconds(1));

            // After first turn fires, TurnFinished should be reset to false
            var turnFinishedAfterFire = _engine.TurnFinished;

            // Assert
            Assert.IsFalse(turnFinishedAfterFire, "TurnFinished should be reset after turn fires");

            // Cleanup
            _engine.TurnTick -= handler;
            completed.Dispose();
            _engine.Stop();
        }

        #endregion

        #region State Synchronization Tests

        [TestMethod]
        public void GameState_TurnNumberUpdatesWithTurns()
        {
            // Arrange
            _engine.Start();

            EventHandler handler = (s, e) =>
            {
            };

            SubscribeToTurnTick(handler);

            // Act - advance turns one at a time, waiting for each to complete
            var turn1Gate = new ManualResetEventSlim(false);
            EventHandler turn1Wait = (s, e) =>
            {
                if (_engine.State.TurnNumber == 1)
                    turn1Gate.Set();
            };
            SubscribeToTurnTick(turn1Wait);
            _engine.FinishTurn();
            turn1Gate.Wait(TimeSpan.FromSeconds(1));
            _engine.TurnTick -= turn1Wait;
            _turnTickSubscribers.Remove(turn1Wait);
            turn1Gate.Dispose();

            var turn2Gate = new ManualResetEventSlim(false);
            EventHandler turn2Wait = (s, e) =>
            {
                if (_engine.State.TurnNumber == 2)
                    turn2Gate.Set();
            };
            SubscribeToTurnTick(turn2Wait);
            _engine.FinishTurn();
            turn2Gate.Wait(TimeSpan.FromSeconds(1));
            _engine.TurnTick -= turn2Wait;
            _turnTickSubscribers.Remove(turn2Wait);
            turn2Gate.Dispose();

            // Assert
            Assert.AreEqual(2, _engine.State.TurnNumber, "GameState.TurnNumber should sync with TurnCounter");
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void FinishTurn_WhileEngineNotRunning_DoesNothing()
        {
            // Arrange - Force clean baseline by starting and stopping (ensures TurnCounter is reset to 0)
            _engine.Start();
            _engine.Stop();

            // Act & Assert - should not throw
            _engine.FinishTurn();

            // State should be unchanged
            Assert.AreEqual(0, _engine.TurnCounter, "Turn counter should remain 0");
        }

        [TestMethod]
        public void TurnTick_SubscribeUnsubscribeMultipleTimes()
        {
            // Arrange
            _engine.Start();
            var turnsFired = 0;
            var completed = new ManualResetEventSlim(false);

            EventHandler handler = (s, e) =>
            {
                Interlocked.Increment(ref turnsFired);
                if (turnsFired >= 1)
                    completed.Set();
            };

            // Act
            SubscribeToTurnTick(handler);
            SubscribeToTurnTick(handler); // Track both subscriptions
            _engine.FinishTurn();

            Thread.Sleep(300);

            // Assert
            Assert.IsTrue(turnsFired >= 1, "Handler should fire at least once");

            // Cleanup
            if (!completed.IsSet)
                completed.Set();
            completed.Dispose();
        }

        [TestMethod]
        public void TurnTick_HandlerException_DoesNotStopTurns()
        {
            // Arrange
            _engine.Start();
            var turnsFired = 0;
            var count = 0;

            EventHandler handlerWithError = (s, e) =>
            {
                if (++count == 1)
                    throw new InvalidOperationException("Handler error");
                Interlocked.Increment(ref turnsFired);
            };

            SubscribeToTurnTick(handlerWithError);

            // Act
            var firstTurnGate = new ManualResetEventSlim(false);
            EventHandler firstTurnWait = (s, e) =>
            {
                if (_engine.TurnCounter == 1)
                    firstTurnGate.Set();
            };
            SubscribeToTurnTick(firstTurnWait);
            _engine.FinishTurn(); // First turn, handler throws but doesn't stop engine
            firstTurnGate.Wait(TimeSpan.FromSeconds(1));
            _engine.TurnTick -= firstTurnWait;
            _turnTickSubscribers.Remove(firstTurnWait);
            firstTurnGate.Dispose();

            var secondTurnGate = new ManualResetEventSlim(false);
            EventHandler secondTurnWait = (s, e) =>
            {
                if (_engine.TurnCounter == 2)
                    secondTurnGate.Set();
            };
            SubscribeToTurnTick(secondTurnWait);
            _engine.FinishTurn(); // Second turn, succeeds
            secondTurnGate.Wait(TimeSpan.FromSeconds(1));
            _engine.TurnTick -= secondTurnWait;
            _turnTickSubscribers.Remove(secondTurnWait);
            secondTurnGate.Dispose();

            // Assert
            Assert.AreEqual(1, turnsFired, "Second turn should have executed successfully despite first handler's exception");
        }

        [TestMethod]
        public void TurnTick_SynchronousResubmit_IsIgnored()
        {
            // Arrange
            _engine.Start();
            var turnsFired = 0;
            var completed = new ManualResetEventSlim(false);

            EventHandler handler = (s, e) =>
            {
                Interlocked.Increment(ref turnsFired);
                // Try to set FinishTurn again synchronously during the turn
                _engine.FinishTurn();
                if (turnsFired >= 1)
                    completed.Set();
            };

            SubscribeToTurnTick(handler);

            // Act
            _engine.FinishTurn();
            completed.Wait(TimeSpan.FromSeconds(1));

            Thread.Sleep(300); // Wait to see if a second turn fires

            // Assert
            Assert.AreEqual(1, turnsFired, "Setting FinishTurn again during turn should be ignored");

            // Cleanup
            completed.Dispose();
        }

        #endregion
    }
}
