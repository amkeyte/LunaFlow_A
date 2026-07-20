using LunaFlow.Rules;

namespace LunaFlow.Rom.NineRooms
{
    /// <summary>
    /// Inserts a <see cref="WinMarker"/> once every room in the top row (rooms 1, 2, 3) is solved.
    /// </summary>
    public sealed class WinLineTopRowRule : ARule
    {
        /// <inheritdoc/>
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("WinLineTopRow");

        /// <inheritdoc/>
        public override void Define()
        {
            When()
                .Match<RoomSolved>(rs => rs.RoomId == 1)
                .Match<RoomSolved>(rs => rs.RoomId == 2)
                .Match<RoomSolved>(rs => rs.RoomId == 3)
                .Not<WinMarker>();
            Then()
                .Do(ctx => ctx.Insert(new WinMarker(new[] { 1, 2, 3 })));
        }
    }

    /// <summary>
    /// Inserts a <see cref="WinMarker"/> once every room in the middle row (rooms 4, 5, 6) is solved.
    /// </summary>
    public sealed class WinLineMiddleRowRule : ARule
    {
        /// <inheritdoc/>
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("WinLineMiddleRow");

        /// <inheritdoc/>
        public override void Define()
        {
            When()
                .Match<RoomSolved>(rs => rs.RoomId == 4)
                .Match<RoomSolved>(rs => rs.RoomId == 5)
                .Match<RoomSolved>(rs => rs.RoomId == 6)
                .Not<WinMarker>();
            Then()
                .Do(ctx => ctx.Insert(new WinMarker(new[] { 4, 5, 6 })));
        }
    }

    /// <summary>
    /// Inserts a <see cref="WinMarker"/> once every room in the bottom row (rooms 7, 8, 9) is solved.
    /// </summary>
    public sealed class WinLineBottomRowRule : ARule
    {
        /// <inheritdoc/>
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("WinLineBottomRow");

        /// <inheritdoc/>
        public override void Define()
        {
            When()
                .Match<RoomSolved>(rs => rs.RoomId == 7)
                .Match<RoomSolved>(rs => rs.RoomId == 8)
                .Match<RoomSolved>(rs => rs.RoomId == 9)
                .Not<WinMarker>();
            Then()
                .Do(ctx => ctx.Insert(new WinMarker(new[] { 7, 8, 9 })));
        }
    }

    /// <summary>
    /// Inserts a <see cref="WinMarker"/> once every room in the left column (rooms 1, 4, 7) is solved.
    /// </summary>
    public sealed class WinLineLeftColumnRule : ARule
    {
        /// <inheritdoc/>
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("WinLineLeftColumn");

        /// <inheritdoc/>
        public override void Define()
        {
            When()
                .Match<RoomSolved>(rs => rs.RoomId == 1)
                .Match<RoomSolved>(rs => rs.RoomId == 4)
                .Match<RoomSolved>(rs => rs.RoomId == 7)
                .Not<WinMarker>();
            Then()
                .Do(ctx => ctx.Insert(new WinMarker(new[] { 1, 4, 7 })));
        }
    }

    /// <summary>
    /// Inserts a <see cref="WinMarker"/> once every room in the middle column (rooms 2, 5, 8) is solved.
    /// </summary>
    public sealed class WinLineMiddleColumnRule : ARule
    {
        /// <inheritdoc/>
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("WinLineMiddleColumn");

        /// <inheritdoc/>
        public override void Define()
        {
            When()
                .Match<RoomSolved>(rs => rs.RoomId == 2)
                .Match<RoomSolved>(rs => rs.RoomId == 5)
                .Match<RoomSolved>(rs => rs.RoomId == 8)
                .Not<WinMarker>();
            Then()
                .Do(ctx => ctx.Insert(new WinMarker(new[] { 2, 5, 8 })));
        }
    }

    /// <summary>
    /// Inserts a <see cref="WinMarker"/> once every room in the right column (rooms 3, 6, 9) is solved.
    /// </summary>
    public sealed class WinLineRightColumnRule : ARule
    {
        /// <inheritdoc/>
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("WinLineRightColumn");

        /// <inheritdoc/>
        public override void Define()
        {
            When()
                .Match<RoomSolved>(rs => rs.RoomId == 3)
                .Match<RoomSolved>(rs => rs.RoomId == 6)
                .Match<RoomSolved>(rs => rs.RoomId == 9)
                .Not<WinMarker>();
            Then()
                .Do(ctx => ctx.Insert(new WinMarker(new[] { 3, 6, 9 })));
        }
    }

    /// <summary>
    /// Inserts a <see cref="WinMarker"/> once every room in the top-left-to-bottom-right diagonal (rooms 1, 5, 9) is solved.
    /// </summary>
    public sealed class WinLineDiagonalDownRule : ARule
    {
        /// <inheritdoc/>
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("WinLineDiagonalDown");

        /// <inheritdoc/>
        public override void Define()
        {
            When()
                .Match<RoomSolved>(rs => rs.RoomId == 1)
                .Match<RoomSolved>(rs => rs.RoomId == 5)
                .Match<RoomSolved>(rs => rs.RoomId == 9)
                .Not<WinMarker>();
            Then()
                .Do(ctx => ctx.Insert(new WinMarker(new[] { 1, 5, 9 })));
        }
    }

    /// <summary>
    /// Inserts a <see cref="WinMarker"/> once every room in the bottom-left-to-top-right diagonal (rooms 3, 5, 7) is solved.
    /// </summary>
    public sealed class WinLineDiagonalUpRule : ARule
    {
        /// <inheritdoc/>
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("WinLineDiagonalUp");

        /// <inheritdoc/>
        public override void Define()
        {
            When()
                .Match<RoomSolved>(rs => rs.RoomId == 3)
                .Match<RoomSolved>(rs => rs.RoomId == 5)
                .Match<RoomSolved>(rs => rs.RoomId == 7)
                .Not<WinMarker>();
            Then()
                .Do(ctx => ctx.Insert(new WinMarker(new[] { 3, 5, 7 })));
        }
    }
}
