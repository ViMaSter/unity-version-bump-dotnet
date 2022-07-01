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
    .ConfigureServices(collection => {
        collection
            .AddHttpClient("github", client => {
                client.BaseAddress = new(githubInfo.apiServer);
            })
            .AddPolicyHandler(_ => HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetryAsync(new List<TimeSpan> {
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
    host.Services.GetRequiredService<IHttpClientFactory>()
));
await host.RunAsync();

static async Task StartAnalysisAsync(ActionInputs inputs, IHttpClientFactory clientFactory)
{
    var repositoryInfo = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY_OWNER")!.Split("/");
    var projectInfo = new PullRequestManager.RepositoryInfo {
        RelativePathToUnityProject = inputs.UnityProjectPath,
        UserName = repositoryInfo[0],
        RepositoryName = repositoryInfo[1]
    };
    var commitUserInfo = new PullRequestManager.CommitInfo {
        APIToken = inputs.GithubToken,
        FullName = "UnityVersionBump (bot)",
        EmailAddress = "unity-version-bump@vincent.mahn.ke",
        PullRequestLabels = inputs.pullRequestLabels,
        PullRequestPrefix = inputs.PullRequestPrefix
    };

    var projectVersionTxt = File.ReadAllText(Path.Join(Directory.GetCurrentDirectory(), inputs.UnityProjectPath, "ProjectSettings", "ProjectVersion.txt"));
    var currentVersion = ProjectVersion.FromProjectVersionTXT(projectVersionTxt);
    var highestVersion = ProjectVersion.GetLatestFromHub(clientFactory.CreateClient(), inputs.releaseStreams.Select(Enum.Parse<UnityVersion.ReleaseStreamType>));

    var newPullRequestID = await PullRequestManager.CreatePullRequestIfTargetVersionNewer(
        clientFactory.CreateClient("github"),
        commitUserInfo,
        projectInfo,
        currentVersion,
        highestVersion
    );

    GitHubActionsUtilities.GitHubActionsWriteLine($"Current Unity Version: {currentVersion.ToUnityStringWithRevision()}");
    GitHubActionsUtilities.GitHubActionsWriteLine($"Latest Unity Version:  {highestVersion.ToUnityStringWithRevision()}");
    if (!string.IsNullOrEmpty(newPullRequestID))
    {
        GitHubActionsUtilities.GitHubActionsWriteLine($"Creating new pull request at: https://github.com/{projectInfo.UserName}/{projectInfo.RepositoryName}/pull/{newPullRequestID} ---");
    }
    else
    {
        GitHubActionsUtilities.GitHubActionsWriteLine("--- Creating no pull request, as no newer version is available inside specified release streams ---");
    }


    GitHubActionsUtilities.SetOutputVariable("has-newer-version", (highestVersion.GetComparable() > currentVersion.GetComparable()).ToString());
    GitHubActionsUtilities.SetOutputVariable("current-unity-version", currentVersion.ToUnityStringWithRevision());
    GitHubActionsUtilities.SetOutputVariable("newest-unity-version", highestVersion.ToUnityStringWithRevision());
    if (!string.IsNullOrEmpty(newPullRequestID))
    {
        GitHubActionsUtilities.SetOutputVariable("pull-request-id", newPullRequestID);
    }

    Environment.Exit(0);
}