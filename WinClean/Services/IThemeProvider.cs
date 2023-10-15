using System.Windows;
using System.Windows.Media;

namespace Scover.WinClean.Services;

/// <summary>Provides themed styles for UI components.</summary>
public interface IThemeProvider
{
    TextStyle MainInstruction { get; }

    Brush PausedProgressBarBrush { get; }
}

public sealed record TextStyle(
    Brush Foreground,
    double FontSize,
    FontFamily FontFamily,
    FontWeight FontWeight);