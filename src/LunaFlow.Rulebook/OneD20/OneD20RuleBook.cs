using LunaFlow.Engine.Components;
using LunaFlow.Rules;

namespace LunaFlow.Rulebook.OneD20
{
    /// <summary>
    /// The 1.d20 Rulebook's <see cref="IRuleBook"/> declaration — depends on the <c>GridMap</c>,
    /// <c>D20System</c>, and <c>Loot</c> ROMs (spatial, challenge/combat, and loot/inventory
    /// concerns, respectively). Carries no logic of its own: this project's job is to be pure data
    /// (<see cref="OneD20RulebookContent"/>/<see cref="OneD20FlavorText"/>/
    /// <see cref="OneD20SessionAssembler"/>) plus this declaration, never a reference to
    /// <c>LunaFlow.Engine</c>.
    /// </summary>
    public sealed class OneD20RuleBook : IRuleBook
    {
        /// <inheritdoc/>
        public ILunaFlowReference Reference { get; } = new OneD20RulebookReference("OneD20");

        /// <inheritdoc/>
        public IReadOnlyList<string> DependsOnRoms { get; } = new[] { "GridMap", "D20System", "Loot" };

        /// <inheritdoc/>
        public IReadOnlyList<Capability> Provides { get; } = Array.Empty<Capability>();

        /// <inheritdoc/>
        public IReadOnlyList<Capability> Requires { get; } = Array.Empty<Capability>();
    }
}
