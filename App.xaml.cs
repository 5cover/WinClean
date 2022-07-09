global using System.IO;

using Scover.WinClean.DataAccess;
using Scover.WinClean.Presentation;
using Scover.WinClean.Presentation.Windows;
using Scover.WinClean.Resources;

using System.Reflection;
using System.Windows;

namespace Scover.WinClean;

public partial class App : Application
{
    static App()
    {
        string? uriString = Assembly.GetExecutingAssembly()?.GetCustomAttributes()
                            .OfType<AssemblyMetadataAttribute>()
                            .SingleOrDefault(attr => attr.Key == "RepositoryUrl")?
                            .Value;
        RepositoryUrl = uriString is null ? null : new Uri(uriString);
    }

    public static string Company { get; } = Assembly.GetExecutingAssembly()?
                                             .GetCustomAttributes()
                                             .OfType<AssemblyCompanyAttribute>()
                                             .SingleOrDefault()?
                                             .Company ?? string.Empty;

    public static string Copyright { get; } = Assembly.GetExecutingAssembly()?
                                               .GetCustomAttributes()
                                               .OfType<AssemblyCopyrightAttribute>()
                                               .SingleOrDefault()?
                                               .Copyright ?? string.Empty;

    public static string Name { get; } = Assembly.GetExecutingAssembly()?
                                          .GetCustomAttributes()
                                          .OfType<AssemblyProductAttribute>()
                                          .SingleOrDefault()?
                                          .Product ?? string.Empty;

    public static Uri? RepositoryUrl { get; }

    public static Properties.Settings Settings => WinClean.Properties.Settings.Default;

    public static string Version { get; } = Assembly.GetExecutingAssembly()?
                                             .GetCustomAttributes()
                                             .OfType<AssemblyInformationalVersionAttribute>()
                                             .SingleOrDefault()?
                                             .InformationalVersion ?? string.Empty;

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        Settings.Save();

        Happenings.Exit.SetAsHappening();
        Logs.Exiting.Log();
    }

    private void Appplication_Startup(object sender, StartupEventArgs e)
    {
        Happenings.Start.SetAsHappening();
        Logs.Started.Log();

        new MainWindow().Show();
    }

    public static Stream GetContentStream(string filename) => GetContentStream(new Uri(filename, UriKind.Relative)).Stream;
}