namespace LunaFlow.Cartridge
{
    /// <summary>
    /// Cross-ROM fact: which item (by id) the PC currently has equipped as their weapon, if any —
    /// absent means the PC fights with the default starting sword. Written by Loot's <c>equip</c>
    /// command, read by D20System's <c>fight</c> handler to pick the PC-attack step's mechanical
    /// profile. A plain shared record directly in the Cartridge contract surface rather than an
    /// interface-plus-per-owner-concrete-type pair (the pattern <c>IChallengeSolved</c>/
    /// <c>ICombatStatus</c>/etc. use): there is exactly one writer and one shape here, so the
    /// indirection those contracts need to keep a multi-provider capability declarable buys nothing.
    /// Both D20System and Loot already reference this assembly, so sharing the concrete type needs no
    /// new <c>Capability</c>/<c>ComponentRegistry</c> declaration — ordinary NRules fact matching on a
    /// type in the shared interop assembly, per <c>engine-core.md</c>'s "Cross-ROM correlation" note.
    /// At most one at a time — equipping retracts the old, inserts the new, same convention as
    /// <c>PlayerPosition</c>.
    /// </summary>
    public sealed record EquippedWeapon(string ItemId);
}
