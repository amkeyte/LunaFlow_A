namespace LunaFlow.Rules.GeneratorServices
{
    /// <summary>
    /// Defines the interface for converting parsed rules into a format suitable for code generation.
    /// </summary>
    public interface IRuleIRGenerator
    {
        /// <summary>
        /// Converts an intermediate rule group into a fully-formed code unit.
        /// </summary>
        /// <param name="ir">The rule group to convert.</param>
        /// <returns>A code unit ready for compilation or emission.</returns>
        RuleCodeUnit Generate(RuleIRGroup ir);
    }
}
