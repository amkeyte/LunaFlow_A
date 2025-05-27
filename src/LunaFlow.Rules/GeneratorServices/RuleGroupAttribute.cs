namespace LunaFlow.Rules.GeneratorServices
{
    /// <summary>
    /// Identifies a static class that groups related rule templates.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class RuleGroupAttribute : Attribute
    {
        /// <summary>
        /// The name of the rule group.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleGroupAttribute"/> class.
        /// </summary>
        /// <param name="name">The name used to categorize the group of rules.</param>
        public RuleGroupAttribute(string name)
        {
            Name = name;
        }
    }
}
