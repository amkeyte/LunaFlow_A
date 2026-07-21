using LunaFlow.Engine;

namespace LunaFlow.Rom.D20System
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
        private readonly IReadOnlyDictionary<int, Room> _rooms;
        private readonly IReadOnlyDictionary<string, int> _abilityScores;

        /// <summary>Uses the given arguments if supplied, else the configured <see cref="D20SystemContent"/> data — the seam ticket 0019 asked for, widened to cover Scenario data.</summary>
        public CheckCommandHandler(
            D20ResolutionMechanic? mechanic = null,
            IReadOnlyDictionary<int, Room>? rooms = null,
            IReadOnlyDictionary<string, int>? abilityScores = null)
        {
            _mechanic = mechanic ?? D20SystemContent.Mechanic;
            _rooms = rooms ?? D20SystemContent.Rooms;
            _abilityScores = abilityScores ?? D20SystemContent.PlayerAbilityScores;
        }

        /// <inheritdoc/>
        public void Handle(ICommand command)
        {
            var rules = LunaFlowEngine.Host.Rules;
            var current = rules.Query<PlayerPosition>().First();
            var room = _rooms[current.RoomId];

            if (room.ChallengeType != RoomChallengeType.SkillCheck)
                return;
            if (rules.Query<RoomSolved>().Any(rs => rs.RoomId == room.Id))
                return;

            var abilityMod = D20SystemContent.AbilityModifier(_abilityScores[room.SkillAbility!]);
            var result = _mechanic.ResolveCheck(abilityMod, room.DC!.Value);

            if (result.Success)
            {
                rules.Insert(new RoomSolved(room.Id));
                LunaFlowEngine.Host.Announce(D20SystemNarration.CheckNarration(room.SuccessKey!, room.Skill!, result.Total, room.DC.Value));
            }
            else
            {
                LunaFlowEngine.Host.Announce(D20SystemNarration.CheckNarration(room.FailureKey!, room.Skill!, result.Total, room.DC.Value));
            }
        }
    }
}
