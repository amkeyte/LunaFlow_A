namespace LunaFlow.Engine.Commands
{
    /// <summary>
    /// Parses raw input (e.g. a console line) into an <see cref="ICommand"/> ready for dispatch.
    /// </summary>
    public interface ICommandParser
    {
        /// <summary>Parses <paramref name="input"/> into an <see cref="ICommand"/>.</summary>
        public ICommand Parse(object input);
    }
}
