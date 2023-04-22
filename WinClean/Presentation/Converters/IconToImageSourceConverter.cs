﻿using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace Scover.WinClean.Presentation.Converters;

public sealed class IconToImageSourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => Imaging.CreateBitmapSourceFromHIcon(((Icon)value).Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}