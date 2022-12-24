using System.Windows;
using System.Windows.Navigation;
using Scover.WinClean.BusinessLogic;

namespace Scover.WinClean.Presentation.Windows;

public sealed partial class AboutWindow
{
    public AboutWindow() => InitializeComponent();

    private void ButtonOKClick(object sender, RoutedEventArgs e) => Close();

    private void RepoUrlRequestNavigate(object sender, RequestNavigateEventArgs e) => AppMetadata.RepositoryUrl.Open();
}