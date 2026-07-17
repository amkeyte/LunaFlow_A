namespace LunaFlow.Engine
{
    /// <summary>
    /// Responsible for routing incoming commands to the appropriate command handler.
    /// Maintains a registry of handlers and manages dispatch logic.
    /// </summary>
    public class DefaultCommandDispatcher : ICommandDispatcher
    {
        private readonly Dictionary<string, (ICommandHandler Handler, string Description)> _handlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommandDispatcher"/> class.
        /// </summary>
        public DefaultCommandDispatcher()
        {
            _handlers = new Dictionary<string, (ICommandHandler Handler, string Description)>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Registers a handler for the specified command name.
        /// </summary>
        /// <param name="commandName">The name of the command to handle (e.g., "move").</param>
        /// <param name="handler">The handler instance responsible for executing the command.</param>
        /// <param name="description">A short, human-readable description shown by the "help" command.</param>
        public void RegisterHandler(string commandName, ICommandHandler handler, string description = "")
        {
            if (string.IsNullOrWhiteSpace(commandName))
                throw new ArgumentException("Command name cannot be null or whitespace.", nameof(commandName));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _handlers[commandName] = (handler, description ?? "");
        }

        /// <summary>
        /// Dispatches a command to the appropriate handler, if one is registered.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public void Dispatch(ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (_handlers.TryGetValue(command.Name, out var entry))
            {
                entry.Handler.Handle(command);
            }
            else
            {
                Console.WriteLine($"Unknown command: {command.Name}");
            }
        }

        /// <summary>
        /// Lists all registered command names alongside their descriptions.
        /// </summary>
        public IEnumerable<(string Name, string Description)> GetCommandList() =>
            _handlers.Select(kvp => (kvp.Key, kvp.Value.Description));
    }
}
