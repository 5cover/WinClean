using Octokit;

namespace Scover.WinClean.BusinessLogic;

/// <summary>Used to interact with the remote source control system hosting the application online (such as GitHub).</summary>
public class SourceControlClient
{
    private SourceControlClient()
    {
    }

    public static Lazy<SourceControlClient> Instance { get; } = new(() =>
    {
        try
        {
            return new ActualSourceControlClient();
        }
        catch (AggregateException)
        {
            // Return a mock object if the "real" source control client is unavailable due to, for instance, a network error.
            return new SourceControlClient();
        }
    });

    public virtual string LatestVersionName => AppInfo.Version;
    public virtual string LatestVersionUrl => "";

    private sealed class ActualSourceControlClient : SourceControlClient
    {
        // WinClean repository ID (https://github.com/5cover/WinClean)
        private const long RepoId = 511304031;

        private readonly IGitHubClient _github;
        private readonly Release _latestRelease;

        /// <summary>Creates a new <see cref="SourceControlClient"/> instance.</summary>
        /// <exception cref="AggregateException">
        /// An error occured with the source control API. See the inner exception for more details.
        /// </exception>
        public ActualSourceControlClient()
        {
            _github = new GitHubClient(new ProductHeaderValue(AppInfo.Name + AppInfo.Version));
            _latestRelease = _github.Repository.Release.GetLatest(RepoId).Result;
        }

        public override string LatestVersionName => _latestRelease.Name;

        public override string LatestVersionUrl => _latestRelease.HtmlUrl;
    }
}