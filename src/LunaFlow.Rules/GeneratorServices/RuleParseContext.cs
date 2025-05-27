namespace LunaFlow.Rules.GeneratorServices
{
    /// <summary>
    /// Provides context and input information to the rule parsing stage of the pipeline.
    /// Used to identify which assemblies or types should be inspected for rule definitions.
    /// </summary>
    public sealed class RuleParseContext
    {
        /// <summary>
        /// Gets the list of rule group types that will be parsed.
        /// </summary>
        public List<Type> RuleGroupTypes { get; } = new();

        /// <summary>
        /// Creates a new <see cref="RuleParseContext"/> for a given assembly.
        /// </summary>
        /// <param name="assembly">The assembly to scan for rule groups.</param>
        /// <returns>A populated parsing context.</returns>
        public static RuleParseContext FromAssembly(System.Reflection.Assembly assembly)
        {
            var context = new RuleParseContext();
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass && type.IsAbstract && type.IsSealed && Attribute.IsDefined(type, typeof(RuleGroupAttribute)))
                {
                    context.RuleGroupTypes.Add(type);
                }
            }
            return context;
        }

        /// <summary>
        /// Adds a single rule group type to the context.
        /// </summary>
        public RuleParseContext AddRuleGroup(Type type)
        {
            RuleGroupTypes.Add(type);
            return this;
        }
    }
}
