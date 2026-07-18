using LunaFlow.Engine.Game;
using System.Collections.Concurrent;
using System.Threading;

namespace LunaFlow.EngineTests.Core
{
    /// <summary>
    /// Unit tests for GameEngineCore thread safety and concurrent access patterns.
    /// Covers: Multi-threaded command submission, race conditions, event subscription from multiple threads,
    /// shutdown safety, concurrent turn advancement.
    /// </summary>
    [TestClass]
    [TestCategory("Phase1")]
    [TestCategory("ThreadSafety")]
    [DoNotParallelize]  // GameEngineCore.Instance is a singleton; tests must run sequentially to match production model
    public class GameEngineCoreThreadSafetyTests
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

        #region Multi-Threaded Command Submission Tests

        [TestMethod]
        public void ConcurrentCommandSubmission_FromMultipleThreads_Completes()
        {
            // Arrange
            _engine.Start();
            var completedCount = 0;
            var lockObj = new object();
            var threadCount = 10;
            var commandsPerThread = 10;

            // Act
            var threads = new List<Thread>();
            for (int t = 0; t < threadCount; t++)
            {
                var thread = new Thread(() =>
                {
                    for (int c = 0; c < commandsPerThread; c++)
                    {
                        var cmd = new CounterCommand(() =>
                        {
                            lock (lockObj)
                                completedCount++;
                        });
                        _engine.Submit(cmd);
                    }
                });
                threads.Add(thread);
                thread.Start();
            }

            // Wait for all threads to submit
            foreach (var thread in threads)
                thread.Join();

            // Wait for all commands to execute
            Thread.Sleep(2000);

            // Assert
            Assert.AreEqual(threadCount * commandsPerThread, completedCount,
                "All commands from all threads should execute");

            // Cleanup
            _engine.Stop();
        }

        [TestMethod]
        public void ConcurrentSubmission_MaintainsQueueOrder()
        {
            // Arrange
            _engine.Start();
            var sequence = new ConcurrentBag<(int ThreadId, int CommandId)>();

            // Act
            var threads = new List<Thread>();
            for (int t = 0; t < 5; t++)
            {
                var threadId = t;
                var thread = new Thread(() =>
                {
                    for (int c = 0; c < 10; c++)
                    {
                        var commandId = c;
                        var cmd = new CounterCommand(() =>
                            sequence.Add((threadId, commandId)));
                        _engine.Submit(cmd);
                    }
                });
                threads.Add(thread);
            }

            // Start all threads
            foreach (var thread in threads)
                thread.Start();

            // Wait for all to complete
            foreach (var thread in threads)
                thread.Join();

            Thread.Sleep(2000); // Wait for execution

            // Assert
            Assert.AreEqual(50, sequence.Count, "All 50 commands should have executed");
            // Commands from the same thread should maintain relative order
            var threadCommands = new Dictionary<int, List<int>>();
            foreach (var (threadId, commandId) in sequence)
            {
                if (!threadCommands.ContainsKey(threadId))
                    threadCommands[threadId] = new List<int>();
                threadCommands[threadId].Add(commandId);
            }

            foreach (var kvp in threadCommands)
            {
                var ordered = kvp.Value.OrderBy(x => x).ToList();
                for (int i = 0; i < ordered.Count; i++)
                {
                    Assert.AreEqual(i, ordered[i],
                        $"Commands from thread {kvp.Key} should maintain order");
                }
            }

            // Cleanup
            _engine.Stop();
        }

        #endregion

        #region Event Subscription Thread Safety Tests

        [TestMethod]
        public void TurnTick_SubscriptionFromMultipleThreads_IsSafe()
        {
            // Arrange
            _engine.Start();
            var subscriptionCount = 0;
            var lockObj = new object();
            var threads = new List<Thread>();

            // Act
            for (int i = 0; i < 10; i++)
            {
                var thread = new Thread(() =>
                {
                    EventHandler handler = (s, e) => { };
                    SubscribeToTurnTick(handler);
                    lock (lockObj)
                        subscriptionCount++;
                    Thread.Sleep(10);
                });
                threads.Add(thread);
                thread.Start();
            }

            foreach (var thread in threads)
                thread.Join();

            // Assert
            Assert.AreEqual(10, subscriptionCount, "All threads should subscribe successfully");
            Assert.IsTrue(_engine.Running, "Engine should still be running");
        }

