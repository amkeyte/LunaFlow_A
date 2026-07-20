namespace LunaFlow.Engine
{
    /// <summary>
    /// Defines the interface for handling a specific type of command.
    /// Implementers execute logic in response to a matching command name.
    /// </summary>
    public interface ICommandHandler
    {
        /// <summary>Executes this handler's logic for <paramref name="command"/>.</summary>
        void Handle(ICommand command);
    }
}
