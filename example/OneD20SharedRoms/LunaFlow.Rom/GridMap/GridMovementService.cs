using LunaFlow.Engine;
using LunaFlow.Cartridge;

namespace LunaFlow.Rom.GridMap
{
    /// <summary>
    /// Adapter implementing IPlayerMovement: allows D20System to request movement actions
    /// on facts that GridMap owns. Called by FleeCommandHandler to return to previous room.
    /// </summary>
    public sealed class GridMovementService : IPlayerMovement
    {
        /// <summary>
        /// Moves the player back to PreviousRoomId. Returns false (no-op) if none
        /// exists yet — the room-5-pre-any-move edge case.
        /// </summary>
        public bool ReturnToPreviousRoom()
        {
            var rules = LunaFlowEngine.Host.Rules;
            var previous = rules.Query<PreviousRoomId>().FirstOrDefault();

            if (previous is null)
                return false;

            var current = rules.Query<PlayerPosition>().First();
            rules.Retract(current);
            rules.Insert(new PlayerPosition(previous.RoomId));

            return true;
        }
    }
}
