using Scover.WinClean.DataAccess;

using System.Windows;
using System.Windows.Navigation;

namespace Scover.WinClean.Presentation.Windows;

public partial class AboutWindow : Window
{
    public AboutWindow() => InitializeComponent();

    private void Button_Click(object sender, RoutedEventArgs e) => Close();

    private void RepoUrl_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        if (App.RepositoryUrl is not null)
        {
            Helpers.OpenUrl(App.RepositoryUrl);
        }
    }
}