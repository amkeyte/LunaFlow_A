namespace LunaFlow.Engine.Commands
{
    /// <summary>
    /// The default <see cref="ICommandParser"/>, which wraps raw input as a <see cref="DefaultCommand"/> without further parsing.
    /// </summary>
    public class DefaultCommandParser : ICommandParser
    {
        /// <summary>
        /// Wraps <paramref name="input"/> in a <see cref="DefaultCommand"/>.
        /// </summary>
        public ICommand Parse(object input) => new DefaultCommand(input);
    }
}
