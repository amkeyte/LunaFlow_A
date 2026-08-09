// Phase 2.12 checked-in verification spike (ticket 0084). Not a maintained test — see the .csproj
// comment. Run with `dotnet run --project example/Phase2_12Spike`. Exits non-zero if any check
// fails, so it doubles as a CI-able smoke check as well as a QA-rerunnable transcript.

using LunaFlow.Rom.D20System;
using LunaFlow.Rom.GridMap;
using LunaFlow.Rulebook.OneD20;
using Phase2_12Spike;

_ = typeof(NRules.Fluent.Dsl.Rule).Assembly;
_ = typeof(System.Linq.Expressions.Expression).Assembly;
_ = typeof(NRules.RuleModel.IContext).Assembly;
_ = typeof(NRules.ISession).Assembly;
_ = typeof(LunaFlow.Cartridge.IChallengeSolved).Assembly;

var failures = 0;
void Check(string description, bool passed)
{
    Console.WriteLine($"[{(passed ? "PASS" : "FAIL")}] {description}");
    if (!passed) failures++;
}

// "Signal Static" is loaded once, from the same embedded resource Program.cs would use if this
// module were ever wired in as the live default — never actually done here (per item 12's probes'
// own discipline: adversarial/proof content is exercised standalone, not by editing Program.cs).
var signalStatic = LuaOneD20WorldModule.FromEmbeddedResource(
    resourceName: "LunaFlow.Rulebook.OneD20.Content.SignalStaticWorldModule.lua");

Console.WriteLine("==================== Items 9/10 — marshal, validate, field round-trip ====================");
RunMarshalAndFieldChecks();

Console.WriteLine();
Console.WriteLine("==================== Scenario A — combat-free win (column 3: rooms 3,6,9) ====================");
RunScenarioA();

Console.WriteLine();
Console.WriteLine("==================== Scenario B — combat-touching win + Enrage full chain ====================");
RunScenarioB();

Console.WriteLine();
Console.WriteLine("==================== Scenario C — RoomFailed via 3-round timeout ====================");
RunScenarioC();

Console.WriteLine();
Console.WriteLine("==================== Item 11 — OneD20WorldModuleValidator rejections ====================");
RunValidatorRejectionChecks();

Console.WriteLine();
Console.WriteLine("==================== Item 12 — Lua-path failure probes ====================");
RunItem12Probes();

Console.WriteLine();
Console.WriteLine(failures == 0 ? "All checks passed." : $"{failures} check(s) FAILED.");
return failures == 0 ? 0 : 1;

// ---- Pinned room assignment shared by all three live scenarios ----
// Fight rooms 1/5/8 get signal-monster-1 (Enrage)/5 (poison)/2 (plain); SkillCheck rooms 2/3/4/6/7/9
// draw 6 of Signal Static's 9-challenge pool. Bypasses OneD20SessionAssembler's randomness
// deliberately, same technique D20SystemHandlerTests.cs uses — the assembler's own randomization is
// covered elsewhere (ticket 0054), unaffected by and irrelevant to this phase.
IReadOnlyDictionary<int, string> ChallengeAssignment() => new Dictionary<int, string>
{
    [2] = "signal-wade", [3] = "signal-scan", [4] = "signal-listen",
    [6] = "signal-climb", [7] = "signal-bypass", [9] = "signal-balance",
};
IReadOnlyDictionary<int, string> MonsterAssignment() => new Dictionary<int, string>
{
    [1] = "signal-monster-1", [5] = "signal-monster-5", [8] = "signal-monster-2",
};

