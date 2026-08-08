// Phase 2.11 checked-in verification spike (ticket 0076). Not a maintained test — see the .csproj
// comment. Run with `dotnet run --project example/Phase2_11Spike`. Exits non-zero if any check fails,
// so it doubles as a CI-able smoke check as well as a QA-rerunnable transcript.
//
// Uses LunaFlow.Engine.RuleEngine directly (not the full ConsoleEngine/GameEngineCore session) —
// same technique tests/LunaFlow.ContentTests/Rom/Loot/LootRuleGroupFixtureTests.cs uses for
// fixture-level rule coverage: deterministic, no RNG/command-dispatch involved, and it doesn't need
// GameEngineCore (internal, friend-assembly-only) at all. Behavior is verified by querying working
// memory after each Fire() — a stronger signal than announcement prose, and what the acceptance
// criteria actually turn on.

using LunaFlow.Engine;
using LunaFlow.Engine.Components;
using LunaFlow.Rom.D20System;
using LunaFlow.Rom.Loot;
using LunaFlow.Rom.Reputation;
using LunaFlow.Rom.Probes;
using LunaFlow.Rules.GeneratorServices;

// Force already-loaded, file-backed assemblies before the first RuleExport.Compile call — the same
// gotcha LootRuleGroupFixtureTests/D20SystemRuleFixtureTests work around (RoslynRuleCompiler only
// sees assemblies already loaded in the AppDomain).
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

Console.WriteLine("==================== Item 7 — Reputation relay + Loot bonus-drop ====================");
RunItem7();

Console.WriteLine();
Console.WriteLine("==================== Item 6 — clash probe (ComponentRegistry.Validate) ====================");
RunItem6();

Console.WriteLine();
Console.WriteLine("==================== Item 8 — cycle probe (ComponentRegistry.Validate) ====================");
RunItem8();

Console.WriteLine();
Console.WriteLine(failures == 0 ? "All checks passed." : $"{failures} check(s) FAILED.");
return failures == 0 ? 0 : 1;

void RunItem7()
{
    var reputationRules = RuleExport.Compile(typeof(ReputationRuleGroup));
    var lootRules = RuleExport.Compile(typeof(LootRuleGroup));

    var engine = new RuleEngine();
    engine.LoadRulesFromAssembly(reputationRules);
    engine.LoadRulesFromAssembly(lootRules);

    void DumpState(string label)
    {
        var pts = engine.Query<ReputationPoints>().SingleOrDefault();
        var tiers = string.Join(", ", engine.Query<ReputationTierReached>().Select(t => t.Tier));
        var items = string.Join(", ", engine.Query<ItemAwarded>().Select(i => $"{i.ItemId}@room{i.RoomId}"));
        var bonuses = string.Join(", ", engine.Query<BonusItemAwarded>().Select(b => $"{b.ItemId}@room{b.RoomId}"));
        Console.WriteLine($"-- {label} -- ReputationPoints={pts?.Total.ToString() ?? "(none)"} TiersReached=[{tiers}] ItemAwarded=[{items}] BonusItemAwarded=[{bonuses}]");
    }

    // Fixed session-content facts: two fight rooms (1 early, 5 late) each with a loot
    // assignment/pickup text, plus the bonus item's pickup text — the same shape Program.cs
    // seeds for a real session.
    engine.Insert(new LootAssignment(1, "rapier"));
    engine.Insert(new LootPickupText("rapier", "You take his rapier."));
    engine.Insert(new LootAssignment(5, "vest"));
    engine.Insert(new LootPickupText("vest", "You strip the vest off him."));
    engine.Insert(new LootPickupText(LootRuleGroup.BonusItemId, "You're handed a loyalty charm."));

    // Solve 1 (early fight, room 1) — first reputation point, no tier yet, no bonus.
    engine.Insert(new RoomSolved(1));
    engine.Fire();
    DumpState("After solving room 1 (fight, early)");

    var pointsAfterRoom1 = engine.Query<ReputationPoints>().SingleOrDefault();
    Check("ReputationPoints.Total == 1 after the first solved challenge", pointsAfterRoom1?.Total == 1);
    Check("Room 1's ItemAwarded exists (rapier)", engine.Query<ItemAwarded>().Any(a => a.RoomId == 1 && a.ItemId == "rapier"));
    Check("No BonusItemAwarded yet (below Veteran)", !engine.Query<BonusItemAwarded>().Any());
    Check("No ReputationTierReached yet (below Veteran)", !engine.Query<ReputationTierReached>().Any());

    // Solves 2 and 3 (skill-check rooms, no LootAssignment) — cross the Veteran threshold (3).
    engine.Insert(new RoomSolved(2));
    engine.Fire();
    DumpState("After solving room 2 (skill-check)");

    engine.Insert(new RoomSolved(3));
    engine.Fire();
    DumpState("After solving room 3 (skill-check) — crosses Veteran (3)");

    var pointsAfterRoom3 = engine.Query<ReputationPoints>().SingleOrDefault();
    Check("ReputationPoints.Total == 3 after the third solved challenge", pointsAfterRoom3?.Total == 3);
    var veteranFacts = engine.Query<ReputationTierReached>().Where(t => t.Tier == "Veteran").ToList();
    Check("ReputationTierReached(\"Veteran\") inserted exactly once", veteranFacts.Count == 1);
    Check("Skill-check rooms 2/3 never produced ItemAwarded", !engine.Query<ItemAwarded>().Any(a => a.RoomId == 2 || a.RoomId == 3));
    Check("Skill-check rooms 2/3 never produced BonusItemAwarded", !engine.Query<BonusItemAwarded>().Any(b => b.RoomId == 2 || b.RoomId == 3));

    // Solve 4 (late fight, room 5) — now Veteran, so this fight's ItemAwarded should also
    // produce a BonusItemAwarded, unlike room 1's earlier award.
    engine.Insert(new RoomSolved(5));
    engine.Fire();
    DumpState("After solving room 5 (fight, late, post-Veteran)");

    Check("Room 5's ItemAwarded exists (vest)", engine.Query<ItemAwarded>().Any(a => a.RoomId == 5 && a.ItemId == "vest"));
    Check("Room 5's BonusItemAwarded exists (post-Veteran fight)", engine.Query<BonusItemAwarded>().Any(b => b.RoomId == 5));
    Check("Room 1's earlier fight still has no BonusItemAwarded (comparison point)", !engine.Query<BonusItemAwarded>().Any(b => b.RoomId == 1));

    // Solves 5 and 6 — cross Legend (6); Veteran must not re-insert.
    engine.Insert(new RoomSolved(6));
    engine.Fire();
    engine.Insert(new RoomSolved(7));
    engine.Fire();
    DumpState("After solving rooms 6-7 — crosses Legend (6)");

    Check("ReputationTierReached(\"Veteran\") still inserted exactly once after further solves",
        engine.Query<ReputationTierReached>().Count(t => t.Tier == "Veteran") == 1);
    Check("ReputationTierReached(\"Legend\") inserted exactly once", engine.Query<ReputationTierReached>().Count(t => t.Tier == "Legend") == 1);
    Check("ReputationPoints.Total == 6 after six solves", engine.Query<ReputationPoints>().Single().Total == 6);

    // A third fight room (8), solved after both Veteran and Legend tier facts coexist — the edge
    // case AwardItemWithBonus's Match<IReputationTier>(t => Veteran || Legend) could in principle
    // double-match (one join per coexisting tier instance) if NRules didn't retract the losing
    // activation once the winning one's Then() inserts ItemAwarded. Confirms it doesn't.
    engine.Insert(new LootAssignment(8, "knuckles"));
    engine.Insert(new LootPickupText("knuckles", "You pocket a pair of brass knuckles."));
    engine.Insert(new RoomSolved(8));
    engine.Fire();
    DumpState("After solving room 8 (fight, with both Veteran and Legend tiers already present)");

    Check("Room 8's ItemAwarded inserted exactly once despite two coexisting tier facts",
        engine.Query<ItemAwarded>().Count(a => a.RoomId == 8) == 1);
    Check("Room 8's BonusItemAwarded inserted exactly once despite two coexisting tier facts",
        engine.Query<BonusItemAwarded>().Count(b => b.RoomId == 8) == 1);
}

