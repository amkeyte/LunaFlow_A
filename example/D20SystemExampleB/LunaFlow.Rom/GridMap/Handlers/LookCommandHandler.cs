using LunaFlow.Engine;
using LunaFlow.Cartridge;

namespace LunaFlow.Rom.GridMap.Handlers
{
    /// <summary>Displays the current room's description and any relevant combat status.</summary>
    public sealed class LookCommandHandler : ICommandHandler
    {
        private readonly GridMapRom _gridMap;
        private readonly IReadOnlyDictionary<string, string> _flavorText;
        private readonly int _playerMaxHp;
        private readonly int _monsterMaxHp;

        /// <summary>
        /// Constructs the handler with the GridMap ROM (room data), the Rulebook's flavor text, and
        /// the combatants' max HP — static Scenario data, constructor-injected rather than read from
        /// D20System's concrete types, which GridMap must never reference.
        /// </summary>
        public LookCommandHandler(GridMapRom gridMap, IReadOnlyDictionary<string, string> flavorText, int playerMaxHp, int monsterMaxHp)
        {
            _gridMap = gridMap;
            _flavorText = flavorText;
            _playerMaxHp = playerMaxHp;
            _monsterMaxHp = monsterMaxHp;
        }

        /// <summary>Redisplay the current room's description and combat status if active.</summary>
        public void Handle(ICommand command)
        {
            var rules = LunaFlowEngine.Host.Rules;
            var current = rules.Query<IPlayerLocation>().FirstOrDefault();
            if (current is null)
            {
                LunaFlowEngine.Host.Announce("You are nowhere.");
                return;
            }

            var room = _gridMap.RoomData.FirstOrDefault(r => r.Id == current.RoomId);
            if (room is not null)
            {
                LunaFlowEngine.Host.Announce(_flavorText[room.DescriptionKey]);
            }

            var combatStatus = rules.Query<ICombatStatus>()
                .FirstOrDefault(c => c.RoomId == current.RoomId);

            if (combatStatus is not null)
            {
                LunaFlowEngine.Host.Announce(
                    $"You're at {combatStatus.PlayerHp}/{_playerMaxHp} HP. He's at {combatStatus.MonsterHp}/{_monsterMaxHp} HP.");
            }
        }
    }
}
