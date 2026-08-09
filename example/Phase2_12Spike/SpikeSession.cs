using LunaFlow.Engine;
using LunaFlow.Engine.Commands;
using LunaFlow.Engine.Game;
using LunaFlow.Rom.D20System;
using LunaFlow.Rom.GridMap;
using LunaFlow.Rom.GridMap.Handlers;
using LunaFlow.Rulebook.OneD20;
using LunaFlow.Rules.GeneratorServices;

namespace Phase2_12Spike
{
    /// <summary>
    /// Drives the real <see cref="GameEngineCore"/> singleton through a pinned (non-randomized)
    /// room-to-challenge/room-to-monster assignment — same technique
    /// <c>tests/LunaFlow.ContentTests/Rom/D20System/D20SystemHandlerTests.cs</c> uses, adapted for
    /// "Signal Static" instead of <c>DefaultOneD20WorldModule</c>. GridMap/D20System rules only
    /// (Loot/Reputation are out of scope for items 9-12 and untouched by this phase). One instance
    /// per scenario — construct fresh, call <see cref="Start"/>, <see cref="Submit"/> commands,
    /// query <see cref="GameEngineCore.Rules"/> directly, then <see cref="Stop"/> before the next
    /// scenario reuses the singleton.
    /// </summary>
    internal sealed class SpikeSession
    {
        public GameEngineCore Engine { get; }
        public DefaultCommandDispatcher Dispatcher { get; }
        public GridMapRom GridMap { get; private set; } = null!;
        public D20SystemRom D20System { get; private set; } = null!;

        private readonly IOneD20WorldModule _worldModule;
        private readonly IReadOnlyDictionary<int, string> _challengeKeyByRoom;
        private readonly IReadOnlyDictionary<int, string> _monsterIdByRoom;

        public SpikeSession(
            IOneD20WorldModule worldModule,
            IReadOnlyDictionary<int, string> challengeKeyByRoom,
            IReadOnlyDictionary<int, string> monsterIdByRoom)
        {
            Engine = GameEngineCore.Instance;
            if (Engine.Running) Engine.Stop(force: true);
            _worldModule = worldModule;
            _challengeKeyByRoom = challengeKeyByRoom;
            _monsterIdByRoom = monsterIdByRoom;
            Dispatcher = new DefaultCommandDispatcher();
        }

        private static RoomChallenge ToRoomChallenge(int roomId, OneD20ChallengeTemplate challenge)
        {
            CheckKind? kind = challenge.Kind switch
            {
                OneD20CheckKind.Skill => CheckKind.Skill,
                OneD20CheckKind.Save => CheckKind.Save,
                _ => null,
            };
            return new RoomChallenge(roomId, RoomChallengeType.SkillCheck, kind,
                challenge.Skill, challenge.SkillAbility, challenge.DC, challenge.SaveType,
                challenge.SuccessKey, challenge.FailureKey, challenge.PromptKey, MonsterId: null);
        }

