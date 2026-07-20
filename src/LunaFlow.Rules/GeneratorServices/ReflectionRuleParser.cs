using System.Reflection;

namespace LunaFlow.Rules.GeneratorServices
{
    /// <summary>
    /// Reflects over <see cref="RuleParseContext.RuleGroupTypes"/> — classes marked
    /// <see cref="RuleGroupAttribute"/> — and their <see cref="RuleTemplateAttribute"/>-marked
    /// static properties, mapping each returned <see cref="RulePrototype"/> to a <see cref="RuleIR"/>
    /// per the D1 join rules in rule-pipeline.md.
    /// </summary>
    public sealed class ReflectionRuleParser : IRuleParser
    {
        /// <summary>
        /// Parses the single rule-group type in <paramref name="context"/> into a <see cref="RuleIRGroup"/>.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// <paramref name="context"/> has zero or more than one rule-group type, or its type is missing <see cref="RuleGroupAttribute"/>.
        /// </exception>
        public RuleIRGroup Parse(RuleParseContext context)
        {
            if (context.RuleGroupTypes.Count == 0)
                throw new ArgumentException("RuleParseContext has no rule group types to parse.", nameof(context));

            // MVP parses exactly one group per call, matching RulePipeline's single RuleIRGroup
            // return type — RuleParseContext.FromAssembly may collect several groups from one
            // assembly, but combining them into one RuleIRGroup would silently merge distinct
            // GroupNames. One group per pipeline run keeps that explicit at the call site.
            if (context.RuleGroupTypes.Count > 1)
                throw new ArgumentException(
                    $"RuleParseContext has {context.RuleGroupTypes.Count} rule group types, but Parse() " +
                    "produces one RuleIRGroup per call. Run the pipeline once per group.", nameof(context));

            var groupType = context.RuleGroupTypes[0];
            var groupAttr = groupType.GetCustomAttribute<RuleGroupAttribute>()
                ?? throw new ArgumentException($"'{groupType.FullName}' is missing [RuleGroup].", nameof(context));

            var group = new RuleIRGroup(groupAttr.Name);
            var classLevelStandingRules = groupType
                .GetCustomAttributes<StandingRuleAttribute>()
                .Select(a => a.NotType)
                .ToList();

            var templateProperties = groupType.GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.IsDefined(typeof(RuleTemplateAttribute)));

            foreach (var property in templateProperties)
            {
                if (property.GetValue(null) is not RulePrototype prototype)
                    throw new InvalidOperationException(
                        $"'{groupType.FullName}.{property.Name}' is marked [RuleTemplate] but did not return a RulePrototype.");

                var standingRules = classLevelStandingRules
                    .Concat(property.GetCustomAttributes<StandingRuleAttribute>().Select(a => a.NotType))
                    .Distinct()
                    .ToList();

                group.Rules.Add(new RuleIR
                {
                    Name = prototype.Name,
                    Description = prototype.Description,
                    StandingRules = standingRules,
                    RequiredUsings = new List<string>(prototype.RequiredUsings),
                    PreambleCode = string.Join(Environment.NewLine, prototype.PreambleStatements),
                    ConditionCode = JoinFluentStatements(prototype.WhenStatements),
                    ActionCode = JoinFluentStatements(prototype.ThenStatements),
                });
            }

            return group;
        }

        /// <summary>
        /// Joins DSL lines into a fluent chain following When()/Then() — each statement prefixed
        /// with '.' and a newline, per D1 in rule-pipeline.md.
        /// </summary>
        private static string JoinFluentStatements(List<string> statements) =>
            string.Concat(statements.Select(s => Environment.NewLine + "." + s));
    }
}
