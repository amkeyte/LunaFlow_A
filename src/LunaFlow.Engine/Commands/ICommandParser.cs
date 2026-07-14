namespace LunaFlow.Engine.Commands
{
    public interface ICommandParser
    {
        public ICommand Parse(object input);
    }
}
