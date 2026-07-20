namespace LunaFlow.Engine.Commands
{
    /// <summary>
    /// Handles the <c>help</c> command by listing every command registered on the dispatcher.
    /// </summary>
    public sealed class HelpCommandHandler : ICommandHandler
    {
        private readonly ICommandDispatcher _dispatcher;

        /// <summary>
        /// Creates a handler that lists commands registered on <paramref name="dispatcher"/>.
        /// </summary>
        public HelpCommandHandler(ICommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        /// <summary>
        /// Writes each registered command's name and description to the console.
        /// </summary>
        public void Handle(ICommand command)
        {
            foreach (var (name, description) in _dispatcher.GetCommandList())
            {
                Console.WriteLine(string.IsNullOrWhiteSpace(description) ? name : $"{name} - {description}");
            }
        }
    }
}
