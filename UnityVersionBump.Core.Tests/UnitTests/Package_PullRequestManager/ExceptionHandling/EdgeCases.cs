using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityVersionBump.Core.Tests.Stubs;

namespace UnityVersionBump.Core.Tests.UnitTests.Package_PullRequestManager;

class EdgeCases
{
    private readonly Core.PullRequestManager.RepositoryInfo _repositoryInfo = new Core.PullRequestManager.RepositoryInfo
    {
        RelativePathToUnityProject = "",
        UserName = "ViMaSter",
        RepositoryName = "unity-version-bump-test-fixture"
    };
    private readonly Core.PullRequestManager.CommitInfo _commitInfo = new Core.PullRequestManager.CommitInfo
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
        var failingClient = new HttpClient(new LocalFileMessageHandler("UnitTests.Editor_PullRequestManager.Resources.FailedBranch")).SetupGitHub(_repositoryInfo, _commitInfo);

        Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await Core.PullRequestManager.CreatePullRequestIfTargetVersionNewer(
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
        var failingClient = new HttpClient(new LocalFileMessageHandler("UnitTests.Editor_PullRequestManager.Resources.FailedLabel")).SetupGitHub(_repositoryInfo, _commitInfo);

        Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await Core.PullRequestManager.CreatePullRequestIfTargetVersionNewer(
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
        var mockedHTTPClient = new HttpClient(new LocalFileMessageHandler("UnitTests.Editor_PullRequestManager.Resources.NoExistingPRs")).SetupGitHub(_repositoryInfo, _commitInfo);

        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        Core.UnityVersion? highestVersion = null;

        var alreadyUpToDatePR = await Core.PullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, _commitInfo, _repositoryInfo, currentVersion, highestVersion);
        Assert.IsNull(alreadyUpToDatePR);
    }

    [TestCase]
    public async Task CanHandleTwoPages()
    {
        var mockedHTTPClient = new HttpClient(new LocalFileMessageHandler("UnitTests.Editor_PullRequestManager.Resources.TwoPagesOfPRs")).SetupGitHub(_repositoryInfo, _commitInfo);

        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        Core.UnityVersion? highestVersion = null;

        var alreadyUpToDatePR = await Core.PullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, _commitInfo, _repositoryInfo, currentVersion, highestVersion);
        Assert.IsNull(alreadyUpToDatePR);
    }

    [TestCase]
    public async Task TwoOpenPRs()
    {
        var mockedHTTPClient = new HttpClient(new LocalFileMessageHandler("UnitTests.Editor_PullRequestManager.Resources.TwoOpenPRs")).SetupGitHub(_repositoryInfo, _commitInfo);

        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        Core.UnityVersion? highestVersion = null;

        var alreadyUpToDatePR = await Core.PullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, _commitInfo, _repositoryInfo, currentVersion, highestVersion);
        Assert.IsNull(alreadyUpToDatePR);
    }
}
