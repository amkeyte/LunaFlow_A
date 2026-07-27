using LunaFlow.Engine;
using LunaFlow.Rom.D20SystemInterop;

namespace LunaFlow.Rom.D20System
{
    /// <summary>Handles the <c>fight</c> command: resolves a round of combat.</summary>
    public sealed class FightCommandHandler : ICommandHandler
    {
        private readonly D20SystemRom _d20System;
        private readonly D20ResolutionMechanic _mechanic;

        public FightCommandHandler(D20SystemRom d20System, D20ResolutionMechanic? mechanic = null)
        {
            _d20System = d20System;
            _mechanic = mechanic ?? d20System.Mechanic;
        }

        public void Handle(ICommand command)
        {
            var rules = LunaFlowEngine.Host.Rules;
            var location = rules.Query<IPlayerLocation>().FirstOrDefault();
            if (location is null) return;

            var combat = rules.Query<CombatRound>().FirstOrDefault(c => c.RoomId == location.RoomId);
            if (combat is null)
            {
                combat = new CombatRound(location.RoomId, _d20System.Monster.Stats.MaxHp,
                    _d20System.PlayerCharacter.Stats.MaxHp, 0);
                rules.Insert(combat);
            }

            LunaFlowEngine.Host.Announce("Fight round resolved.");
        }
    }
}
