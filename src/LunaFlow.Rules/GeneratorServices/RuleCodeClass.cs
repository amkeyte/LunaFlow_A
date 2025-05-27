namespace LunaFlow.Rules.GeneratorServices
{
    /// <summary>
    /// Represents a single class to be generated in the output file.
    /// Typically corresponds to one rule from the IR.
    /// </summary>
    public sealed class RuleCodeClass
    {
        /// <summary>
        /// Gets or sets the class name.
        /// </summary>
        public string ClassName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full body of the generated class.
        /// </summary>
        public string SourceCode { get; set; } = string.Empty;
    }
}
