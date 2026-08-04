namespace LunaFlow.Cartridge
{
    /// <summary>
    /// Contract interface for cross-ROM fact: which item (by id) the PC currently has equipped as
    /// their weapon, if any — absent means the PC fights with the default starting sword. Written by
    /// Loot's <c>equip</c> command, matched by D20System's <c>fight</c> handler to pick the PC-attack
    /// step's mechanical profile. Fact-shaped (observed via <c>Match&lt;T&gt;</c>/<c>Query&lt;T&gt;()</c>),
    /// same family as <see cref="IChallengeSolved"/>/<see cref="ICombatStatus"/> — not service-shaped
    /// like <see cref="IPlayerMovement"/>, since nothing needs Loot to perform an action on D20System's
    /// behalf here. D20System deliberately does not declare a <c>Capability.Requires</c> entry for this
    /// contract — see <c>D20SystemRom.Requires</c>'s doc comment and ticket 0071's general ruling.
    /// </summary>
    public interface IEquippedWeapon
    {
        /// <summary>The equipped item's id.</summary>
        string ItemId { get; }
    }
}
