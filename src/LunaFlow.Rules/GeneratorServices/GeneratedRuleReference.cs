namespace LunaFlow.Rules.GeneratorServices
{
    /// <summary>
    /// The <see cref="ILunaFlowReference"/> the generator assigns to every generated rule class,
    /// via <c>public override ILunaFlowReference Reference { get; } = new
    /// GeneratedRuleReference(groupName, ruleName);</c> in the emitted source.
    /// </summary>
    /// <remarks>
    /// Flat and disposable, same shape as Nine Rooms' hand-written <c>NineRoomsReference</c>
    /// (<c>LunaFlow.Rom.NineRooms</c>) — deliberately not a real parent/child chain. A genuine
    /// hierarchy (group-level reference as <c>Parent</c>, rules as its <c>Subreferences</c>)
    /// belongs on <c>IRuleBook</c>/<c>IRuleGroup</c> once those stop being empty marker
    /// interfaces; building that chain here on the strength of the pipeline's first real
    /// consumer would be the premature-abstraction move non-goals.md warns against elsewhere.
    /// See the "D3 — generated-rule Reference strategy" decision in rule-pipeline.md.
    /// <para>
    /// <c>public</c>, unlike <c>NineRoomsReference</c>'s <c>internal</c> — that one's only ever
    /// constructed by rule classes in the same assembly (<c>LunaFlow.Rom</c>). Generated rule
    /// source gets in-memory-compiled (per the D2 compiler decision) into a brand-new assembly
    /// that merely references <c>LunaFlow.Rules.dll</c>, so the type it constructs has to be
    /// visible across that assembly boundary.
    /// </para>
    /// </remarks>
    public sealed class GeneratedRuleReference : ILunaFlowReference
    {
        /// <summary>Creates a flat reference identifying a generated rule by its group and rule name.</summary>
        public GeneratedRuleReference(string groupName, string ruleName)
        {
            Identifier = ruleName;
            ClassUID = $"{groupName}.{ruleName}";
            FullPath = $"{groupName}.{ruleName}";
        }

        /// <inheritdoc/>
        public string ClassUID { get; }
        /// <inheritdoc/>
        public string Identifier { get; }
        /// <inheritdoc/>
        public string FullPath { get; }
        /// <inheritdoc/>
        public string Path => Identifier;
        /// <inheritdoc/>
        public ILunaFlowReference Parent => null!;
        /// <inheritdoc/>
        public IEnumerable<ILunaFlowReference> Subreferences => Array.Empty<ILunaFlowReference>();
    }
}
