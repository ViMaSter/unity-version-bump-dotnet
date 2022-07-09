using System;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityVersionBump.Core.Tests.Stubs;

namespace UnityVersionBump.Core.Tests.UnitTests.Editor_PullRequestManager.SkipsIfNoNewerVersion;

class PRExistsForNewerVersionInDifferentStream
{
    private Core.PullRequestManager.RepositoryInfo repositoryInfo;
    private Core.PullRequestManager.CommitInfo commitInfo;
    private HttpClient mockedHTTPClient;

    public PRExistsForNewerVersionInDifferentStream()
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

        mockedHTTPClient = new HttpClient(new LocalFileMessageHandler("UnitTests.Editor_PullRequestManager.Resources.ExistingDanglingBranches")).SetupGitHub(repositoryInfo, commitInfo);
    }

    [TestCase]
    public async Task RunWithNewerStableVersion()
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2022.2.0a17", "234567890abc", true);

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

    [TestCase]
    public async Task RunWithNewerAlphaVersion()
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2022.2.0a17", "234567890abc", true);

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

    [TestCase]
    public async Task RunWithIdenticalVersion()
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2022.2.0a17", "1234567890ab", true);

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

    [TestCase]
    public async Task RunWithLowerVersion()
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2022.2.0a17", "234567890abc", true);

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


    [TestCase]
    public async Task SucceedsIfNoLabel()
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2022.2.0a17", "234567890abc", true);

        var alreadyUpToDatePR = await Core.PullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.IsNotNull(alreadyUpToDatePR);

        Assert.IsEmpty(await Core.PullRequestManager.CreatePullRequestIfTargetVersionNewer(
            mockedHTTPClient,
            new Core.PullRequestManager.CommitInfo
            {
                APIToken = "ghp_api",
                FullName = "UnityVersionBump (bot)",
                EmailAddress = "unity-version-bump@vincent.mahn.ke",
                PullRequestLabels = Array.Empty<string>(),
                PullRequestPrefix = "unity-version-bump"
            },
            repositoryInfo,
            currentVersion,
            highestVersion
        ));
    }
}
