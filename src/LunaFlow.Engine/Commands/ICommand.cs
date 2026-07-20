namespace LunaFlow.Engine
{
    /// <summary>
    /// A parsed command ready for dispatch: a name plus its arguments.
    /// </summary>
    public interface ICommand
    {
        /// <summary>The command's arguments, in order.</summary>
        IEnumerable<object?> Arguments { get; }

        /// <summary>The command name used to look up its handler.</summary>
        string Name { get; }

        /// <summary>Returns a display-friendly representation of the command.</summary>
        string ToString();

    }
}