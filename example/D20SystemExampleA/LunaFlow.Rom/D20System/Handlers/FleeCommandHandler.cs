using LunaFlow.Engine;
using LunaFlow.Cartridge;

namespace LunaFlow.Rom.D20System
{
    /// <summary>Handles the <c>flee</c> command: escapes combat via IPlayerMovement capability.</summary>
    public sealed class FleeCommandHandler : ICommandHandler
    {
        private readonly IPlayerMovement _movement;
        private readonly IReadOnlyDictionary<string, string> _flavorText;

        /// <summary>Constructs the handler with the movement capability and the Rulebook's flavor text.</summary>
        public FleeCommandHandler(IPlayerMovement movement, IReadOnlyDictionary<string, string> flavorText)
        {
            _movement = movement;
            _flavorText = flavorText;
        }

        /// <summary>
        /// Retracts the active CombatRound (reversible — no RoomSolved/RoomFailed) and moves the PC
        /// to PreviousRoomId. If none exists yet (room 5's fight, before any move), stays put —
        /// encounter abandoned, same "pc-flee" announcement either way.
        /// </summary>
        public void Handle(ICommand command)
        {
            var rules = LunaFlowEngine.Host.Rules;
            var combat = rules.Query<CombatRound>().FirstOrDefault();
            if (combat is null)
            {
                LunaFlowEngine.Host.Announce("Not in combat.");
                return;
            }

            rules.Retract(combat);
            _movement.ReturnToPreviousRoom();
            LunaFlowEngine.Host.Announce(_flavorText["pc-flee"]);
        }
    }
}
