namespace LunaFlow.Engine.Commands
{
    public sealed class HelpCommandHandler : ICommandHandler
    {
        private readonly ICommandDispatcher _dispatcher;

        public HelpCommandHandler(ICommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public void Handle(ICommand command)
        {
            foreach (var (name, description) in _dispatcher.GetCommandList())
            {
                Console.WriteLine(string.IsNullOrWhiteSpace(description) ? name : $"{name} - {description}");
            }
        }
    }
}
