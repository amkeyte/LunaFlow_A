using LunaFlow.Engine;

namespace LunaFlow.Rom.D20System
{
    /// <summary>
    /// Handles the <c>fight</c> command: one round of attack-resolution against the current
    /// combat room's monster. Starts a fresh <see cref="CombatRound"/> (both combatants at full
    /// HP) if none is active, resolves the PC's attack — narrating every roll, hit or miss — then
    /// (unless that ends the fight) the monster's counterattack, then the 3-round clock. Losing a
    /// fight fails the room (<see cref="RoomFailed"/>), same as the timeout; it never ends the
    /// session. No-ops silently outside a fight room or once the room is solved/failed.
    /// </summary>
    public sealed class FightCommandHandler : ICommandHandler
    {
        private readonly D20ResolutionMechanic _mechanic;
        private readonly IReadOnlyDictionary<int, Room> _rooms;
        private readonly PlayerCharacter _pc;
        private readonly MonsterTemplate _monster;

        /// <summary>Uses the given arguments if supplied, else the configured <see cref="D20SystemContent"/> data — the seam ticket 0019 asked for, widened to cover Scenario data.</summary>
        public FightCommandHandler(
            D20ResolutionMechanic? mechanic = null,
            IReadOnlyDictionary<int, Room>? rooms = null,
            PlayerCharacter? pc = null,
            MonsterTemplate? monster = null)
        {
            _mechanic = mechanic ?? D20SystemContent.Mechanic;
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

            if (room.ChallengeType != RoomChallengeType.Fight)
                return;
            if (rules.Query<RoomSolved>().Any(rs => rs.RoomId == room.Id))
                return;
            if (rules.Query<RoomFailed>().Any(rf => rf.RoomId == room.Id))
                return;

            var mechanic = _mechanic;
            var pc = _pc;
            var monster = _monster;

            var existingRound = rules.Query<CombatRound>().FirstOrDefault(cr => cr.RoomId == room.Id);
            var monsterHp = existingRound?.MonsterHp ?? monster.Stats.MaxHp;
            var playerHp = existingRound?.PlayerHp ?? pc.Stats.MaxHp;
            var roundsElapsed = existingRound?.RoundsElapsed ?? 0;
            if (existingRound is not null)
                rules.Retract(existingRound);

            // 1. PC attack. Lethality is checked before choosing which line to announce — a
            // killing blow announces monster-defeated only, never pc-hit first (that read as a
            // contradiction: "he's still on his feet" immediately followed by "he drops dead").
            var pcAttack = mechanic.ResolveCheck(pc.Stats.AttackBonus, monster.Stats.Defense);
            if (pcAttack.Success)
            {
                var damage = mechanic.Roll(pc.Stats.DamageDie) + pc.Stats.DamageBonus;
                monsterHp -= damage;
                if (monsterHp <= 0)
                {
                    rules.Insert(new RoomSolved(room.Id));
                    LunaFlowEngine.Host.Announce(D20SystemContent.FlavorText["monster-defeated"]);
                    return;
                }
                LunaFlowEngine.Host.Announce(D20SystemNarration.HitNarration("pc-hit", damage, monsterHp));
            }
            else
            {
                LunaFlowEngine.Host.Announce(D20SystemContent.FlavorText["pc-miss"]);
            }

            // 2. Monster counterattack (only reached if the PC's attack didn't end the fight).
            // Same lethality-first order as the PC's attack, same reason.
            var monsterAttack = mechanic.ResolveCheck(monster.Stats.AttackBonus, pc.Stats.Defense);
            if (monsterAttack.Success)
            {
                var damage = mechanic.Roll(monster.Stats.DamageDie) + monster.Stats.DamageBonus;
                playerHp -= damage;
                if (playerHp <= 0)
                {
                    rules.Insert(new RoomFailed(room.Id));
                    LunaFlowEngine.Host.Announce(D20SystemContent.FlavorText["pc-lost"]);
                    return;
                }
                LunaFlowEngine.Host.Announce(D20SystemNarration.HitNarration("monster-hit", damage, playerHp));
            }
            else
            {
                LunaFlowEngine.Host.Announce(D20SystemContent.FlavorText["monster-miss"]);
            }

            // 3. The 3-round clock.
            roundsElapsed++;
            if (roundsElapsed == 3)
            {
                rules.Insert(new RoomFailed(room.Id));
                LunaFlowEngine.Host.Announce(D20SystemContent.FlavorText["monster-fled"]);
            }
            else
            {
                rules.Insert(new CombatRound(room.Id, monsterHp, playerHp, roundsElapsed));
            }
        }
    }
}
