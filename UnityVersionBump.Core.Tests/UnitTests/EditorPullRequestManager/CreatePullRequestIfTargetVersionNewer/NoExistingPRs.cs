using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityVersionBump.Core.Tests.Stubs;

namespace UnityVersionBump.Core.Tests.UnitTests.EditorPullRequestManager.CreatePullRequestIfTargetVersionNewer.CreatePullRequestIfTargetVersionNewer;

class NoExistingPRs
{
    private Core.EditorPullRequestManager.RepositoryInfo repositoryInfo;
    private Core.EditorPullRequestManager.CommitInfo commitInfo;
    private HttpClient mockedHTTPClient;

    public NoExistingPRs()
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

        mockedHTTPClient = new HttpClient(new LocalFileMessageHandler("UnitTests.EditorPullRequestManager.Resources.NoExistingPRs")).SetupGitHub(repositoryInfo, commitInfo);
    }

    [TestCase]
    public async Task RunWithNewerStableVersion()
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2022.2.1f2", "234567890abc", true);

        var alreadyUpToDatePR = await Core.EditorPullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.IsNull(alreadyUpToDatePR);

        Assert.IsNotEmpty(await Core.EditorPullRequestManager.CreatePullRequestIfTargetVersionNewer(
            mockedHTTPClient,
            commitInfo,
            repositoryInfo,
            currentVersion,
            highestVersion
        ));
    }

    [TestCase]
    public async Task RunWithNewerAlphaVersion()
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2022.2.1a2", "234567890abc", true);

        var alreadyUpToDatePR = await Core.EditorPullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.IsNull(alreadyUpToDatePR);

        Assert.IsNotEmpty(await Core.EditorPullRequestManager.CreatePullRequestIfTargetVersionNewer(
            mockedHTTPClient,
            commitInfo,
            repositoryInfo,
            currentVersion,
            highestVersion
        ));
    }

    [TestCase]
    public async Task RunWithIdenticalVersion()
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);

        var alreadyUpToDatePR = await Core.EditorPullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.IsNull(alreadyUpToDatePR);

        Assert.IsEmpty(await Core.EditorPullRequestManager.CreatePullRequestIfTargetVersionNewer(
            mockedHTTPClient,
            commitInfo,
            repositoryInfo,
            currentVersion,
            highestVersion
        ));
    }

    [TestCase]
    public async Task RunWithLowerVersion()
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2021.2.0p1", "234567890abc", true);

        var alreadyUpToDatePR = await Core.EditorPullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.IsNull(alreadyUpToDatePR);

        Assert.IsEmpty(await Core.EditorPullRequestManager.CreatePullRequestIfTargetVersionNewer(
            mockedHTTPClient,
            commitInfo,
            repositoryInfo,
            currentVersion,
            highestVersion
        ));
    }
}
