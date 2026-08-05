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
        private readonly object _lifecycleLock = new(); // serializes Start/Stop/RequestStop/Submit against each other

        // Work sources: commands + turn gate
        private BlockingCollection<IGameCommand> _commands = new(new ConcurrentQueue<IGameCommand>());
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
            lock (_lifecycleLock)
            {
                if (Running) return;

                // A Start() is a new game, not a resume — turn state resets every time, the same
                // way Rules already gets a fresh RuleEngine below. Also clears any turn-advance
                // signal armed by a stray FinishTurn() call while the engine wasn't running (see
                // FinishTurn_WhileEngineNotRunning_DoesNothing), so a new game doesn't inherit an
                // already-triggered turn gate.
                TurnCounter = 0;
                State.TurnNumber = 0;
                TurnFinished = false;
                _turnAdvance.Reset();

                // Timed separately from RuleExport.Compile's Roslyn codegen pass (MyLunaFlowServer's
                // own "Session setup" metric, upstream of this call): ApplyTo's LoadRulesFromAssembly
                // is what actually invokes NRules' RuleRepository.Compile()/CreateSession() — the real
                // Rete network build — not the C#-source-to-assembly compile that already happened.
                Rules = new RuleEngine();
                var reteStopwatch = System.Diagnostics.Stopwatch.StartNew();
                Options.Rules.ApplyTo(Rules);
                reteStopwatch.Stop();
                Post($"Rete network compiled in {reteStopwatch.ElapsedMilliseconds}ms (NRules RuleRepository.Compile/CreateSession).");

                if (Rules.IsInitialized)
                {
                    var fireStopwatch = System.Diagnostics.Stopwatch.StartNew();
                    Rules.Fire(); // "initial settle": rules already satisfied by seeded facts get a chance to fire before turn one
                    fireStopwatch.Stop();
                    Post($"Initial settle fired in {fireStopwatch.ElapsedMilliseconds}ms.");
                }
                // BlockingCollection can't un-complete itself — a prior Stop() leaves it permanently
                // closed to additions, so a restart needs a fresh one.
                if (_commands.IsAddingCompleted)
                    _commands = new BlockingCollection<IGameCommand>(new ConcurrentQueue<IGameCommand>());
                _cts = new CancellationTokenSource();
                // Capture the token and queue as locals rather than reading _cts/_commands by field
                // reference inside RunLoop: Thread.Start() only requests scheduling, so the OS may not
                // actually run this lambda until after a later Stop() has nulled _cts (or a later Start()
                // has swapped _commands) — reading the fields at that point would race. A captured local
                // stays bound to the run it was created for.
                var runToken = _cts.Token;
                var runCommands = _commands;
                _loopThread = new Thread(() => RunLoop(runToken, runCommands))
                {
                    IsBackground = true,
                    Name = "LunaFlow.GameEngineCore.Loop"
                };
                Running = true;
                _loopThread.Start();
            }
            Post("GameEngine loop started (server mode, turn-based).");
        }

        public bool Stop(bool force = false)
        {
            bool selfJoin;
            Thread? threadToJoin = null;

            lock (_lifecycleLock)
            {
                if (_loopThread == null) return true;
                if (Thread.CurrentThread == _loopThread)
                {
                    // Called from inside a command's Execute() — that's this thread. Never self-join.
                    RequestStopLocked();
                    selfJoin = true;
                }
                else
                {
                    if (!force && _turnTickSubs > 1) return false;

                    selfJoin = false;
                    _cts?.Cancel();
                    _commands.CompleteAdding();
                    threadToJoin = _loopThread;
                    Running = false;
                    _cts?.Dispose();
                    _cts = null;
                    _loopThread = null;
                    // keep queues/events for possible restart
                }
            }

            if (selfJoin)
            {
                Post("GameEngine stop requested.");
                return true;
            }

            threadToJoin!.Join();
            Post("GameEngine loop stopped.");
            return true;
        }

        /// <summary>Non-blocking stop request. Safe to call from any thread, including from
        /// within a command's Execute(), which runs on _loopThread — never Joins.</summary>
        public void RequestStop()
        {
            lock (_lifecycleLock)
            {
                RequestStopLocked();
            }
            Post("GameEngine stop requested.");
        }

        /// <summary>Body of RequestStop(), for callers that already hold _lifecycleLock
        /// (lock is reentrant per-thread, so this is safe to call from within Stop()'s lock too).</summary>
        private void RequestStopLocked()
        {
            if (_loopThread == null) return;
            _cts?.Cancel();
            _commands.CompleteAdding();
            Running = false;
        }

        /// <summary>Submit a command (admin or gameplay) from any client/thread.</summary>
        public void Submit(IGameCommand cmd)
        {
            if (cmd == null) return;
            lock (_lifecycleLock)
            {
                // If CompleteAdding() was called, TryAdd throws rather than failing silently —
                // check first so a command racing a Stop()/RequestStop() is just dropped.
                if (_commands.IsAddingCompleted) return;
                _commands.TryAdd(cmd);
            }
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

        private void RunLoop(CancellationToken token, BlockingCollection<IGameCommand> commands)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // If nobody listens to ticks, still process commands, but do not advance turns.
                        // We'll poll commands non-blocking first to keep admin UX snappy.
                        DrainPendingCommands(commands);

                        // If we do have tick listeners, try to advance a turn when flagged.
                        if (_turnTickSubs > 0)
                        {
                            if (TurnFinished)
                                AdvanceOneTurnSafe();
                        }

                        // Now block until *either* a command arrives *or* TurnFinished gets set *or* we’re cancelled.
                        IGameCommand? cmd;
                        if (commands.TryTake(out cmd, millisecondsTimeout: 250))
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

        private void DrainPendingCommands(BlockingCollection<IGameCommand> commands)
        {
            while (commands.TryTake(out var cmd))
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
                // Make sure someone cares; otherwise skip work, but still clear the gate.
                if (_turnTickSubs <= 0)
                {
                    TurnFinished = false;
                    _turnAdvance.Reset();
                    return;
                }

                TurnCounter++;
                State.TurnNumber = TurnCounter;
                var handler = _turnTick;
                try
                {
                    handler?.Invoke(this, EventArgs.Empty);
                }
                finally
                {
                    // Reset AFTER the handler runs, not before: FinishTurn() represents a
                    // deliberate external turn-end decision (command/Rulebook layer), not
                    // something the engine's own TurnTick notification should be able to trigger
                    // on itself. Resetting first let a handler's synchronous FinishTurn() call
                    // re-arm a fresh signal the next loop pass would honor, cascading turns
                    // instead of suppressing the resubmit as originally intended. In `finally` so
                    // the gate still clears if the handler throws.
                    TurnFinished = false;
                    _turnAdvance.Reset();
                }
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

    /// <summary>Event data for <see cref="GameEngineCore.MessagePosted"/>, carrying a single announcement string.</summary>
    public sealed class GameEngineMessagePostedEventArgs : EventArgs
    {
        /// <summary>The announced message text.</summary>
        public string Message { get; }

        /// <summary>Creates event data carrying <paramref name="message"/>.</summary>
        public GameEngineMessagePostedEventArgs(string message) => Message = message;
    }

    /// <summary>Startup configuration for a <see cref="GameEngineCore"/> instance.</summary>
    public sealed class GameEngineOptions
    {
        /// <summary>The display name for the running game. Defaults to "Unnamed".</summary>
        public string GameName { get; set; } = "Unnamed";

        /// <summary>Whether the NRules-backed rule engine is active. Defaults to true.</summary>
        public bool EnableRulesEngine { get; set; } = true;

        /// <summary>Which rule types and seed facts the rule engine loads at startup.</summary>
        public RuleEngineOptions Rules { get; } = new();
    }
}