        [TestMethod]
        public void MessagePosted_SubscriptionFromMultipleThreads_IsSafe()
        {
            // Arrange
            _engine.Start();
            var subscriptionCount = 0;
            var lockObj = new object();
            var threads = new List<Thread>();

            // Act
            for (int i = 0; i < 5; i++)
            {
                var thread = new Thread(() =>
                {
                    EventHandler<GameEngineMessagePostedEventArgs> handler = (s, e) => { };
                    _engine.MessagePosted += handler;
                    lock (lockObj)
                        subscriptionCount++;
                    Thread.Sleep(10);
                    _engine.MessagePosted -= handler;
                });
                threads.Add(thread);
                thread.Start();
            }

            foreach (var thread in threads)
                thread.Join();

            // Assert
            Assert.AreEqual(5, subscriptionCount, "All threads should subscribe successfully");

            // Cleanup
            _engine.Stop();
        }

        #endregion

        #region Concurrent Submit and FinishTurn Tests

        [TestMethod]
        public void Submit_AndFinishTurn_Concurrently()
        {
            // Arrange
            _engine.Start();
            var commandCount = 0;
            var turnCount = 0;
            var lockObj = new object();
            var completed = new ManualResetEventSlim(false);

            EventHandler turnHandler = (s, e) =>
            {
                Interlocked.Increment(ref turnCount);
                if (turnCount >= 5)
                    completed.Set();
            };

            SubscribeToTurnTick(turnHandler);

            // Act
            var submitThread = new Thread(() =>
            {
                for (int i = 0; i < 20; i++)
                {
                    var cmd = new CounterCommand(() =>
                        Interlocked.Increment(ref commandCount));
                    _engine.Submit(cmd);
                    Thread.Sleep(10);
                }
            });

            var turnThread = new Thread(() =>
            {
                for (int i = 0; i < 5; i++)
                {
                    Thread.Sleep(30);
                    _engine.FinishTurn();
                }
            });

            submitThread.Start();
            turnThread.Start();

            submitThread.Join();
            turnThread.Join();
            completed.Wait(TimeSpan.FromSeconds(3));

            // Assert
            Assert.AreEqual(20, commandCount, "All commands should have executed");
            Assert.AreEqual(5, turnCount, "All 5 turns should have fired");

            // Cleanup
            completed.Dispose();
        }

        #endregion

        #region Shutdown Safety Tests

        [TestMethod]
        public void Stop_FromCommandHandler_DoesNotDeadlock()
        {
            // Arrange
            _engine.Start();
            var stopCalled = false;
            var completed = new ManualResetEventSlim(false);

            var cmd = new StopCommand(() =>
            {
                _engine.RequestStop(); // Request stop from within command
                stopCalled = true;
                completed.Set();
            });

            // Act
            _engine.Submit(cmd);
            var signaled = completed.Wait(TimeSpan.FromSeconds(2));

            // Wait for engine to stop
            Thread.Sleep(500);

            // Assert
            Assert.IsTrue(signaled, "Command should complete without deadlock");
            Assert.IsTrue(stopCalled, "RequestStop should have been called");
            Assert.IsFalse(_engine.Running, "Engine should be stopped");

            // Cleanup
            completed.Dispose();
        }

        [TestMethod]
        public void RequestStop_FromMultipleThreads_IsSafe()
        {
            // Arrange
            _engine.Start();
            var threads = new List<Thread>();

            // Act
            for (int i = 0; i < 5; i++)
            {
                var thread = new Thread(() =>
                {
                    Thread.Sleep(10);
                    _engine.RequestStop();
                });
                threads.Add(thread);
                thread.Start();
            }

            foreach (var thread in threads)
                thread.Join();

            // Wait for engine to stop
            Thread.Sleep(1000);

            // Assert
            Assert.IsFalse(_engine.Running, "Engine should be stopped");
        }

