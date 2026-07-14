namespace LunaFlow.Rules
{
    /// <summary>
    /// The base class for creating a rule in LunaFlow
    /// </summary>
    public abstract class ARule : NRule, IHasLunaFlowReference
    {
        /// <inheritdoc/>
        public abstract ILunaFlowReference Reference { get; }
        /// <summary>
        /// A list of references that are supersceded by this one.
        /// </summary>
        public virtual IEnumerable<ILunaFlowReference>? Superscedes { get; protected set; } = null;


    }
}
