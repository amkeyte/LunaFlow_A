using LunaFlow.Engine;

namespace LunaFlow.Rom.D20System
{
    /// <summary>
    /// Handles the <c>look</c> command: announces the current room's description, plus both
    /// combatants' current HP if a <see cref="CombatRound"/> is active there — the on-demand way
    /// to check status mid-fight.
    /// </summary>
    public sealed class LookCommandHandler : ICommandHandler
    {
        private readonly IReadOnlyDictionary<int, Room> _rooms;
        private readonly PlayerCharacter _pc;
        private readonly MonsterTemplate _monster;

        /// <summary>Uses the given arguments if supplied, else the configured <see cref="D20SystemContent"/> data.</summary>
        public LookCommandHandler(IReadOnlyDictionary<int, Room>? rooms = null, PlayerCharacter? pc = null, MonsterTemplate? monster = null)
        {
            _rooms = rooms ?? D20SystemContent.Rooms;
            _pc = pc ?? D20SystemContent.PlayerCharacter;
            _monster = monster ?? D20SystemContent.Monster;
        }

        /// <inheritdoc/>
        public void Handle(ICommand command)
        {
            var rules = LunaFlowEngine.Host.Rules;
            var current = rules.Query<PlayerPosition>().First();
            var room = _rooms[current.RoomId];
            LunaFlowEngine.Host.Announce(D20SystemContent.FlavorText[room.DescriptionKey]);

            var round = rules.Query<CombatRound>().FirstOrDefault(cr => cr.RoomId == current.RoomId);
            if (round is not null)
            {
                LunaFlowEngine.Host.Announce(D20SystemNarration.CombatStatus(
                    round.PlayerHp, _pc.Stats.MaxHp,
                    round.MonsterHp, _monster.Stats.MaxHp));
            }
        }
    }
}
