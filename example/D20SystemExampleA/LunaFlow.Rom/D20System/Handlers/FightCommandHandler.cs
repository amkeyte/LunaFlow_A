using LunaFlow.Engine;
using LunaFlow.Cartridge;

namespace LunaFlow.Rom.D20System
{
    /// <summary>Handles the <c>fight</c> command: resolves a round of combat.</summary>
    public sealed class FightCommandHandler : ICommandHandler
    {
        private readonly D20SystemRom _d20System;
        private readonly D20ResolutionMechanic _mechanic;
        private readonly IReadOnlyDictionary<string, string> _flavorText;

        /// <summary>Constructs the handler with the D20System ROM, resolution mechanic, and the Rulebook's flavor text.</summary>
        public FightCommandHandler(D20SystemRom d20System, IReadOnlyDictionary<string, string> flavorText, D20ResolutionMechanic? mechanic = null)
        {
            _d20System = d20System;
            _flavorText = flavorText;
            _mechanic = mechanic ?? d20System.Mechanic;
        }

        /// <summary>Resolves one round of combat for the current room. Silent no-op if already solved/failed.</summary>
        public void Handle(ICommand command)
        {
            var rules = LunaFlowEngine.Host.Rules;
            var location = rules.Query<IPlayerLocation>().FirstOrDefault();
            if (location is null) return;

            if (rules.Query<RoomSolved>().Any(rs => rs.RoomId == location.RoomId)) return;
            if (rules.Query<RoomFailed>().Any(rf => rf.RoomId == location.RoomId)) return;

            var challenge = _d20System.Challenges.Values.FirstOrDefault(c => c.Id == location.RoomId);
            if (challenge?.MonsterId is null) return;
            var monsterTemplate = _d20System.Monsters[challenge.MonsterId];

            // Read the active round's HP into plain locals and retract the fact once up front —
            // exactly one CombatRound/terminal fact gets inserted at the end of this method,
            // never a partially-updated intermediate instance (replace-not-update, same as
            // PlayerPosition elsewhere).
            var combat = rules.Query<CombatRound>().FirstOrDefault(c => c.RoomId == location.RoomId);
            int monsterHp, playerHp, roundsElapsed;
            if (combat is null)
            {
                monsterHp = monsterTemplate.Stats.MaxHp;
                playerHp = _d20System.PlayerCharacter.Stats.MaxHp;
                roundsElapsed = 0;
            }
            else
            {
                rules.Retract(combat);
                monsterHp = combat.MonsterHp;
                playerHp = combat.PlayerHp;
                roundsElapsed = combat.RoundsElapsed;
            }

            var pc = _d20System.PlayerCharacter.Stats;
            var monster = monsterTemplate.Stats;

            // Equipped-weapon branch (items 2/4): substitute attack bonus/damage die/bonus from the
            // equipped weapon's profile if one is active, else fall back to the default sword's own
            // stats on PlayerCharacter.Stats — Defense is never affected by which weapon is equipped.
            var equipped = rules.Query<EquippedWeapon>().FirstOrDefault();
            var attackBonus = pc.AttackBonus;
            var damageDie = pc.DamageDie;
            var damageBonus = pc.DamageBonus;
            if (equipped is not null && _d20System.WeaponProfiles.TryGetValue(equipped.ItemId, out var weapon))
            {
                attackBonus = weapon.AttackBonus;
                damageDie = weapon.DamageDie;
                damageBonus = weapon.DamageBonus;
            }

            // PC's attack.
            var pcRoll = _mechanic.Roll("1d20") + PoisonEffect.ApplyAndAdvance(rules);
            if (pcRoll + attackBonus >= monster.Defense)
            {
                var damage = _mechanic.Roll(damageDie) + damageBonus;
                monsterHp -= damage;

                if (monsterHp <= 0)
                {
                    rules.Insert(new RoomSolved(location.RoomId));
                    LunaFlowEngine.Host.Announce(_flavorText["monster-defeated"]);
                    return;
                }

                LunaFlowEngine.Host.Announce(_flavorText["pc-hit"]);
                LunaFlowEngine.Host.Announce($"You deal {damage} damage. Monster HP: {monsterHp}/{monster.MaxHp}.");
            }
            else
            {
                LunaFlowEngine.Host.Announce(_flavorText["pc-miss"]);
            }

            // Monster's attack (only reached if the PC's attack didn't end the fight).
            var monsterRoll = _mechanic.Roll("1d20");
            if (monsterRoll + monster.AttackBonus >= pc.Defense)
            {
                var damage = _mechanic.Roll(monster.DamageDie) + monster.DamageBonus;
                playerHp -= damage;

                if (playerHp <= 0)
                {
                    rules.Insert(new RoomFailed(location.RoomId));
                    LunaFlowEngine.Host.Announce(_flavorText["pc-lost"]);
                    return;
                }

                if (monsterTemplate.InflictsPoison)
                {
                    var existingPoison = rules.Query<Poisoned>().FirstOrDefault();
                    if (existingPoison is not null) rules.Retract(existingPoison);
                    rules.Insert(new Poisoned(3));
                }

                LunaFlowEngine.Host.Announce(_flavorText["monster-hit"]);
                LunaFlowEngine.Host.Announce($"You take {damage} damage. Your HP: {playerHp}/{pc.MaxHp}.");
            }
            else
            {
                LunaFlowEngine.Host.Announce(_flavorText["monster-miss"]);
            }

            // Neither combatant reached 0 — advance the round, or time out at 3.
            roundsElapsed += 1;
            if (roundsElapsed >= 3)
            {
                rules.Insert(new RoomFailed(location.RoomId));
                LunaFlowEngine.Host.Announce(_flavorText["monster-fled"]);
                return;
            }

            rules.Insert(new CombatRound(location.RoomId, monsterHp, playerHp, roundsElapsed));
        }
    }
}
