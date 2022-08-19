using System.Net.Http;

using Octokit;

using Scover.WinClean.BusinessLogic.Scripts;

namespace Scover.WinClean.BusinessLogic;

public class SourceControlClient
{
    private readonly IGitHubClient _github;
    private readonly Release _latestRelease;
    private readonly long _repoID;
    private readonly IScriptSerializer _serializer = new Xml.ScriptXmlSerializer();

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

    /// <summary>Downloads the default scripts from the GitHub repository.</summary>
    /// <param name="progress">Progress reporting for this asynchronous operation.</param>
    /// <returns>
    /// An <see cref="IAsyncEnumerable{T}"/> of <see cref="Stream"/> s with read access representing the default scripts.
    /// </returns>
    /// <exception cref="AggregateException">
    /// An error occured with the source control API. See the inner exception for more details.
    /// </exception>
    public async Task<IReadOnlyList<Script>> DownloadDefaultScripts(IProgress<DefaultScriptDownloadProgress> progress)
    {
        var defaultScriptInfos = await _github.Repository.Content.GetAllContents(_repoID, "Scripts");
        var http = new HttpClient();
        List<Script> scripts = new();
        for (int i = 0; i < defaultScriptInfos.Count; ++i)
        {
            progress.Report(new()
            {
                CurrentScriptIndex = i,
                ScriptCount = defaultScriptInfos.Count,
                CurrentScriptName = defaultScriptInfos[i].Name
            });
            scripts.Add(_serializer.Deserialize(await http.GetStreamAsync(defaultScriptInfos[i].DownloadUrl)));
        }
        return scripts;
    }
}