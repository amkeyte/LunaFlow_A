namespace LunaFlow.Engine.Components
{
    /// <summary>
    /// A named capability that a component can provide or require.
    /// The capability acts as a mediation point for peer components to depend on each other
    /// without direct assembly references.
    /// </summary>
    public sealed record Capability
    {
        /// <summary>Creates a capability with the given name and contract type. Used for Requires-side
        /// entries — a requirement doesn't own an implementation, so there's nothing to pair.</summary>
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

        /// <summary>The concrete type implementing <see cref="ContractType"/>, set only on
        /// Provides-side entries via <see cref="Provided{TContract, TProvider}"/>. Null for
        /// Requires-side entries.</summary>
        public Type? ProviderType { get; private init; }

        /// <summary>
        /// Declares a Provides-side capability. <c>TProvider : TContract</c> makes a mismatched
        /// pairing a compile error, per ticket 0048's ruling — this is what replaces a runtime
        /// assignability check: a ROM can't ship a capability paired with a type that doesn't
        /// implement its contract.
        /// </summary>
        public static Capability Provided<TContract, TProvider>(string name) where TProvider : TContract
            => new Capability(name, typeof(TContract)) { ProviderType = typeof(TProvider) };

        /// <summary>
        /// Forces JIT resolution of every method in <see cref="ProviderType"/>'s implementation of
        /// <see cref="ContractType"/>, without invoking any of them — catching a member-signature
        /// change in the installed contract assembly that a compile-time check against this ROM's own
        /// build can't see (cross-rom-capability-spec.md §5.3). No-op for Requires-side capabilities
        /// (<see cref="ProviderType"/> null — nothing to force).
        /// </summary>
        public void ForceMemberResolution()
        {
            if (ProviderType is null) return;

            var map = ProviderType.GetInterfaceMap(ContractType);
            foreach (var target in map.TargetMethods)
                System.Runtime.CompilerServices.RuntimeHelpers.PrepareMethod(target.MethodHandle);
        }
    }
}
