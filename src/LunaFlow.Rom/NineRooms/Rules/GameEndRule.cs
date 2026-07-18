using LunaFlow.Engine;
using LunaFlow.Rules;

namespace LunaFlow.Rom.NineRooms
{
    public sealed class GameEndRule : ARule
    {
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("GameEnd");

        public override void Define()
        {
            WinMarker marker = null!;
            When()
                .Match<WinMarker>(() => marker);
            Then()
                .Do(ctx => LunaFlowEngine.Host.Announce(
                    $"You solved the {WinLineCatalog.Name(marker.Line)}! (rooms {string.Join(", ", marker.Line)})"));
        }
    }
}
