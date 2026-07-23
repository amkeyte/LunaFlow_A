namespace LunaFlow.Engine.Components
{
    /// <summary>
    /// Tags a component (ROM, Rulebook, World Module, GM Overlay, or Player) with its tier
    /// in the architecture's layered stack. Used to enforce tier-ordering rules:
    /// a component may depend on lower tiers but not higher ones.
    /// </summary>
    public enum EngineTier
    {
        /// <summary>The ROM (Rule Object Model) tier: reusable, domain-agnostic game-system toolboxes.</summary>
        Rom = 0,

        /// <summary>The Rulebook tier: a specific game's rules built on one or more ROMs.</summary>
        Rulebook = 1,

        /// <summary>The World Module tier: authored-content regions, encounters, and world structure.</summary>
        WorldModule = 2,

        /// <summary>The GM Overlay tier: live, session-scoped rules and world-state edits.</summary>
        GmOverlay = 3,

        /// <summary>The Player tier: cosmetic, sandboxed player-contributed content.</summary>
        Player = 4,
    }
}
