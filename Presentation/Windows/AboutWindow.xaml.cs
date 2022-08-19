using System.Windows;
using System.Windows.Navigation;

using Scover.WinClean.BusinessLogic;
using Scover.WinClean.DataAccess;

namespace Scover.WinClean.Presentation.Windows;

public partial class AboutWindow
{
    public AboutWindow() => InitializeComponent();

    private void ButtonOKClick(object sender, RoutedEventArgs e) => Close();

    private void RepoUrlRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        if (AppInfo.RepositoryUrl is not null)
        {
            Helpers.Open(AppInfo.RepositoryUrl);
        }
    }
}