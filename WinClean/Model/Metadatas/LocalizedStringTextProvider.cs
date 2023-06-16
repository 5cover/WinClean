using System.Globalization;

namespace Scover.WinClean.Model.Metadatas;

public sealed class LocalizedStringTextProvider : ITextProvider
{
    private readonly LocalizedString _description, _name;

    public LocalizedStringTextProvider(LocalizedString name, LocalizedString description) => (_name, _description) = (name, description);

    public string GetDescription(CultureInfo culture) => _description[culture];

    public string GetName(CultureInfo culture) => _name[culture];
}