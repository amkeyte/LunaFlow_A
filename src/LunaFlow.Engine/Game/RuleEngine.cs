using System.Reflection;
using NRules;
using NRules.Fluent;
using NRules.Fluent.Dsl;

namespace LunaFlow.Engine
{
    /// <summary>
    /// Provides a simplified wrapper around the NRules engine for dynamic rule loading and execution.
    /// </summary>
    public class RuleEngine
    {
        private RuleRepository _repository = new();
        private ISessionFactory _factory;
        private ISession _session;

        /// <summary>
        /// Loads rules from the specified <see cref="Assembly"/>. All types that derive from <see cref="Rule"/> are included.
        /// </summary>
        /// <param name="assembly">The assembly containing rule types.</param>
        public void LoadRulesFromAssembly(Assembly assembly)
        {
            _repository.Load(x => x.From(assembly)
                                   .Where(t => typeof(Rule).IsAssignableFrom(t.RuleType)));
            Compile();
        }

        /// <summary>
        /// Loads a single rule type into the engine.
        /// </summary>
        /// <param name="ruleType">The type of the rule to load.</param>
        public void LoadRulesFromType(Type ruleType)
        {
            LoadRulesFromTypes(new[] { ruleType });
        }

        /// <summary>
        /// Loads a set of rule types into the engine.
        /// </summary>
        /// <param name="ruleTypes">The collection of rule types to load.</param>
        public void LoadRulesFromTypes(IEnumerable<Type> ruleTypes)
        {
            _repository.Load(x => x.From(ruleTypes));
            Compile();
        }

        /// <summary>
        /// Inserts a fact into the current rule session.
        /// </summary>
        /// <param name="fact">The fact object to insert.</param>
        /// <exception cref="InvalidOperationException">Thrown if the rule engine has not been initialized.</exception>
        public void Insert(object fact)
        {
            EnsureSessionInitialized();
            _session.Insert(fact);
        }

        /// <summary>
        /// Retracts a fact from the current rule session.
        /// </summary>
        /// <param name="fact">The fact object to retract.</param>
        /// <exception cref="InvalidOperationException">Thrown if the rule engine has not been initialized.</exception>
        public void Retract(object fact)
        {
            EnsureSessionInitialized();
            _session.Retract(fact);
        }

        /// <summary>
        /// Updates a fact in the current rule session.
        /// </summary>
        /// <param name="fact">The fact object to update.</param>
        /// <exception cref="InvalidOperationException">Thrown if the rule engine has not been initialized.</exception>
        public void Update(object fact)
        {
            EnsureSessionInitialized();
            _session.Update(fact);
        }

        /// <summary>
        /// Executes all active rules in the current rule session.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the rule engine has not been initialized.</exception>
        public void Fire()
        {
            EnsureSessionInitialized();
            _session.Fire();
        }
        /// <summary>
        /// Clears the rule repository and disposes the current session.
        /// This can be used to reset the engine before reloading a new set of rules.
        /// </summary>
        public void Clear()
        {
            _repository = new RuleRepository();
            _factory = null;
            _session = null;
        }

        private void EnsureSessionInitialized()
        {
            if (_session == null)
                throw new InvalidOperationException("RuleEngine has not been initialized. Call LoadRulesFromAssembly or LoadRulesFromTypes before using this method.");
        }
        /// <summary>
        /// Compiles the rule repository and creates a new session.
        /// </summary>
        private void Compile()
        {
            _factory = _repository.Compile();
            _session = _factory.CreateSession();
        }
    }
}


