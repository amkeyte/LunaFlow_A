namespace LunaFlow.Engine.Commands
{
    public class DefaultCommandParser : ICommandParser
    {
        public ICommand Parse(object input) => new DefaultCommand(input);
    }
}
