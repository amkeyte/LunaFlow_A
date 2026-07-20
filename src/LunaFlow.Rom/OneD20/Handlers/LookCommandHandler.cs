using LunaFlow.Engine;

namespace LunaFlow.Rom.OneD20
{
    /// <summary>
    /// Handles the <c>look</c> command: announces the current room's description, plus both
    /// combatants' current HP if a <see cref="CombatRound"/> is active there — the on-demand way
    /// to check status mid-fight.
    /// </summary>
    public sealed class LookCommandHandler : ICommandHandler
    {
        /// <inheritdoc/>
        public void Handle(ICommand command)
        {
            var rules = LunaFlowEngine.Host.Rules;
            var current = rules.Query<PlayerPosition>().First();
            var room = OneD20Content.Rooms[current.RoomId];
            LunaFlowEngine.Host.Announce(OneD20Flavor.Text[room.DescriptionKey]);

            var round = rules.Query<CombatRound>().FirstOrDefault(cr => cr.RoomId == current.RoomId);
            if (round is not null)
            {
                LunaFlowEngine.Host.Announce(OneD20Flavor.CombatStatus(
                    round.PlayerHp, OneD20Content.PlayerCharacter.Stats.MaxHp,
                    round.MonsterHp, OneD20Content.Monster.Stats.MaxHp));
            }
        }
    }
}
