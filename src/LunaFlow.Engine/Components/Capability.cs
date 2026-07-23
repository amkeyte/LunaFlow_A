namespace LunaFlow.Engine.Components
{
    /// <summary>
    /// A named capability that a component can provide or require.
    /// The capability acts as a mediation point for peer components to depend on each other
    /// without direct assembly references.
    /// </summary>
    public sealed record Capability
    {
        /// <summary>Creates a capability with the given name and contract type.</summary>
        public Capability(string name, Type contractType)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Capability name cannot be null or empty.", nameof(name));
            if (contractType == null)
                throw new ArgumentNullException(nameof(contractType));

            Name = name;
            ContractType = contractType;
        }

        /// <summary>The unique name of this capability.</summary>
        public string Name { get; }

        /// <summary>The contract type this capability represents.</summary>
        public Type ContractType { get; }
    }
}
