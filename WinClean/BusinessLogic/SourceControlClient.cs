using Octokit;

namespace Scover.WinClean.BusinessLogic;

/// <summary>Used to interact with the remote source control system hosting the application online (such as GitHub).</summary>
public class SourceControlClient
{
    private static readonly Lazy<SourceControlClient> _instance = new(() =>
    {
        try
        {
            return new ActualSourceControlClient();
        }
        catch (AggregateException)
        {
            // Return a mock object if the "real" source control client is unavailable due to, for instance, a network error.
            return new();
        }
    });

    private SourceControlClient()
    {
    }

    public static SourceControlClient Instance => _instance.Value;

    public virtual string LatestVersionName => AppMetadata.Version;
    public virtual string LatestVersionUrl => "";
    public virtual string NewIssueUrl => "";

    private sealed class ActualSourceControlClient : SourceControlClient
    {
        // WinClean repository ID (https://github.com/5cover/WinClean)
        private const long RepoId = 511304031;

        private readonly Release _latestRelease;
        private readonly Repository _repo;

        /// <summary>Creates a new <see cref="SourceControlClient"/> instance.</summary>
        /// <exception cref="AggregateException">
        /// An error occured with the source control API. See the inner exception for more details.
        /// </exception>
        public ActualSourceControlClient()
        {
            GitHubClient github = new(new ProductHeaderValue(AppMetadata.Name + AppMetadata.Version));
            _latestRelease = github.Repository.Release.GetLatest(RepoId).Result;
            _repo = github.Repository.Get(RepoId).Result;
        }

        public override string LatestVersionName => _latestRelease.Name;
        public override string LatestVersionUrl => _latestRelease.HtmlUrl;
        public override string NewIssueUrl => $"{_repo.HtmlUrl}/issues/new";
    }
}