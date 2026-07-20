using LunaFlow.Engine;

namespace LunaFlow.Rom.OneD20
{
    /// <summary>
    /// Handles the <c>flee</c> command: auto-success escape from an active <see cref="CombatRound"/>,
    /// no roll. Retracts the round (no <see cref="RoomSolved"/>/<see cref="RoomFailed"/> — reversible)
    /// and returns the PC to <see cref="PreviousRoomId"/>. No-ops silently if no fight is active.
    /// </summary>
    public sealed class FleeCommandHandler : ICommandHandler
    {
        /// <inheritdoc/>
        public void Handle(ICommand command)
        {
            var rules = LunaFlowEngine.Host.Rules;
            var current = rules.Query<PlayerPosition>().First();
            var round = rules.Query<CombatRound>().FirstOrDefault(cr => cr.RoomId == current.RoomId);
            if (round is null)
                return;

            rules.Retract(round);
            LunaFlowEngine.Host.Announce(OneD20Flavor.Text["pc-flee"]);

            // No PreviousRoomId exists if the very first fight (room 5, the start room) is fled
            // before any move has ever happened — nowhere defined to retreat to, so the PC just
            // stays put. Formalized in 1-d20-spec.md's Open questions section (was a disclosed
            // judgment call in ticket 0016).
            var previous = rules.Query<PreviousRoomId>().FirstOrDefault();
            if (previous is null)
                return;

            rules.Retract(current);
            rules.Insert(new PlayerPosition(previous.RoomId));
        }
    }
}
