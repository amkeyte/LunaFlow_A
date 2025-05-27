namespace LunaFlow.Rules.GeneratorServices
{
    /// <summary>
    /// Defines the interface for parsing rule declarations from author code into an intermediate representation.
    /// </summary>
    public interface IRuleParser
    {
        /// <summary>
        /// Parses one or more rule groups and returns the intermediate rule data.
        /// </summary>
        /// <param name="context">The context describing what to parse.</param>
        /// <returns>A parsed group of intermediate rule data.</returns>
        RuleIRGroup Parse(RuleParseContext context);
    }
}
