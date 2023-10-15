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

        HRESULT.ThrowIfFailed(GetThemeColor(hTheme, TEXTSTYLEPARTS.TEXT_MAININSTRUCTION, 0, ThemeProperty.TMT_TEXTCOLOR, out var color));
        Brush foreground = new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));

        HRESULT.ThrowIfFailed(GetThemeFont(hTheme, IntPtr.Zero, (int)TEXTSTYLEPARTS.TEXT_MAININSTRUCTION, 0, (int)ThemeProperty.TMT_FONT, out var font));
        FontFamily fontFamily = new(font.lfFaceName);
        FontWeight fontWeight = FontWeight.FromOpenTypeWeight(font.lfWeight);

        LengthConverter converter = new();
        double fontSize = (double)converter.ConvertFrom($"{Math.Abs(font.GetPointSize())}pt").NotNull();

        return new(foreground, fontSize, fontFamily, fontWeight);
    });

    private static readonly Lazy<Brush> pausedProgressBarBrush = new(() =>
        // This fails with HRESULT 0x80070490
        /*using var hTheme = Win32Error.ThrowLastErrorIfInvalid(OpenThemeData(IntPtr.Zero, "PROGRESS"));
        HRESULT.ThrowIfFailed(GetThemeColor(hTheme, PROGRESSPARTS.PP_FILL, FILLSTATES.PBFS_PAUSED, ThemeProperty.TMT_FILLCOLOR, out var color));
        return new SolidColorBrush(color.ToColor());*/
        new SolidColorBrush(Color.FromRgb(0xDA, 0xCB, 0x26)));

    public TextStyle MainInstruction => mainInstruction.Value;
    public Brush PausedProgressBarBrush => pausedProgressBarBrush.Value;
}