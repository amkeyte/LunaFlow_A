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
        /// A component's registered name is its identity within this registry and must be unique.
        /// </summary>
        /// <exception cref="ArgumentNullException">If component is null.</exception>
        /// <exception cref="ArgumentException">If name is null/empty/whitespace, or if a component with this name is already registered.</exception>
        public void Register(IHasDependencies component, EngineTier tier, string name)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Component name cannot be null or empty.", nameof(name));
            if (_registrations.Any(r => r.Item3 == name))
                throw new ArgumentException(
                    $"A component named '{name}' is already registered in this registry.", nameof(name));

            _registrations.Add((component, tier, name));
        }

        /// <summary>
        /// Confirms every name in <paramref name="dependsOnRoms"/> matches a component already
        /// registered in this registry — the enforcement <c>IRuleBook.DependsOnRoms</c> has been
        /// missing since it was declared (ticket 0047). Kept separate from <see cref="Validate"/>
        /// rather than folded into it: <see cref="IHasDependencies"/> stays ROM-agnostic
        /// (Provides/Requires only), so the registry has no structural way to know which of its
        /// registered components carry a named-dependency list like a Rulebook's — the caller
        /// (the composition root, which can see both tiers) supplies it explicitly instead.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="dependsOnRoms"/> names a component that isn't registered.
        /// </exception>
        public void ValidateDependsOn(string dependentName, IReadOnlyList<string> dependsOnRoms)
        {
            foreach (var romName in dependsOnRoms)
            {
                if (!_registrations.Any(r => r.Name == romName))
                {
                    throw new InvalidOperationException(
                        $"'{dependentName}' declares DependsOnRoms naming '{romName}', but no component " +
                        $"named '{romName}' is registered.");
                }
            }
        }

        /// <summary>
        /// Validates the complete component graph: no duplicate capability providers,
        /// all requirements satisfied, tier-ordering constraints met, and no circular dependencies.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If validation fails (duplicate provider, missing requirement, tier violation, or cycle).
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

            // Phase 4: Detect cycles in the capability dependency graph
            DetectCycles(providedByName);
        }

        private void DetectCycles(Dictionary<string, (IHasDependencies Component, string ComponentName)> providedByName)
        {
            var visited = new HashSet<string>();
            var stack = new HashSet<string>();

            foreach (var (component, tier, name) in _registrations)
            {
                if (!visited.Contains(name))
                {
                    DetectCyclesDfs(name, visited, stack, providedByName);
                }
            }
        }

        private void DetectCyclesDfs(
            string componentName,
            HashSet<string> visited,
            HashSet<string> stack,
            Dictionary<string, (IHasDependencies Component, string ComponentName)> providedByName)
        {
            visited.Add(componentName);
            stack.Add(componentName);

            var componentReg = _registrations.First(r => r.Item3 == componentName);
            var component = componentReg.Component;

            foreach (var req in component.Requires)
            {
                if (providedByName.TryGetValue(req.Name, out var provider))
                {
                    var providerName = provider.ComponentName;

                    if (!visited.Contains(providerName))
                    {
                        DetectCyclesDfs(providerName, visited, stack, providedByName);
                    }
                    else if (stack.Contains(providerName))
                    {
                        throw new InvalidOperationException(
                            $"Circular dependency detected: '{componentName}' depends on capability '{req.Name}' " +
                            $"provided by '{providerName}', which (transitively) depends on capabilities provided by '{componentName}'.");
                    }
                }
            }

            stack.Remove(componentName);
        }
    }
}
