namespace LunaFlow.Rom.NineRooms
{
    public static class NineRoomsContent
    {
        public static readonly IReadOnlyDictionary<int, Room> Rooms = new Dictionary<int, Room>
        {
            [1] = new Room(1, 0, 0, "The Gatehouse", "The Gatehouse.", "What is 7 + 5?", "12"),
            [2] = new Room(2, 0, 1, "The Long Hall", "The Long Hall.", "I have keys but no locks, space but no room. What am I?", "keyboard"),
            [3] = new Room(3, 0, 2, "The Observatory", "The Observatory.", "What is 9 × 3?", "27"),
            [4] = new Room(4, 1, 0, "The Cellar", "The Cellar.", "The more you take, the more you leave behind. What am I?", "footsteps"),
            [5] = new Room(5, 1, 1, "The Atrium", "The Atrium.", "What is 6 × 6?", "36"),
            [6] = new Room(6, 1, 2, "The Armory", "The Armory.", "What is 15 − 6?", "9"),
            [7] = new Room(7, 2, 0, "The Well", "The Well.", "What has a neck but no head?", "bottle"),
            [8] = new Room(8, 2, 1, "The Garden", "The Garden.", "What is 4 squared?", "16"),
            [9] = new Room(9, 2, 2, "The Tower", "The Tower.", "What comes once in a minute, twice in a moment, but never in a thousand years?", "the letter m"),
        };

        public const int StartRoomId = 5;

        public static bool TryMove(int fromId, string direction, out int toId)
        {
            switch (direction.ToLowerInvariant())
            {
                case "north":
                    toId = fromId - 3;
                    return toId >= 1;
                case "south":
                    toId = fromId + 3;
                    return toId <= 9;
                case "east":
                    toId = fromId + 1;
                    return (fromId - 1) % 3 != 2;
                case "west":
                    toId = fromId - 1;
                    return (fromId - 1) % 3 != 0;
                default:
                    toId = -1;
                    return false;
            }
        }

        public static void Validate()
        {
            var reachable = WinLineCatalog.ReachableLines(Rooms);
            if (reachable.Count != WinLineCatalog.AllLines.Length)
            {
                throw new InvalidOperationException(
                    $"Nine Rooms content is misconfigured: only {reachable.Count} of " +
                    $"{WinLineCatalog.AllLines.Length} win lines are reachable. Every room must " +
                    "have a Challenge/Answer for all win lines to stay winnable.");
            }
        }
    }
}
