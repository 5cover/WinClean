using System.Globalization;
using System.Windows.Data;
using static System.Convert;

namespace Scover.WinClean.Presentation.Converters;

public sealed class GoldenRatioConverter : IValueConverter
{
    private const double GoldenRatio = 1.61803398874989484820458683436;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ToDouble(value, culture) * GoldenRatio;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => ToDouble(value, culture) / GoldenRatio;
}