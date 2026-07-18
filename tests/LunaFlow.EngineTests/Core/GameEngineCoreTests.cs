using LunaFlow.Engine.Game;
using System.Collections.Concurrent;

namespace LunaFlow.EngineTests.Core
{
    /// <summary>
    /// Comprehensive unit tests for GameEngineCore lifecycle, options, and basic engine operations.
    /// Covers: Start/Stop/Restart, options application, message events, state initialization.
    /// These tests form the foundation for all engine-dependent tests.
    /// </summary>
    [TestClass]
    [TestCategory("Phase2")]
    [TestCategory("Lifecycle")]
    [DoNotParallelize]  // GameEngineCore.Instance is a singleton; tests must run sequentially to match production model
    public class GameEngineCoreTests
    {
        private GameEngineCore _engine = null!;
        private List<EventHandler<GameEngineMessagePostedEventArgs>> _messagePostedSubscribers = new();

        [TestInitialize]
        public void Setup()
        {
            // Get the singleton instance (same for all tests in this class)
            _engine = GameEngineCore.Instance;
            // Ensure clean state between tests
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

            // Ensure engine is stopped after each test
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

        #region Lifecycle Tests

        [TestMethod]
        public void Start_InitializesEngineInRunningState()
        {
            // Arrange
            Assert.IsFalse(_engine.Running, "Engine should not be running before Start()");

            // Act
            _engine.Start();

            // Assert
            Assert.IsTrue(_engine.Running, "Engine should be running after Start()");
            Assert.IsNotNull(_engine.Rules, "RuleEngine should be initialized on Start()");
            Assert.AreEqual(0, _engine.TurnCounter, "Turn counter should begin at 0");

            // Cleanup
            _engine.Stop();
        }

        [TestMethod]
        public void Start_IsIdempotent()
        {
            // Arrange
            _engine.Start();
            var firstThread = System.Diagnostics.Process.GetCurrentProcess().Threads.Count;

            // Act
            _engine.Start(); // Should be a no-op

            // Assert
            Assert.IsTrue(_engine.Running, "Engine should still be running");
            var secondThreadCount = System.Diagnostics.Process.GetCurrentProcess().Threads.Count;
            // Thread count should not have increased significantly (allowing small variance)
            Assert.IsTrue(secondThreadCount <= firstThread + 2, "Start() should not spawn duplicate threads");

            // Cleanup
            _engine.Stop();
        }

        [TestMethod]
        public void Stop_StopsEngineGracefully()
        {
            // Arrange
            _engine.Start();
            Assert.IsTrue(_engine.Running, "Engine should be running");

            // Act
            var stopped = _engine.Stop();

            // Assert
            Assert.IsTrue(stopped, "Stop() should return true on successful shutdown");
            Assert.IsFalse(_engine.Running, "Engine should not be running after Stop()");

            // Wait a moment for thread to fully join
            System.Threading.Thread.Sleep(100);
        }

        [TestMethod]
        public void Stop_CanRestartAfterStopping()
        {
            // Arrange & Act
            _engine.Start();
            _engine.Stop();
            Assert.IsFalse(_engine.Running, "Engine should be stopped");

            var restarted = false;
            try
            {
                _engine.Start();
                restarted = _engine.Running;
            }
            finally
            {
                if (_engine.Running)
                    _engine.Stop();
            }

            // Assert
            Assert.IsTrue(restarted, "Engine should be able to restart after Stop()");
        }

        [TestMethod]
        public void Stop_ReturnsFalseIfMultipleSubscribersExistAndNotForced()
        {
            // Arrange
            _engine.Start();
            var subscriber1Fired = false;
            var subscriber2Fired = false;

            EventHandler handler1 = (s, e) => subscriber1Fired = true;
            EventHandler handler2 = (s, e) => subscriber2Fired = true;

            _engine.TurnTick += handler1;
            _engine.TurnTick += handler2;

            // Act
            var result = _engine.Stop(force: false);

            // Assert
            Assert.IsFalse(result, "Stop(force:false) should return false when multiple subscribers exist");
            Assert.IsTrue(_engine.Running, "Engine should still be running");

            // Cleanup
            _engine.TurnTick -= handler1;
            _engine.TurnTick -= handler2;
            _engine.Stop(force: true);
        }

        [TestMethod]
        public void Stop_SucceedsWhenForcedRegardlessOfSubscribers()
        {
            // Arrange
            _engine.Start();
            EventHandler handler = (s, e) => { };
            _engine.TurnTick += handler;
            _engine.TurnTick += handler;

            // Act
            var result = _engine.Stop(force: true);

            // Assert
            Assert.IsTrue(result, "Stop(force:true) should return true even with subscribers");
            Assert.IsFalse(_engine.Running, "Engine should be stopped");

            // Cleanup
            _engine.TurnTick -= handler;
            _engine.TurnTick -= handler;
        }

        [TestMethod]
        public void RequestStop_StopsEngineWithoutBlocking()
        {
            // Arrange
            _engine.Start();
            Assert.IsTrue(_engine.Running, "Engine should be running");

            // Act
            _engine.RequestStop();

            // Assert - RequestStop is non-blocking, so engine should stop on its own
            // Give it time to process the cancellation
            var timeout = System.DateTime.UtcNow.AddMilliseconds(2000);
            while (_engine.Running && System.DateTime.UtcNow < timeout)
            {
                System.Threading.Thread.Sleep(10);
            }

            Assert.IsFalse(_engine.Running, "Engine should be stopped after RequestStop()");
        }

        #endregion

        #region Options Tests

        [TestMethod]
        public void SetOptions_AppliesOptionsBeforeStart()
        {
            // Arrange
            var options = new GameEngineOptions
            {
                GameName = "Test Game",
                EnableRulesEngine = true
            };

            // Act
            var result = _engine.SetOptions(options);

            // Assert
            Assert.IsNotNull(result, "SetOptions should return the options object");
            Assert.AreEqual("Test Game", _engine.Options.GameName, "GameName should be set");
            Assert.IsTrue(_engine.Options.EnableRulesEngine, "EnableRulesEngine should be set");
        }

        [TestMethod]
        public void SetOptions_NullOptionsCreatesDefault()
        {
            // Act
            var result = _engine.SetOptions(null!);

            // Assert
            Assert.IsNotNull(result, "SetOptions should create a default GameEngineOptions when null is passed");
            Assert.AreEqual("Unnamed", result.GameName, "Default GameName should be 'Unnamed'");
        }

        [TestMethod]
        public void Start_AppliesRuleEngineOptionsCorrectly()
        {
            // Arrange
            var options = new GameEngineOptions { EnableRulesEngine = true };
            _engine.SetOptions(options);

            // Act
            _engine.Start();

            // Assert
            Assert.IsNotNull(_engine.Rules, "RuleEngine should be initialized when EnableRulesEngine is true");

            // Cleanup
            _engine.Stop();
        }

        #endregion

        #region Message Event Tests

        [TestMethod]
        public void MessagePosted_EventFiresOnStartup()
        {
            // Arrange
            var startupMessageFired = new ManualResetEventSlim(false);
            EventHandler<GameEngineMessagePostedEventArgs> handler = (s, e) =>
            {
                if (e.Message.Contains("initialized") || e.Message.Contains("started"))
                    startupMessageFired.Set();
            };
            SubscribeToMessagePosted(handler);

            // Act
            _engine.Start();

            // Assert - wait for startup message event, don't guess with sleep
            var signaled = startupMessageFired.Wait(TimeSpan.FromSeconds(1));
            Assert.IsTrue(signaled, "Startup message should be posted on Start()");

            // Cleanup
            startupMessageFired.Dispose();
        }

        [TestMethod]
        public void MessagePosted_EventFiresOnStop()
        {
            // Arrange
            var stopMessageFired = new ManualResetEventSlim(false);
            EventHandler<GameEngineMessagePostedEventArgs> handler = (s, e) =>
            {
                if (e.Message.Contains("stopped"))
                    stopMessageFired.Set();
            };
            _engine.Start();
            SubscribeToMessagePosted(handler);

            // Act
            _engine.Stop();

            // Assert - wait for stop message event, don't guess with sleep
            var signaled = stopMessageFired.Wait(TimeSpan.FromSeconds(1));
            Assert.IsTrue(signaled, "Stop message should be posted when engine stops");

            // Cleanup
            stopMessageFired.Dispose();
        }

        [TestMethod]
        public void MessagePosted_EventFiresOnRequestStop()
        {
            // Arrange
            var stopMessageFired = new ManualResetEventSlim(false);
            EventHandler<GameEngineMessagePostedEventArgs> handler = (s, e) =>
            {
                if (e.Message.Contains("stop") || e.Message.Contains("Stop"))
                    stopMessageFired.Set();
            };
            _engine.Start();
            SubscribeToMessagePosted(handler);

            // Act
            _engine.RequestStop();

            // Assert - wait for stop message event, don't poll
            var signaled = stopMessageFired.Wait(TimeSpan.FromSeconds(2));
            Assert.IsTrue(signaled, "Stop message should be posted when RequestStop() is called");

            // RequestStop is non-blocking; give the background thread time to finish.
            // Once the message fired, the engine is stopping; allow a moment for thread join.
            var deadline = System.DateTime.UtcNow.AddMilliseconds(2000);
            while (_engine.Running && System.DateTime.UtcNow < deadline)
            {
                System.Threading.Thread.SpinWait(10000); // Spin wait instead of sleep
            }

            // Cleanup
            stopMessageFired.Dispose();
        }

        #endregion

        #region State Tests

        [TestMethod]
        public void State_TurnNumberInitializesAtZero()
        {
            // Arrange & Act
            _engine.Start();

            // Assert
            Assert.AreEqual(0, _engine.State.TurnNumber, "Initial turn number should be 0");

            // Cleanup
            _engine.Stop();
        }

        [TestMethod]
        public void TurnCounter_StartsAtZero()
        {
            // Arrange & Act
            _engine.Start();

            // Assert
            Assert.AreEqual(0, _engine.TurnCounter, "Initial turn counter should be 0");

            // Cleanup
            _engine.Stop();
        }

        #endregion

        #region World Tests

        [TestMethod]
        public void World_IsAvailableAfterStart()
        {
            // Arrange & Act
            _engine.Start();

            // Assert
            Assert.IsNotNull(_engine.World, "GameWorld should be available");
            Assert.AreEqual(0, _engine.World.Sessions.Count, "World should start with no sessions");

            // Cleanup
            _engine.Stop();
        }

        [TestMethod]
        public void World_PersistsBetweenStarts()
        {
            // Arrange
            _engine.Start();
            _engine.World.AddSession(new Session("test-session"));
            _engine.Stop();

            // Act
            _engine.Start();

            // Assert
            Assert.AreEqual(1, _engine.World.Sessions.Count, "World state should persist after restart");

            // Cleanup
            _engine.Stop();
        }

        #endregion

        #region Edge Cases

        [TestMethod]
        public void Stop_BeforeStart_ReturnsTrueWithoutError()
        {
            // Act
            var result = _engine.Stop();

            // Assert
            Assert.IsTrue(result, "Stop() before Start() should return true without error");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Rules_BeforeStart_ThrowsWhenAccessed()
        {
            // This tests that Rules is null until Start() is called
            // Actually, Rules is initialized in Start(), so this test should verify
            // that using Rules before initialization would fail. We'll test Insert() instead.
            _engine.Rules?.Insert(new object());
        }

        [TestMethod]
        public void Options_CanBeModifiedAfterCreation()
        {
            // Arrange
            var options = new GameEngineOptions { GameName = "Game 1" };
            _engine.SetOptions(options);

            // Act
            _engine.Options.GameName = "Game 2";

            // Assert
            Assert.AreEqual("Game 2", _engine.Options.GameName, "Options should be modifiable");
        }

        #endregion
    }
}
