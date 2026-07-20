using LunaFlow.Engine;

namespace LunaFlow.Rom.OneD20
{
    /// <summary>
    /// Handles the <c>move</c> command: moves the player one step in a cardinal direction if the
    /// grid allows it. Same adjacency logic as Nine Rooms' <c>MoveCommandHandler</c>, plus tracking
    /// <see cref="PreviousRoomId"/> on every successful move (needed by <c>flee</c>).
    /// </summary>
    public sealed class MoveCommandHandler : ICommandHandler
    {
        /// <summary>Moves the player per <paramref name="command"/>'s direction argument, or announces the move is invalid.</summary>
        public void Handle(ICommand command)
        {
            var rules = LunaFlowEngine.Host.Rules;
            var direction = command.Arguments.FirstOrDefault()?.ToString() ?? "";
            var current = rules.Query<PlayerPosition>().First();

            if (!OneD20Content.TryMove(current.RoomId, direction, out var toId))
            {
                LunaFlowEngine.Host.Announce($"You can't go {direction} from here.");
                return;
            }

            rules.Retract(current);
            rules.Insert(new PlayerPosition(toId));

            var previous = rules.Query<PreviousRoomId>().FirstOrDefault();
            if (previous is not null)
                rules.Retract(previous);
            rules.Insert(new PreviousRoomId(current.RoomId));

            LunaFlowEngine.Host.Announce(OneD20Flavor.Text[OneD20Content.Rooms[toId].DescriptionKey]);
        }
    }
}
