using LunaFlow.Engine;

namespace LunaFlow.Rom.NineRooms
{
    public sealed class LookCommandHandler : ICommandHandler
    {
        public void Handle(ICommand command)
        {
            var current = LunaFlowEngine.Host.Rules.Query<PlayerPosition>().First();
            var room = NineRoomsContent.Rooms[current.RoomId];
            LunaFlowEngine.Host.Announce(room.Description);
        }
    }
}
