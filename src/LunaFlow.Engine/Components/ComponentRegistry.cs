namespace LunaFlow.Engine.Components
{
    /// <summary>
    /// Registry and validator for ROM/Rulebook components. Enforces that all declared
    /// capabilities are unique and that all requirements can be satisfied.
    /// </summary>
    public class ComponentRegistry
    {
        private readonly List<(IHasDependencies Component, EngineTier Tier, string Name)> _registrations = new();

        /// <summary>
        /// Registers a component (ROM or Rulebook) with its tier and an identifying name.
        /// </summary>
        public void Register(IHasDependencies component, EngineTier tier, string name)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Component name cannot be null or empty.", nameof(name));

            _registrations.Add((component, tier, name));
        }

        /// <summary>
        /// Validates the complete component graph: no duplicate capability providers,
        /// all requirements satisfied, tier-ordering constraints met.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If validation fails (duplicate provider, missing requirement, or tier violation).
        /// </exception>
        public void Validate()
        {
            // Phase 1: Collect all provided capabilities and check for clashes
            var providedByName = new Dictionary<string, (IHasDependencies Component, string ComponentName)>();
            var allCapabilities = new List<Capability>();

            foreach (var (component, tier, name) in _registrations)
            {
                foreach (var cap in component.Provides)
                {
                    if (providedByName.TryGetValue(cap.Name, out var existing))
                    {
                        throw new InvalidOperationException(
                            $"Duplicate capability '{cap.Name}': provided by both '{name}' and '{existing.ComponentName}'.");
                    }
                    providedByName[cap.Name] = (component, name);
                    allCapabilities.Add(cap);
                }
            }

            // Phase 2: Validate all requirements are satisfied
            foreach (var (component, tier, name) in _registrations)
            {
                foreach (var req in component.Requires)
                {
                    if (!providedByName.ContainsKey(req.Name))
                    {
                        throw new InvalidOperationException(
                            $"Unsatisfied requirement: '{name}' requires capability '{req.Name}' but no component provides it.");
                    }
                }
            }

            // Phase 3: Validate tier ordering (no component depends on a higher tier)
            foreach (var (component, tier, name) in _registrations)
            {
                foreach (var req in component.Requires)
                {
                    if (providedByName.TryGetValue(req.Name, out var provider))
                    {
                        // Find the provider's tier by looking through registrations
                        var providerTier = _registrations
                            .First(r => r.Component == provider.Component)
                            .Tier;

                        if (providerTier > tier)
                        {
                            throw new InvalidOperationException(
                                $"Tier violation: '{name}' (tier {tier}) depends on capability '{req.Name}' " +
                                $"provided by '{provider.ComponentName}' (tier {providerTier}). " +
                                $"Components cannot depend on higher tiers.");
                        }
                    }
                }
            }
        }
    }
}
