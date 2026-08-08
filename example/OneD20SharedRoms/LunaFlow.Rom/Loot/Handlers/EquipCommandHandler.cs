using LunaFlow.Engine;
using LunaFlow.Cartridge;

namespace LunaFlow.Rom.Loot
{
    /// <summary>
    /// Handles the <c>equip</c> command: swaps the PC's active weapon between the default sword and
    /// an equippable held item's mechanical profile. Loot-owned — reads/writes only <c>ItemAwarded</c>
    /// (its own fact) and <c>EquippedWeapon</c> (the shared Cartridge fact D20System's <c>fight</c>
    /// also reads); no cross-ROM reference of any kind.
    /// </summary>
    public sealed class EquipCommandHandler : ICommandHandler
    {
        /// <summary>The argument that re-selects the PC's default starting sword, retracting
        /// <c>EquippedWeapon</c> rather than inserting one — the spec's default assumption for
        /// reversibility, not stated explicitly there so recorded here instead.</summary>
        private const string DefaultWeaponName = "sword";

        /// <summary>Fixed Loot-tier data: which item ids are flagged equippable. Not World-Module-
        /// authored, same reasoning as D20System's weapon-profile table this pairs with.</summary>
        private static readonly IReadOnlySet<string> DefaultEquippableItemIds = new HashSet<string> { "pistol" };

        private readonly IReadOnlyDictionary<string, string> _flavorText;
        private readonly IReadOnlySet<string> _equippableItemIds;

        /// <summary>Constructs the handler with the Rulebook's flavor text and, optionally, a
        /// non-default equippable-item set (defaults to the shipped Backup Piece-only set).</summary>
        public EquipCommandHandler(IReadOnlyDictionary<string, string> flavorText, IReadOnlySet<string>? equippableItemIds = null)
        {
            _flavorText = flavorText;
            _equippableItemIds = equippableItemIds ?? DefaultEquippableItemIds;
        }

        /// <summary>
        /// Equips the named item if the PC holds it (an <c>ItemAwarded</c> exists for it) and it's
        /// flagged equippable; re-selects the default sword for <c>equip sword</c>; silently rejects
        /// otherwise.
        /// </summary>
        public void Handle(ICommand command)
        {
            var rules = LunaFlowEngine.Host.Rules;
            var argument = string.Join(' ', command.Arguments).Trim();
            if (argument.Length == 0)
            {
                LunaFlowEngine.Host.Announce("Equip what?");
                return;
            }

            if (string.Equals(argument, DefaultWeaponName, StringComparison.OrdinalIgnoreCase))
            {
                var current = rules.Query<EquippedWeapon>().FirstOrDefault();
                if (current is not null) rules.Retract(current);
                LunaFlowEngine.Host.Announce("You ready your sword.");
                return;
            }

            var held = rules.Query<ItemAwarded>().FirstOrDefault(item =>
                string.Equals(_flavorText[$"item-{item.ItemId}-name"], argument, StringComparison.OrdinalIgnoreCase));
            if (held is null || !_equippableItemIds.Contains(held.ItemId))
            {
                LunaFlowEngine.Host.Announce("You can't equip that.");
                return;
            }

            var existing = rules.Query<EquippedWeapon>().FirstOrDefault();
            if (existing is not null) rules.Retract(existing);
            rules.Insert(new EquippedWeapon(held.ItemId));
            LunaFlowEngine.Host.Announce($"You equip the {_flavorText[$"item-{held.ItemId}-name"]}.");
        }
    }
}
