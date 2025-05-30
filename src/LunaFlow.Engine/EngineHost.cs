using LunaFlow.Engine.Commands;

namespace LunaFlow.Engine
{
    /// <summary>
    /// Acts as the entry point for the LunaFlow engine.
    /// Initializes the engine environment and hosts the main run loop.
    /// </summary>
    public class EngineHost
    {
        // Configuration containers for game-specific and command-related settings.
        private readonly GameEngineOptions _gameOptions = new();
        private readonly CommandOptions _commandOptions = new();

        // Core engine instance that handles rule logic, world state, etc.
        private readonly GameEngine _engine = new();

        /// <summary>
        /// Configures engine-level options using fluent style.
        /// </summary>
        public EngineHost ConfigureGameEngine(Action<GameEngineOptions> configure)
        {
            configure?.Invoke(_gameOptions);
            return this;
        }

        /// <summary>
        /// Configures command input and dispatch behavior.
        /// </summary>
        public EngineHost ConfigureCommand(Action<CommandOptions> configure)
        {
            configure?.Invoke(_commandOptions);
            return this;
        }

        /// <summary>
        /// Boots the engine and enters the interactive run loop.
        /// </summary>
        public void Run()
        {
            Console.WriteLine($"[LunaFlow] Starting game '{_gameOptions.GameName}'...");

            if (_gameOptions.EnableRulesEngine)
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
            _engine.Initialize(_gameOptions);

            var parser = _commandOptions.CommandParser;
            var dispatcher = _commandOptions.CommandDispatcher;

            // Basic interactive loop — should be abstracted for CLI/WebSocket/test environments.
            while (true)
            {
                string input = ReadInput();
                if (string.IsNullOrWhiteSpace(input)) continue;

                var command = parser.Parse(input);
                dispatcher.Dispatch(command);
            }
        }

        /// <summary>
        /// Reads input from the command source.
        /// Will need implementation depending on CLI vs WebSocket vs test harness.
        /// </summary>
        private string ReadInput()
        {
            throw new NotImplementedException();
            // Suggestion:
            // return Console.ReadLine() in CLI mode
        }
    }

    /// <summary>
    /// Options for configuring the core game engine.
    /// </summary>
    public class GameEngineOptions
    {
        public string GameName { get; set; } = "Unnamed";

        // Controls whether rule evaluation is active
        public bool EnableRulesEngine { get; set; } = true;
    }

    /// <summary>
    /// Options for parsing and dispatching input commands.
    /// </summary>
    public class CommandOptions
    {
        // Suggest making these nullable with proper null guards, or use required properties in .NET 8
        public ICommandParser CommandParser { get; set; } = new DefaultCommandParser();
        public ICommandDispatcher CommandDispatcher { get; set; } = new DefaultCommandDispatcher();
    }
}
