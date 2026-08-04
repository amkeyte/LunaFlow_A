using LunaFlow.Engine;
using LunaFlow.Cartridge;

namespace LunaFlow.Rom.D20System
{
    /// <summary>Handles the <c>check</c> command: resolves a skill check against a room's DC.</summary>
    public sealed class CheckCommandHandler : ICommandHandler
    {
        private readonly D20SystemRom _d20System;
        private readonly D20ResolutionMechanic _mechanic;
        private readonly IReadOnlyDictionary<string, string> _flavorText;

        /// <summary>Constructs the handler with the D20System ROM, resolution mechanic, and the Rulebook's flavor text.</summary>
        public CheckCommandHandler(D20SystemRom d20System, IReadOnlyDictionary<string, string> flavorText, D20ResolutionMechanic? mechanic = null)
        {
            _d20System = d20System;
            _flavorText = flavorText;
            _mechanic = mechanic ?? d20System.Mechanic;
        }

        /// <summary>Resolves a skill or saving-throw check for the current room. Silent no-op if
        /// already solved.</summary>
        public void Handle(ICommand command)
        {
            var rules = LunaFlowEngine.Host.Rules;
            var location = rules.Query<IPlayerLocation>().FirstOrDefault();
            if (location is null) return;

            var challenge = _d20System.Challenges.Values.FirstOrDefault(c => c.Id == location.RoomId);
            if (challenge?.Kind is null) return;

            if (rules.Query<RoomSolved>().Any(rs => rs.RoomId == location.RoomId))
                return;

            int modifier;
            string readoutLabel;
            if (challenge.Kind == CheckKind.Save)
            {
                modifier = challenge.SaveType switch
                {
                    "Fortitude" => _d20System.SavingThrows.Fortitude,
                    "Reflex" => _d20System.SavingThrows.Reflex,
                    "Will" => _d20System.SavingThrows.Will,
                    _ => 0,
                };
                readoutLabel = challenge.SaveType!;
            }
            else
            {
                var abilityScore = _d20System.PlayerAbilityScores[challenge.SkillAbility!];
                modifier = D20SystemRom.AbilityModifier(abilityScore);
                readoutLabel = challenge.Skill!;
            }

            modifier += PoisonEffect.ApplyAndAdvance(rules);

            var result = _mechanic.ResolveCheck(modifier, challenge.DC ?? 10);
            var readout = $"{readoutLabel}: rolled {result.Total} vs. DC {result.TargetNumber}.";

            if (result.Success)
            {
                rules.Insert(new RoomSolved(location.RoomId));
                LunaFlowEngine.Host.Announce(_flavorText[challenge.SuccessKey!]);
                LunaFlowEngine.Host.Announce(readout);
            }
            else
            {
                LunaFlowEngine.Host.Announce(_flavorText[challenge.FailureKey!]);
                LunaFlowEngine.Host.Announce(readout);
            }
        }
    }
}
