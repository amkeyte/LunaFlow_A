namespace LunaFlow.Engine
{
    /// <summary>
    /// Responsible for routing incoming commands to the appropriate command handler.
    /// Maintains a registry of handlers and manages dispatch logic.
    /// </summary>
    public class CommandDispatcher
    {
        private readonly Dictionary<string, ICommandHandler> _handlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandDispatcher"/> class.
        /// </summary>
        public CommandDispatcher()
        {
            _handlers = new Dictionary<string, ICommandHandler>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Registers a handler for the specified command name.
        /// </summary>
        /// <param name="commandName">The name of the command to handle (e.g., "move").</param>
        /// <param name="handler">The handler instance responsible for executing the command.</param>
        public void RegisterHandler(string commandName, ICommandHandler handler)
        {
            if (string.IsNullOrWhiteSpace(commandName))
                throw new ArgumentException("Command name cannot be null or whitespace.", nameof(commandName));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _handlers[commandName] = handler;
        }

        /// <summary>
        /// Dispatches a command to the appropriate handler, if one is registered.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public void Dispatch(Command command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (_handlers.TryGetValue(command.Name, out var handler))
            {
                handler.Handle(command);
            }
            else
            {
                Console.WriteLine($"Unknown command: {command.Name}");
            }
        }
    }
}
