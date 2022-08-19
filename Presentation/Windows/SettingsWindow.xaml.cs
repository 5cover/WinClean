using System.Windows;

using Scover.WinClean.BusinessLogic;

namespace Scover.WinClean.Presentation.Windows;

public partial class SettingsWindow
{
    public SettingsWindow() => InitializeComponent();

    private void ButtonOKClick(object sender, RoutedEventArgs e) => Close();

    private void ButtonResetClick(object sender, RoutedEventArgs e) => AppInfo.Settings.Reset();
}