namespace LunaFlow.Rules
{
    /// <summary>
    /// Each Rule, Mechanic or Fact has a reference. (MyRuleBook.Chapter(A).Subsection(s)) or something similar. These are
    /// for tracking throughout the system as well as 
    /// </summary>
    public interface ILunaFlowReference
    {
        /// <summary>
        /// unique class ID assigned at complie time
        /// </summary>
        string ClassUID { get; }
        /// <summary>
        /// String identifier for this reference
        /// </summary>
        string Identifier { get; }
        /// <summary>
        /// String path including all reference (including top level data) information delimited by '.'
        /// </summary>
        string FullPath { get; }
        /// <summary>
        /// String path including reference information from the Rulebook level down.
        /// </summary>
        string Path { get; }
        /// <summary>
        /// The reference to the parent container. Referencing the parent of the top container is an error.
        /// </summary>
        ILunaFlowReference Parent { get; }
        /// <summary>
        /// A list of sub references (empty if none)
        /// </summary>
        IEnumerable<ILunaFlowReference> Subreferences { get; }
    }

    /// <summary>
    /// Denotes an object containing an ILunaFlowReference
    /// </summary>
    public interface IHasLunaFlowReference
    {

        ILunaFlowReference Reference { get; }
    }
}
