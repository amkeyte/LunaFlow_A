namespace LunaFlow.Rom.NineRooms
{
    internal static class WinLineCatalog
    {
        public static readonly int[][] AllLines =
        {
            new[] { 1, 2, 3 }, new[] { 4, 5, 6 }, new[] { 7, 8, 9 },
            new[] { 1, 4, 7 }, new[] { 2, 5, 8 }, new[] { 3, 6, 9 },
            new[] { 1, 5, 9 }, new[] { 3, 5, 7 },
        };

        private static readonly Dictionary<string, string> Names = new()
        {
            ["1,2,3"] = "top row",
            ["4,5,6"] = "middle row",
            ["7,8,9"] = "bottom row",
            ["1,4,7"] = "left column",
            ["2,5,8"] = "middle column",
            ["3,6,9"] = "right column",
            ["1,5,9"] = "top-left to bottom-right diagonal",
            ["3,5,7"] = "top-right to bottom-left diagonal",
        };

        public static IReadOnlyList<int[]> ReachableLines(IReadOnlyDictionary<int, Room> rooms) =>
            AllLines.Where(line => line.All(id => rooms[id].Answer is not null)).ToList();

        public static string Name(IReadOnlyList<int> line) =>
            Names.TryGetValue(string.Join(",", line), out var name) ? name : "line";
    }
}
