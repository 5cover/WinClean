using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Scover.WinClean.DataAccess;

/// <summary>Provides a set of extension methods that fulfill a relatively generic role in the <see cref="DataAccess"/> layer.</summary>
public static class Extensions
{
    /// <summary>Asserts that <paramref name="t"/> isn't <see langword="null"/>.</summary>
    /// <returns><paramref name="t"/>, not null.</returns>
    public static T AssertNotNull<T>([NotNull] this T? t)
    {
        Debug.Assert(t is not null);
        return t;
    }

    /// <summary>Checks if 2 dictionaries are equivalent and hold the same content.</summary>
    public static bool EqualsContent<TKey, TValue>(this IDictionary<TKey, TValue> d1, IDictionary<TKey, TValue> d2)
            => d1.Count == d2.Count && !d1.Except(d2).Any();

    public static Process? StartPowerShellWithArguments(this string arguments) => Process.Start(new ProcessStartInfo(Path.Join(Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe"), arguments)
    {
        UseShellExecute = true,
        WindowStyle = ProcessWindowStyle.Hidden
    });
}