using System.Globalization;
using System.Windows.Data;

namespace Scover.WinClean.Presentation.Converters;

public sealed class GoldenRatioConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        const double goldenRatio = 1.61803398874989484820458683436;
        return System.Convert.ToDouble(value, CultureInfo.InvariantCulture) * goldenRatio;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}