namespace LunaFlow.Rules.GeneratorServices
{
    /// <summary>
    /// Represents the output of a code generation step: a C# file containing generated rule classes.
    /// </summary>
    public sealed class RuleCodeUnit
    {
        /// <summary>
        /// Gets or sets the name of the file to emit.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the target namespace of the emitted code.
        /// </summary>
        public string Namespace { get; set; } = "LunaFlow.GeneratedRules";

        /// <summary>
        /// Gets the list of rule classes to include in the file.
        /// </summary>
        public List<RuleCodeClass> Classes { get; set; } = new();

        /// <summary>
        /// Gets or sets the registry method body for exposing all rules.
        /// This is injected into a static group class for use at runtime.
        /// </summary>
        public string RegistryMethodBody { get; set; } = string.Empty;
    }
}
