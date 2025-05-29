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

            _engine.Initialize();

            while (true)
            {
                string input = ReadInput();
                if (string.IsNullOrWhiteSpace(input)) continue;

                Command command = ParseCommand(input);
                _dispatcher.Dispatch(command);
            }

        }
    }

    public class GameEngineOptions
    {
        public string GameName { get; set; } = "Unnamed";
        public bool EnableRulesEngine { get; set; } = true;
    }

    public class CommandOptions
    {
        public object? CommandDispatcher { get; set; }
    }
}
