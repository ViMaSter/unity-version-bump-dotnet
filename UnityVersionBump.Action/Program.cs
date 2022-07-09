using System.Diagnostics.CodeAnalysis;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using UnityVersionBump.Action;
using UnityVersionBump.Core;
using static CommandLine.Parser;

var githubInfo = new
{
    apiServer = Environment.GetEnvironmentVariable("GITHUB_API_URL")!
};

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(collection =>
    {
        collection
            .AddHttpClient("github", client =>
            {
                client.BaseAddress = new(githubInfo.apiServer);
            })
            .AddPolicyHandler(_ => HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(new List<TimeSpan>
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(3)
            }));

        collection
            .AddHttpClient("unityHub", _ =>
            {
            })
            .AddPolicyHandler(_ => HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(new List<TimeSpan>
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(3)
            }));
    })
    .Build();

var parser = Default.ParseArguments<ActionInputs>(() => new(), args);
parser.WithNotParsed(
    errors =>
    {
        host.Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DotNet.GitHubAction.Program")
            .LogError(
                string.Join(
                    Environment.NewLine, errors.Select(error => error.ToString())));
        
        Environment.Exit(2);
    });

await parser.WithParsedAsync(inputs => StartAnalysisAsync(
    inputs, 
    host.Services.GetRequiredService<IHttpClientFactory>(),
    host.Services.GetRequiredService<ILoggerFactory>()
));
await host.RunAsync();

static async Task HandleUnityEditorVersionUpdate(HttpClient unityHubHttpClient, HttpClient gitHubHttpClient, PullRequestManager.RepositoryInfo repositoryInfo, PullRequestManager.CommitInfo commitInfo, IEnumerable<UnityVersion.ReleaseStreamType> releaseStreams)
{
    var projectVersionTxt = File.ReadAllText(Path.Join(Directory.GetCurrentDirectory(), repositoryInfo.RelativePathToUnityProject, "ProjectSettings", "ProjectVersion.txt"));
    var currentVersion = ProjectVersion.FromProjectVersionTXT(projectVersionTxt);
    var highestVersion = await ProjectVersion.GetLatestFromHub(unityHubHttpClient, releaseStreams);
    GitHubActionsUtilities.GitHubActionsWriteLine($"Current Unity Version: {currentVersion.ToUnityStringWithRevision()}");

    // if there is no new version, exit gracefully
    if (highestVersion == null)
    {
        GitHubActionsUtilities.GitHubActionsWriteLine("No newer version available on selected streams");
        return;
    }

    var alreadyUpToDatePR = await PullRequestManager.CleanupAndCheckForAlreadyExistingPR(gitHubHttpClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
    if (alreadyUpToDatePR != null)
    {
        GitHubActionsUtilities.GitHubActionsWriteLine($"PR https://github.com/{repositoryInfo.UserName}/{repositoryInfo.RepositoryName}/pull/{alreadyUpToDatePR} is newer or identical to {highestVersion.ToUnityStringWithRevision()}");
        return;
    }

    GitHubActionsUtilities.GitHubActionsWriteLine($"Latest Unity Version:  {highestVersion.ToUnityStringWithRevision()}");

    // open new PR if project is outdated
    var newPullRequestID = await PullRequestManager.CreatePullRequestIfTargetVersionNewer(
        gitHubHttpClient,
        commitInfo,
        repositoryInfo,
        currentVersion,
        highestVersion
    );

    GitHubActionsUtilities.GitHubActionsWriteLine($"Created new pull request for 'editor' at: https://github.com/{repositoryInfo.UserName}/{repositoryInfo.RepositoryName}/pull/{newPullRequestID} ---");

    GitHubActionsUtilities.SetOutputVariable("has-newer-version", (highestVersion.GetComparable() > currentVersion.GetComparable()).ToString());
    GitHubActionsUtilities.SetOutputVariable("current-unity-version", currentVersion.ToUnityStringWithRevision());
    GitHubActionsUtilities.SetOutputVariable("newest-unity-version", highestVersion.ToUnityStringWithRevision());
    if (!string.IsNullOrEmpty(newPullRequestID))
    {
        GitHubActionsUtilities.SetOutputVariable("pull-request-id", newPullRequestID);
    }
};

static async Task HandlePackageVersionUpdate(IHttpClientFactory clientFactory, HttpClient gitHubHttpClient, ILoggerFactory loggerFactor, PullRequestManager.RepositoryInfo repositoryInfo, PullRequestManager.CommitInfo commitInfo, bool includePreReleasePackages)
{
    var newPRIDsByPackageName = await PullRequestManager.PackagePRs.GeneratePRs(
        clientFactory,
        loggerFactor,
        gitHubHttpClient,
        repositoryInfo,
        commitInfo,
        includePreReleasePackages
    );

    foreach (var (packageName, prID) in newPRIDsByPackageName)
    {
        GitHubActionsUtilities.GitHubActionsWriteLine($"Created new pull request for '{packageName}' at: https://github.com/{repositoryInfo.UserName}/{repositoryInfo.RepositoryName}/pull/{prID} ---");
    }
}

static async Task StartAnalysisAsync(ActionInputs inputs, IHttpClientFactory clientFactory, ILoggerFactory loggerFactory)
{
    if (!inputs.TargetRepository.Contains('/'))
    {
        GitHubActionsUtilities.GitHubActionsWriteLine("TargetRepository isn't set to a proper 'owner/repoName' value");
        Environment.Exit(1);
    }

    var repositoryPath = inputs.TargetRepository.Split("/");
    GitHubActionsUtilities.GitHubActionsWriteLine($"Owner:  {string.Join(",", repositoryPath)}");

    var repositoryInfo = new PullRequestManager.RepositoryInfo {
        RelativePathToUnityProject = inputs.UnityProjectPath,
        UserName = repositoryPath[0],
        RepositoryName = repositoryPath[1]
    };
    var commitInfo = new PullRequestManager.CommitInfo {
        APIToken = inputs.GithubToken,
        FullName = "UnityVersionBump (bot)",
        EmailAddress = "unity-version-bump@vincent.mahn.ke",
        PullRequestLabels = inputs.pullRequestLabels,
        PullRequestPrefix = inputs.PullRequestPrefix
    };

    var gitHubHttpClient = clientFactory.CreateClient("github").SetupGitHub(repositoryInfo, commitInfo);

    
    await HandlePackageVersionUpdate(
        clientFactory,
        gitHubHttpClient,
        loggerFactory,
        repositoryInfo,
        commitInfo,
        inputs.IncludePreReleasePackages
    );

    await HandleUnityEditorVersionUpdate(
        clientFactory.CreateClient("unityHub"),
        gitHubHttpClient,
        repositoryInfo,
        commitInfo,
        inputs.releaseStreams.Select(Enum.Parse<UnityVersion.ReleaseStreamType>)
    );

    Environment.Exit(0);
}