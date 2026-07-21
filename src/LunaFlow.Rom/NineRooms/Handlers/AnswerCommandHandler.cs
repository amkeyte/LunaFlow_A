using LunaFlow.Engine;

namespace LunaFlow.Rom.NineRooms
{
    /// <summary>
    /// Deprecated — see <see cref="NineRoomsContent"/>'s Remarks. Handles the <c>answer</c>
    /// command: checks the player's answer against the current room's challenge and marks the
    /// room solved if it matches.
    /// </summary>
    public sealed class AnswerCommandHandler : ICommandHandler
    {
        /// <summary>Compares <paramref name="command"/>'s arguments (joined) against the current room's answer.</summary>
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
