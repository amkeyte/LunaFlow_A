namespace LunaFlow.Engine
{
    public interface ICommand
    {
        IEnumerable<object?> Arguments { get; }
        string Name { get; }

        string ToString();

    }
}