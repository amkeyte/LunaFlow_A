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
            var equipped = rules.Query<IEquippedWeapon>().FirstOrDefault();
            var attackBonus = pc.AttackBonus;
            var damageDie = pc.DamageDie;
            var damageBonus = pc.DamageBonus;
            if (equipped is not null && _d20System.WeaponProfiles.TryGetValue(equipped.ItemId, out var weapon))
            {
                attackBonus = weapon.AttackBonus;
                damageDie = weapon.DamageDie;
                damageBonus = weapon.DamageBonus;
            }

            // Item 11 (phase-2-12-lua-worldmodule-stress-test-spec.md): while MonsterEnraged is
            // present for this room, the monster's effective Defense for the PC's attack (only —
            // never its own attack roll, per ticket 0083's ruling) is boosted. Read once, up front,
            // since crossing the threshold this same round must not affect this round's own attack
            // (see the crossing check below, which happens strictly after this read).
            var wasEnraged = rules.Query<MonsterEnraged>().Any(e => e.RoomId == location.RoomId);
            var effectiveDefense = wasEnraged
                ? monster.Defense + monsterTemplate.EnrageDefenseBonus!.Value
                : monster.Defense;

            // PC's attack.
            var pcRoll = _mechanic.Roll("1d20") + PoisonEffect.ApplyAndAdvance(rules);
            if (pcRoll + attackBonus >= effectiveDefense)
            {
                var damage = _mechanic.Roll(damageDie) + damageBonus;
                monsterHp -= damage;

                if (monsterHp <= 0)
                {
                    EnrageEffect.RetractIfPresent(rules, location.RoomId);
                    rules.Insert(new RoomSolved(location.RoomId));
                    LunaFlowEngine.Host.Announce(_flavorText["monster-defeated"]);
                    return;
                }

                LunaFlowEngine.Host.Announce(_flavorText["pc-hit"]);
                LunaFlowEngine.Host.Announce($"You deal {damage} damage. Monster HP: {monsterHp}/{monster.MaxHp}.");

                // Item 11: crossing the Enrage threshold this hit. Guarded by !wasEnraged (not a
                // fresh MonsterEnraged query) so it can never double-insert within one round; the
                // fight continuing at all past this point means monsterHp > 0, matching "once, when
                // MonsterHp <= Threshold * MaxHp" — never re-evaluated once already enraged.
                if (!wasEnraged && monsterTemplate.EnrageThresholdFraction is not null &&
                    monsterHp <= monsterTemplate.EnrageThresholdFraction.Value * monster.MaxHp)
                {
                    rules.Insert(new MonsterEnraged(location.RoomId));
                    LunaFlowEngine.Host.Announce(_flavorText["monster-enraged"]);
                }
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
                    EnrageEffect.RetractIfPresent(rules, location.RoomId);
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
                EnrageEffect.RetractIfPresent(rules, location.RoomId);
                rules.Insert(new RoomFailed(location.RoomId));
                LunaFlowEngine.Host.Announce(_flavorText["monster-fled"]);
                return;
            }

            rules.Insert(new CombatRound(location.RoomId, monsterHp, playerHp, roundsElapsed));
        }
    }
}
