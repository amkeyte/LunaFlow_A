using LunaFlow.Engine;

namespace LunaFlow.Rom.D20System
{
    /// <summary>
    /// Handles the <c>move</c> command: moves the player one step in a cardinal direction if the
    /// grid allows it. Same adjacency logic as Nine Rooms' <c>MoveCommandHandler</c>, plus tracking
    /// <see cref="PreviousRoomId"/> on every successful move (needed by <c>flee</c>).
    /// </summary>
    public sealed class MoveCommandHandler : ICommandHandler
    {
        private readonly IReadOnlyDictionary<int, Room> _rooms;

        /// <summary>Uses <paramref name="rooms"/> if given, else the configured <see cref="D20SystemContent.Rooms"/> — the current Rulebook's room set.</summary>
        public MoveCommandHandler(IReadOnlyDictionary<int, Room>? rooms = null)
        {
            _rooms = rooms ?? D20SystemContent.Rooms;
        }

        /// <summary>Moves the player per <paramref name="command"/>'s direction argument, or announces the move is invalid.</summary>
        public void Handle(ICommand command)
        {
            var rules = LunaFlowEngine.Host.Rules;
            var direction = command.Arguments.FirstOrDefault()?.ToString() ?? "";
            var current = rules.Query<PlayerPosition>().First();

            if (!D20SystemContent.TryMove(current.RoomId, direction, out var toId))
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

            LunaFlowEngine.Host.Announce(D20SystemContent.FlavorText[_rooms[toId].DescriptionKey]);
        }
    }
}
