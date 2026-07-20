namespace LunaFlow.Engine
{
    /// <summary>
    /// Routes a parsed <see cref="ICommand"/> to its registered <see cref="ICommandHandler"/> by name.
    /// </summary>
    public interface ICommandDispatcher
    {
        /// <summary>Invokes the handler registered for <paramref name="command"/>'s name.</summary>
        void Dispatch(ICommand command);

        /// <summary>Registers <paramref name="handler"/> to handle commands named <paramref name="commandName"/>.</summary>
        void RegisterHandler(string commandName, ICommandHandler handler, string description = "");

        /// <summary>Lists every registered command name and its description.</summary>
        IEnumerable<(string Name, string Description)> GetCommandList();
    }
}