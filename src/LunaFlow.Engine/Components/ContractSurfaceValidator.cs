using System.Reflection;

namespace LunaFlow.Engine.Components
{
    /// <summary>
    /// Bootstrap-time shape check for a Cartridge's contract-surface assembly, per
    /// <c>cross-rom-capability-spec.md</c> §5.4: <c>cartridge.md</c>'s Structure section states the
    /// contract surface holds interfaces only — "the pinout, not the chip" — but nothing enforced it
    /// until a bare concrete record (<c>EquippedWeapon</c>) shipped directly in
    /// <c>LunaFlow.Cartridge</c> and was caught only in review (ticket 0071). This turns that manual
    /// bar into an automated, bootstrap-enforced one, same "hard fail before play starts, never a
    /// silent degradation" discipline the existing compatibility probe (§5.1–§5.3) already holds, over
    /// a different question: not whether a declared capability's pairing binds, but whether everything
    /// exported from the contract surface is actually an interface.
    /// </summary>
    public static class ContractSurfaceValidator
    {
        /// <summary>
        /// Throws <see cref="InvalidOperationException"/>, naming every offending type, if
        /// <paramref name="contractSurface"/> publicly exports anything that isn't an interface.
        /// Scope is deliberately narrow: one assembly, one pass, no allowlist — a real need for a
        /// non-interface member in the contract surface is a deliberate, reviewed loosening of this
        /// check with its own ticket, not a default this method should anticipate.
        /// </summary>
        public static void ValidateInterfacesOnly(Assembly contractSurface)
        {
            var offenders = contractSurface.GetExportedTypes()
                .Where(t => !t.IsInterface)
                .ToList();

            if (offenders.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Contract surface '{contractSurface.GetName().Name}' exposes non-interface type(s): " +
                    $"{string.Join(", ", offenders.Select(t => t.FullName))}. The contract surface may hold " +
                    "interfaces only — see cartridge.md's Structure section. A shared concrete type belongs " +
                    "behind an interface here, implemented by the owning ROM's concrete type, never named " +
                    "directly outside the Cartridge.");
            }
        }
    }
}
