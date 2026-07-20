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
        /// Gets or sets the target namespace of the emitted code. This default is only a
        /// fallback for constructing a <see cref="RuleCodeUnit"/> directly (e.g. in a test);
        /// <see cref="IRuleIRGenerator"/> implementations must always set this explicitly to
        /// <c>$"LunaFlow.Generated.{group.GroupName}"</c> — see the "D4 — generated namespace
        /// convention" decision in rule-pipeline.md.
        /// </summary>
        public string Namespace { get; set; } = "LunaFlow.Generated";

        /// <summary>
        /// Gets or sets the usings the emitted file needs. <see cref="IRuleIRGenerator"/>
        /// implementations must populate this as the union/dedup of every <see cref="RuleIR"/>'s
        /// <see cref="RuleIR.RequiredUsings"/> in the group, plus whatever the generated shape
        /// itself always needs (e.g. <c>LunaFlow.Rules</c>, <c>NRules.Fluent.Dsl</c>) — this is
        /// where that union lands; the DTO had no slot for it until this field was added
        /// alongside the Phase 2 implementation (ticket 0008), the same kind of gap D1 closed on
        /// <c>RuleIR</c> itself.
        /// </summary>
        public List<string> RequiredUsings { get; set; } = new();

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
