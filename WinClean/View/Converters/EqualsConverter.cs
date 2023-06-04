using System.Globalization;
using System.Windows.Data;

namespace Scover.WinClean.View.Converters;

public sealed class EqualsConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => values.Distinct().Count() == 1;

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}