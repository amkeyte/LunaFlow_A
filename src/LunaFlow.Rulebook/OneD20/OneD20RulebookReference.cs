using LunaFlow.Rules;

namespace LunaFlow.Rulebook.OneD20
{
    /// <summary>
    /// Minimal ILunaFlowReference for the 1.d20 Rulebook. Flat and disposable, same shape as
    /// <c>D20SystemReference</c>/Nine Rooms' <c>NineRoomsReference</c> — not a shared scheme.
    /// </summary>
    internal sealed class OneD20RulebookReference : ILunaFlowReference
    {
        public OneD20RulebookReference(string identifier)
        {
            Identifier = identifier;
            ClassUID = identifier;
        }

        public string ClassUID { get; }
        public string Identifier { get; }
        public string FullPath => $"OneD20Rulebook.{Identifier}";
        public string Path => Identifier;
        public ILunaFlowReference Parent => null!;
        public IEnumerable<ILunaFlowReference> Subreferences => Array.Empty<ILunaFlowReference>();
    }
}
