using LunaFlow.Engine;

namespace LunaFlow.Rom.OneD20
{
    /// <summary>
    /// Handles the <c>check</c> command: resolves the current skill-check room's obstacle via
    /// <see cref="D20ResolutionMechanic.ResolveCheck"/>, announcing the room's own success/failure
    /// text plus a mechanical roll/DC readout. No-ops silently outside a skill-check room or once
    /// the room is already solved (1-d20-spec.md's now-formalized behavior for that case).
    /// </summary>
    public sealed class CheckCommandHandler : ICommandHandler
    {
        private readonly D20ResolutionMechanic _mechanic;

        /// <summary>Uses <paramref name="mechanic"/> if given, else the shared <see cref="OneD20Content.Mechanic"/> — the seam ticket 0019 asked for.</summary>
        public CheckCommandHandler(D20ResolutionMechanic? mechanic = null)
        {
            _mechanic = mechanic ?? OneD20Content.Mechanic;
        }

        /// <inheritdoc/>
        public void Handle(ICommand command)
        {
            var rules = LunaFlowEngine.Host.Rules;
            var current = rules.Query<PlayerPosition>().First();
            var room = OneD20Content.Rooms[current.RoomId];

            if (room.ChallengeType != RoomChallengeType.SkillCheck)
                return;
            if (rules.Query<RoomSolved>().Any(rs => rs.RoomId == room.Id))
                return;

            var abilityMod = OneD20Content.AbilityModifier(OneD20Content.PlayerAbilityScores[room.SkillAbility!]);
            var result = _mechanic.ResolveCheck(abilityMod, room.DC!.Value);

            if (result.Success)
            {
                rules.Insert(new RoomSolved(room.Id));
                LunaFlowEngine.Host.Announce(OneD20Flavor.CheckNarration(room.SuccessKey!, room.Skill!, result.Total, room.DC.Value));
            }
            else
            {
                LunaFlowEngine.Host.Announce(OneD20Flavor.CheckNarration(room.FailureKey!, room.Skill!, result.Total, room.DC.Value));
            }
        }
    }
}
