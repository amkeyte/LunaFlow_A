using NRules.Fluent.Dsl;

namespace LunaFlow.EngineTests.Rules
{
    /// <summary>
    /// Captures and formats rule output messages during test execution.
    /// </summary>
    internal static class RuleResults
    {
        private static readonly List<string> _results = new();

        /// <summary>
        /// Adds a formatted result message for a rule.
        /// </summary>
        /// <param name="rule">The rule reporting the result.</param>
        /// <param name="message">The message to store.</param>
        /// <param name="callerName">Automatically filled by the calling method name.</param>
        public static void AddResult(this Rule rule, string message)
        {
            var ruleType = rule.GetType().Name;
            var x = $"{ruleType} said {message}.";
            _results.Add(x);
        }

        /// <summary>
        /// Dumps all recorded rule result messages in order.
        /// </summary>
        /// <returns>A newline-separated string of all captured messages.</returns>
        public static string Dump()
        {
            return string.Join(Environment.NewLine, _results);
        }

        /// <summary>
        /// Clears all stored results.
        /// </summary>
        public static void Clear()
        {
            _results.Clear();
        }
    }
}
