using LunaFlow.Engine.Components;
using LunaFlow.Rom;
using LunaFlow.Rules;

namespace LunaFlow.Rom.D20System
{
    /// <summary>
    /// The trimmed D20System ROM: handles challenge/combat concerns for the 1.d20 grid.
    /// Owns facts: RoomChallenge, CombatRound, RoomFailed.
    /// Provides: ChallengeSolved, CombatStatus capabilities.
    /// Requires: PlayerLocation, PlayerMovement, GameWon capabilities (from GridMap).
    /// </summary>
    public sealed class D20SystemRom : IRom
    {
        /// <summary>The fixed Strong-hero level-1 base save bonuses (Fort/Ref/Will), per
        /// <c>1-d20.md</c> — added to the matching ability modifier (Con/Dex/Wis) to derive
        /// <see cref="SavingThrows"/> once at construction. No World Module field; not session data.</summary>
        private static readonly SavingThrows BaseSaves = new(Fortitude: 1, Reflex: 0, Will: 0);

        /// <summary>Fixed Loot/D20System-tier weapon profiles for equippable items, keyed by item id —
        /// not World-Module-authored (see <see cref="WeaponProfile"/>). Backup Piece (item id
        /// "pistol"): 1d6, Dex-based, +0.</summary>
        private static readonly IReadOnlyDictionary<string, WeaponProfile> DefaultWeaponProfiles =
            new Dictionary<string, WeaponProfile>
            {
                ["pistol"] = new WeaponProfile(AttackBonus: 0, DamageDie: "1d6", DamageBonus: 0),
            };

        private readonly PlayerCharacter _playerCharacter;
        private readonly IReadOnlyDictionary<string, MonsterTemplate> _monsters;
        private readonly IReadOnlyDictionary<int, RoomChallenge> _challenges;
        private readonly IReadOnlyDictionary<string, int> _playerAbilityScores;
        private readonly SavingThrows _savingThrows;
        private readonly IReadOnlyDictionary<string, WeaponProfile> _weaponProfiles;
        private readonly D20ResolutionMechanic _mechanic;

        /// <summary>Constructs the D20System ROM with challenge and combat data.</summary>
        public D20SystemRom(
            PlayerCharacter playerCharacter,
            IReadOnlyDictionary<string, MonsterTemplate> monsters,
            IReadOnlyDictionary<int, RoomChallenge> challenges,
            IReadOnlyDictionary<string, int> playerAbilityScores,
            D20ResolutionMechanic? mechanic = null,
            IReadOnlyDictionary<string, WeaponProfile>? weaponProfiles = null)
        {
            _playerCharacter = playerCharacter;
            _monsters = monsters;
            _challenges = challenges;
            _playerAbilityScores = playerAbilityScores;
            _mechanic = mechanic ?? new D20ResolutionMechanic();
            _weaponProfiles = weaponProfiles ?? DefaultWeaponProfiles;
            _savingThrows = new SavingThrows(
                Fortitude: BaseSaves.Fortitude + AbilityModifier(playerAbilityScores["Con"]),
                Reflex: BaseSaves.Reflex + AbilityModifier(playerAbilityScores["Dex"]),
                Will: BaseSaves.Will + AbilityModifier(playerAbilityScores["Wis"]));
        }

        /// <summary>The ROM's display name.</summary>
        public string Name => "D20System";

        /// <summary>Credits for the D20System ROM.</summary>
        public string Credits => "D20 challenge resolution and combat mechanics";

        /// <summary>Capabilities this ROM provides: ChallengeSolved and CombatStatus.
        /// Each pairs its contract with the concrete type that implements it — a mismatched pairing
        /// is a compile error, per ticket 0048.</summary>
        public IReadOnlyList<Capability> Provides => new[]
        {
            Capability.Provided<LunaFlow.Cartridge.IChallengeSolved, RoomSolved>("ChallengeSolved"),
            Capability.Provided<LunaFlow.Cartridge.ICombatStatus, CombatRound>("CombatStatus"),
        };

        /// <summary>Capabilities this ROM requires: PlayerLocation, PlayerMovement, and GameWon (from GridMap).</summary>
        public IReadOnlyList<Capability> Requires => new[]
        {
            new Capability("PlayerLocation", typeof(LunaFlow.Cartridge.IPlayerLocation)),
            new Capability("PlayerMovement", typeof(LunaFlow.Cartridge.IPlayerMovement)),
            new Capability("GameWon", typeof(LunaFlow.Cartridge.IGameWon)),
        };

        /// <summary>The ROM's LunaFlow reference.</summary>
        public ILunaFlowReference Reference => new D20SystemReference();

        /// <summary>Provides access to player character stats.</summary>
        public PlayerCharacter PlayerCharacter => _playerCharacter;

        /// <summary>Provides access to the monster roster, keyed by <see cref="MonsterTemplate.Id"/>.</summary>
        public IReadOnlyDictionary<string, MonsterTemplate> Monsters => _monsters;

        /// <summary>Provides access to room challenges by ID.</summary>
        public IReadOnlyDictionary<int, RoomChallenge> Challenges => _challenges;

        /// <summary>Provides access to PC ability scores.</summary>
        public IReadOnlyDictionary<string, int> PlayerAbilityScores => _playerAbilityScores;

        /// <summary>The PC's saving-throw bonuses, derived once at construction from
        /// <see cref="PlayerAbilityScores"/> and the fixed base-save table.</summary>
        public SavingThrows SavingThrows => _savingThrows;

        /// <summary>Fixed weapon profiles for equippable items, keyed by item id.</summary>
        public IReadOnlyDictionary<string, WeaponProfile> WeaponProfiles => _weaponProfiles;

        /// <summary>Provides access to the resolution mechanic for d20 rolls.</summary>
        public D20ResolutionMechanic Mechanic => _mechanic;

        /// <summary>Calculates ability modifier: (score / 2) - 5.</summary>
        public static int AbilityModifier(int score) => (int)Math.Floor(score / 2.0) - 5;
    }

    /// <summary>Reference identity for D20System ROM.</summary>
    internal sealed class D20SystemReference : ILunaFlowReference
    {
        public string ClassUID => "D20System";
        public string Identifier => "D20System";
        public string FullPath => $"D20System.{Identifier}";
        public string Path => Identifier;
        public ILunaFlowReference Parent => null!;
        public IEnumerable<ILunaFlowReference> Subreferences => Array.Empty<ILunaFlowReference>();
    }
}
