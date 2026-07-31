// See https://aka.ms/new-console-template for more information
using LunaFlow.Engine;
using LunaFlow.Engine.Commands;
using LunaFlow.Engine.Components;
using LunaFlow.Rom.D20System;
using LunaFlow.Rom.GridMap;
using LunaFlow.Rom.GridMap.Handlers;
using LunaFlow.Rom.Loot;
using LunaFlow.Rulebook.OneD20;
using LunaFlow.Rules.GeneratorServices;

Console.WriteLine("Welcome to MyLunaFlowServer, powered by LunaFlow_A");
Console.WriteLine("Starting server");


////////////////////////////////////////////////////////////////////////////////////////////////////
// Arryn's BIG  note:
// under no circumstances should the current existence of content code living here
// be construed as future-proof. Content code should not bleed into other infrastructure
// classes. Content code should also not become overly dependent on the shape of this Program.cs
// file, as it WILL change.
///////////////////////////////////////////////////////////////////////////////////////////////////



// Bootstrap contract probe (cross-rom-capability-spec.md §5). Top-level statements compile into
// ONE method, and the CLR resolves every type referenced anywhere in a method before executing any
// of it — so a naive try/catch inline in this file would not actually catch a missing/incompatible
// ROM assembly: the *whole* top-level method would fail to JIT before a single statement ran,
// including this try/catch. The only real boundary is a genuine method call, so everything that
// touches a ROM/Cartridge type lives in Bootstrap()/RunGame() below, and this top-level code never
// names one directly (object only) — confirmed by fault injection (deleting the Cartridge assembly
// and re-running produced an unhandled crash until this was restructured this way). Using
// directives are compile-time only and don't force runtime type loading, so they stay here safely.

object gridMapObj;
object d20SystemObj;
object lootObj;
OneD20SessionAssignment assignment;

try
{
    (gridMapObj, d20SystemObj, lootObj, assignment) = Bootstrap();
}
catch (BootstrapContractException ex)
{
    // Must not degrade: report and exit non-zero, never catch-log-continue (cross-rom-capability-
    // spec.md §5.1 — "converts a loud failure into a quiet one, the exact outcome this prevents").
    Console.Error.WriteLine(ex.Message);
    Environment.Exit(1);
    return;
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Component registry validation failed: {ex.Message}");
    throw;
}

RunGame(gridMapObj, d20SystemObj, lootObj, assignment);
Console.WriteLine("Server stopped.");

// Constructs and registers both ROMs plus the Rulebook, validates the registry, and forces member
// resolution over every declared Provides entry (§5.3) — all before returning. Deliberately typed
// in terms of IHasDependencies/object only, never a concrete ROM/Cartridge type by name, so THIS
// method's own JIT can never fail on a binding fault either; the actual risky construction is one
// level further down, in ConstructGridMap()/ConstructD20System(), whose call sites are where a
// fault surfaces — attributable to whichever module was current when it did.
static (object GridMap, object D20System, object Loot, OneD20SessionAssignment Assignment) Bootstrap()
{
    string? currentModule = null;
    Type? currentContract = null;

    try
    {
        // Plain Rulebook-tier data, no ROM/Cartridge type reference — safe to construct before the
        // isolated Construct*() call sites below without affecting this method's JIT-failure
        // isolation (that isolation is specifically about ROM/Cartridge assembly binding).
        var assignment = new OneD20SessionAssembler().Assemble();

        var registry = new ComponentRegistry();

        currentModule = "GridMap";
        var gridMap = ConstructGridMap();
        registry.Register(gridMap, EngineTier.Rom, "GridMap");

        currentModule = "D20System";
        var d20System = ConstructD20System(assignment);
        registry.Register(d20System, EngineTier.Rom, "D20System");

        currentModule = "Loot";
        var loot = ConstructLoot();
        registry.Register(loot, EngineTier.Rom, "Loot");

        currentModule = "OneD20";
        var (ruleBook, dependsOnRoms) = ConstructRuleBook();
        registry.Register(ruleBook, EngineTier.Rulebook, "OneD20");

        // Checked before Validate() deliberately (ticket 0047): DependsOnRoms is Rulebook-specific
        // vocabulary Validate()'s capability-based checks know nothing about, and in the shipped
        // GridMap/D20System pair a missing ROM would also incidentally trip a capability-mismatch
        // error — this way the DependsOnRoms-specific message is what's actually reported, not an
        // unrelated one that happens to fire for the same underlying cause.
        registry.ValidateDependsOn("OneD20", dependsOnRoms);

        currentModule = null;
        registry.Validate();
        Console.WriteLine("Component registry validated successfully.");

        // §5.3 member-resolution probe: force JIT of every Provides entry's contract implementation,
        // without invoking anything. Catches a member-signature change in the installed contract
        // assembly that §5.2's compile-time constraint (checked only against this ROM's own build)
        // can't see. Loot's Provides is empty (first pure-sink ROM shape) — the loop iterates zero
        // capabilities for it, no special-casing needed.
        foreach (var (component, name) in new (IHasDependencies Component, string Name)[]
            { (gridMap, "GridMap"), (d20System, "D20System"), (loot, "Loot") })
        {
            currentModule = name;
            foreach (var capability in component.Provides)
            {
                currentContract = capability.ContractType;
                capability.ForceMemberResolution();
            }
        }

        return (gridMap, d20System, loot, assignment);
    }
    catch (Exception ex) when (ex is TypeLoadException or MissingMethodException or MissingFieldException
        or FileLoadException or FileNotFoundException or BadImageFormatException)
    {
        var where = currentContract is not null
            ? $"module '{currentModule}', contract '{currentContract.FullName}'"
            : $"module '{currentModule}'";
        throw new BootstrapContractException(
            $"Bootstrap contract binding failure in {where}: {ex.GetType().Name}: {ex.Message}", ex);
    }
}

