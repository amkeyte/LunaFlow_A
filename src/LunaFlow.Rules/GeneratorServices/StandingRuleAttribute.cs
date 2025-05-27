namespace LunaFlow.Rules.GeneratorServices
{
    /// <summary>
    /// Specifies that a rule or rule group requires the absence of a marker component in the context.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class StandingRuleAttribute : Attribute
    {
        /// <summary>
        /// The type of marker to be tested with <c>!ctx.Has(type)</c>.
        /// </summary>
        public Type NotType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandingRuleAttribute"/> class.
        /// </summary>
        /// <param name="notType">The marker type to exclude when evaluating the rule.</param>
        public StandingRuleAttribute(Type notType)
        {
            NotType = notType;
        }
    }
}
