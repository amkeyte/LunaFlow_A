using LunaFlow.Engine.Components;
using LunaFlow.Rom;
using LunaFlow.Rules;

namespace LunaFlow.Rom.GridMap
{
    /// <summary>
    /// The GridMap ROM: handles spatial concerns for the 1.d20 grid.
    /// Owns facts: PlayerPosition, PreviousRoomId, WinMarker.
    /// Provides: PlayerLocation, PlayerMovement capabilities.
    /// Requires: ChallengeSolved capability (from D20System).
    /// </summary>
    public sealed class GridMapRom : IRom
    {
        private readonly IReadOnlyList<GridRoomData> _roomData;
        private readonly int _startRoomId;

        /// <summary>Constructs the GridMap ROM with grid room data and start position.</summary>
        public GridMapRom(IReadOnlyList<GridRoomData> roomData, int startRoomId)
        {
            _roomData = roomData;
            _startRoomId = startRoomId;
        }

        /// <summary>The ROM's display name.</summary>
        public string Name => "GridMap";

        /// <summary>Credits for the GridMap ROM.</summary>
        public string Credits => "Spatial navigation and win-condition logic";

        /// <summary>Capabilities this ROM provides: PlayerLocation and PlayerMovement.</summary>
        public IReadOnlyList<Capability> Provides => new[]
        {
            new Capability("PlayerLocation", typeof(LunaFlow.Rom.D20SystemInterop.IPlayerLocation)),
            new Capability("PlayerMovement", typeof(LunaFlow.Rom.D20SystemInterop.IPlayerMovement)),
        };

        /// <summary>Capabilities this ROM requires: none. Win-line rules match IChallengeSolved facts through NRules, not capability mediation.</summary>
        public IReadOnlyList<Capability> Requires => Array.Empty<Capability>();

        /// <summary>The ROM's LunaFlow reference.</summary>
        public ILunaFlowReference Reference => new GridMapReference();

        /// <summary>Provides access to room data for handlers.</summary>
        public IReadOnlyList<GridRoomData> RoomData => _roomData;

        /// <summary>Provides the start room ID for initialization.</summary>
        public int StartRoomId => _startRoomId;
    }

    /// <summary>Reference identity for GridMap ROM.</summary>
    internal sealed class GridMapReference : ILunaFlowReference
    {
        public string ClassUID => "GridMap";
        public string Identifier => "GridMap";
        public string FullPath => $"GridMap.{Identifier}";
        public string Path => Identifier;
        public ILunaFlowReference Parent => null!;
        public IEnumerable<ILunaFlowReference> Subreferences => Array.Empty<ILunaFlowReference>();
    }
}