/// <summary>
/// Isolated so a binding fault touching GridMap or the Cartridge contract surface fails to JIT
/// exactly here, at this call site, with currentModule already set to "GridMap" in the caller —
/// not somewhere inside a larger method sharing state with D20System's construction.
/// </summary>
static IHasDependencies ConstructGridMap()
{
    var roomData = OneD20RulebookContent.Rooms.Values
        .Select(r => new GridRoomData(r.Id, r.Row, r.Col, r.DescriptionKey))
        .ToList();
    var rom = new GridMapRom(roomData, OneD20RulebookContent.StartRoomId);
    _ = rom.Provides;
    _ = rom.Requires;
    return rom;
}

/// <summary>Same isolation reasoning as <see cref="ConstructGridMap"/>, for D20System. Builds each
/// room's RoomChallenge from this session's assignment: the assigned pool challenge for
/// skill-check rooms, or the assigned MonsterId for fight rooms.</summary>
static IHasDependencies ConstructD20System(OneD20SessionAssignment assignment)
{
    var challenges = OneD20RulebookContent.Rooms.Values
        .Select(r => r.ChallengeType == OneD20ChallengeType.Fight
            ? new RoomChallenge(r.Id, RoomChallengeType.Fight, null, null, null, null, null, null,
                MonsterId: assignment.MonsterIdByRoom[r.Id])
            : ToRoomChallenge(r.Id, OneD20RulebookContent.Challenges[assignment.ChallengeKeyByRoom[r.Id]]))
        .ToDictionary(c => c.Id);
    var playerStats = OneD20RulebookContent.PlayerCharacter;
    var monsters = OneD20RulebookContent.Monsters.Values
        .Select(m => new MonsterTemplate(m.Id, m.Name, m.DescriptionKey,
            new CombatantStats(m.Stats.Defense, m.Stats.AttackBonus, m.Stats.DamageDie, m.Stats.DamageBonus, m.Stats.MaxHp)))
        .ToDictionary(m => m.Id);
    var rom = new D20SystemRom(
        new PlayerCharacter(new CombatantStats(playerStats.Defense, playerStats.AttackBonus, playerStats.DamageDie, playerStats.DamageBonus, playerStats.MaxHp)),
        monsters,
        challenges,
        OneD20RulebookContent.PlayerAbilityScores);
    _ = rom.Provides;
    _ = rom.Requires;
    return rom;
}

