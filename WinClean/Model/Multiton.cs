using System.Reflection;

namespace Scover.WinClean.Model;

/// <summary>Multiton pattern. Uses cached reflection to fetch instances.</summary>
public static class Multiton<TEnum, TInstance>
{
    public static IReadOnlyCollection<TInstance> Instances => Cache.ReflectedInstances.Value;

    /// <exception cref="InvalidOperationException">
    /// No or more than one element satisfied the predicate.
    /// </exception>
    public static TInstance GetInstance(Func<TInstance, bool> predicate) => Instances.Single(predicate);

    /// <exception cref="InvalidOperationException">
    /// More than one element satisfied the predicate.
    /// </exception>
    public static TInstance? GetInstanceOrDefault(Func<TInstance, bool> predicate) => Instances.SingleOrDefault(predicate);

    private static class Cache
    {
        public static readonly Lazy<List<TInstance>> ReflectedInstances = new(()
            => typeof(TEnum).GetProperties(BindingFlags.Public | BindingFlags.Static)
                // Check type before querying value to prevent useless evaluations. Also prevents infinite
                // recursion when TEnum has a property that calls Instances.
                .Where(p => typeof(TInstance).IsAssignableFrom(p.PropertyType))
                .Select(prop => (TInstance?)prop.GetValue(null))
                .WithoutNull().ToList(),
            true);
    }
}