void RunMarshalAndFieldChecks()
{
    try
    {
        OneD20WorldModuleValidator.Validate(signalStatic);
        Check("Signal Static passes OneD20WorldModuleValidator unmodified", true);
    }
    catch (Exception ex)
    {
        Check($"Signal Static passes OneD20WorldModuleValidator unmodified ({ex.Message})", false);
    }

    // Item 9: at least one Save-kind challenge and one InflictsPoison monster, verified against
    // actual constructed field values.
    var arc = signalStatic.Challenges["signal-arc"];
    Check("signal-arc marshaled as Save-kind with SaveType 'Reflex'",
        arc.Kind == OneD20CheckKind.Save && arc.SaveType == "Reflex" && arc.Skill is null && arc.SkillAbility is null);
    var haze = signalStatic.Challenges["signal-haze"];
    Check("signal-haze marshaled as Save-kind with SaveType 'Fortitude'",
        haze.Kind == OneD20CheckKind.Save && haze.SaveType == "Fortitude");

    var poisonMonster = signalStatic.Monsters["signal-monster-5"];
    Check("signal-monster-5 marshaled with InflictsPoison = true", poisonMonster.InflictsPoison);
    Check("Only signal-monster-5 inflicts poison",
        signalStatic.Monsters.Values.Count(m => m.InflictsPoison) == 1);

    // Item 11: the Enrage fields survive Lua source -> LuaOneD20WorldModule marshaling, verified
    // against actual values (exact spec'd numbers: 0.5 / 3).
    var enrageMonster = signalStatic.Monsters["signal-monster-1"];
    Check("signal-monster-1 marshaled with EnrageThresholdFraction == 0.5", enrageMonster.EnrageThresholdFraction == 0.5);
    Check("signal-monster-1 marshaled with EnrageDefenseBonus == 3", enrageMonster.EnrageDefenseBonus == 3);
    Check("Only signal-monster-1 has Enrage fields set",
        signalStatic.Monsters.Values.Count(m => m.EnrageThresholdFraction is not null) == 1);
}

// Item 10: the procedurally-generated roster's stats must match the loop's own arithmetic exactly —
// computed by hand here from the same base-plus-step formula the Lua source uses
// (defense = 11 + i, maxHp = 6 + i*2, attackBonus = 2 constant), not re-derived from the marshaled
// values themselves (that would prove nothing).
void RunScenarioA()
{
    const int baseDefense = 11, baseMaxHp = 6, baseAttackBonus = 2;
    for (var i = 1; i <= 5; i++)
    {
        var m = signalStatic.Monsters[$"signal-monster-{i}"];
        Check($"signal-monster-{i}.Stats.Defense == {baseDefense + i}", m.Stats.Defense == baseDefense + i);
        Check($"signal-monster-{i}.Stats.MaxHp == {baseMaxHp + i * 2}", m.Stats.MaxHp == baseMaxHp + i * 2);
        Check($"signal-monster-{i}.Stats.AttackBonus == {baseAttackBonus}", m.Stats.AttackBonus == baseAttackBonus);
    }

    // Combat-free win: column 3 = rooms {3, 6, 9}, none of them Fight rooms. Path from start (room
    // 5): north -> 2? no -- 5's neighbors: north=2, south=8, east=6, west=4. To reach 3: north(2)
    // then east(3). Then south(6) [already assigned, re-visits col-3's own member]. Then south(9).
    var session = new SpikeSession(signalStatic, ChallengeAssignment(), MonsterAssignment());
    var checkMechanic = new D20ResolutionMechanic(new ScriptedRandom(10, 10, 10)); // 3 checks, all roll 10
    session.Start(checkMechanic: checkMechanic);
    try
    {
        session.Submit("move north");  // 5 -> 2
        session.Submit("move east");   // 2 -> 3
        session.Submit("check");       // signal-scan: Int(+0) + 10 = 10 >= DC 8, success
        session.Submit("move south");  // 3 -> 6
        session.Submit("check");       // signal-climb: Str(+3) + 10 = 13 >= DC 10, success
        session.Submit("move south");  // 6 -> 9
        session.Submit("check");       // signal-balance: Dex(+0) + 10 = 10 >= DC 8, success

        var rules = session.Engine.Rules;
        Check("Rooms 3, 6, 9 all show RoomSolved",
            new[] { 3, 6, 9 }.All(id => rules.Query<RoomSolved>().Any(rs => rs.RoomId == id)));
        Check("Column-3 WinMarker exists (combat-free win)",
            rules.Query<WinMarker>().Any(w => w.Line.OrderBy(x => x).SequenceEqual(new[] { 3, 6, 9 })));
        Check("No RoomFailed anywhere in this scenario", !rules.Query<RoomFailed>().Any());
    }
    finally { session.Stop(); }
}

