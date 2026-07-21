using LunaFlow.Rules;

namespace LunaFlow.Rom.D20System
{
    /// <summary>
    /// Minimal ILunaFlowReference for the d20 System's hand-written types (the resolution Mechanic).
    /// Flat and disposable, same shape as Nine Rooms' <c>NineRoomsReference</c> — not a shared
    /// ROM-level reference scheme.
    /// </summary>
    internal sealed class D20SystemReference : ILunaFlowReference
    {
        public D20SystemReference(string identifier)
        {
            Identifier = identifier;
            ClassUID = identifier;
        }

        public string ClassUID { get; }
        public string Identifier { get; }
        public string FullPath => $"D20System.{Identifier}";
        public string Path => Identifier;
        public ILunaFlowReference Parent => null!;
        public IEnumerable<ILunaFlowReference> Subreferences => Array.Empty<ILunaFlowReference>();
    }
}
