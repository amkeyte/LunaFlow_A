using LunaFlow.Engine;
using LunaFlow.Engine.Game;
using LunaFlow.Rom.Loot;
using LunaFlow.Rulebook.OneD20;
using LunaFlow.Rules.GeneratorServices;

namespace LunaFlow.ContentTests.Rom.Loot
{
    /// <summary>
    /// Handler-level coverage for <see cref="InventoryCommandHandler"/> (<c>inventory</c>/<c>inv</c>),
    /// driven through the real <see cref="GameEngineCore"/> singleton and
    /// <see cref="DispatchGameCommand"/> pipeline — same real-path requirement as
    /// <c>D20SystemHandlerTests</c>. Loads only Loot's own compiled <see cref="LootRuleGroup"/>
    /// assembly (required to initialize the NRules session at all — <c>RuleEngine.Insert</c> throws
    /// otherwise) and never GridMap's or D20System's, to pin the handler's own claim: it reads only
    /// its own <see cref="ItemAwarded"/> facts, no cross-ROM read.
    /// </summary>
    [TestClass]
    [TestCategory("Rom_OneD20")]
    [TestCategory("Phase2.7.1")]
    [DoNotParallelize] // GameEngineCore.Instance is a singleton; tests must run sequentially
    public class InventoryCommandHandlerTests
    {
        private static System.Reflection.Assembly _compiledLootRules = null!;
        private GameEngineCore _engine = null!;
        private DefaultCommandDispatcher _dispatcher = null!;
        private readonly List<string> _messages = new();
        private EventHandler<GameEngineMessagePostedEventArgs>? _messageHandler;

        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            // RoslynRuleCompiler only sees already-loaded, file-backed assemblies — force NRules in
            // before the first Compile() call. Same gotcha D20SystemRuleFixtureTests works around.
            // LootRuleGroup.AwardItem also names LunaFlow.Cartridge.IChallengeSolved in its
            // RequiredUsings, so that assembly needs forcing too — relying on some other test class
            // happening to load it first (run-order dependent) produced an intermittent
            // "type or namespace 'Cartridge' does not exist" compile failure when this class ran
            // filtered/first.
            _ = typeof(NRules.Fluent.Dsl.Rule).Assembly;
            _ = typeof(System.Linq.Expressions.Expression).Assembly;
            _ = typeof(NRules.RuleModel.IContext).Assembly;
            _ = typeof(NRules.ISession).Assembly;
            _ = typeof(LunaFlow.Cartridge.IChallengeSolved).Assembly;

            _compiledLootRules = RuleExport.Compile(typeof(LootRuleGroup));
        }

        [TestInitialize]
        public void Setup()
        {
            _engine = GameEngineCore.Instance;
            if (_engine.Running)
                _engine.Stop(force: true);
            _messages.Clear();
            _dispatcher = new DefaultCommandDispatcher();
            _dispatcher.RegisterHandler("inventory", new InventoryCommandHandler(OneD20FlavorText.Text));
            _dispatcher.RegisterHandler("inv", new InventoryCommandHandler(OneD20FlavorText.Text));

            _messageHandler = (s, e) => _messages.Add(e.Message);
            _engine.MessagePosted += _messageHandler;
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_messageHandler is not null)
                _engine.MessagePosted -= _messageHandler;
            _messageHandler = null;
            if (_engine.Running)
                _engine.Stop(force: true);
        }

        private void StartWithFacts(params object[] facts)
        {
            var options = new GameEngineOptions();
            options.Rules.FromAssembly(_compiledLootRules).Seed(() => facts);
            _engine.SetOptions(options);
            _engine.Start();
        }

        private void Submit(string commandLine)
        {
            var command = new DefaultCommand(commandLine);
            using var completed = new ManualResetEventSlim(false);
            _engine.Submit(new DispatchGameCommand(command, _dispatcher, completed));
            Assert.IsTrue(completed.Wait(TimeSpan.FromSeconds(2)), $"Command '{commandLine}' should complete");
        }

        [TestMethod]
        public void Inventory_NoItemsAwarded_AnnouncesEmptyMessage()
        {
            StartWithFacts();
            var baseline = _messages.Count;

            Submit("inventory");

            Assert.IsTrue(_messages.Skip(baseline).Contains(OneD20FlavorText.Text["inventory-empty"]));
        }

        [TestMethod]
        public void Inventory_ItemsAwarded_AnnouncesHeaderThenEachItemByName()
        {
            StartWithFacts(new ItemAwarded("rapier", 1), new ItemAwarded("vest", 5));
            var baseline = _messages.Count;

            Submit("inventory");

            var since = _messages.Skip(baseline).ToList();
            Assert.IsTrue(since.Contains(OneD20FlavorText.Text["inventory-header"]));
            Assert.IsTrue(since.Contains(OneD20FlavorText.Text["item-rapier-name"]));
            Assert.IsTrue(since.Contains(OneD20FlavorText.Text["item-vest-name"]));
            Assert.IsFalse(since.Contains(OneD20FlavorText.Text["inventory-empty"]),
                "A populated inventory must never also announce the empty message");
        }

        [TestMethod]
        public void Inv_IsAnAliasForInventory_SameHandlerSameOutput()
        {
            StartWithFacts(new ItemAwarded("rapier", 1));
            var baseline = _messages.Count;

            Submit("inv");

            Assert.IsTrue(_messages.Skip(baseline).Contains(OneD20FlavorText.Text["item-rapier-name"]));
        }

        [TestMethod]
        public void Inventory_WithNoGridMapOrD20SystemRulesLoaded_StillWorks_ReadsOnlyItsOwnFacts()
        {
            // No GridMapRuleGroup/D20SystemRuleGroup compiled or loaded at all — if the handler read
            // anything beyond ItemAwarded (a GridMap/D20System capability, a peer ROM's fact type),
            // this would have nothing to resolve against. It succeeds because Handle() only ever
            // queries ItemAwarded, exactly as its own doc comment claims.
            StartWithFacts(new ItemAwarded("knife", 8));
            var baseline = _messages.Count;

            Submit("inventory");

            Assert.IsTrue(_messages.Skip(baseline).Contains(OneD20FlavorText.Text["item-knife-name"]));
        }
    }
}