// Item 11's full chain, live: threshold-crossing insert + announce, purely-defensive effect (same
// roll hits pre-threshold, misses post-threshold against the identical monster/PC), persistence
// across a round that neither wins nor loses, retraction on flee, a retry starting un-enraged,
// re-triggering, and retraction on win. Exact roll sequence documented per round — signal-monster-1:
// Defense 12, AttackBonus 2, 1d6+0 damage, MaxHp 8, EnrageThresholdFraction 0.5 (crosses at HP<=4),
// EnrageDefenseBonus 3. PC: AttackBonus 4, 1d8+3 damage, Defense 11.
void RunScenarioB()
{
    var fightRolls = new ScriptedRandom(
        /* R1: pcRoll(1d20)=8 -> 8+4=12>=12 hit */ 8,
        /* R1: damage(1d8)=1 -> 1+3=4 dmg, HP 8-4=4 (<=4, crosses threshold, enrage) */ 1,
        /* R1: monsterRoll(1d20)=1 -> 1+2=3<11 miss */ 1,
        /* R2: pcRoll(1d20)=8 (same roll) -> effectiveDefense=12+3=15, 8+4=12<15 miss (no dmg roll) */ 8,
        /* R2: monsterRoll(1d20)=1 -> miss */ 1,
        /* flee, then move back, then retry R1: pcRoll=8 -> vs baseline Defense 12 again, 12>=12 hit */ 8,
        /* retry R1: damage(1d8)=1 -> dmg 4, HP 8-4=4, re-enrages */ 1,
        /* retry R1: monsterRoll=1 -> miss */ 1,
        /* retry R2: pcRoll=11 -> effectiveDefense=15, 11+4=15>=15 hit, finishes it */ 11,
        /* retry R2: damage(1d8)=1 -> dmg 4, HP 4-4=0, WIN */ 1);
    var checkRolls = new ScriptedRandom(10, 10); // rooms 2, 3

    var session = new SpikeSession(signalStatic, ChallengeAssignment(), MonsterAssignment());
    session.Start(
        fightMechanic: new D20ResolutionMechanic(fightRolls),
        checkMechanic: new D20ResolutionMechanic(checkRolls));
    try
    {
        var rules = session.Engine.Rules;

        session.Submit("move west");  // 5 -> 4
        session.Submit("move north"); // 4 -> 1 (signal-monster-1, the Enrage monster)

        session.Submit("fight"); // Round 1: hits, crosses threshold, enrages
        Check("MonsterEnraged present after round 1 (threshold crossed)",
            rules.Query<MonsterEnraged>().Any(e => e.RoomId == 1));
        Check("CombatRound MonsterHp == 4 after round 1", CombatRoundInRoom(rules, 1).MonsterHp == 4);

        session.Submit("fight"); // Round 2: same roll, now misses (effective Defense 15)
        Check("Round 2's identical roll misses once enraged — HP unchanged (purely defensive effect demonstrated)",
            CombatRoundInRoom(rules, 1).MonsterHp == 4);
        Check("MonsterEnraged still present mid-fight (not cleared by an ordinary round)",
            rules.Query<MonsterEnraged>().Any(e => e.RoomId == 1));

        session.Submit("flee");
        Check("MonsterEnraged retracted on flee", !rules.Query<MonsterEnraged>().Any(e => e.RoomId == 1));
        Check("CombatRound retracted on flee", !rules.Query<CombatRound>().Any(c => c.RoomId == 1));

        session.Submit("move north"); // 4 -> 1, retry

        session.Submit("fight"); // Retry round 1: fresh CombatRound (MaxHp 8), same roll hits again
                                  // against baseline Defense (confirms "starts un-enraged"), re-crosses threshold
        Check("Retry starts un-enraged: identical roll hits again at baseline Defense",
            CombatRoundInRoom(rules, 1).MonsterHp == 4);
        Check("MonsterEnraged re-triggers on the retry's own threshold crossing",
            rules.Query<MonsterEnraged>().Any(e => e.RoomId == 1));

        session.Submit("fight"); // Retry round 2: higher roll overcomes the enraged Defense, wins
        Check("Room 1 shows RoomSolved (won while enraged)", rules.Query<RoomSolved>().Any(rs => rs.RoomId == 1));
        Check("MonsterEnraged retracted on win", !rules.Query<MonsterEnraged>().Any(e => e.RoomId == 1));

        session.Submit("move east"); // 1 -> 2 (still at room 1 after the win — fighting doesn't move the PC)
        session.Submit("check"); // room 2: signal-wade, Str(+3)+10=13>=DC10
        session.Submit("move east"); // 2 -> 3
        session.Submit("check"); // room 3: signal-scan, Int(+0)+10=10>=DC8

        Check("Row-1 WinMarker exists (combat-touching win)",
            rules.Query<WinMarker>().Any(w => w.Line.OrderBy(x => x).SequenceEqual(new[] { 1, 2, 3 })));
    }
    finally { session.Stop(); }
}

