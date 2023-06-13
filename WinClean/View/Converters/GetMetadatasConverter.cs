using System.Globalization;
using System.Windows.Data;

using Scover.WinClean.Services;

namespace Scover.WinClean.View.Converters;

public sealed class GetMetadatasConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => ServiceProvider.Get<IMetadatasProvider>().Metadatas[(Type)value];

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}