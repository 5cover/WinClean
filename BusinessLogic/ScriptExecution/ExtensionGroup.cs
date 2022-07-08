using Microsoft.Win32;

using System.Collections;

namespace Scover.WinClean.BusinessLogic;

/// <summary>A group of related extensions</summary>
public class ExtensionGroup : IReadOnlyList<string>
{
    private readonly IReadOnlyList<string> _extensions;

    /// <param name="extensions">The extensions in the group.</param>
    public ExtensionGroup(IEnumerable<string> extensions) => _extensions = extensions.ToList();

    /// <inheritdoc cref="ExtensionGroup(IEnumerable{string})"/>
    public ExtensionGroup(params string[] extensions) : this((IEnumerable<string>)extensions)
    {
    }

    public int Count => _extensions.Count;

    public string this[int index] => _extensions[index];

    public IEnumerator<string> GetEnumerator() => _extensions.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_extensions).GetEnumerator();

    public string GetName(int i) => (Registry.ClassesRoot.OpenSubKey(_extensions[i])?.GetValue(null) is string name
                                    ? Registry.ClassesRoot.OpenSubKey(name)?.GetValue(null) as string
                                    : null) ?? string.Empty;
}