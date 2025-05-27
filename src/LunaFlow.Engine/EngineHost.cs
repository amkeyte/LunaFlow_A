namespace LunaFlow.Engine
{
    /// <summary>
    /// Acts as the entry point for the LunaFlow engine.
    /// Initializes the engine environment and hosts the main run loop.
    /// </summary>
    public class EngineHost
    {
        private readonly IGameEngine _engine;
        private readonly CommandDispatcher _dispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineHost"/> class.
        /// </summary>
        /// <param name="engine">The core game engine to use for processing.</param>
        /// <param name="dispatcher">The command dispatcher responsible for routing user commands.</param>
        public EngineHost(IGameEngine engine, CommandDispatcher dispatcher)
        {
            _engine = engine;
            _dispatcher = dispatcher;
        }

        public EngineHost()
        {
        }

        /// <summary>
        /// Starts the engine and enters the main command-processing loop.
        /// </summary>
        public void Run()
        {
            _engine.Initialize();

            while (true)
            {
                string input = ReadInput();
                if (string.IsNullOrWhiteSpace(input)) continue;

                Command command = ParseCommand(input);
                _dispatcher.Dispatch(command);
            }
        }

        /// <summary>
        /// Reads user input from the current I/O source (e.g., console or WebSocket).
        /// </summary>
        /// <returns>Raw input string.</returns>
        private string ReadInput()
        {
            // This is stubbed for now — you may later inject an input adapter here.
            Console.Write("> ");
            return Console.ReadLine() ?? "";
        }

        /// <summary>
        /// Parses raw user input into a <see cref="Command"/> object.
        /// </summary>
        /// <param name="input">Raw input string from the user.</param>
        /// <returns>A structured command object ready for dispatch.</returns>
        private Command ParseCommand(string input)
        {
            // TODO: Replace with more robust parsing logic and argument extraction.
            return new Command(input.Trim());
        }
    }
}
