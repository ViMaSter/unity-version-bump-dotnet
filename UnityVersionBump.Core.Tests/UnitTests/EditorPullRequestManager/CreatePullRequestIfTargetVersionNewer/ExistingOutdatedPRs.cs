using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityVersionBump.Core.Tests.Stubs;

namespace UnityVersionBump.Core.Tests.UnitTests.EditorPullRequestManager.CreatePullRequestIfTargetVersionNewer;

class ExistingOutdatedPRs
{
    private Core.EditorPullRequestManager.RepositoryInfo repositoryInfo;
    private Core.EditorPullRequestManager.CommitInfo commitInfo;
    private HttpClient mockedHTTPClient;

    public ExistingOutdatedPRs()
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

        mockedHTTPClient = new HttpClient(new LocalFileMessageHandler("UnitTests.EditorPullRequestManager.Resources.ExistingOutdatedVersion")).SetupGitHub(repositoryInfo, commitInfo);
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

        var alreadyUpToDatePR = await Core.EditorPullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.That(alreadyUpToDatePR, Is.Null);

        Assert.That(await Core.EditorPullRequestManager.CreatePullRequestIfTargetVersionNewer(
            mockedHTTPClient,
            commitInfo,
            repositoryInfo,
            currentVersion,
            highestVersion
        ), Is.Not.Empty);
    }

    [TestCaseSource(nameof(_labels))]
    public async Task RunWithNewerAlphaVersion(string[] labels)
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2022.2.1a2", "234567890abc", true);

        commitInfo.PullRequestLabels = labels;

        var alreadyUpToDatePR = await Core.EditorPullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.That(alreadyUpToDatePR, Is.Null);

        Assert.That(await Core.EditorPullRequestManager.CreatePullRequestIfTargetVersionNewer(
            mockedHTTPClient,
            commitInfo,
            repositoryInfo,
            currentVersion,
            highestVersion
        ), Is.Not.Empty);
    }

    [TestCaseSource(nameof(_labels))]
    public async Task RunWithIdenticalVersion(string[] labels)
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);

        commitInfo.PullRequestLabels = labels;

        var alreadyUpToDatePR = await Core.EditorPullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.That(alreadyUpToDatePR, Is.Null);

        Assert.That(await Core.EditorPullRequestManager.CreatePullRequestIfTargetVersionNewer(
            mockedHTTPClient,
            commitInfo,
            repositoryInfo,
            currentVersion,
            highestVersion
        ), Is.Empty);
    }

    [TestCaseSource(nameof(_labels))]
    public async Task RunWithLowerVersion(string[] labels)
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2021.2.0p1", "234567890abc", true);

        commitInfo.PullRequestLabels = labels;

        var alreadyUpToDatePR = await Core.EditorPullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.That(alreadyUpToDatePR, Is.Not.Null);

        Assert.That(await Core.EditorPullRequestManager.CreatePullRequestIfTargetVersionNewer(
            mockedHTTPClient,
            commitInfo,
            repositoryInfo,
            currentVersion,
            highestVersion
        ), Is.Empty);
    }

    [TestCaseSource(nameof(_labels))]
    public void FailsIfPRClosureCommentCantBePosted(string[] labels)
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2021.2.0p1", "234567890abc", true);

        commitInfo.PullRequestLabels = labels;

        var failingClient = new HttpClient(new LocalFileMessageHandler("UnitTests.EditorPullRequestManager.Resources.FailedToCreatePRClosureComment")).SetupGitHub(repositoryInfo, commitInfo);

        Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            var alreadyUpToDatePR = await Core.EditorPullRequestManager.CleanupAndCheckForAlreadyExistingPR(failingClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
            Assert.That(alreadyUpToDatePR, Is.Not.Null);
        });
    }

    [TestCaseSource(nameof(_labels))]
    public void FailsIfPRCantBeClosed(string[] labels)
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2021.2.0p1", "234567890abc", true);

        commitInfo.PullRequestLabels = labels;

        var failingClient = new HttpClient(new LocalFileMessageHandler("UnitTests.EditorPullRequestManager.Resources.FailedToClosePR")).SetupGitHub(repositoryInfo, commitInfo);

        Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            var alreadyUpToDatePR = await Core.EditorPullRequestManager.CleanupAndCheckForAlreadyExistingPR(failingClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
            Assert.That(alreadyUpToDatePR, Is.Not.Null);
        });
    }

    [TestCaseSource(nameof(_labels))]
    public void FailsIfBranchesCantBeFetched(string[] labels)
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2021.2.0p1", "234567890abc", true);

        commitInfo.PullRequestLabels = labels;

        var failingClient = new HttpClient(new LocalFileMessageHandler("UnitTests.EditorPullRequestManager.Resources.FailedToFetchBranches")).SetupGitHub(repositoryInfo, commitInfo);

        Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            var alreadyUpToDatePR = await Core.EditorPullRequestManager.CleanupAndCheckForAlreadyExistingPR(failingClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
            Assert.That(alreadyUpToDatePR, Is.Not.Null);
        });
    }
}
