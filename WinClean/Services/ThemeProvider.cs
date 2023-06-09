using System.Windows;
using System.Windows.Media;

using Vanara.Extensions;
using Vanara.PInvoke;

using static Vanara.PInvoke.UxTheme;

using FontFamily = System.Windows.Media.FontFamily;

namespace Scover.WinClean.Services;

public sealed class ThemeProvider : IThemeProvider
{
    private static readonly Lazy<TextStyle> mainInstruction = new(() =>
    {
        using var hTheme = Win32Error.ThrowLastErrorIfInvalid(OpenThemeData(IntPtr.Zero, "TEXTSTYLE"));

        HRESULT.ThrowIfFailed(GetThemeColor(hTheme, (int)TEXTSTYLEPARTS.TEXT_MAININSTRUCTION, 0, (int)ThemeProperty.TMT_TEXTCOLOR, out var pColor));
        Brush foreground = new SolidColorBrush(Color.FromRgb(pColor.R, pColor.G, pColor.B));

        HRESULT.ThrowIfFailed(GetThemeFont(hTheme, IntPtr.Zero, (int)TEXTSTYLEPARTS.TEXT_MAININSTRUCTION, 0, (int)ThemeProperty.TMT_FONT, out var pFont));
        FontFamily fontFamily = new(pFont.lfFaceName);
        FontWeight fontWeight = FontWeight.FromOpenTypeWeight(pFont.lfWeight);

        LengthConverter converter = new();
        double fontSize = (double)converter.ConvertFrom($"{Math.Abs(pFont.GetPointSize())}pt").NotNull();

        return new(foreground, fontSize, fontFamily, fontWeight);
    });

    public TextStyle MainInstruction => mainInstruction.Value;
}

public sealed record TextStyle(
    Brush Foreground,
    double FontSize,
    FontFamily FontFamily,
    FontWeight FontWeight);