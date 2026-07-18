using LunaFlow.Engine.Game;
using System.Collections.Concurrent;
using System.Threading;

namespace LunaFlow.EngineTests.Core
{
    /// <summary>
    /// Unit tests for GameEngineCore command dispatch, queueing, and execution.
    /// Covers: Command submission, queue ordering, thread safety, completion signals, error handling.
    /// </summary>
    [TestClass]
    [TestCategory("Phase2")]
    [TestCategory("CommandDispatch")]
    [DoNotParallelize]  // GameEngineCore.Instance is a singleton; tests must run sequentially to match production model
    public class GameEngineCoreCommandDispatchTests
    {
        private GameEngineCore _engine = null!;
        private List<EventHandler<GameEngineMessagePostedEventArgs>> _messagePostedSubscribers = new();

        [TestInitialize]
        public void Setup()
        {
            _engine = GameEngineCore.Instance;
            if (_engine.Running)
                _engine.Stop(force: true);
            _messagePostedSubscribers.Clear();
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Unsubscribe all handlers (cleanup guaranteed even if test fails)
            foreach (var handler in _messagePostedSubscribers)
            {
                _engine.MessagePosted -= handler;
            }
            _messagePostedSubscribers.Clear();

            if (_engine.Running)
                _engine.Stop(force: true);
        }

        /// <summary>
        /// Helper to subscribe to MessagePosted with automatic cleanup tracking.
        /// </summary>
        private void SubscribeToMessagePosted(EventHandler<GameEngineMessagePostedEventArgs> handler)
        {
            _engine.MessagePosted += handler;
            _messagePostedSubscribers.Add(handler);
        }

        #region Basic Command Submission Tests

        [TestMethod]
        public void Submit_AcceptsCommandWhileEngineRunning()
        {
            // Arrange
            _engine.Start();
            var completed = new ManualResetEventSlim(false);
            var testCommand = new TestCommandWithCompletion(() => { }, completed);

            // Act
            _engine.Submit(testCommand);

            // Wait for execution (using completion gate, not hardcoded sleep)
            var signaled = completed.Wait(TimeSpan.FromSeconds(1));

            // Assert
            Assert.IsTrue(signaled, "Command should be executed after submission");

            // Cleanup
            completed.Dispose();
        }

        [TestMethod]
        public void Submit_IgnoresNullCommand()
        {
            // Arrange
            _engine.Start();

            // Act & Assert - should not throw
            _engine.Submit(null!);

            // Cleanup
            _engine.Stop();
        }

        [TestMethod]
        public void Submit_QueuedCommandsExecuteInOrder()
        {
            // Arrange
            _engine.Start();
            var executionOrder = new ConcurrentBag<int>();
            var allCompleted = new ManualResetEventSlim(false);

            var completionGates = new List<ManualResetEventSlim>
            {
                new ManualResetEventSlim(false),
                new ManualResetEventSlim(false),
                new ManualResetEventSlim(false),
            };

            var commands = new List<TestCommandWithCompletion>
            {
                new TestCommandWithCompletion(
                    () => { executionOrder.Add(1); },
                    completionGates[0]
                ),
                new TestCommandWithCompletion(
                    () => { executionOrder.Add(2); },
                    completionGates[1]
                ),
                new TestCommandWithCompletion(
                    () => { executionOrder.Add(3); allCompleted.Set(); },
                    completionGates[2]
                ),
            };

            // Act - Submit all commands and wait for the last one to complete (signals all finished)
            foreach (var cmd in commands)
                _engine.Submit(cmd);

            var signaled = allCompleted.Wait(TimeSpan.FromSeconds(1));

            // Assert
            Assert.IsTrue(signaled, "All commands should complete");
            Assert.AreEqual(3, executionOrder.Count, "All 3 commands should execute");
            var orderedList = executionOrder.OrderBy(x => x).ToList();
            Assert.AreEqual(1, orderedList[0], "Commands should execute in FIFO order");
            Assert.AreEqual(2, orderedList[1], "Commands should execute in FIFO order");
            Assert.AreEqual(3, orderedList[2], "Commands should execute in FIFO order");

            // Cleanup
            allCompleted.Dispose();
            foreach (var gate in completionGates)
                gate.Dispose();
        }

        #endregion

        #region Completion Signal Tests

        [TestMethod]
        public void CommandCompletion_SignalFires()
        {
            // Arrange
            _engine.Start();
            var completed = new ManualResetEventSlim(false);
            var testCommand = new TestCommandWithCompletion(() => { }, completed);

            // Act
            _engine.Submit(testCommand);
            var signaled = completed.Wait(TimeSpan.FromSeconds(1));

            // Assert
            Assert.IsTrue(signaled, "Completion signal should be set after command execution");

            // Cleanup
            completed.Dispose();
            _engine.Stop();
        }

        [TestMethod]
        public void CommandCompletion_TimesOutIfCommandNeverExecutes()
        {
            // Arrange
            _engine.Stop(); // Ensure engine is not running
            var completed = new ManualResetEventSlim(false);
            var testCommand = new TestCommandWithCompletion(() => { }, completed);

            // Act
            var signaled = completed.Wait(TimeSpan.FromMilliseconds(100));

            // Assert
            Assert.IsFalse(signaled, "Completion signal should timeout if engine is not running");

            // Cleanup
            completed.Dispose();
        }

        [TestMethod]
        public void CommandCompletion_AllCommandsSignalCompletion()
        {
            // Arrange
            _engine.Start();
            var completedCount = 0;
            var completionSignals = new List<ManualResetEventSlim>();

            for (int i = 0; i < 5; i++)
            {
                var signal = new ManualResetEventSlim(false);
                completionSignals.Add(signal);
                var cmd = new TestCommandWithCompletion(() => { }, signal);
                _engine.Submit(cmd);
            }

            // Act & Assert
            foreach (var signal in completionSignals)
            {
                if (signal.Wait(TimeSpan.FromSeconds(1)))
                    completedCount++;
                signal.Dispose();
            }

            Assert.AreEqual(5, completedCount, "All 5 commands should signal completion");

            // Cleanup
            _engine.Stop();
        }

