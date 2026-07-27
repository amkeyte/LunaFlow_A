using LunaFlow.Engine;

namespace LunaFlow.Rom.GridMap
{
    /// <summary>
    /// Handles the <c>move</c> command: moves the player one step in a cardinal direction if the
    /// grid allows it. Tracks PreviousRoomId on every successful move (needed by <c>flee</c>).
    /// </summary>
    public sealed class MoveCommandHandler : ICommandHandler
    {
        private readonly GridMapRom _gridMap;

        /// <summary>Constructs the handler with the GridMap ROM for room data access.</summary>
        public MoveCommandHandler(GridMapRom gridMap)
        {
            _gridMap = gridMap;
        }

        /// <summary>Moves the player per <paramref name="command"/>'s direction argument, or announces the move is invalid.</summary>
        public void Handle(ICommand command)
        {
            var rules = LunaFlowEngine.Host.Rules;
            var direction = command.Arguments.FirstOrDefault()?.ToString() ?? "";
            var current = rules.Query<PlayerPosition>().First();

            if (!TryMove(current.RoomId, direction, out var toId))
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

            var room = _gridMap.RoomData.FirstOrDefault(r => r.Id == toId);
            if (room is not null)
            {
                LunaFlowEngine.Host.Announce("TODO: flavor text lookup");
            }
        }

        /// <summary>
        /// Resolves moving from <paramref name="fromId"/> in <paramref name="direction"/> ("north"/"south"/"east"/"west").
        /// Pure grid adjacency math — identical shape to Nine Rooms' grid.
        /// </summary>
        private static bool TryMove(int fromId, string direction, out int toId)
        {
            switch (direction.ToLowerInvariant())
            {
                case "north":
                    toId = fromId - 3;
                    return toId >= 1;
                case "south":
                    toId = fromId + 3;
                    return toId <= 9;
                case "east":
                    toId = fromId + 1;
                    return (fromId - 1) % 3 != 2;
                case "west":
                    toId = fromId - 1;
                    return (fromId - 1) % 3 != 0;
                default:
                    toId = -1;
                    return false;
            }
        }
    }
}
