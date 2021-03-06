using Microsoft.Win32;

using System.Collections;

namespace Scover.WinClean.DataAccess;

/// <summary>A group of related extensions</summary>
public class ExtensionGroup : IEnumerable<string>
{
    private readonly IEnumerable<string> _extensions;

    /// <param name="extensions">The extensions in the group.</param>
    public ExtensionGroup(IEnumerable<string> extensions) => _extensions = extensions;

    /// <inheritdoc cref="ExtensionGroup(IEnumerable{string})"/>
    public ExtensionGroup(params string[] extensions) : this((IEnumerable<string>)extensions)
    {
    }

    public IEnumerator<string> GetEnumerator() => _extensions.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_extensions).GetEnumerator();

    public string GetName() => (Registry.ClassesRoot.OpenSubKey(_extensions.First())?.GetValue(null) is string name
                                        ? Registry.ClassesRoot.OpenSubKey(name)?.GetValue(null) as string
                                    : null) ?? string.Empty;
}