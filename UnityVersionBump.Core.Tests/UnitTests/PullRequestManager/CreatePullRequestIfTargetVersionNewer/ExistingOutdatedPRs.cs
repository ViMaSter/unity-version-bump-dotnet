using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityVersionBump.Core.Tests.Stubs;

namespace UnityVersionBump.Core.Tests.UnitTests.PullRequestManager.CreatePullRequestIfTargetVersionNewer;

class ExistingOutdatedPRs
{
    private Core.PullRequestManager.RepositoryInfo repositoryInfo;
    private Core.PullRequestManager.CommitInfo commitInfo;
    private HttpClient mockedHTTPClient;

    public ExistingOutdatedPRs()
    {
        repositoryInfo = new Core.PullRequestManager.RepositoryInfo
        {
            RelativePathToUnityProject = "",
            UserName = "ViMaSter",
            RepositoryName = "unity-version-bump-test-fixture"
        };
        commitInfo = new Core.PullRequestManager.CommitInfo
        {
            APIToken = "ghp_api",
            FullName = "UnityVersionBump (bot)",
            EmailAddress = "unity-version-bump@vincent.mahn.ke",
            PullRequestLabels = new[] { "autoupdate" },
            PullRequestPrefix = "unity-version-bump"
        };

        mockedHTTPClient = new HttpClient(new LocalFileMessageHandler("UnitTests.PullRequestManager.Resources.ExistingOutdatedVersion")).SetupGitHub(repositoryInfo, commitInfo);
    }

    private static readonly object[] _labels =
    {
        new string[] { "autoupdate" },
        new string[] { },
    };

    [TestCaseSource(nameof(_labels))]
    public async Task RunWithNewerStableVersion(string[] labels)
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2022.2.1f2", "234567890abc", true);

        commitInfo.PullRequestLabels = labels;

        var alreadyUpToDatePR = await Core.PullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.IsNull(alreadyUpToDatePR);

        Assert.IsNotEmpty(await Core.PullRequestManager.CreatePullRequestIfTargetVersionNewer(
            mockedHTTPClient,
            commitInfo,
            repositoryInfo,
            currentVersion,
            highestVersion
        ));
    }

    [TestCaseSource(nameof(_labels))]
    public async Task RunWithNewerAlphaVersion(string[] labels)
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2022.2.1a2", "234567890abc", true);

        commitInfo.PullRequestLabels = labels;

        var alreadyUpToDatePR = await Core.PullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.IsNull(alreadyUpToDatePR);

        Assert.IsNotEmpty(await Core.PullRequestManager.CreatePullRequestIfTargetVersionNewer(
            mockedHTTPClient,
            commitInfo,
            repositoryInfo,
            currentVersion,
            highestVersion
        ));
    }

    [TestCaseSource(nameof(_labels))]
    public async Task RunWithIdenticalVersion(string[] labels)
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);

        commitInfo.PullRequestLabels = labels;

        var alreadyUpToDatePR = await Core.PullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.IsNull(alreadyUpToDatePR);

        Assert.IsEmpty(await Core.PullRequestManager.CreatePullRequestIfTargetVersionNewer(
            mockedHTTPClient,
            commitInfo,
            repositoryInfo,
            currentVersion,
            highestVersion
        ));
    }

    [TestCaseSource(nameof(_labels))]
    public async Task RunWithLowerVersion(string[] labels)
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2021.2.0p1", "234567890abc", true);

        commitInfo.PullRequestLabels = labels;

        var alreadyUpToDatePR = await Core.PullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.IsNotNull(alreadyUpToDatePR);

        Assert.IsEmpty(await Core.PullRequestManager.CreatePullRequestIfTargetVersionNewer(
            mockedHTTPClient,
            commitInfo,
            repositoryInfo,
            currentVersion,
            highestVersion
        ));
    }

    [TestCaseSource(nameof(_labels))]
    public void FailsIfPRClosureCommentCantBePosted(string[] labels)
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2021.2.0p1", "234567890abc", true);

        commitInfo.PullRequestLabels = labels;

        var failingClient = new HttpClient(new LocalFileMessageHandler("UnitTests.PullRequestManager.Resources.FailedToCreatePRClosureComment")).SetupGitHub(repositoryInfo, commitInfo);

        Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            var alreadyUpToDatePR = await Core.PullRequestManager.CleanupAndCheckForAlreadyExistingPR(failingClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
            Assert.IsNotNull(alreadyUpToDatePR);
        });
    }

    [TestCaseSource(nameof(_labels))]
    public void FailsIfPRCantBeClosed(string[] labels)
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2021.2.0p1", "234567890abc", true);

        commitInfo.PullRequestLabels = labels;

        var failingClient = new HttpClient(new LocalFileMessageHandler("UnitTests.PullRequestManager.Resources.FailedToClosePR")).SetupGitHub(repositoryInfo, commitInfo);

        Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            var alreadyUpToDatePR = await Core.PullRequestManager.CleanupAndCheckForAlreadyExistingPR(failingClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
            Assert.IsNotNull(alreadyUpToDatePR);
        });
    }

    [TestCaseSource(nameof(_labels))]
    public void FailsIfBranchesCantBeFetched(string[] labels)
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2021.2.0p1", "234567890abc", true);

        commitInfo.PullRequestLabels = labels;

        var failingClient = new HttpClient(new LocalFileMessageHandler("UnitTests.PullRequestManager.Resources.FailedToFetchBranches")).SetupGitHub(repositoryInfo, commitInfo);

        Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            var alreadyUpToDatePR = await Core.PullRequestManager.CleanupAndCheckForAlreadyExistingPR(failingClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
            Assert.IsNotNull(alreadyUpToDatePR);
        });
    }
}
