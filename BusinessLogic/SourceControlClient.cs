using Octokit;

namespace Scover.WinClean.BusinessLogic;

public sealed class SourceControlClient
{
    private readonly IGitHubClient _github;
    private readonly Release _latestRelease;
    private readonly long _repoID;

    /// <summary>Creates a new <see cref="SourceControlClient"/> instance.</summary>
    /// <exception cref="AggregateException">
    /// An error occured with the source control API. See the inner exception for more details.
    /// </exception>
    private SourceControlClient()
    {
        _github = new GitHubClient(new ProductHeaderValue(AppInfo.Name + AppInfo.Version));
        _repoID = _github.Repository.Get("5cover", "WinClean").Result.Id;
        _latestRelease = _github.Repository.Release.GetLatest(_repoID).Result;
    }

    public static Lazy<SourceControlClient> Instance { get; } = new(() => new SourceControlClient());
    public string LatestVersionName => _latestRelease.Name;

    public string LatestVersionUrl => _latestRelease.HtmlUrl;
}