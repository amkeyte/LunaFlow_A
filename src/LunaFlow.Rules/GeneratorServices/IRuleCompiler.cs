using System.Reflection;

namespace LunaFlow.Rules.GeneratorServices
{
    /// <summary>
    /// Defines the interface responsible for compiling a generated rule unit into a loadable
    /// assembly. Compile-time MVP is in-memory Roslyn compilation — see the "Compiler MVP
    /// semantics" decision in rule-pipeline.md — chosen specifically because the phase-2 done
    /// bar (B4) requires a generated rule to load and fire in the same test run, which a
    /// write-.cs-to-disk-only compiler can't satisfy without an out-of-process build step.
    /// </summary>
    public interface IRuleCompiler
    {
        /// <summary>
        /// Compiles a generated code unit into a loadable assembly. Throws on compilation
        /// failure (diagnostics included in the exception message) — there is no partial-success
        /// return value in the MVP.
        /// </summary>
        /// <param name="codeUnit">The generated code unit to compile.</param>
        /// <returns>
        /// The compiled assembly, loadable via the same <c>RuleRepository.Load(x =>
        /// x.From(assembly)...)</c> path <c>LunaFlow.Engine.RuleEngine</c> already uses.
        /// </returns>
        Assembly Compile(RuleCodeUnit codeUnit);
    }
}
