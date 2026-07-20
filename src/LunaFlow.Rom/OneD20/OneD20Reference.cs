using LunaFlow.Rules;

namespace LunaFlow.Rom.OneD20
{
    /// <summary>
    /// Minimal ILunaFlowReference for 1.d20's hand-written types (the resolution Mechanic).
    /// Flat and disposable, same shape as Nine Rooms' <c>NineRoomsReference</c> — not a shared
    /// ROM-level reference scheme.
    /// </summary>
    internal sealed class OneD20Reference : ILunaFlowReference
    {
        public OneD20Reference(string identifier)
        {
            Identifier = identifier;
            ClassUID = identifier;
        }

        public string ClassUID { get; }
        public string Identifier { get; }
        public string FullPath => $"OneD20.{Identifier}";
        public string Path => Identifier;
        public ILunaFlowReference Parent => null!;
        public IEnumerable<ILunaFlowReference> Subreferences => Array.Empty<ILunaFlowReference>();
    }
}
