namespace LunaFlow.Rules.GeneratorServices
{
    /// <summary>
    /// Coordinates the full rule generation pipeline, from parsing to code generation to output.
    /// </summary>
    public sealed class RulePipeline
    {
        private readonly IRuleParser _parser;
        private readonly IRuleIRGenerator _generator;
        private readonly IRuleCompiler _compiler;

        /// <summary>
        /// Initializes a new instance of the <see cref="RulePipeline"/> class.
        /// </summary>
        /// <param name="parser">The parser to extract rule definitions.</param>
        /// <param name="generator">The IR generator to transform parsed rules.</param>
        /// <param name="compiler">The compiler to emit or build rule output.</param>
        public RulePipeline(IRuleParser parser, IRuleIRGenerator generator, IRuleCompiler compiler)
        {
            _parser = parser;
            _generator = generator;
            _compiler = compiler;
        }

        /// <summary>
        /// Executes the pipeline on the given context, performing parse, generation, and compilation steps.
        /// </summary>
        /// <param name="context">The context describing which rule groups to process.</param>
        public void Compile(RuleParseContext context)
        {
            var ir = _parser.Parse(context);
            var unit = _generator.Generate(ir);
            _compiler.Compile(unit);
        }
    }
}