void RunItem6()
{
    var registry = new ComponentRegistry();
    registry.Register(new BountyThreatProbe(), EngineTier.Rom, "BountyThreatProbe");
    registry.Register(new HazardThreatProbe(), EngineTier.Rom, "HazardThreatProbe");

    try
    {
        registry.Validate();
        Check("Validate() throws on the ThreatLevel clash", false);
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"    Exception message: {ex.Message}");
        Check("Validate() throws InvalidOperationException", true);
        Check("Message names 'BountyThreatProbe'", ex.Message.Contains("BountyThreatProbe"));
        Check("Message names 'HazardThreatProbe'", ex.Message.Contains("HazardThreatProbe"));
        Check("Message names the capability 'ThreatLevel'", ex.Message.Contains("ThreatLevel"));
        Check("Message does not claim a cycle (distinguishable from item 8's error)", !ex.Message.Contains("Circular", StringComparison.OrdinalIgnoreCase));
    }
}

void RunItem8()
{
    var registry = new ComponentRegistry();
    registry.Register(new CycleProbeAlpha(), EngineTier.Rom, "CycleProbeAlpha");
    registry.Register(new CycleProbeBeta(), EngineTier.Rom, "CycleProbeBeta");

    try
    {
        registry.Validate();
        Check("Validate() throws on the Alpha/Beta cycle", false);
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"    Exception message: {ex.Message}");
        Check("Validate() throws InvalidOperationException", true);
        Check("Message identifies a dependency cycle", ex.Message.Contains("Circular", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("cycle", StringComparison.OrdinalIgnoreCase));
        Check("Message does not read as item 6's duplicate-capability clash", !ex.Message.Contains("Duplicate capability", StringComparison.OrdinalIgnoreCase));
        Check("Message does not read as a plain missing-requirement error", !ex.Message.Contains("Unsatisfied requirement", StringComparison.OrdinalIgnoreCase));
        Check("Message names both probe components", ex.Message.Contains("CycleProbeAlpha") && ex.Message.Contains("CycleProbeBeta"));
    }
}
