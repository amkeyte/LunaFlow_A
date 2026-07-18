using LunaFlow.Engine;

namespace LunaFlow.Rom.NineRooms
{
    public sealed class AnswerCommandHandler : ICommandHandler
    {
        public void Handle(ICommand command)
        {
            var answer = string.Join(' ', command.Arguments).Trim();
            var current = LunaFlowEngine.Host.Rules.Query<PlayerPosition>().First();
            var room = NineRoomsContent.Rooms[current.RoomId];

            if (LunaFlowEngine.Host.Rules.Query<RoomSolved>().Any(rs => rs.RoomId == room.Id))
            {
                LunaFlowEngine.Host.Announce("Already solved.");
                return;
            }

            if (string.Equals(answer, room.Answer, StringComparison.OrdinalIgnoreCase))
            {
                LunaFlowEngine.Host.Rules.Insert(new RoomSolved(room.Id));
                LunaFlowEngine.Host.Announce("Solved.");
            }
            else
            {
                LunaFlowEngine.Host.Announce(room.ChallengePrompt!);
            }
        }
    }
}
