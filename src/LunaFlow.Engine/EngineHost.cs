using LunaFlow.Engine.Commands;

namespace LunaFlow.Engine
{
    /// <summary>
    /// Acts as the entry point for the LunaFlow engine.
    /// Initializes the engine environment and hosts the main run loop.
    /// </summary>
    public class EngineHost
    {
        private readonly GameEngineOptions _gameOptions = new();
        private readonly CommandOptions _commandOptions = new();
        private readonly GameEngine _engine = new();

        public EngineHost ConfigureGameEngine(Action<GameEngineOptions> configure)
        {
            configure?.Invoke(_gameOptions);
            return this;
        }

        public EngineHost ConfigureCommand(Action<CommandOptions> configure)
        {
            configure?.Invoke(_commandOptions);
            return this;
        }

        public void Run()
        {
            Console.WriteLine($"[LunaFlow] Starting game '{_gameOptions.GameName}'...");

            if (_gameOptions.EnableRulesEngine)
            {
                Console.WriteLine("[LunaFlow] Rules engine enabled.");
                // TODO: Initialize rule engine
            }

            if (_commandOptions.CommandDispatcher is not null)
            {
                Console.WriteLine($"[LunaFlow] Using command dispatcher: {_commandOptions.CommandDispatcher.GetType().Name}");
                // TODO: Start dispatch loop
            }
            else
            {
                Console.WriteLine("[LunaFlow] No command dispatcher specified.");
            }

            _engine.Initialize(_gameOptions);

            while (true)
            {
                string input = ReadInput();
                if (string.IsNullOrWhiteSpace(input)) continue;

                DefaultCommand command = ParseCommand(input);
                _dispatcher.Dispatch(command);
            }

        }

        private string ReadInput()
        {
            throw new NotImplementedException();
        }
    }

    public class GameEngineOptions
    {
        public string GameName { get; set; } = "Unnamed";
        public bool EnableRulesEngine { get; set; } = true;
    }

    public class CommandOptions
    {
        public ICommandParser CommandParser { get; set; } = new DefaultCommandParser();
        public DefaultCommandDispatcher CommandDispatcher { get; set; } = new DefaultCommandDispatcher();
    }
}