        /// <summary>Registers handlers, seeds facts (mirroring <c>Program.cs</c>'s own seed shape
        /// exactly, including the start-room announcement trick), and starts a fresh session against
        /// the pinned assignment. <paramref name="fightMechanic"/>/<paramref name="checkMechanic"/>
        /// default to unseeded (real) RNG if omitted.</summary>
        public void Start(D20ResolutionMechanic? fightMechanic = null, D20ResolutionMechanic? checkMechanic = null)
        {
            var gridMapData = _worldModule.Rooms.Values
                .Select(r => new GridRoomData(r.Id, r.Row, r.Col, r.DescriptionKey))
                .ToList();
            GridMap = new GridMapRom(gridMapData, _worldModule.StartRoomId);

            var d20Challenges = _worldModule.Rooms.Values
                .Select(r => r.ChallengeType == OneD20ChallengeType.Fight
                    ? new RoomChallenge(r.Id, RoomChallengeType.Fight, null, null, null, null, null, null, null, null,
                        MonsterId: _monsterIdByRoom[r.Id])
                    : ToRoomChallenge(r.Id, _worldModule.Challenges[_challengeKeyByRoom[r.Id]]))
                .ToDictionary(c => c.Id);

            var playerStats = _worldModule.PlayerCharacter;
            var monsters = _worldModule.Monsters.Values
                .Select(m => new MonsterTemplate(m.Id, m.Name, m.DescriptionKey,
                    new CombatantStats(m.Stats.Defense, m.Stats.AttackBonus, m.Stats.DamageDie, m.Stats.DamageBonus, m.Stats.MaxHp),
                    m.InflictsPoison, m.EnrageThresholdFraction, m.EnrageDefenseBonus))
                .ToDictionary(m => m.Id);

            D20System = new D20SystemRom(
                new PlayerCharacter(new CombatantStats(playerStats.Defense, playerStats.AttackBonus, playerStats.DamageDie, playerStats.DamageBonus, playerStats.MaxHp)),
                monsters, d20Challenges, _worldModule.PlayerAbilityScores,
                fightMechanic ?? new D20ResolutionMechanic());

            var movement = new GridMovementService();
            var flavorText = _worldModule.FlavorText;
            var monsterMaxHpByRoom = _monsterIdByRoom.ToDictionary(
                kv => kv.Key, kv => _worldModule.Monsters[kv.Value].Stats.MaxHp);

            Dispatcher.RegisterHandler("move", new MoveCommandHandler(GridMap, flavorText));
            Dispatcher.RegisterHandler("look", new LookCommandHandler(GridMap, flavorText, playerStats.MaxHp, monsterMaxHpByRoom));
            Dispatcher.RegisterHandler("check", new CheckCommandHandler(D20System, flavorText, checkMechanic));
            Dispatcher.RegisterHandler("fight", new FightCommandHandler(D20System, flavorText, fightMechanic));
            Dispatcher.RegisterHandler("flee", new FleeCommandHandler(movement, flavorText));

            var options = new GameEngineOptions();
            options.Rules
                .FromAssembly(RuleExport.Compile(typeof(GridMapRuleGroup)))
                .FromAssembly(RuleExport.Compile(typeof(D20SystemRuleGroup)))
                .Seed(() =>
                {
                    var startRoom = _worldModule.Rooms[_worldModule.StartRoomId];
                    LunaFlowEngine.Host.Announce(flavorText[startRoom.DescriptionKey]);
                    return new object[] { GridMap, D20System, new PlayerPosition(_worldModule.StartRoomId), D20System.PlayerCharacter };
                })
                .Seed(() =>
                {
                    var prompts = _worldModule.Rooms.Values
                        .Select(r => (object)new RoomChallengePrompt(
                            r.Id,
                            r.ChallengeType == OneD20ChallengeType.Fight
                                ? flavorText[_worldModule.Monsters[_monsterIdByRoom[r.Id]].DescriptionKey]
                                : flavorText[_worldModule.Challenges[_challengeKeyByRoom[r.Id]].PromptKey]));
                    return prompts.Append(new GameEndAnnouncement(flavorText["win-announce"]));
                });
            Engine.SetOptions(options);
            Engine.Start();
        }

        /// <summary>Submits <paramref name="commandLine"/> through the real dispatch+Fire pipeline and
        /// waits for completion, same path production goes through via <c>ConsoleEngine.Run()</c>.</summary>
        public void Submit(string commandLine)
        {
            var command = new DefaultCommand(commandLine);
            using var completed = new ManualResetEventSlim(false);
            Engine.Submit(new DispatchGameCommand(command, Dispatcher, completed));
            if (!completed.Wait(TimeSpan.FromSeconds(5)))
                throw new InvalidOperationException($"Command '{commandLine}' did not complete in time.");
        }

        public void Stop()
        {
            if (Engine.Running) Engine.Stop(force: true);
        }
    }
}
