using System.Globalization;
using System.Windows.Data;

using Semver;

namespace Scover.WinClean.View.Converters;

public sealed class SemVersionRangeConverter : IValueConverter
{
    public int MaxLength { get; set; } = 2048;
    public SemVersionRangeOptions Options { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((SemVersionRange)value).ToString();

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => SemVersionRange.Parse((string)value, Options, MaxLength);
}