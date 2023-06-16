using System.Reflection;

namespace Scover.WinClean.Model;

public static class Multiton<TEnum, TInstance>
{
    public static IReadOnlyCollection<TInstance> Instances => Cache.Instances.Value;

    /// <exception cref="InvalidOperationException"/>
    public static TInstance GetInstance(Func<TInstance, bool> predicate) => Instances.Single(predicate);

    /// <exception cref="InvalidOperationException"/>
    public static TInstance? GetInstanceOrDefault(Func<TInstance, bool> predicate) => Instances.SingleOrDefault(predicate);

    private static class Cache
    {
        public static Lazy<List<TInstance>> Instances = new(()
            => typeof(TEnum).GetProperties(BindingFlags.Public | BindingFlags.Static)
                // Check type before querying value to prevent useless evaluations.
                .Where(p => typeof(TInstance).IsAssignableFrom(p.PropertyType))
                .Select(prop => (TInstance?)prop.GetValue(null))
                .WithoutNull().ToList(),
            true);
    }
}