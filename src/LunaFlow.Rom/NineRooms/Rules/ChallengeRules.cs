using LunaFlow.Engine;
using LunaFlow.Rules;

namespace LunaFlow.Rom.NineRooms
{
    /// <summary>
    /// Deprecated — see <see cref="NineRoomsContent"/>'s Remarks. Announces Room 1's
    /// challenge prompt while the player is there, the room is unsolved, and no win line has
    /// fired yet.
    /// </summary>
    public sealed class Room1ChallengeRule : ARule
    {
        /// <inheritdoc/>
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("Room1Challenge");

        /// <inheritdoc/>
        public override void Define()
        {
            Room room = null!;
            When()
                .Match<PlayerPosition>(p => p.RoomId == 1)
                .Match<Room>(() => room, r => r.Id == 1)
                .Not<RoomSolved>(rs => rs.RoomId == 1)
                .Not<WinMarker>();
            Then()
                .Do(ctx => LunaFlowEngine.Host.Announce(room.ChallengePrompt!));
        }
    }

    /// <summary>
    /// Deprecated — see <see cref="NineRoomsContent"/>'s Remarks. Announces Room 2's
    /// challenge prompt while the player is there, the room is unsolved, and no win line has
    /// fired yet.
    /// </summary>
    public sealed class Room2ChallengeRule : ARule
    {
        /// <inheritdoc/>
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("Room2Challenge");

        /// <inheritdoc/>
        public override void Define()
        {
            Room room = null!;
            When()
                .Match<PlayerPosition>(p => p.RoomId == 2)
                .Match<Room>(() => room, r => r.Id == 2)
                .Not<RoomSolved>(rs => rs.RoomId == 2)
                .Not<WinMarker>();
            Then()
                .Do(ctx => LunaFlowEngine.Host.Announce(room.ChallengePrompt!));
        }
    }

    /// <summary>
    /// Deprecated — see <see cref="NineRoomsContent"/>'s Remarks. Announces Room 3's
    /// challenge prompt while the player is there, the room is unsolved, and no win line has
    /// fired yet.
    /// </summary>
    public sealed class Room3ChallengeRule : ARule
    {
        /// <inheritdoc/>
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("Room3Challenge");

        /// <inheritdoc/>
        public override void Define()
        {
            Room room = null!;
            When()
                .Match<PlayerPosition>(p => p.RoomId == 3)
                .Match<Room>(() => room, r => r.Id == 3)
                .Not<RoomSolved>(rs => rs.RoomId == 3)
                .Not<WinMarker>();
            Then()
                .Do(ctx => LunaFlowEngine.Host.Announce(room.ChallengePrompt!));
        }
    }

    /// <summary>
    /// Deprecated — see <see cref="NineRoomsContent"/>'s Remarks. Announces Room 4's
    /// challenge prompt while the player is there, the room is unsolved, and no win line has
    /// fired yet.
    /// </summary>
    public sealed class Room4ChallengeRule : ARule
    {
        /// <inheritdoc/>
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("Room4Challenge");

        /// <inheritdoc/>
        public override void Define()
        {
            Room room = null!;
            When()
                .Match<PlayerPosition>(p => p.RoomId == 4)
                .Match<Room>(() => room, r => r.Id == 4)
                .Not<RoomSolved>(rs => rs.RoomId == 4)
                .Not<WinMarker>();
            Then()
                .Do(ctx => LunaFlowEngine.Host.Announce(room.ChallengePrompt!));
        }
    }

    /// <summary>
    /// Deprecated — see <see cref="NineRoomsContent"/>'s Remarks. Announces Room 5's
    /// challenge prompt while the player is there, the room is unsolved, and no win line has
    /// fired yet.
    /// </summary>
    public sealed class Room5ChallengeRule : ARule
    {
        /// <inheritdoc/>
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("Room5Challenge");

        /// <inheritdoc/>
        public override void Define()
        {
            Room room = null!;
            When()
                .Match<PlayerPosition>(p => p.RoomId == 5)
                .Match<Room>(() => room, r => r.Id == 5)
                .Not<RoomSolved>(rs => rs.RoomId == 5)
                .Not<WinMarker>();
            Then()
                .Do(ctx => LunaFlowEngine.Host.Announce(room.ChallengePrompt!));
        }
    }

    /// <summary>
    /// Deprecated — see <see cref="NineRoomsContent"/>'s Remarks. Announces Room 6's
    /// challenge prompt while the player is there, the room is unsolved, and no win line has
    /// fired yet.
    /// </summary>
    public sealed class Room6ChallengeRule : ARule
    {
        /// <inheritdoc/>
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("Room6Challenge");

        /// <inheritdoc/>
        public override void Define()
        {
            Room room = null!;
            When()
                .Match<PlayerPosition>(p => p.RoomId == 6)
                .Match<Room>(() => room, r => r.Id == 6)
                .Not<RoomSolved>(rs => rs.RoomId == 6)
                .Not<WinMarker>();
            Then()
                .Do(ctx => LunaFlowEngine.Host.Announce(room.ChallengePrompt!));
        }
    }

    /// <summary>
    /// Deprecated — see <see cref="NineRoomsContent"/>'s Remarks. Announces Room 7's
    /// challenge prompt while the player is there, the room is unsolved, and no win line has
    /// fired yet.
    /// </summary>
    public sealed class Room7ChallengeRule : ARule
    {
        /// <inheritdoc/>
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("Room7Challenge");

        /// <inheritdoc/>
        public override void Define()
        {
            Room room = null!;
            When()
                .Match<PlayerPosition>(p => p.RoomId == 7)
                .Match<Room>(() => room, r => r.Id == 7)
                .Not<RoomSolved>(rs => rs.RoomId == 7)
                .Not<WinMarker>();
            Then()
                .Do(ctx => LunaFlowEngine.Host.Announce(room.ChallengePrompt!));
        }
    }

    /// <summary>
    /// Deprecated — see <see cref="NineRoomsContent"/>'s Remarks. Announces Room 8's
    /// challenge prompt while the player is there, the room is unsolved, and no win line has
    /// fired yet.
    /// </summary>
    public sealed class Room8ChallengeRule : ARule
    {
        /// <inheritdoc/>
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("Room8Challenge");

        /// <inheritdoc/>
        public override void Define()
        {
            Room room = null!;
            When()
                .Match<PlayerPosition>(p => p.RoomId == 8)
                .Match<Room>(() => room, r => r.Id == 8)
                .Not<RoomSolved>(rs => rs.RoomId == 8)
                .Not<WinMarker>();
            Then()
                .Do(ctx => LunaFlowEngine.Host.Announce(room.ChallengePrompt!));
        }
    }

    /// <summary>
    /// Deprecated — see <see cref="NineRoomsContent"/>'s Remarks. Announces Room 9's
    /// challenge prompt while the player is there, the room is unsolved, and no win line has
    /// fired yet.
    /// </summary>
    public sealed class Room9ChallengeRule : ARule
    {
        /// <inheritdoc/>
        public override ILunaFlowReference Reference { get; } = new NineRoomsReference("Room9Challenge");

        /// <inheritdoc/>
        public override void Define()
        {
            Room room = null!;
            When()
                .Match<PlayerPosition>(p => p.RoomId == 9)
                .Match<Room>(() => room, r => r.Id == 9)
                .Not<RoomSolved>(rs => rs.RoomId == 9)
                .Not<WinMarker>();
            Then()
                .Do(ctx => LunaFlowEngine.Host.Announce(room.ChallengePrompt!));
        }
    }
}
