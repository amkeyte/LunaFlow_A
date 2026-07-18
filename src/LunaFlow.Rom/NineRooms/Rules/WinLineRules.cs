using LunaFlow.Rules;

namespace LunaFlow.Rom.NineRooms
{
    public sealed class WinLineTopRowRule : ARule
    {
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("WinLineTopRow");

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

    public sealed class WinLineMiddleRowRule : ARule
    {
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("WinLineMiddleRow");

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

    public sealed class WinLineBottomRowRule : ARule
    {
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("WinLineBottomRow");

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

    public sealed class WinLineLeftColumnRule : ARule
    {
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("WinLineLeftColumn");

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

    public sealed class WinLineMiddleColumnRule : ARule
    {
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("WinLineMiddleColumn");

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

    public sealed class WinLineRightColumnRule : ARule
    {
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("WinLineRightColumn");

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

    public sealed class WinLineDiagonalDownRule : ARule
    {
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("WinLineDiagonalDown");

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

    public sealed class WinLineDiagonalUpRule : ARule
    {
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("WinLineDiagonalUp");

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
