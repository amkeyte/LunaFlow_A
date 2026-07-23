using LunaFlow.Engine.Components;
using LunaFlow.Rules;

namespace LunaFlow.Rulebook
{
    /// <summary>
    /// Top-level organizational unit of a specific, compiled game built on one or more ROMs.
    /// A Rulebook declares which ROM(s) it depends on and implements the capability-mediation
    /// contract for its dependencies, but never references <c>LunaFlow.Engine</c> primitives directly
    /// (the infrastructure-type participation in <c>IHasDependencies</c> is exempt; that's how the
    /// tier's own identity integrates with the engine's module system).
    /// </summary>
    /// <remarks>
    /// Smallest shape that distinguishes Rulebook-tier content from ROM-tier content.
    /// <see cref="DependsOnRoms"/> names the ROM(s) this Rulebook depends on; that dependency
    /// is static data only, not ordered or arbitrated at runtime — those concerns stay deferred
    /// per <c>non-goals.md</c> until a second real ROM composition needs them (Phase 2.7).
    /// </remarks>
    public interface IRuleBook : IHasLunaFlowReference, IHasDependencies
    {
        /// <summary>The name(s) of the ROM(s) this Rulebook depends on.</summary>
        IReadOnlyList<string> DependsOnRoms { get; }
    }
}