void RunScenarioC()
{
    // All 6 rolls miss (pcRoll+4<13 and monsterRoll+2<11) for 3 full rounds -> timeout.
    var fightRolls = new ScriptedRandom(1, 1, 1, 1, 1, 1);
    var session = new SpikeSession(signalStatic, ChallengeAssignment(), MonsterAssignment());
    session.Start(fightMechanic: new D20ResolutionMechanic(fightRolls));
    try
    {
        var rules = session.Engine.Rules;
        session.Submit("move south"); // 5 -> 8 (signal-monster-2, plain)
        session.Submit("fight"); // round 1: miss/miss
        session.Submit("fight"); // round 2: miss/miss
        session.Submit("fight"); // round 3: miss/miss -> timeout

        Check("Room 8 shows RoomFailed (3-round timeout)", rules.Query<RoomFailed>().Any(rf => rf.RoomId == 8));
        Check("No CombatRound remains for room 8", !rules.Query<CombatRound>().Any(c => c.RoomId == 8));
        Check("MonsterEnraged retraction is a safe no-op when nothing was enraged",
            !rules.Query<MonsterEnraged>().Any(e => e.RoomId == 8));
    }
    finally { session.Stop(); }
}

CombatRound CombatRoundInRoom(LunaFlow.Engine.RuleEngine rules, int roomId) =>
    rules.Query<CombatRound>().First(c => c.RoomId == roomId);

void RunValidatorRejectionChecks()
{
    var onlyThreshold = new OneD20MonsterTemplate("x", "X", "x-desc", new OneD20CombatantStats(10, 2, "1d6", 0, 6),
        EnrageThresholdFraction: 0.5, EnrageDefenseBonus: null);
    CheckRejectsWithEnrageMessage("EnrageThresholdFraction set without EnrageDefenseBonus", onlyThreshold);

    var onlyBonus = new OneD20MonsterTemplate("x", "X", "x-desc", new OneD20CombatantStats(10, 2, "1d6", 0, 6),
        EnrageThresholdFraction: null, EnrageDefenseBonus: 3);
    CheckRejectsWithEnrageMessage("EnrageDefenseBonus set without EnrageThresholdFraction", onlyBonus);

    var outOfRangeHigh = new OneD20MonsterTemplate("x", "X", "x-desc", new OneD20CombatantStats(10, 2, "1d6", 0, 6),
        EnrageThresholdFraction: 1.5, EnrageDefenseBonus: 3);
    CheckRejectsWithEnrageMessage("EnrageThresholdFraction > 1", outOfRangeHigh);

    var outOfRangeZero = new OneD20MonsterTemplate("x", "X", "x-desc", new OneD20CombatantStats(10, 2, "1d6", 0, 6),
        EnrageThresholdFraction: 0.0, EnrageDefenseBonus: 3);
    CheckRejectsWithEnrageMessage("EnrageThresholdFraction == 0 (exclusive lower bound)", outOfRangeZero);

    var valid = new OneD20MonsterTemplate("x", "X", "x-desc", new OneD20CombatantStats(10, 2, "1d6", 0, 6),
        EnrageThresholdFraction: 1.0, EnrageDefenseBonus: 3);
    try
    {
        ValidateSingleMonster(valid);
        Check("Validator accepts EnrageThresholdFraction == 1 (inclusive upper bound)", true);
    }
    catch (InvalidOperationException ex)
    {
        Check($"Validator accepts EnrageThresholdFraction == 1 (inclusive upper bound) — threw instead: {ex.Message}", false);
    }
}

