namespace LunaFlow.Rules.GeneratorServices
{
    /// <summary>
    /// Marks a static property returning a <see cref="RulePrototype"/> for inclusion in rule generation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public sealed class RuleTemplateAttribute : Attribute
    {
    }
}
