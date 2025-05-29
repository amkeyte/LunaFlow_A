namespace LunaFlow.Engine
{
    /// <summary>
    /// Represents a user-issued command within the LunaFlow system.
    /// A command consists of a name and optional arguments that drive game behavior.
    /// </summary>
    public class DefaultCommand : ICommand
    {
        /// <summary>
        /// Gets the name of the command (e.g., "move", "look", "cast").
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the raw arguments supplied with the command, if any.
        /// </summary>
        public IEnumerable<string> Arguments { get; }

        IEnumerable<object?> ICommand.Arguments => Arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommand"/> class using raw input.
        /// Parses the first token as the command name and the rest as arguments.
        /// </summary>
        /// <param name="input">The raw user input string.</param>
        public DefaultCommand(object input)
        {
            if (input is not string strInput) throw new ArgumentException("Command ctor requires string type.");

            var parts = (strInput ?? "").Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Name = parts.Length > 0 ? parts[0].ToLowerInvariant() : "";
            Arguments = parts.Skip(1).ToArray();
        }

        /// <summary>
        /// Returns a string representation of the command and its arguments.
        /// </summary>
        /// <returns>A human-readable summary of the command.</returns>
        public override string ToString()
        {
            return $"{Name} {string.Join(' ', Arguments)}".Trim();
        }
    }
}
