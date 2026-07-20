using System.Reflection;

namespace LunaFlow.Rules.GeneratorServices
{
    /// <summary>
    /// Entry point used to trigger the rule pipeline generation.
    /// Authors may invoke this manually to compile or emit rule outputs.
    /// Wires <see cref="ReflectionRuleParser"/>, <see cref="RuleCodeGenerator"/>, and
    /// <see cref="RoslynRuleCompiler"/> through <see cref="RulePipeline"/> — the compile-time
    /// mode of the pipeline described in rule-pipeline.md.
    /// </summary>
    public static partial class RuleExport
    {
        private static RulePipeline CreatePipeline() =>
            new(new ReflectionRuleParser(), new RuleCodeGenerator(), new RoslynRuleCompiler());

        /// <summary>
        /// Compiles a single <c>[RuleGroup]</c>-attributed class into a loadable assembly.
        /// </summary>
        public static Assembly Compile(Type ruleGroupType)
        {
            var context = new RuleParseContext().AddRuleGroup(ruleGroupType);
            return CreatePipeline().Compile(context);
        }

        /// <summary>
        /// Compiles every <c>[RuleGroup]</c>-attributed class found in <paramref name="source"/>,
        /// one compiled assembly per group — <see cref="IRuleParser"/> produces one
        /// <see cref="RuleIRGroup"/> per call, so an assembly with multiple groups can't be
        /// collapsed into a single output assembly without merging distinct GroupNames.
        /// </summary>
        public static IEnumerable<Assembly> CompileAll(Assembly source)
        {
            foreach (var groupType in RuleParseContext.FromAssembly(source).RuleGroupTypes)
                yield return Compile(groupType);
        }
    }
}
