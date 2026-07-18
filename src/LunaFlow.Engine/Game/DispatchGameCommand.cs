namespace LunaFlow.Engine.Game
{
    internal sealed class DispatchGameCommand : IGameCommand
    {
        private readonly ICommand _command;
        private readonly ICommandDispatcher _dispatcher;
        private readonly ManualResetEventSlim? _completed;

        public DispatchGameCommand(ICommand command, ICommandDispatcher dispatcher, ManualResetEventSlim? completed = null)
        {
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _completed = completed;
        }

        public void Execute(GameEngineCore engine)
        {
            try
            {
                _dispatcher.Dispatch(_command);
                engine.Rules.Fire();
            }
            finally { _completed?.Set(); }
        }
    }
}
