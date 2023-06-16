namespace Scover.WinClean.Services;

public sealed class FilterBuilder : IFilterBuilder
{
    public string Combine(IEnumerable<string> filters) => string.Join('|', filters);

    public string Make(string name, IEnumerable<string> extensions)
    {
        string extensionsPart = string.Join(";", extensions.Select(ext => $"*{ext}"));
        return $"{name} ({extensionsPart})|{extensionsPart}";
    }
}