using LunaFlow.Engine;
using LunaFlow.Rom.D20SystemInterop;

namespace LunaFlow.Rom.GridMap.Handlers
{
    /// <summary>Displays the current room's description and any relevant combat status.</summary>
    public sealed class LookCommandHandler : ICommandHandler
    {
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

            // Try to display combat status if a fight is active in this room
            var combatStatus = rules.Query<ICombatStatus>()
                .Where(c => c.RoomId == current.RoomId)
                .FirstOrDefault();

            if (combatStatus is not null)
            {
                LunaFlowEngine.Host.Announce($"[Room {current.RoomId}] Monster HP: {combatStatus.MonsterHp}, Your HP: {combatStatus.PlayerHp}");
            }
            else
            {
                LunaFlowEngine.Host.Announce($"[Room {current.RoomId}] No active combat.");
            }
        }
    }
}
