using LunaFlow.Engine.Components;
using LunaFlow.Rom;
using LunaFlow.Rules;

namespace LunaFlow.Rom.D20SystemInterop
{
    /// <summary>
    /// The D20System/GridMap capability broker. Implements <see cref="IInteropRom"/> to aggregate
    /// and resolve capabilities published by peer ROMs, allowing them to exchange facts and services
    /// with zero assembly reference between them.
    /// </summary>
    public sealed class D20SystemInteropRom : IInteropRom
    {
        private readonly Dictionary<string, object> _providers = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>The broker's display name.</summary>
        public string Name => "D20SystemInterop";

        /// <summary>Credits for the interop broker layer.</summary>
        public string Credits => "D20System/GridMap capability mediation";

        /// <summary>The broker provides no capabilities itself; it is infrastructure.</summary>
        public IReadOnlyList<Capability> Provides => Array.Empty<Capability>();

        /// <summary>The broker requires no capabilities; it is infrastructure.</summary>
        public IReadOnlyList<Capability> Requires => Array.Empty<Capability>();

        /// <summary>The broker's LunaFlow reference (minimal; infrastructure only).</summary>
        public ILunaFlowReference Reference => new D20SystemInteropReference();

        /// <summary>Registers a capability provider. Called by the composition root during setup.</summary>
        public void RegisterProvider(string capabilityName, object provider)
        {
            _providers[capabilityName] = provider;
        }

        /// <summary>Resolves a declared capability to the instance providing it.</summary>
        /// <exception cref="InvalidOperationException">No registered provider for that capability.</exception>
        public object Resolve(Capability capability)
        {
            if (!_providers.TryGetValue(capability.Name, out var provider))
            {
                throw new InvalidOperationException(
                    $"No registered provider for capability '{capability.Name}'. Declare it in the consuming ROM's Requires.");
            }
            return provider;
        }

        /// <summary>Typed convenience over <see cref="Resolve"/>.</summary>
        public T Resolve<T>(string capabilityName) where T : class
        {
            var provider = Resolve(new Capability(capabilityName, typeof(T)));
            return (T)provider;
        }
    }

    /// <summary>Reference identity for the interop broker (minimal; infrastructure only).</summary>
    internal sealed class D20SystemInteropReference : ILunaFlowReference
    {
        public string ClassUID => "D20SystemInterop";
        public string Identifier => "D20SystemInterop";
        public string FullPath => $"D20SystemInterop.{Identifier}";
        public string Path => Identifier;
        public ILunaFlowReference Parent => null!;
        public IEnumerable<ILunaFlowReference> Subreferences => Array.Empty<ILunaFlowReference>();
    }
}
