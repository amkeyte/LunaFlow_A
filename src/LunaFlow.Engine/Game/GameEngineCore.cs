using System.Collections.Concurrent;

namespace LunaFlow.Engine.Game
{
    internal interface IGameCommand
    {
        void Execute(GameEngineCore engine);
    }

    internal sealed class GameEngineCore : IEngineHost
    {
        private static readonly Lazy<GameEngineCore> _lazy = new(() => new GameEngineCore());
        public static GameEngineCore Instance => _lazy.Value;

        // Options & state
        public GameEngineOptions Options { get; private set; } = new GameEngineOptions();
        public GameState State { get; } = new();
        public GameWorld World { get; } = new();
        public RuleEngine Rules { get; private set; } = null!; // populated in Start(), per nine-rooms-spec.md item 1
        public bool Running { get; private set; }
        public int TurnCounter { get; private set; }
        public volatile bool TurnFinished; // set this from clients/UI to advance exactly one turn

        // Events
        public event EventHandler<GameEngineMessagePostedEventArgs>? MessagePosted;

        // Loop infra
        private Thread? _loopThread;
        private CancellationTokenSource? _cts;

        // Work sources: commands + turn gate
        private readonly BlockingCollection<IGameCommand> _commands = new(new ConcurrentQueue<IGameCommand>());
        private readonly ManualResetEventSlim _turnAdvance = new(false);          // signaled when TurnFinished==true
        private readonly ManualResetEventSlim _hasTickSubscribers = new(false);    // idle when nobody cares
        private int _turnTickSubs;
        private EventHandler? _turnTick;

        private GameEngineCore() { }

        public GameEngineOptions SetOptions(GameEngineOptions opt)
        {
            Options = opt ?? new GameEngineOptions();
            Post("GameEngine initialized.");
            return Options;
        }

        public void Start()
        {
            if (Running) return;
            _cts = new CancellationTokenSource();
            _loopThread = new Thread(() => RunLoop(_cts.Token))
            {
                IsBackground = true,
                Name = "LunaFlow.GameEngineCore.Loop"
            };
            Running = true;
            _loopThread.Start();
            Post("GameEngine loop started (server mode, turn-based).");
        }

        public bool Stop(bool force = false)
        {
            if (_loopThread == null) return true;
            if (Thread.CurrentThread == _loopThread)
            {
                // Called from inside a command's Execute() — that's this thread. Never self-join.
                RequestStop();
                return true;
            }
            if (!force && _turnTickSubs > 1) return false;

            try
            {
                _cts?.Cancel();
                _commands.CompleteAdding();
                _loopThread.Join();
                Post("GameEngine loop stopped.");
                return true;
            }
            finally
            {
                Running = false;
                _cts?.Dispose();
                _cts = null;
                _loopThread = null;
                // keep queues/events for possible restart
            }
        }

        /// <summary>Non-blocking stop request. Safe to call from any thread, including from
        /// within a command's Execute(), which runs on _loopThread — never Joins.</summary>
        public void RequestStop()
        {
            if (_loopThread == null) return;
            _cts?.Cancel();
            _commands.CompleteAdding();
            Running = false;
            Post("GameEngine stop requested.");
        }

        /// <summary>Submit a command (admin or gameplay) from any client/thread.</summary>
        public void Submit(IGameCommand cmd)
        {
            if (cmd == null) return;
            // If CompleteAdding() was called, TryAdd will just fail silently; that’s fine for shutdown.
            _commands.TryAdd(cmd);
        }

        /// <summary>Signal that the current turn is finished; advances exactly one turn.</summary>
        public void FinishTurn()
        {
            TurnFinished = true;
            _turnAdvance.Set();
        }

        public event EventHandler TurnTick
        {
            add
            {
                var becameOne = Interlocked.Increment(ref _turnTickSubs) == 1;
                _turnTick += value;
                if (becameOne) _hasTickSubscribers.Set();
            }
            remove
            {
                _turnTick -= value;
                if (Interlocked.Decrement(ref _turnTickSubs) <= 0)
                {
                    Interlocked.Exchange(ref _turnTickSubs, 0);
                    _hasTickSubscribers.Reset();
                }
            }
        }

        private void RunLoop(CancellationToken token)
        {
            // WaitHandles we’ll race: command available, turn finished, or cancel
            WaitHandle[] waits = { _turnAdvance.WaitHandle, token.WaitHandle };

            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // If nobody listens to ticks, still process commands, but do not advance turns.
                        // We'll poll commands non-blocking first to keep admin UX snappy.
                        DrainPendingCommands(token);

                        // If we do have tick listeners, try to advance a turn when flagged.
                        if (_turnTickSubs > 0)
                        {
                            if (TurnFinished)
                                AdvanceOneTurnSafe();
                        }

                        // Now block until *either* a command arrives *or* TurnFinished gets set *or* we’re cancelled.
                        // We block on: commands via TryTake with timeout, and turn gate via WaitAny.
                        IGameCommand? cmd;
                        if (_commands.TryTake(out cmd, millisecondsTimeout: 250))
                        {
                            ExecuteCommandSafe(cmd);
                            continue;
                        }

                        if (_turnTickSubs > 0 && _turnAdvance.Wait(millisecondsTimeout: 0))
                        {
                            // Turn signal already set — advance now.
                            AdvanceOneTurnSafe();
                            continue;
                        }

                        // If nothing happened, optionally sleep a hair to reduce churn
                        // (the 250ms TryTake already throttled us)
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Post($"Engine loop error: {ex.GetType().Name}: {ex.Message}");
                    }
                }
            }
            finally
            {
                Running = false;
            }
        }

        private void DrainPendingCommands(CancellationToken token)
        {
            while (_commands.TryTake(out var cmd))
                ExecuteCommandSafe(cmd);
        }

        private void ExecuteCommandSafe(IGameCommand cmd)
        {
            try
            {
                cmd.Execute(this);
            }
            catch (Exception ex)
            {
                Post($"Command error: {ex.GetType().Name}: {ex.Message}");
            }
        }

        private void AdvanceOneTurnSafe()
        {
            try
            {
                // Reset gate before firing to avoid double-advance if handlers set FinishTurn again synchronously.
                TurnFinished = false;
                _turnAdvance.Reset();

                // Make sure someone cares; otherwise skip work
                if (_turnTickSubs <= 0) return;

                TurnCounter++;
                State.TurnNumber = TurnCounter;
                var handler = _turnTick;
                handler?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Post($"Turn error: {ex.GetType().Name}: {ex.Message}");
            }
        }

        private void Post(string message)
        {
            MessagePosted?.Invoke(this, new GameEngineMessagePostedEventArgs(message));
        }

        void IEngineHost.Announce(string message) => Post(message);
    }

    public sealed class GameEngineMessagePostedEventArgs : EventArgs
    {
        public string Message { get; }
        public GameEngineMessagePostedEventArgs(string message) => Message = message;
    }

    public sealed class GameEngineOptions
    {
        public string GameName { get; set; } = "Unnamed";
        public bool EnableRulesEngine { get; set; } = true;
    }
}
