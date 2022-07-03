using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityVersionBump.Core.Tests.Stubs;

namespace UnityVersionBump.Core.Tests.UnitTests.PullRequestManager.CreatePullRequestIfTargetVersionNewer;

class ExistingOutdatedPRs
{
    private Core.PullRequestManager.RepositoryInfo repositoryInfo;
    private Core.PullRequestManager.CommitInfo commitInfo;
    private HttpClient mockedHTTPClient;

    [SetUp]
    public void Setup()
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

    [TestCase]
    public async Task RunWithNewerStableVersion()
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2022.2.1f2", "234567890abc", true);

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

    [TestCase]
    public async Task RunWithNewerAlphaVersion()
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2022.2.1a2", "234567890abc", true);

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

    [TestCase]
    public async Task RunWithIdenticalVersion()
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);

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

    [TestCase]
    public async Task RunWithLowerVersion()
    {
        var currentVersion = new Core.UnityVersion("2022.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2021.2.0p1", "234567890abc", true);

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
}
