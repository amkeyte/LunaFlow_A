namespace LunaFlow.Rules.GeneratorServices
{
    /// <summary>
    /// Represents a single parsed rule in the intermediate stage.
    /// Contains all data required for code generation.
    /// </summary>
    public sealed class RuleIR
    {
        /// <summary>
        /// Gets or sets the unique name of the rule.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a short description of the rule.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the list of type-level standing rules (e.g. markers that must not be present).
        /// </summary>
        public List<Type> StandingRules { get; set; } = new();

        /// <summary>
        /// Gets or sets the string representation of the condition logic.
        /// This is emitted directly in the generated class.
        /// </summary>
        public string ConditionCode { get; set; } = "true";

        /// <summary>
        /// Gets or sets the string representation of the action logic.
        /// This is emitted directly in the generated class.
        /// </summary>
        public string ActionCode { get; set; } = "{}";
    }
}
