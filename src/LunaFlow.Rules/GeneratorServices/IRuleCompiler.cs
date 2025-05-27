namespace LunaFlow.Rules.GeneratorServices
{
    /// <summary>
    /// Defines the interface responsible for emitting code files or compiling rules to assemblies.
    /// </summary>
    public interface IRuleCompiler
    {
        /// <summary>
        /// Compiles or emits code for a generated rule unit.
        /// </summary>
        /// <param name="codeUnit">The generated code unit to emit.</param>
        void Compile(RuleCodeUnit codeUnit);
    }
}
