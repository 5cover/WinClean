using System.Globalization;
using System.Windows.Data;

using static System.Convert;

namespace Scover.WinClean.View.Converters;

public sealed class GoldenRatioConverter : IValueConverter
{
    private static double GoldenRatio => (1 + Math.Sqrt(5)) / 2;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => ToDouble(value, culture) * GoldenRatio;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => ToDouble(value, culture) / GoldenRatio;
}