/// <summary>Translates one Rulebook-owned pool challenge into D20System's RoomChallenge, assigned to
/// <paramref name="roomId"/> this session.</summary>
static RoomChallenge ToRoomChallenge(int roomId, OneD20ChallengeTemplate challenge) =>
    new(roomId, RoomChallengeType.SkillCheck, challenge.Skill, challenge.SkillAbility, challenge.DC,
        challenge.SuccessKey, challenge.FailureKey, challenge.PromptKey, MonsterId: null);

/// <summary>Same isolation reasoning as <see cref="ConstructGridMap"/>, for Loot. The full 5-entry
/// loot table is always constructed — session assignment only decides which 3 monsters (and thus
/// which 3 items) are actually in play this session.</summary>
static IHasDependencies ConstructLoot()
{
    var lootTable = new MonsterLootTable(OneD20RulebookContent.ItemIdByMonsterId);
    var rom = new LootRom(lootTable);
    _ = rom.Provides;
    _ = rom.Requires;
    return rom;
}

/// <summary>
/// Same isolation reasoning as <see cref="ConstructGridMap"/>, for the Rulebook. Returns
/// <c>DependsOnRoms</c> alongside the component (rather than requiring the caller to know about
/// <c>IRuleBook</c>, a Rulebook-tier-specific interface) so <see cref="Bootstrap"/> itself never
/// needs to name a Rulebook type either — plain strings are as safe as <see cref="IHasDependencies"/>.
/// </summary>
static (IHasDependencies Component, IReadOnlyList<string> DependsOnRoms) ConstructRuleBook()
{
    var ruleBook = new OneD20RuleBook();
    return (ruleBook, ruleBook.DependsOnRoms);
}

