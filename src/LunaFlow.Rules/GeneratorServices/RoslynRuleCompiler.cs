using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace LunaFlow.Rules.GeneratorServices
{
    /// <summary>
    /// In-memory Roslyn compiler per the D2 decision in rule-pipeline.md: assembles a
    /// <see cref="RuleCodeUnit"/>'s usings/namespace/classes/registry into one source file,
    /// compiles it against the currently loaded assemblies, and loads the result directly from
    /// memory — no path on disk, so a generated rule can load and fire in the same run.
    /// </summary>
    public sealed class RoslynRuleCompiler : IRuleCompiler
    {
        public Assembly Compile(RuleCodeUnit codeUnit)
        {
            var source = BuildSource(codeUnit);
            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            var compilation = CSharpCompilation.Create(
                assemblyName: codeUnit.FileName,
                syntaxTrees: new[] { syntaxTree },
                references: GetReferences(),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var stream = new MemoryStream();
            var result = compilation.Emit(stream);

            if (!result.Success)
            {
                var diagnostics = string.Join(
                    Environment.NewLine,
                    result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
                throw new InvalidOperationException(
                    $"Failed to compile generated rule unit '{codeUnit.FileName}':{Environment.NewLine}{diagnostics}" +
                    $"{Environment.NewLine}{Environment.NewLine}--- Generated source ---{Environment.NewLine}{source}");
            }

            stream.Seek(0, SeekOrigin.Begin);
            return Assembly.Load(stream.ToArray());
        }

        private static string BuildSource(RuleCodeUnit codeUnit)
        {
            var classesSource = string.Join(Environment.NewLine + Environment.NewLine, codeUnit.Classes.Select(c => c.SourceCode));
            var registryClassName = codeUnit.Namespace.Split('.').Last() + "_Generated";

            return $$"""
                {{string.Join(Environment.NewLine, codeUnit.RequiredUsings.Distinct().Select(u => $"using {u};"))}}

                namespace {{codeUnit.Namespace}}
                {
                {{classesSource}}

                    public static class {{registryClassName}}
                    {
                        {{codeUnit.RegistryMethodBody}}
                    }
                }
                """;
        }

        /// <summary>
        /// References every currently loaded, file-backed assembly — the MVP's pragmatic
        /// stand-in for real reference tracking (out of scope: rule-pipeline.md doesn't specify
        /// how a generated rule's domain-fact types get resolved). Covers LunaFlow.Rules,
        /// NRules.Fluent, the .NET runtime, and whichever assembly is calling the pipeline (so its
        /// fact/marker types are always in scope) without needing the caller to pass reference
        /// assemblies through RuleCodeUnit, which has no slot for them today.
        /// </summary>
        private static List<MetadataReference> GetReferences() =>
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => (MetadataReference)MetadataReference.CreateFromFile(a.Location))
                .ToList();
    }
}
