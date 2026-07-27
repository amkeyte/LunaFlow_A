using LunaFlow.Engine;
using LunaFlow.Rom.D20SystemInterop;

namespace LunaFlow.Rom.D20System
{
    /// <summary>Handles the <c>flee</c> command: escapes combat via IPlayerMovement capability.</summary>
    public sealed class FleeCommandHandler : ICommandHandler
    {
        private readonly IPlayerMovement _movement;

        public FleeCommandHandler(IPlayerMovement movement)
        {
            _movement = movement;
        }

        public void Handle(ICommand command)
        {
            var rules = LunaFlowEngine.Host.Rules;
            var combat = rules.Query<CombatRound>().FirstOrDefault();
            if (combat is null)
            {
                LunaFlowEngine.Host.Announce("Not in combat.");
                return;
            }

            if (_movement.ReturnToPreviousRoom())
            {
                rules.Retract(combat);
                LunaFlowEngine.Host.Announce("Fled to previous room.");
            }
            else
            {
                LunaFlowEngine.Host.Announce("Can't flee (no previous room).");
            }
        }
    }
}
