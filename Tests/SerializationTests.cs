using System.Globalization;

using Scover.WinClean.Model;

using static System.Globalization.CultureInfo;

namespace Tests;

public class SerializationTests
{
    protected static CultureInfo OtherCulture => GetCultureInfo("fr");

    protected static LocalizedString Localize(string invariant, string other) => new()
    {
        [InvariantCulture] = invariant,
        [OtherCulture] = other,
    };
}