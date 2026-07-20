using LunaFlow.Engine;
using LunaFlow.Rules;

namespace LunaFlow.Rom.NineRooms
{
    /// <summary>
    /// Announces the win-line name and its rooms once any <see cref="WinMarker"/> is inserted.
    /// </summary>
    public sealed class GameEndRule : ARule
    {
        /// <inheritdoc/>
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("GameEnd");

        /// <inheritdoc/>
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
