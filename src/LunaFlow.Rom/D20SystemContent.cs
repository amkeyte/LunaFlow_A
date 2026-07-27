using System;

namespace LunaFlow.Rom.D20System
{
    /// <summary>
    /// Legacy static holder for D20System data. Deprecated in Phase 2.7 in favor of constructor injection
    /// on D20SystemRom and GridMapRom. Retained temporarily for regression tests (Nine Rooms).
    /// Uses object types to avoid cross-project dependencies.
    /// </summary>
    [Obsolete("Use constructor injection on D20SystemRom and GridMapRom instead. This class exists only for regression-test compatibility.")]
    public static class D20SystemContent
    {
        private static object? _rooms;
        private static object? _flavorText;
        private static object? _playerCharacter;
        private static object? _monster;
        private static object? _playerAbilityScores;
        private static int _startRoomId;

        /// <summary>Configure the static holder with rulebook data (Nine Rooms regression support only).</summary>
        [Obsolete("Use constructor injection instead.")]
        public static void Configure(
            object rooms,
            object flavorText,
            object playerCharacter,
            object monster,
            object playerAbilityScores,
            int startRoomId)
        {
            _rooms = rooms;
            _flavorText = flavorText;
            _playerCharacter = playerCharacter;
            _monster = monster;
            _playerAbilityScores = playerAbilityScores;
            _startRoomId = startRoomId;
        }

        /// <summary>The nine rooms (Nine Rooms regression support only).</summary>
        [Obsolete("Use constructor injection instead.")]
        public static object? Rooms => _rooms ?? new Dictionary<int, object>();

        /// <summary>Flavor text by key (Nine Rooms regression support only).</summary>
        [Obsolete("Use constructor injection instead.")]
        public static object? FlavorText => _flavorText ?? new Dictionary<string, string>();

        /// <summary>The pregenerated player character (Nine Rooms regression support only).</summary>
        [Obsolete("Use constructor injection instead.")]
        public static object? PlayerCharacter => _playerCharacter ?? throw new InvalidOperationException("D20SystemContent not configured");

        /// <summary>The monster template (Nine Rooms regression support only).</summary>
        [Obsolete("Use constructor injection instead.")]
        public static object? Monster => _monster ?? throw new InvalidOperationException("D20SystemContent not configured");

        /// <summary>Player ability scores (Nine Rooms regression support only).</summary>
        [Obsolete("Use constructor injection instead.")]
        public static object? PlayerAbilityScores => _playerAbilityScores ?? new Dictionary<string, int>();

        /// <summary>Start room ID (Nine Rooms regression support only).</summary>
        [Obsolete("Use constructor injection instead.")]
        public static int StartRoomId => _startRoomId;
    }
}