/// <summary>Asserts <paramref name="monster"/> is rejected, and specifically that the exception
/// names Enrage and this monster's id ('x') — not merely that some <see cref="InvalidOperationException"/>
/// fired, which the first draft of this probe (a structurally-invalid single-monster World Module)
/// would have passed for the wrong reason. See <see cref="SingleMonsterProbeWorldModule"/>'s doc
/// comment for what that looked like.</summary>
void CheckRejectsWithEnrageMessage(string scenario, OneD20MonsterTemplate monster)
{
    try
    {
        ValidateSingleMonster(monster);
        Check($"Validator rejects {scenario}", false);
    }
    catch (InvalidOperationException ex)
    {
        Check($"Validator rejects {scenario}", ex.Message.Contains("Enrage") && ex.Message.Contains("'x'"));
    }
}

void ValidateSingleMonster(OneD20MonsterTemplate monster)
{
    var module = new SingleMonsterProbeWorldModule(monster);
    OneD20WorldModuleValidator.Validate(module);
}

void RunItem12Probes()
{
    // Marshal-failure probe: a dc field written as a string. LuaOneD20WorldModule's constructor must
    // throw before OneD20WorldModuleValidator.Validate is ever reachable — proven by a flag that can
    // only ever be set on the far side of a successful construction.
    const string marshalBroken = """
        return {
          rooms = {
            [1] = { row = 0, col = 0, challengeType = "SkillCheck", descriptionKey = "x" },
          },
          challenges = {
            broken = { skill = "Search", skillAbility = "Int", dc = "ten",
                       successKey = "s", failureKey = "f", promptKey = "p" },
          },
          monsters = {},
          itemIdByMonsterId = {},
          playerCharacter = { defense = 10, attackBonus = 0, damageDie = "1d4", damageBonus = 0, maxHp = 10 },
          playerAbilityScores = { Str = 10, Dex = 10, Con = 10, Int = 10, Wis = 10, Cha = 10 },
          flavorText = {},
          startRoomId = 1,
        }
        """;
    var validatorReached = false;
    try
    {
        var probe = new LuaOneD20WorldModule(marshalBroken, "item-12-marshal-probe");
        OneD20WorldModuleValidator.Validate(probe);
        validatorReached = true;
    }
    catch (InvalidOperationException ex)
    {
        Check("Marshal-failure probe throws InvalidOperationException", true);
        Check("Marshal-failure message names the script", ex.Message.Contains("item-12-marshal-probe"));
        Check("Marshal-failure message names the offending field ('dc')", ex.Message.Contains("'dc'"));
        Check("Marshal-failure message names the offending entry ('broken')", ex.Message.Contains("'broken'"));
    }
    Check("OneD20WorldModuleValidator was never reached (constructor never returned)", !validatorReached);

    // Validator-failure probe: marshal-clean (every field the right type, 9 valid rooms) but only 4
    // challenges against the grid's 6-slot requirement. Must construct successfully, then fail at
    // OneD20WorldModuleValidator's existing pool-size check specifically.
    var roomsBlock = string.Join(",\n", Enumerable.Range(1, 9)
        .Select(i => $"[{i}] = {{ row = 0, col = 0, challengeType = \"SkillCheck\", descriptionKey = \"x\" }}"));
    var validatorBroken = $$"""
        return {
          rooms = {
            {{roomsBlock}}
          },
          challenges = {
            c1 = { skill = "Search", skillAbility = "Int", dc = 8, successKey = "s", failureKey = "f", promptKey = "p" },
            c2 = { skill = "Search", skillAbility = "Int", dc = 8, successKey = "s", failureKey = "f", promptKey = "p" },
            c3 = { skill = "Search", skillAbility = "Int", dc = 8, successKey = "s", failureKey = "f", promptKey = "p" },
            c4 = { skill = "Search", skillAbility = "Int", dc = 8, successKey = "s", failureKey = "f", promptKey = "p" },
          },
          monsters = {},
          itemIdByMonsterId = {},
          playerCharacter = { defense = 10, attackBonus = 0, damageDie = "1d4", damageBonus = 0, maxHp = 10 },
          playerAbilityScores = { Str = 10, Dex = 10, Con = 10, Int = 10, Wis = 10, Cha = 10 },
          flavorText = {},
          startRoomId = 1,
        }
        """;
    try
    {
        var probe = new LuaOneD20WorldModule(validatorBroken, "item-12-validator-probe");
        Check("Validator-failure probe constructs successfully (marshal-clean)", true);
        try
        {
            OneD20WorldModuleValidator.Validate(probe);
            Check("Validator-failure probe rejected by OneD20WorldModuleValidator", false);
        }
        catch (InvalidOperationException ex)
        {
            Check("Validator-failure probe rejected by OneD20WorldModuleValidator", true);
            Check("Rejection names the shortfall (4 challenges, needs >= 6)",
                ex.Message.Contains("Challenges") && ex.Message.Contains("4"));
        }
    }
    catch (InvalidOperationException ex)
    {
        Check($"Validator-failure probe constructs successfully (marshal-clean) — threw instead: {ex.Message}", false);
    }
}

