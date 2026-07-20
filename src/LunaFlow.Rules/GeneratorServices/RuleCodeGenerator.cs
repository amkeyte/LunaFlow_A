namespace LunaFlow.Rules.GeneratorServices
{
    /// <summary>
    /// Turns a parsed <see cref="RuleIRGroup"/> into the C# source of one <see cref="RuleCodeUnit"/> —
    /// one <see cref="ARule"/>-derived class per <see cref="RuleIR"/>, plus a group registry class.
    /// Shape follows rule-pipeline.md's "Output conventions" section exactly (namespace per D4,
    /// Reference per D3, .Not&lt;T&gt;() injection per [StandingRule]).
    /// </summary>
    public sealed class RuleCodeGenerator : IRuleIRGenerator
    {
        public RuleCodeUnit Generate(RuleIRGroup ir)
        {
            var unit = new RuleCodeUnit
            {
                FileName = $"{ir.GroupName}.g.cs",
                Namespace = $"LunaFlow.Generated.{ir.GroupName}",
            };

            foreach (var rule in ir.Rules)
                unit.Classes.Add(GenerateClass(ir.GroupName, rule));

            unit.RegistryMethodBody = GenerateRegistryBody(unit.Classes);

            // Union/dedup every rule's RequiredUsings, plus what the generated shape itself
            // always needs regardless of what any individual rule declared.
            var baseline = new[] { "System", "System.Collections.Generic", "NRules.Fluent.Dsl", "LunaFlow.Rules", "LunaFlow.Rules.GeneratorServices" };
            unit.RequiredUsings = baseline
                .Concat(ir.Rules.SelectMany(r => r.RequiredUsings))
                .Distinct()
                .ToList();

            return unit;
        }

        private static RuleCodeClass GenerateClass(string groupName, RuleIR rule)
        {
            var className = $"{rule.Name}Rule";
            var notGuards = string.Concat(rule.StandingRules.Select(t => $".Not<{t.FullName ?? t.Name}>()"));

            var source = $$"""
                public class {{className}} : ARule
                {
                    public override ILunaFlowReference Reference { get; } =
                        new GeneratedRuleReference("{{groupName}}", "{{rule.Name}}");

                    public override void Define()
                    {
                        {{Indent(rule.PreambleCode, 1)}}

                        When(){{Indent(rule.ConditionCode, 2)}}{{notGuards}};

                        Then(){{Indent(rule.ActionCode, 2)}};
                    }
                }
                """;

            return new RuleCodeClass { ClassName = className, SourceCode = source };
        }

        private static string GenerateRegistryBody(List<RuleCodeClass> classes)
        {
            var typeList = string.Join(", ", classes.Select(c => $"typeof({c.ClassName})"));
            return $"public static IEnumerable<Type> GetRules() => new Type[] {{ {typeList} }};";
        }

        /// <summary>Re-indents a multi-line opaque code block emitted verbatim from the IR.</summary>
        private static string Indent(string code, int levels)
        {
            if (string.IsNullOrEmpty(code)) return string.Empty;
            var pad = new string(' ', levels * 4);
            var lines = code.Replace("\r\n", "\n").Split('\n');
            return string.Join(Environment.NewLine, lines.Select(l => l.Length == 0 ? l : pad + l));
        }
    }
}
