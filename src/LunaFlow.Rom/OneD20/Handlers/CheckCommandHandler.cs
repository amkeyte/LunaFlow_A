using LunaFlow.Engine;

namespace LunaFlow.Rom.OneD20
{
    /// <summary>
    /// Handles the <c>check</c> command: resolves the current skill-check room's obstacle via
    /// <see cref="D20ResolutionMechanic.ResolveCheck"/>. No-ops silently outside a skill-check
    /// room or once the room is already solved — no player-facing behavior was specified for
    /// that case (1-d20-spec.md's open questions).
    /// </summary>
    public sealed class CheckCommandHandler : ICommandHandler
    {
        /// <inheritdoc/>
        public void Handle(ICommand command)
        {
            var rules = LunaFlowEngine.Host.Rules;
            if (rules.Query<PlayerDown>().Any())
                return;

            var current = rules.Query<PlayerPosition>().First();
            var room = OneD20Content.Rooms[current.RoomId];

            if (room.ChallengeType != RoomChallengeType.SkillCheck)
                return;
            if (rules.Query<RoomSolved>().Any(rs => rs.RoomId == room.Id))
                return;

            var abilityMod = OneD20Content.AbilityModifier(OneD20Content.PlayerAbilityScores[room.SkillAbility!]);
            var success = OneD20Content.Mechanic.ResolveCheck(abilityMod, room.DC!.Value);

            if (success)
            {
                rules.Insert(new RoomSolved(room.Id));
                LunaFlowEngine.Host.Announce(OneD20Flavor.Text["check-success"]);
            }
            else
            {
                LunaFlowEngine.Host.Announce(OneD20Flavor.Text["check-failure"]);
            }
        }
    }
}
