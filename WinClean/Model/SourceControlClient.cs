using System.Net.Http;

using Octokit;

using Scover.WinClean.Services;

namespace Scover.WinClean.Model;

/// <summary>
/// Used to interact with the remote source control system hosting the application online (such as GitHub).
/// </summary>
public class SourceControlClient
{
    private SourceControlClient(string latestVersionName)
        => LatestVersionName = latestVersionName;

    public static Task<SourceControlClient> Instance { get; } = GetInstance();

    public string LatestVersionName { get; }

    private static async Task<SourceControlClient> GetInstance()
    {
        var appInfo = ServiceProvider.Get<IApplicationInfo>();

        string latestVersionName;

        try
        {
            GitHubClient github = new(new ProductHeaderValue(appInfo.Name + appInfo.Version));
            var latestRelease = await github.Repository.Release.GetLatest(ServiceProvider.Get<ISettings>().RepositoryId);
            latestVersionName = latestRelease.Name;
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            latestVersionName = appInfo.Version;
        }

        return new(latestVersionName);
    }
}