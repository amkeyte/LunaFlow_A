using LunaFlow.Engine;

namespace LunaFlow.Rom.NineRooms
{
    /// <summary>Handles the <c>look</c> command by announcing the current room's description.</summary>
    public sealed class LookCommandHandler : ICommandHandler
    {
        /// <summary>Announces the description of the room the player currently occupies.</summary>
        public void Handle(ICommand command)
        {
            var current = LunaFlowEngine.Host.Rules.Query<PlayerPosition>().First();
            var room = NineRoomsContent.Rooms[current.RoomId];
            LunaFlowEngine.Host.Announce(room.Description);
        }
    }
}
