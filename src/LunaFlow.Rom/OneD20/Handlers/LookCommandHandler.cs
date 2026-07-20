using LunaFlow.Engine;

namespace LunaFlow.Rom.OneD20
{
    /// <summary>Handles the <c>look</c> command by announcing the current room's description.</summary>
    public sealed class LookCommandHandler : ICommandHandler
    {
        /// <summary>Announces the description of the room the player currently occupies.</summary>
        public void Handle(ICommand command)
        {
            var current = LunaFlowEngine.Host.Rules.Query<PlayerPosition>().First();
            var room = OneD20Content.Rooms[current.RoomId];
            LunaFlowEngine.Host.Announce(OneD20Flavor.Text[room.DescriptionKey]);
        }
    }
}
