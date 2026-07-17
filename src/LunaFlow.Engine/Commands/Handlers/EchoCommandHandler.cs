namespace LunaFlow.Engine.Commands
{
    public sealed class EchoCommandHandler : ICommandHandler
    {
        public void Handle(ICommand command) =>
            Console.WriteLine(string.Join(' ', command.Arguments));
    }
}
