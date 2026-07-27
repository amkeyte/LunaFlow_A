using LunaFlow.Engine;
using LunaFlow.Rom.D20SystemInterop;

namespace LunaFlow.Rom.D20System
{
    /// <summary>Handles the <c>check</c> command: resolves a skill check against a room's DC.</summary>
    public sealed class CheckCommandHandler : ICommandHandler
    {
        private readonly D20SystemRom _d20System;
        private readonly D20ResolutionMechanic _mechanic;

        /// <summary>Constructs the handler with the D20System ROM and resolution mechanic.</summary>
        public CheckCommandHandler(D20SystemRom d20System, D20ResolutionMechanic? mechanic = null)
        {
            _d20System = d20System;
            _mechanic = mechanic ?? d20System.Mechanic;
        }

        /// <summary>Resolves a skill check for the current room.</summary>
        public void Handle(ICommand command)
        {
            var rules = LunaFlowEngine.Host.Rules;
            var location = rules.Query<IPlayerLocation>().FirstOrDefault();
            if (location is null) return;

            var challenge = _d20System.Challenges.Values.FirstOrDefault(c => c.Id == location.RoomId);
            if (challenge?.SkillAbility is null) return;

            var abilityScore = _d20System.PlayerAbilityScores[challenge.SkillAbility];
            var modifier = D20SystemRom.AbilityModifier(abilityScore);
            var result = _mechanic.ResolveCheck(modifier, challenge.DC ?? 10);

            if (result.Success)
            {
                rules.Insert(new RoomSolved(location.RoomId));
                LunaFlowEngine.Host.Announce("Check succeeded!");
            }
            else
            {
                LunaFlowEngine.Host.Announce("Check failed.");
            }
        }
    }
}