/// <summary>Minimal but otherwise-fully-valid <see cref="IOneD20WorldModule"/> wrapping one tested
/// monster template plus two plain filler monsters, for exercising
/// <see cref="OneD20WorldModuleValidator"/>'s per-monster Enrage pairing/range check in isolation.
/// Every OTHER structural check (room count, challenge-pool size vs. SkillCheck-room count, monster
/// roster size) is satisfied deliberately — first-draft version of this probe had only one monster
/// against 9 all-SkillCheck rooms and 6 challenges, so the roster-size and pool-size checks threw
/// first, and the four "reject" assertions below passed for the wrong reason (any
/// InvalidOperationException, not necessarily the Enrage one) until this was caught by inspecting
/// which check actually fired.</summary>
sealed class SingleMonsterProbeWorldModule : IOneD20WorldModule
{
    private readonly OneD20MonsterTemplate _monster;
    public SingleMonsterProbeWorldModule(OneD20MonsterTemplate monster) => _monster = monster;

    public IReadOnlyDictionary<int, OneD20RoomData> Rooms { get; } = Enumerable.Range(1, 9)
        .ToDictionary(i => i, i => new OneD20RoomData(i, 0, 0, OneD20ChallengeType.SkillCheck, "x-desc"));
    public int StartRoomId => 1;
    public IReadOnlyDictionary<string, OneD20ChallengeTemplate> Challenges { get; } = Enumerable.Range(1, 9)
        .ToDictionary(i => $"c{i}", i => new OneD20ChallengeTemplate($"c{i}", "Search", "Int", 8, "s", "f", "p", Kind: OneD20CheckKind.Skill));
    public IReadOnlyDictionary<string, OneD20MonsterTemplate> Monsters => new Dictionary<string, OneD20MonsterTemplate>
    {
        [_monster.Id] = _monster,
        ["filler-1"] = new("filler-1", "Filler", "x-desc", new OneD20CombatantStats(10, 0, "1d4", 0, 6)),
        ["filler-2"] = new("filler-2", "Filler", "x-desc", new OneD20CombatantStats(10, 0, "1d4", 0, 6)),
    };
    public IReadOnlyDictionary<string, string> ItemIdByMonsterId { get; } = new Dictionary<string, string>();
    public OneD20CombatantStats PlayerCharacter { get; } = new(10, 0, "1d4", 0, 10);
    public IReadOnlyDictionary<string, int> PlayerAbilityScores { get; } = new Dictionary<string, int>
    { ["Str"] = 10, ["Dex"] = 10, ["Con"] = 10, ["Int"] = 10, ["Wis"] = 10, ["Cha"] = 10 };
    public IReadOnlyDictionary<string, string> FlavorText { get; } = new Dictionary<string, string>
    {
        ["x-desc"] = "x", ["s"] = "s", ["f"] = "f", ["p"] = "p",
        ["pc-hit"] = "x", ["pc-miss"] = "x", ["monster-hit"] = "x", ["monster-miss"] = "x",
        ["monster-defeated"] = "x", ["monster-fled"] = "x", ["pc-flee"] = "x", ["pc-lost"] = "x",
        ["win-announce"] = "x", ["inventory-empty"] = "x", ["inventory-header"] = "x",
    };
}
