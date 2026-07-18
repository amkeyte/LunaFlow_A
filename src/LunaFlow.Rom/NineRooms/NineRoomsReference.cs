using LunaFlow.Rules;

namespace LunaFlow.Rom.NineRooms
{
    /// <summary>
    /// Minimal ILunaFlowReference for Nine Rooms' rule classes. Flat and disposable, like the
    /// rest of this module — not a shared ROM-level reference scheme.
    /// </summary>
    internal sealed class NineRoomsReference : ILunaFlowReference
    {
        public NineRoomsReference(string identifier)
        {
            Identifier = identifier;
            ClassUID = identifier;
        }

        public string ClassUID { get; }
        public string Identifier { get; }
        public string FullPath => $"NineRooms.{Identifier}";
        public string Path => Identifier;
        public ILunaFlowReference Parent => null!;
        public IEnumerable<ILunaFlowReference> Subreferences => Array.Empty<ILunaFlowReference>();
    }
}
