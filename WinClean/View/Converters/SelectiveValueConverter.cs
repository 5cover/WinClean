using System.Globalization;

using System.Windows.Data;

namespace Scover.WinClean.View.Converters;

public sealed class SelectiveValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => Equals(value, parameter);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => (bool)value
            ? parameter
            : Binding.DoNothing;
}