using LunaFlow.Engine;

namespace LunaFlow.Rom.OneD20
{
    /// <summary>
    /// Handles the <c>fight</c> command: one round of attack-resolution against the current
    /// combat room's monster. Starts a fresh <see cref="CombatRound"/> if none is active, resolves
    /// the PC's attack, then (unless that ends the fight) the monster's counterattack, then the
    /// 3-round clock. No-ops silently outside a fight room or once the room is solved/failed.
    /// </summary>
    public sealed class FightCommandHandler : ICommandHandler
    {
        /// <inheritdoc/>
        public void Handle(ICommand command)
        {
            var rules = LunaFlowEngine.Host.Rules;
            if (rules.Query<PlayerDown>().Any())
                return;

            var current = rules.Query<PlayerPosition>().First();
            var room = OneD20Content.Rooms[current.RoomId];

            if (room.ChallengeType != RoomChallengeType.Fight)
                return;
            if (rules.Query<RoomSolved>().Any(rs => rs.RoomId == room.Id))
                return;
            if (rules.Query<RoomFailed>().Any(rf => rf.RoomId == room.Id))
                return;

            var mechanic = OneD20Content.Mechanic;
            var pc = OneD20Content.PlayerCharacter;
            var monster = OneD20Content.Monster;

            var existingRound = rules.Query<CombatRound>().FirstOrDefault(cr => cr.RoomId == room.Id);
            var monsterHp = existingRound?.MonsterHp ?? monster.Stats.MaxHp;
            var roundsElapsed = existingRound?.RoundsElapsed ?? 0;
            if (existingRound is not null)
                rules.Retract(existingRound);

            // 1. PC attack.
            if (mechanic.ResolveCheck(pc.Stats.AttackBonus, monster.Stats.Defense))
                monsterHp -= mechanic.Roll(pc.Stats.DamageDie) + pc.Stats.DamageBonus;

            if (monsterHp <= 0)
            {
                rules.Insert(new RoomSolved(room.Id));
                LunaFlowEngine.Host.Announce(OneD20Flavor.Text["monster-defeated"]);
                return;
            }

            // 2. Monster counterattack (only reached if the PC's hit didn't finish the fight).
            var playerHp = rules.Query<PlayerHp>().First();
            if (mechanic.ResolveCheck(monster.Stats.AttackBonus, pc.Stats.Defense))
            {
                var updatedHp = new PlayerHp(playerHp.Current - (mechanic.Roll(monster.Stats.DamageDie) + monster.Stats.DamageBonus));
                rules.Retract(playerHp);
                rules.Insert(updatedHp);
                playerHp = updatedHp;
            }

            if (playerHp.Current <= 0)
            {
                rules.Insert(new PlayerDown());
                LunaFlowEngine.Host.Announce(OneD20Flavor.Text["pc-down"]);
                return;
            }

            // 3. The 3-round clock.
            roundsElapsed++;
            if (roundsElapsed == 3)
            {
                rules.Insert(new RoomFailed(room.Id));
                LunaFlowEngine.Host.Announce(OneD20Flavor.Text["monster-fled"]);
            }
            else
            {
                rules.Insert(new CombatRound(room.Id, monsterHp, roundsElapsed));
            }
        }
    }
}
