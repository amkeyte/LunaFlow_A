
using LunaFlow.Engine.Commands;
using LunaFlow.Engine.Game;

namespace LunaFlow.Engine
{
    /// <summary>
    /// Acts as the entry point for the LunaFlow engine or access an existing GameEngine if one is started.
    /// Initializes the engine environment and hosts the main run loop. Converts console input/output to engine speak.
    /// </summary>
    public class ConsoleEngine : IGameEngineTranslator
    {



        /// <summary>The underlying game engine's configuration, shared with the running <see cref="GameEngineCore"/> instance.</summary>
        public GameEngineOptions GameOptions => _engine.Options;
        private CommandOptions _commandOptions = new();

        // Core engine instance that handles rule logic, world state, etc.
        private readonly GameEngineCore _engine = GameEngineCore.Instance;

        /// <summary>
        /// Configures engine-level options using fluent style.
        /// </summary>
        public ConsoleEngine ConfigureGameEngine(Action<GameEngineOptions> configure)
        {
            configure?.Invoke(GameOptions);
            return this;
        }

        /// <summary>
        /// Configures command input and dispatch behavior.
        /// </summary>
        public ConsoleEngine ConfigureCommand(Action<CommandOptions> configure)
        {
            configure?.Invoke(_commandOptions);
            return this;
        }
        private ICommandParser Parser => _commandOptions.CommandParser;
        private ICommandDispatcher Dispatcher => _commandOptions.CommandDispatcher;
        private void TickHandler()
        {

        }

        /// <summary>
        /// Boots the engine and enters the interactive run loop.
        /// </summary>
        public void Run()
        {
            _engine.MessagePosted += (s, e) => MessagePostedHandler(e);
            _engine.TurnTick += (s, e) => TickHandler();

            _engine.SetOptions(GameOptions);


            var gameName = GameOptions.GameName;

            Console.WriteLine($"[LunaFlow] Accessing game '{gameName}'...");

            if (GameOptions.EnableRulesEngine)
            {
                Console.WriteLine("[LunaFlow] Rules engine enabled.");
                // TODO: Hook up NRules or custom rule system here
            }

            if (_commandOptions.CommandDispatcher is not null)
            {
                Console.WriteLine($"[LunaFlow] Using command dispatcher: {_commandOptions.CommandDispatcher.GetType().Name}");
                // TODO: Allow injecting middleware/command pipeline here
            }
            else
            {
                Console.WriteLine("[LunaFlow] No command dispatcher specified.");
                // Consider throwing or falling back to a default dispatcher here
            }

            // Initialize engine internals (world state, registries, services, etc.)
            _engine.Start();
            _engine.World.AddSession(new Session("console"));

            while (_engine.Running)
            {
                var input = ReadInput();
                if (input is null) break; // stdin closed
                if (string.IsNullOrWhiteSpace(input)) continue;

                var command = Parser.Parse(input);

                using var completed = new ManualResetEventSlim(false);
                _engine.Submit(new DispatchGameCommand(command, Dispatcher, completed));
                completed.Wait();
            }

            _engine.Stop();
        }

        private void MessagePostedHandler(GameEngineMessagePostedEventArgs e)
        {
            Console.WriteLine($"{e.Message}");
        }

        private void InterruptConsole()
        {

        }

        /// <summary>True while the engine's run loop is active.</summary>
        public bool Running => _engine.Running;
        /// <summary>
        /// Reads input from the command source.
        /// Will need implementation depending on CLI vs WebSocket vs test harness.
        /// </summary>
        private string? ReadInput() => Console.ReadLine();
    }



    /// <summary>
    /// Options for parsing and dispatching input commands.
    /// </summary>
    public class CommandOptions
    {
        // Suggest making these nullable with proper null guards, or use required properties in .NET 8
        /// <summary>The parser used to turn raw input into an <see cref="ICommand"/>. Defaults to <see cref="DefaultCommandParser"/>.</summary>
        public ICommandParser CommandParser { get; set; } = new DefaultCommandParser();

        /// <summary>The dispatcher used to route parsed commands to their handlers. Defaults to <see cref="DefaultCommandDispatcher"/>.</summary>
        public ICommandDispatcher CommandDispatcher { get; set; } = new DefaultCommandDispatcher();
    }
}
