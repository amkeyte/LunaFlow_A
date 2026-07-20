namespace LunaFlow.Engine.Commands
{
    /// <summary>
    /// Handles the <c>echo</c> command by writing its arguments back to the console.
    /// </summary>
    public sealed class EchoCommandHandler : ICommandHandler
    {
        /// <summary>
        /// Writes <paramref name="command"/>'s arguments, space-joined, to the console.
        /// </summary>
        public void Handle(ICommand command) =>
            Console.WriteLine(string.Join(' ', command.Arguments));
    }
}
