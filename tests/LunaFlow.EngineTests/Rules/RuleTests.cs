using LunaFlow.EngineTests.Mocks;
using NRules;
using NRules.Fluent;
using NRules.Fluent.Dsl;

namespace LunaFlow.EngineTests.Rules
{
    [TestClass]
    public class RuleTests
    {
        [TestMethod]
        public void SessionFiresExpectedRules()
        {
            // Compile test rule set
            var assemblyUnderTest = typeof(SumGreaterThanTenRule).Assembly;
            var repository = new RuleRepository();
            repository.Load(x => x.From(assemblyUnderTest)
                                  .Where(t => typeof(Rule).IsAssignableFrom(t.RuleType)));

            ISessionFactory factory = repository.Compile();
            ISession session = factory.CreateSession();

            // Insert facts
            session.Insert(new NumberPair(6, 7)); // Product = 42 → matches ProductIsFortyTwoRule
            session.Insert(new NumberPair(4, 9)); // Sum = 13 → matches SumGreaterThanTenRule
            session.Insert(new NumberPair(1, 1)); // No match

            session.Fire();

            var output = RuleResults.Dump();

            // Assertions (based on known Console.WriteLine statements from rules)
            StringAssert.Contains(output, "[Rule: Product == 42] Matched:");
            StringAssert.Contains(output, "[Rule: Sum > 10] Matched:");

            Console.Write(output);
        }
        [TestMethod]
        public void Test2()
        {
            // Compile test rule set
            var assemblyUnderTest = typeof(SumGreaterThanTenRule).Assembly;
            var repository = new RuleRepository();
            repository.Load(x => x.From(assemblyUnderTest)
                                  .Where(t => typeof(Rule).IsAssignableFrom(t.RuleType)));

            ISessionFactory factory = repository.Compile();
            ISession session = factory.CreateSession();

            // Insert facts
            session.Insert(new MatchCounter());
            session.Insert(new NumberPair(6, 7));  // sum = 13
            session.Insert(new NumberPair(8, 5));  // sum = 13
            session.Insert(new NumberPair(10, 1)); // sum = 11
            session.Insert(new NumberPair(2, 9));  // sum = 11
            session.Insert(new NumberPair(4, 8));  // sum = 12
            session.Insert(new NumberPair(6, 6));  // sum = 12 → should push over threshold

            session.Fire();

            var output = RuleResults.Dump();
            StringAssert.Contains(output, "Winner Winner Chicken Dinner");

            Console.Write(output);
        }
    }
}
