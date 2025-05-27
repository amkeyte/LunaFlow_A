namespace LunaFlow.Rules.GeneratorServices
{
    /// <summary>
    /// Represents a group of rules parsed from a single rule group class.
    /// </summary>
    public sealed class RuleIRGroup
    {
        /// <summary>
        /// Gets or sets the group name, usually from the RuleGroup attribute.
        /// </summary>
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// Gets the list of parsed rule intermediate representations.
        /// </summary>
        public List<RuleIR> Rules { get; set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleIRGroup"/> class.
        /// </summary>
        public RuleIRGroup(string groupName)
        {
            GroupName = groupName;
        }
    }
}
