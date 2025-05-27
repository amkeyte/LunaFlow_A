namespace LunaFlow.Rules.GeneratorServices
{
    /// <summary>
    /// Represents a structured rule definition intended for NRules code generation.
    /// Contains all elements needed to synthesize a complete Rule-derived class.
    /// </summary>
    public sealed class RulePrototype
    {
        /// <summary>
        /// Name of the rule. Used as the generated class name and rule metadata.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional rule description. Not emitted unless present.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Usings or fully-qualified namespaces required in the generated file.
        /// </summary>
        public List<string> RequiredUsings { get; set; } = new();

        /// <summary>
        /// Lines of C# to insert before When()/Then() blocks. Commonly used for variable declarations.
        /// </summary>
        public List<string> PreambleStatements { get; set; } = new();

        /// <summary>
        /// DSL statements to insert within the When() block.
        /// Typically Match, Let, or Filter expressions.
        /// </summary>
        public List<string> WhenStatements { get; set; } = new();

        /// <summary>
        /// DSL statements to insert within the Then() block.
        /// Typically Do, Update, Insert, Retract expressions.
        /// </summary>
        public List<string> ThenStatements { get; set; } = new();
    }
}
