using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityVersionBump.Core.Tests.Stubs;

namespace UnityVersionBump.Core.Tests.UnitTests.EditorPullRequestManager.SkipsIfNoNewerVersion;

class PRExistsForEqualVersion
{
    private Core.EditorPullRequestManager.RepositoryInfo repositoryInfo;
    private Core.EditorPullRequestManager.CommitInfo commitInfo;
    private HttpClient mockedHTTPClient;

    public PRExistsForEqualVersion()
    {
        repositoryInfo = new Core.EditorPullRequestManager.RepositoryInfo
        {
            RelativePathToUnityProject = "",
            UserName = "ViMaSter",
            RepositoryName = "unity-version-bump-test-fixture"
        };
        commitInfo = new Core.EditorPullRequestManager.CommitInfo
        {
            APIToken = "ghp_api",
            FullName = "UnityVersionBump (bot)",
            EmailAddress = "unity-version-bump@vincent.mahn.ke",
            PullRequestLabels = new[] { "autoupdate" },
            PullRequestPrefix = "unity-version-bump"
        };

        mockedHTTPClient = new HttpClient(new LocalFileMessageHandler("UnitTests.EditorPullRequestManager.Resources.ExistingEqualVersionStable")).SetupGitHub(repositoryInfo, commitInfo);
    }

    [TestCase]
    public async Task RunWithNewerStableVersion()
    {
        var currentVersion = new Core.UnityVersion("2020.3.5f1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2021.3.5f1", "40eb3a945986", true);

        var alreadyUpToDatePR = await Core.EditorPullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.That(alreadyUpToDatePR, Is.Not.Null); 
    }

    [TestCase]
    public async Task RunWithNewerAlphaVersion()
    {
        var alphaHTTPClient = new HttpClient(new LocalFileMessageHandler("UnitTests.EditorPullRequestManager.Resources.ExistingEqualVersionAlpha")).SetupGitHub(repositoryInfo, commitInfo);

        var currentVersion = new Core.UnityVersion("2020.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2021.3.5a1", "40eb3a945986", true);

        var alreadyUpToDatePR = await Core.EditorPullRequestManager.CleanupAndCheckForAlreadyExistingPR(alphaHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.That(alreadyUpToDatePR, Is.Not.Null);
    }

    [TestCase]
    public async Task RunWithIdenticalVersion()
    {
        var currentVersion = new Core.UnityVersion("2020.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2020.2.0p1", "1234567890ab", true);

        var alreadyUpToDatePR = await Core.EditorPullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.That(alreadyUpToDatePR, Is.Not.Null);
    }

    [TestCase]
    public async Task RunWithLowerVersion()
    {
        var currentVersion = new Core.UnityVersion("2020.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2019.2.0p1", "234567890abc", true);

        var alreadyUpToDatePR = await Core.EditorPullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.That(alreadyUpToDatePR, Is.Not.Null);
    }
}
