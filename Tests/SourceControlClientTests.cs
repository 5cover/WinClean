using Octokit;

using Scover.WinClean.BusinessLogic;

namespace Tests;

[TestFixture(TestOf = typeof(SourceControlClient))]
public sealed class SourceControlClientTests
{
    private static SourceControlClient Scc => SourceControlClient.Instance.Value;
    private Release LatestRelease { get; set; }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        const long RepoId = 511304031;
        var github = new GitHubClient(new ProductHeaderValue(AppInfo.Name + AppInfo.Version));
        LatestRelease = github.Repository.Release.GetLatest(RepoId).Result;
    }

    [Test]
    public void TestLatestVersionName() => Assert.That(Scc.LatestVersionName, Is.EqualTo(LatestRelease.Name));

    [Test]
    public void TestLatestVersionUrl() => Assert.That(Scc.LatestVersionUrl, Is.EqualTo(LatestRelease.HtmlUrl));
}