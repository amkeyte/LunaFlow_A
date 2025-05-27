using NRules;
using NRules.Fluent;
using NRules.Fluent.Dsl;

namespace LunaFlow.Engine
{
    /// <summary>
    /// Provides a central rule evaluation engine for LunaFlow.
    /// All game actions and consequences are evaluated through this system.
    /// </summary>
    public class RuleEngine
    {
        private readonly ISessionFactory _sessionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleEngine"/> class.
        /// Compiles and loads all game rules.
        /// </summary>
        public RuleEngine()
        {
            var repository = new RuleRepository();

            // Load all rule types from the Rules namespace (can also load by assembly)
            repository.Load(x => x.From(typeof(RuleEngine).Assembly).Where(t => typeof(Rule).IsAssignableFrom(t.RuleType)));//verify RuleType is correct

            _sessionFactory = repository.Compile();
        }

        /// <summary>
        /// Creates and returns a fresh rule evaluation session.
        /// Use this to evaluate state after commands or world changes.
        /// </summary>
        /// <returns>An <see cref="ISession"/> for inserting facts and firing rules.</returns>
        public ISession CreateSession()
        {
            return _sessionFactory.CreateSession();
        }
    }
}
