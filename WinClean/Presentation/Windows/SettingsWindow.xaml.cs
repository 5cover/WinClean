using System.Windows;

namespace Scover.WinClean.Presentation.Windows;

public sealed partial class SettingsWindow
{
    public SettingsWindow() => InitializeComponent();

    private void ButtonOKClick(object sender, RoutedEventArgs e) => Close();

    private void ButtonResetClick(object sender, RoutedEventArgs e) => App.Settings.Reset();
}