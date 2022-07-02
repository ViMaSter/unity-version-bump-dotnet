using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityVersionBump.Core.Tests.Stubs;

namespace UnityVersionBump.Core.Tests.UnitTests.PullRequestManager;

class CreatePullRequestIfTargetVersionNewer
{
    private Core.PullRequestManager.RepositoryInfo repositoryInfo;
    private Core.PullRequestManager.CommitInfo commitUserInfo;
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
        commitUserInfo = new Core.PullRequestManager.CommitInfo
        {
            APIToken = "ghp_api",
            FullName = "UnityVersionBump (bot)",
            EmailAddress = "unity-version-bump@vincent.mahn.ke",
            PullRequestLabels = new[] { "autoupdate" },
            PullRequestPrefix = "prefix"
        };

        mockedHTTPClient = new HttpClient(new LocalFileMessageHandler("UnitTests.PullRequestManager.Resources.Success")).SetupGitHub(repositoryInfo, commitUserInfo);
    }

    [TestCase]
    public async Task RunWithNewerStableVersion()
    {
        Assert.IsNotEmpty(await Core.PullRequestManager.CreatePullRequestIfTargetVersionNewer(
            mockedHTTPClient,
            commitUserInfo,
            repositoryInfo,
            new Core.UnityVersion("2022.2.0p1", "1234567890ab", true),
            new Core.UnityVersion("2022.2.1f2", "234567890abc", true)
        ));
    }

    [TestCase]
    public async Task RunWithNewerAlphaVersion()
    {
        Assert.IsNotEmpty(await Core.PullRequestManager.CreatePullRequestIfTargetVersionNewer(
            mockedHTTPClient,
            commitUserInfo,
            repositoryInfo,
            new Core.UnityVersion("2022.2.0p1", "1234567890ab", true),
            new Core.UnityVersion("2022.2.1a2", "234567890abc", true)
        ));
    }

    [TestCase]
    public async Task RunWithIdenticalVersion()
    {
        Assert.IsEmpty(await Core.PullRequestManager.CreatePullRequestIfTargetVersionNewer(
            mockedHTTPClient,
            commitUserInfo,
            repositoryInfo,
            new Core.UnityVersion("2022.2.0p1", "1234567890ab", true),
            new Core.UnityVersion("2022.2.0p1", "1234567890ab", true)
        ));
    }

    [TestCase]
    public async Task RunWithLowerVersion()
    {
        Assert.IsEmpty(await Core.PullRequestManager.CreatePullRequestIfTargetVersionNewer(
            mockedHTTPClient,
            commitUserInfo,
            repositoryInfo,
            new Core.UnityVersion("2022.2.0p1", "1234567890ab", true),
            new Core.UnityVersion("2021.2.0p1", "234567890abc", true)
        ));
    }


    [TestCase]
    public async Task FailsIfBranchCannotBeCreated()
    {
        var failingClient = new HttpClient(new LocalFileMessageHandler("UnitTests.PullRequestManager.Resources.FailedBranch")).SetupGitHub(repositoryInfo, commitUserInfo);

        Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await Core.PullRequestManager.CreatePullRequestIfTargetVersionNewer(
                failingClient,
                commitUserInfo,
                repositoryInfo,
                new Core.UnityVersion("2022.2.0p1", "1234567890ab", true),
                new Core.UnityVersion("2022.2.1f2", "234567890abc", true)
            );
        });
    }


    [TestCase]
    public async Task FailsIfLabelCannotBeAttached()
    {
        var failingClient = new HttpClient(new LocalFileMessageHandler("UnitTests.PullRequestManager.Resources.FailedLabel")).SetupGitHub(repositoryInfo, commitUserInfo);

        Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await Core.PullRequestManager.CreatePullRequestIfTargetVersionNewer(
                failingClient,
                commitUserInfo,
                repositoryInfo,
                new Core.UnityVersion("2022.2.0p1", "1234567890ab", true),
                new Core.UnityVersion("2022.2.1f2", "234567890abc", true)
            );
        });
    }
}
