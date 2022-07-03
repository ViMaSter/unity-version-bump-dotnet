using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityVersionBump.Core.Tests.Stubs;

namespace UnityVersionBump.Core.Tests.UnitTests.PullRequestManager.SkipsIfNoNewerVersion;

class PRExistsForEqualVersion
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

        mockedHTTPClient = new HttpClient(new LocalFileMessageHandler("UnitTests.PullRequestManager.Resources.ExistingEqualVersionStable")).SetupGitHub(repositoryInfo, commitInfo);
    }

    [TestCase]
    public async Task RunWithNewerStableVersion()
    {
        var currentVersion = new Core.UnityVersion("2020.3.5f1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2021.3.5f1", "40eb3a945986", true);

        var alreadyUpToDatePR = await Core.PullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.IsNotNull(alreadyUpToDatePR);
    }

    [TestCase]
    public async Task RunWithNewerAlphaVersion()
    {
        var alphaHTTPClient = new HttpClient(new LocalFileMessageHandler("UnitTests.PullRequestManager.Resources.ExistingEqualVersionAlpha")).SetupGitHub(repositoryInfo, commitInfo);

        var currentVersion = new Core.UnityVersion("2020.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2021.3.5a1", "40eb3a945986", true);

        var alreadyUpToDatePR = await Core.PullRequestManager.CleanupAndCheckForAlreadyExistingPR(alphaHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.IsNotNull(alreadyUpToDatePR);
    }

    [TestCase]
    public async Task RunWithIdenticalVersion()
    {
        var currentVersion = new Core.UnityVersion("2020.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2020.2.0p1", "1234567890ab", true);

        var alreadyUpToDatePR = await Core.PullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.IsNotNull(alreadyUpToDatePR);
    }

    [TestCase]
    public async Task RunWithLowerVersion()
    {
        var currentVersion = new Core.UnityVersion("2020.2.0p1", "1234567890ab", true);
        var highestVersion = new Core.UnityVersion("2019.2.0p1", "234567890abc", true);

        var alreadyUpToDatePR = await Core.PullRequestManager.CleanupAndCheckForAlreadyExistingPR(mockedHTTPClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
        Assert.IsNotNull(alreadyUpToDatePR);
    }
}
