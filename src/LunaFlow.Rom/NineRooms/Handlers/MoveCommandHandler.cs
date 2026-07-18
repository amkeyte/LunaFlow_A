using LunaFlow.Engine;

namespace LunaFlow.Rom.NineRooms
{
    public sealed class MoveCommandHandler : ICommandHandler
    {
        public void Handle(ICommand command)
        {
            var direction = command.Arguments.FirstOrDefault()?.ToString() ?? "";
            var current = LunaFlowEngine.Host.Rules.Query<PlayerPosition>().First();

            if (!NineRoomsContent.TryMove(current.RoomId, direction, out var toId))
            {
                LunaFlowEngine.Host.Announce($"You can't go {direction} from here.");
                return;
            }

            LunaFlowEngine.Host.Rules.Retract(current);
            LunaFlowEngine.Host.Rules.Insert(new PlayerPosition(toId));

            // Announce the destination room on entry — same text `look` shows, and
            // unconditional regardless of solved state. Runs before Fire() (centralized
            // in DispatchGameCommand, per nine-rooms-spec.md item 2), so an unsolved
            // room's description prints first and the challenge-prompt rule's own
            // Announce follows it; a solved room just gets the description, since that
            // rule's Not<RoomSolved> guard keeps it from firing at all. Resolves
            // ClaudeWiki ticket 0001.
            LunaFlowEngine.Host.Announce(NineRoomsContent.Rooms[toId].Description);
        }
    }
}