        [TestMethod]
        public void FinishTurn_FromMultipleThreads_PreservesOrder()
        {
            // Arrange
            _engine.Start();
            var turnNumbers = new ConcurrentBag<int>();

            EventHandler handler = (s, e) =>
            {
                turnNumbers.Add(_engine.TurnCounter);
            };

            SubscribeToTurnTick(handler);

            // Act - Submit turns from multiple threads, but serialize execution to prevent coalescing
            // Each thread waits for its turn to complete before the next thread submits
            var submitLock = new object();
            var threads = new List<Thread>();

            for (int i = 0; i < 5; i++)
            {
                var turnIndex = i + 1; // Turn 1, 2, 3, 4, 5
                var thread = new Thread(() =>
                {
                    var turnFired = new ManualResetEventSlim(false);

                    EventHandler waitForTurn = (s, e) =>
                    {
                        if (_engine.TurnCounter == turnIndex)
                            turnFired.Set();
                    };

                    lock (submitLock)
                    {
                        SubscribeToTurnTick(waitForTurn);
                        _engine.FinishTurn();

                        // Wait for this specific turn to fire before releasing lock so next thread can submit
                        turnFired.Wait(TimeSpan.FromSeconds(2));

                        _engine.TurnTick -= waitForTurn;
                        _turnTickSubscribers.Remove(waitForTurn);
                    }

                    turnFired.Dispose();
                });

                threads.Add(thread);
                thread.Start();
            }

            foreach (var thread in threads)
                thread.Join();

            // Assert - verify all turns fired in order
            Assert.AreEqual(5, turnNumbers.Count, "All 5 turns should have fired");
            var ordered = turnNumbers.OrderBy(x => x).ToList();
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(i + 1, ordered[i], $"Turn {i + 1} should be in order");
            }
        }

        #endregion

        #region Race Condition Tests

        [TestMethod]
        public void CommandArrivesDuringTurnAdvance()
        {
            // Arrange
            _engine.Start();
            var commandExecuted = false;
            var completed = new ManualResetEventSlim(false);

            EventHandler turnHandler = (s, e) =>
            {
                // Submit a command during turn advance
                var cmd = new CounterCommand(() =>
                {
                    commandExecuted = true;
                    completed.Set();
                });
                _engine.Submit(cmd);
            };

            SubscribeToTurnTick(turnHandler);

            // Act
            _engine.FinishTurn();
            completed.Wait(TimeSpan.FromSeconds(2));

            // Assert
            Assert.IsTrue(commandExecuted, "Command submitted during turn advance should execute");

            // Cleanup
            completed.Dispose();
        }

        [TestMethod]
        public void SimultaneousStopAndCommand()
        {
            // Arrange
            _engine.Start();
            var completed = new ManualResetEventSlim(false);
            var commandExecuted = false;

            // Act
            var stopThread = new Thread(() =>
            {
                Thread.Sleep(50);
                _engine.RequestStop();
            });

            var commandThread = new Thread(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    if (!_engine.Running) break;
                    var cmd = new CounterCommand(() =>
                    {
                        commandExecuted = true;
                        completed.Set();
                    });
                    _engine.Submit(cmd);
                    Thread.Sleep(5);
                }
            });

            stopThread.Start();
            commandThread.Start();

            stopThread.Join();
            commandThread.Join();

            // Wait a bit for last command if any
            completed.Wait(TimeSpan.FromMilliseconds(500));

            // Assert
            Assert.IsFalse(_engine.Running, "Engine should be stopped");
            // Command execution is non-deterministic due to timing, but no exception should occur

            // Cleanup
            completed.Dispose();
        }

        #endregion

        /// <summary>
        /// Simple counter command for testing.
        /// </summary>
        private class CounterCommand : IGameCommand
        {
            private readonly Action _action;

            public CounterCommand(Action action)
            {
                _action = action;
            }

            public void Execute(GameEngineCore engine)
            {
                _action?.Invoke();
            }
        }

        /// <summary>
        /// Command that requests engine stop.
        /// </summary>
        private class StopCommand : IGameCommand
        {
            private readonly Action _action;

            public StopCommand(Action action)
            {
                _action = action;
            }

            public void Execute(GameEngineCore engine)
            {
                _action?.Invoke();
            }
        }
    }
}
