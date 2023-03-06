using Octokit;

namespace Scover.WinClean.BusinessLogic;

/// <summary>
/// Used to interact with the remote source control system hosting the application online (such as GitHub).
/// </summary>
public class SourceControlClient
{
    private SourceControlClient(string latestVersionName, string latestVersionUrl, string newIssueUrl, string wikiUrl)
        => (LatestVersionName, LatestVersionUrl, NewIssueUrl, WikiUrl) = (latestVersionName, latestVersionUrl, newIssueUrl, wikiUrl);

    public static Task<SourceControlClient> Instance { get; } = GetInstance();
    public string LatestVersionName { get; }

    public string LatestVersionUrl { get; }

    public string NewIssueUrl { get; }

    public string WikiUrl { get; }

    private static async Task<SourceControlClient> GetInstance()
    {
        try
        {
            const string Owner = "5cover", Name = "WinClean";
            GitHubClient github = new(new ProductHeaderValue(AppInfo.Name + AppInfo.Version));
            var repo = await github.Repository.Get(Owner, Name);
            var latestRelease = await github.Repository.Release.GetLatest(Owner, Name);

            return new(latestRelease.Name, latestRelease.HtmlUrl, $"{repo.HtmlUrl}/issues/new", $"{repo.HtmlUrl}/wiki");
        }
        catch (ApiException)
        {
            // Return mock data if the "real" source control client is unavailable.
            return new(AppInfo.Name, AppInfo.Version, "", "");
        }
    }
}