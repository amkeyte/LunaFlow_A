namespace LunaFlow.Rules
{
    /// <summary>
    /// Top level organizational unit of the LunaFlow Rules system. Distinguishes Rulebook content
    /// from ROM content in the type system, per Phase 2.6's boundary — a Rulebook declares which
    /// ROM(s) it depends on but never references <c>LunaFlow.Engine</c> itself.
    /// </summary>
    /// <remarks>
    /// Smallest shape that does the job, mirroring <see cref="IMechanic"/>'s ticket-0015 treatment.
    /// <see cref="DependsOnRoms"/> is static data only — no dependency-ordering machinery; that
    /// stays deferred per <c>non-goals.md</c> until a second real ROM composition needs it
    /// (Phase 2.7).
    /// </remarks>
    public interface IRuleBook : IHasLunaFlowReference
    {
        /// <summary>The name(s) of the ROM(s) this Rulebook depends on.</summary>
        IReadOnlyList<string> DependsOnRoms { get; }
    }
}