// Downstream gameplay wiring. Only ever called after Bootstrap() has already returned successfully
// — which means these same GridMap/D20System types were just proven loadable — so casting back to
// the concrete types here introduces no new binding risk; this is legitimately outside §5's scope
// (construction/registration/validation/probing), not a gap in it.
static void RunGame(object gridMapObj, object d20SystemObj, object lootObj, OneD20SessionAssignment assignment)
{
    // RoslynRuleCompiler only sees already-loaded, file-backed assemblies (see its own doc comment)
    // — since 1.d20 has no hand-written ARule anywhere in this process to pull these in as a side
    // effect the way Nine Rooms' rules did, force them before the first RuleExport.Compile call. Same
    // gotcha the pipeline's own tests hit and work around (RulePipelineGeneratorServiceTests.cs).
    _ = typeof(NRules.Fluent.Dsl.Rule).Assembly;
    _ = typeof(System.Linq.Expressions.Expression).Assembly;
    _ = typeof(NRules.RuleModel.IContext).Assembly;
    _ = typeof(NRules.ISession).Assembly;

    var gridMap = (GridMapRom)gridMapObj;
    var d20System = (D20SystemRom)d20SystemObj;
    _ = (LootRom)lootObj; // Loot's static content (the loot table) is used only by Bootstrap()/ConstructLoot(); nothing here reads it back.

    var gridMapRoomData = gridMap.RoomData;
    var playerStats = OneD20RulebookContent.PlayerCharacter;

    // Which monster's stat block applies to a given fight room this session — GridMap's LookCommandHandler
    // needs this to report combat status without ever referencing D20System's Monsters dictionary.
    var monsterMaxHpByRoom = assignment.MonsterIdByRoom.ToDictionary(
        kv => kv.Key,
        kv => OneD20RulebookContent.Monsters[kv.Value].Stats.MaxHp);

    // No broker: GridMap's movement service is constructed directly and handed to whoever needs it.
    var movement = new GridMovementService();
    var flavorText = OneD20FlavorText.Text;

    var engine = new ConsoleEngine()
        .ConfigureGameEngine(opts =>
        {
            opts.GameName = "My LunaFlow Game";
            opts.EnableRulesEngine = true;
            opts.Rules
                .FromAssembly(RuleExport.Compile(typeof(GridMapRuleGroup)))
                .FromAssembly(RuleExport.Compile(typeof(D20SystemRuleGroup)))
                .FromAssembly(RuleExport.Compile(typeof(LootRuleGroup)))
                .Seed(() => gridMapRoomData)
                .Seed(() =>
                {
                    // 1-d20-spec.md's explicit fix: PlayerPosition(5) is seeded directly, never reached
                    // via `move`, so MoveCommandHandler's DescriptionKey announcement never runs for it —
                    // announce room 5's description here, once, before anything else.
                    var startRoom = OneD20RulebookContent.Rooms[OneD20RulebookContent.StartRoomId];
                    LunaFlowEngine.Host.Announce(flavorText[startRoom.DescriptionKey]);
                    return new object[]
                    {
                        new PlayerPosition(OneD20RulebookContent.StartRoomId),
                        d20System.PlayerCharacter,
                    };
                })
                .Seed(() =>
                {
                    // Ticket 0043 (Amara's ruling), applied per-session: generated RulePrototype rules
                    // have no constructor/DI seam, so they can't resolve a flavor key (or a monster
                    // assignment) themselves. Resolve each room's prompt text once, here — the one
                    // place the Rulebook's flavor map, the session's assignment, and the room data
                    // already meet — and hand the rule only the finished string.
                    var prompts = OneD20RulebookContent.Rooms.Values
                        .Select(r => (object)new RoomChallengePrompt(
                            r.Id,
                            r.ChallengeType == OneD20ChallengeType.Fight
                                ? flavorText[OneD20RulebookContent.Monsters[assignment.MonsterIdByRoom[r.Id]].DescriptionKey]
                                : flavorText[OneD20RulebookContent.Challenges[assignment.ChallengeKeyByRoom[r.Id]].PromptKey]));
                    return prompts.Append(new GameEndAnnouncement(flavorText["win-announce"]));
                })
                .Seed(() =>
                {
                    // LootAssignment: one per fight room, derived from this session's monster
                    // assignment. LootPickupText: same seeded-fact pattern as RoomChallengePrompt
                    // above, one per item in the full 5-item table (harmless for items not in play
                    // this session — the award rule only fires with a matching LootAssignment).
                    var assignments = assignment.ItemIdByRoom
                        .Select(kv => (object)new LootAssignment(kv.Key, kv.Value));
                    var pickupTexts = OneD20RulebookContent.ItemIdByMonsterId.Values
                        .Select(itemId => (object)new LootPickupText(itemId, flavorText[$"item-{itemId}-pickup"]));
                    return assignments.Concat(pickupTexts);
                });
        })
        .ConfigureCommand(opts =>
        {
            var mechanic = d20System.Mechanic;
            opts.CommandDispatcher.RegisterHandler("quit", new QuitCommandHandler(), "Stop the engine and exit.");
            opts.CommandDispatcher.RegisterHandler("exit", new QuitCommandHandler(), "Stop the engine and exit.");
            opts.CommandDispatcher.RegisterHandler("echo", new EchoCommandHandler(), "Print back the given arguments.");
            opts.CommandDispatcher.RegisterHandler("help", new HelpCommandHandler(opts.CommandDispatcher), "List available commands.");
            opts.CommandDispatcher.RegisterHandler("move", new MoveCommandHandler(gridMap, flavorText), "Move: move north/south/east/west.");
            opts.CommandDispatcher.RegisterHandler("look", new LookCommandHandler(gridMap, flavorText, playerStats.MaxHp, monsterMaxHpByRoom), "Redisplay the current room's description.");
            opts.CommandDispatcher.RegisterHandler("check", new CheckCommandHandler(d20System, flavorText, mechanic), "Attempt the current room's skill check.");
            opts.CommandDispatcher.RegisterHandler("fight", new FightCommandHandler(d20System, flavorText, mechanic), "Attack the monster in the current room.");
            opts.CommandDispatcher.RegisterHandler("flee", new FleeCommandHandler(movement, flavorText), "Retreat from the current fight.");
            opts.CommandDispatcher.RegisterHandler("inventory", new InventoryCommandHandler(flavorText), "List your collected items.");
            opts.CommandDispatcher.RegisterHandler("inv", new InventoryCommandHandler(flavorText), "List your collected items.");
        });

    engine.Run();
}

/// <summary>Wraps a §5.1 binding-exception-family fault with which module/contract was being
/// processed. Caught by the top-level bootstrap try/catch above; never allowed to propagate
/// unhandled — that would defeat the point of controlling where the CLR's binding check fires.</summary>
sealed class BootstrapContractException : Exception
{
    public BootstrapContractException(string message, Exception inner) : base(message, inner) { }
}
