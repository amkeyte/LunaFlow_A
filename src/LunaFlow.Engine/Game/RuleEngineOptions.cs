using System.Reflection;

namespace LunaFlow.Engine.Game
{
    /// <summary>
    /// Configures which rule types a <see cref="RuleEngine"/> loads and which facts it seeds
    /// at startup, before any command is ever dispatched.
    /// </summary>
    public sealed class RuleEngineOptions
    {
        private readonly List<Assembly> _assemblies = new();
        private readonly List<Type> _types = new();
        private readonly List<Func<IEnumerable<object>>> _seedProviders = new();

        public RuleEngineOptions FromAssembly(Assembly assembly)
        {
            _assemblies.Add(assembly);
            return this;
        }

        public RuleEngineOptions FromTypes(params Type[] types)
        {
            _types.AddRange(types);
            return this;
        }

        public RuleEngineOptions Seed(Func<IEnumerable<object>> factsProvider)
        {
            _seedProviders.Add(factsProvider);
            return this;
        }

        internal void ApplyTo(RuleEngine engine)
        {
            foreach (var assembly in _assemblies)
                engine.LoadRulesFromAssembly(assembly);
            if (_types.Count > 0)
                engine.LoadRulesFromTypes(_types);
            foreach (var provider in _seedProviders)
                foreach (var fact in provider())
                    engine.Insert(fact);
        }
    }
}