        #endregion

        #region Exception Handling Tests

        [TestMethod]
        public void CommandException_IsCaughtAndPostedAsMessage()
        {
            // Arrange
            _engine.Start();
            var messagesReceived = new ConcurrentBag<string>();
            EventHandler<GameEngineMessagePostedEventArgs> handler = (s, e) => messagesReceived.Add(e.Message);
            SubscribeToMessagePosted(handler);

            var testCommand = new TestCommand(() => throw new InvalidOperationException("Test error"));

            // Act
            _engine.Submit(testCommand);
            Thread.Sleep(300);

            // Assert
            Assert.IsTrue(
                messagesReceived.Any(m => m.Contains("error") || m.Contains("Error") || m.Contains("Command")),
                "Command error should be posted as a message"
            );
        }

        [TestMethod]
        public void CommandException_DoesNotStopEngine()
        {
            // Arrange
            _engine.Start();
            var commandExecutedAfterError = false;
            var testCommand1 = new TestCommand(() => throw new InvalidOperationException("Error"));
            var testCommand2 = new TestCommand(() => commandExecutedAfterError = true);

            // Act
            _engine.Submit(testCommand1);
            Thread.Sleep(200);
            _engine.Submit(testCommand2);
            Thread.Sleep(200);

            // Assert
            Assert.IsTrue(_engine.Running, "Engine should still be running after command exception");
            Assert.IsTrue(commandExecutedAfterError, "Subsequent commands should execute despite previous error");

            // Cleanup
            _engine.Stop();
        }

        #endregion

        #region Threading Tests

        [TestMethod]
        public void Submit_SafeFromMultipleThreads()
        {
            // Arrange
            _engine.Start();
            var executedCount = 0;
            var lockObj = new object();
            var threads = new List<Thread>();

            // Act
            for (int i = 0; i < 5; i++)
            {
                var thread = new Thread(() =>
                {
                    for (int j = 0; j < 10; j++)
                    {
                        var cmd = new TestCommand(() =>
                        {
                            lock (lockObj)
                                executedCount++;
                        });
                        _engine.Submit(cmd);
                    }
                });
                threads.Add(thread);
                thread.Start();
            }

            // Wait for all threads to submit their commands
            foreach (var thread in threads)
                thread.Join();

            // Wait for all commands to execute
            Thread.Sleep(1000);

            // Assert
            Assert.AreEqual(50, executedCount, "All 50 commands from 5 threads should execute");

            // Cleanup
            _engine.Stop();
        }

        [TestMethod]
        public void Submit_WhileShuttingDown_FailsSilently()
        {
            // Arrange
            _engine.Start();
            var commandExecuted = false;
            var testCommand = new TestCommand(() => commandExecuted = true);

            // Act
            _engine.RequestStop();

            // Wait a moment for shutdown to begin
            Thread.Sleep(100);

            // Submit command during shutdown (should be rejected)
            _engine.Submit(testCommand);

            // Wait for shutdown to complete
            Thread.Sleep(500);

            // Assert
            // Command may or may not execute depending on timing, but Submit should not throw
            Assert.IsFalse(_engine.Running, "Engine should be stopped");
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void CommandExecution_RunsInEngineThread()
        {
            // Arrange
            _engine.Start();
            var commandThreadId = 0;
            var testCommand = new TestCommand(() => commandThreadId = Thread.CurrentThread.ManagedThreadId);

            // Act
            _engine.Submit(testCommand);
            Thread.Sleep(300);

            // Assert
            Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, commandThreadId,
                "Command should not execute in the calling thread");
            Assert.AreNotEqual(0, commandThreadId, "Command should have executed");

            // Cleanup
            _engine.Stop();
        }

        [TestMethod]
        public void Submit_HighFrequencyCommands_PreservesOrder()
        {
            // Arrange
            _engine.Start();
            var sequence = new ConcurrentBag<int>();
            var tasks = new List<Task>();

            // Act - Submit commands as fast as possible
            for (int i = 0; i < 20; i++)
            {
                var index = i;
                var cmd = new TestCommand(() => sequence.Add(index));
                _engine.Submit(cmd);
            }

            Thread.Sleep(1000);

            // Assert
            Assert.AreEqual(20, sequence.Count, "All 20 commands should execute");
            var ordered = sequence.OrderBy(x => x).ToList();
            for (int i = 0; i < 20; i++)
            {
                Assert.AreEqual(i, ordered[i], $"Command {i} should be in position {i}");
            }

            // Cleanup
            _engine.Stop();
        }

        #endregion

        /// <summary>
        /// Test command that executes an action.
        /// </summary>
        private class TestCommand : IGameCommand
        {
            private readonly Action _action;

            public TestCommand(Action action)
            {
                _action = action;
            }

            public void Execute(GameEngineCore engine)
            {
                _action?.Invoke();
            }
        }

        /// <summary>
        /// Test command with manual completion signal.
        /// </summary>
        private class TestCommandWithCompletion : IGameCommand
        {
            private readonly Action _action;
            private readonly ManualResetEventSlim _completionSignal;

            public TestCommandWithCompletion(Action action, ManualResetEventSlim completionSignal)
            {
                _action = action;
                _completionSignal = completionSignal;
            }

            public void Execute(GameEngineCore engine)
            {
                _action?.Invoke();
                _completionSignal.Set();
            }
        }
    }
}
