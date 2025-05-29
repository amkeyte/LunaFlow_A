namespace LunaFlow.Engine
{
    public interface ICommandDispatcher
    {
        void Dispatch(ICommand command);
        void RegisterHandler(string commandName, ICommandHandler handler);
    }
}