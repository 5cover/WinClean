using System.Globalization;

using System.Windows.Data;

using Semver;

namespace Scover.WinClean.View.Converters;

public sealed class CurrentVersionSatisfiesRangeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool satisfies = SemVersion.FromVersion(Environment.OSVersion.Version.WithoutRevision()).Satisfies((SemVersionRange)value.NotNull());
        return parameter is bool expected ? satisfies == expected : satisfies;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}