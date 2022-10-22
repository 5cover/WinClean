using System.Globalization;
using System.Windows.Data;
namespace Scover.WinClean.Presentation.Converters;

public sealed class InvertBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !(bool)value;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => !(bool)value;
}