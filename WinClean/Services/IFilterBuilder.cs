namespace Scover.WinClean.Services;

public interface IFilterBuilder
{
    string Combine(params string[] filters) => Combine((IEnumerable<string>)filters);

    string Combine(IEnumerable<string> filters);

    string Make(ExtensionGroup extensionGroup) => Make(extensionGroup.Name, extensionGroup);

    string Make(string name, IEnumerable<string> extensions);

    string Make(string name, params string[] extensions) => Make(name, (IEnumerable<string>)extensions);
}