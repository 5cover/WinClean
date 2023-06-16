using System.Globalization;

namespace Scover.WinClean.Model.Metadatas;

public interface ITextProvider
{
    string GetDescription(CultureInfo culture);

    string GetName(CultureInfo culture);
}