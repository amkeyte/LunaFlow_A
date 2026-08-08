using LunaFlow.Engine;

namespace LunaFlow.Rom.Loot
{
    /// <summary>Handles the <c>inventory</c>/<c>inv</c> command: lists every awarded item.</summary>
    public sealed class InventoryCommandHandler : ICommandHandler
    {
        private readonly IReadOnlyDictionary<string, string> _flavorText;

        /// <summary>Constructs the handler with the Rulebook's flavor text.</summary>
        public InventoryCommandHandler(IReadOnlyDictionary<string, string> flavorText)
        {
            _flavorText = flavorText;
        }

        /// <summary>Lists every ItemAwarded and BonusItemAwarded fact by name — Loot's own facts only, no cross-ROM read.</summary>
        public void Handle(ICommand command)
        {
            var items = LunaFlowEngine.Host.Rules.Query<ItemAwarded>().ToList();
            var bonusItems = LunaFlowEngine.Host.Rules.Query<BonusItemAwarded>().ToList();
            if (items.Count == 0 && bonusItems.Count == 0)
            {
                LunaFlowEngine.Host.Announce(_flavorText["inventory-empty"]);
                return;
            }

            LunaFlowEngine.Host.Announce(_flavorText["inventory-header"]);
            foreach (var item in items)
            {
                LunaFlowEngine.Host.Announce(_flavorText[$"item-{item.ItemId}-name"]);
            }
            foreach (var bonus in bonusItems)
            {
                LunaFlowEngine.Host.Announce(_flavorText[$"item-{bonus.ItemId}-name"]);
            }
        }
    }
}
