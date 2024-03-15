using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityVersionBump.Core.Tests.Stubs;

namespace UnityVersionBump.Core.Tests.UnitTests.EditorPullRequestManager;

class EdgeCases
{
    private readonly Core.EditorPullRequestManager.RepositoryInfo _repositoryInfo = new Core.EditorPullRequestManager.RepositoryInfo
    {
        RelativePathToUnityProject = "",
        UserName = "ViMaSter",
        RepositoryName = "unity-version-bump-test-fixture"
    };
    private readonly Core.EditorPullRequestManager.CommitInfo _commitInfo = new Core.EditorPullRequestManager.CommitInfo
    {
        APIToken = "ghp_api",
        FullName = "UnityVersionBump (bot)",
        EmailAddress = "unity-version-bump@vincent.mahn.ke",
        PullRequestLabels = new[] { "autoupdate" },
        PullRequestPrefix = "unity-version-bump"
    };


    [TestCase]
    public void FailsIfBranchCannotBeCreated()
    {
        var failingClient = new HttpClient(new LocalFileMessageHandler("UnitTests.EditorPullRequestManager.Resources.FailedBranch")).SetupGitHub(_repositoryInfo, _commitInfo);

        Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await Core.EditorPullRequestManager.CreatePullRequestIfTargetVersionNewer(
                failingClient,
                _commitInfo,
                _repositoryInfo,
                new Core.UnityVersion("2022.2.0p1", "1234567890ab", true),
                new Core.UnityVersion("2022.2.1f2", "234567890abc", true)
            );
        });
    }

    [TestCase]
    public void FailsIfLabelCannotBeAttached()
    {
        var failingClient = new HttpClient(new LocalFileMessageHandler("UnitTests.EditorPullRequestManager.Resources.FailedLabel")).SetupGitHub(_repositoryInfo, _commitInfo);

        Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await Core.EditorPullRequestManager.CreatePullRequestIfTargetVersionNewer(
                failingClient,
                _commitInfo,
                _repositoryInfo,
                new Core.UnityVersion("2022.2.0p1", "1234567890ab", true),
                new Core.UnityVersion("2022.2.1f2", "234567890abc", true)
            );
        });
    }

    [TestCase]
    public async Task SucceedsIfNoLabel()
    {
        var mockedHTTPClient = new HttpClient(new LocalFileMessageHandler("UnitTests.EditorPullRequestManager.Resources.NoExistingPRs")).SetupGitHub(_repositoryInfo, _commitInfo);

        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        Core.UnityVersion? highestVersion = null;

        var alreadyUpToDatePR = await Core.EditorPullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, _commitInfo, _repositoryInfo, currentVersion, highestVersion);
        Assert.That(alreadyUpToDatePR, Is.Null);
    }

    [TestCase]
    public async Task CanHandleTwoPages()
    {
        var mockedHTTPClient = new HttpClient(new LocalFileMessageHandler("UnitTests.EditorPullRequestManager.Resources.TwoPagesOfPRs")).SetupGitHub(_repositoryInfo, _commitInfo);

        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        Core.UnityVersion? highestVersion = null;

        var alreadyUpToDatePR = await Core.EditorPullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, _commitInfo, _repositoryInfo, currentVersion, highestVersion);
        Assert.That(alreadyUpToDatePR, Is.Null);
    }

    [TestCase]
    public async Task TwoOpenPRs()
    {
        var mockedHTTPClient = new HttpClient(new LocalFileMessageHandler("UnitTests.EditorPullRequestManager.Resources.TwoOpenPRs")).SetupGitHub(_repositoryInfo, _commitInfo);

        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        Core.UnityVersion? highestVersion = null;

        var alreadyUpToDatePR = await Core.EditorPullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, _commitInfo, _repositoryInfo, currentVersion, highestVersion);
        Assert.That(alreadyUpToDatePR, Is.Null);
    }
}
