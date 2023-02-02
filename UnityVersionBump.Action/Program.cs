using System.Text;
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

static async Task HandleUnityEditorVersionUpdate(HttpClient unityHubHttpClient, HttpClient gitHubHttpClient, EditorPullRequestManager.RepositoryInfo repositoryInfo, EditorPullRequestManager.CommitInfo commitInfo, IEnumerable<UnityVersion.ReleaseStreamType> releaseStreams)
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

    var alreadyUpToDatePR = await EditorPullRequestManager.CleanupAndCheckForAlreadyExistingPR(gitHubHttpClient, commitInfo, repositoryInfo, currentVersion, highestVersion);
    if (alreadyUpToDatePR != null)
    {
        GitHubActionsUtilities.GitHubActionsWriteLine($"PR https://github.com/{repositoryInfo.UserName}/{repositoryInfo.RepositoryName}/pull/{alreadyUpToDatePR} is newer or identical to {highestVersion.ToUnityStringWithRevision()}");
        return;
    }

    GitHubActionsUtilities.GitHubActionsWriteLine($"Latest Unity Version:  {highestVersion.ToUnityStringWithRevision()}");

    // open new PR if project is outdated
    var newPullRequestID = await EditorPullRequestManager.CreatePullRequestIfTargetVersionNewer(
        gitHubHttpClient,
        commitInfo,
        repositoryInfo,
        currentVersion,
        highestVersion
    );


    GitHubActionsUtilities.SetOutputVariable("editor-has-newer-version", (highestVersion.GetComparable() > currentVersion.GetComparable()).ToString());
    GitHubActionsUtilities.SetOutputVariable("editor-current-version", currentVersion.ToUnityStringWithRevision());
    GitHubActionsUtilities.SetOutputVariable("editor-newest-version", highestVersion.ToUnityStringWithRevision());
    if (!string.IsNullOrEmpty(newPullRequestID))
    {
        GitHubActionsUtilities.GitHubActionsWriteLine($"Created new pull request at: https://github.com/{repositoryInfo.UserName}/{repositoryInfo.RepositoryName}/pull/{newPullRequestID} ---");
        GitHubActionsUtilities.SetOutputVariable("pull-request-id", newPullRequestID);
    }
}

static async Task HandlePackageVersionUpdate(IHttpClientFactory clientFactory, ILoggerFactory loggerFactor, HttpClient gitHubHttpClient, EditorPullRequestManager.RepositoryInfo repositoryInfo, EditorPullRequestManager.CommitInfo commitInfo, IEnumerable<UnityVersion.ReleaseStreamType> releaseStreams)
{
    var manifestJSON = File.ReadAllText(Path.Join(Directory.GetCurrentDirectory(), repositoryInfo.RelativePathToUnityProject, "Packages", "manifest.json"));
    var manifest = UnityVersionBump.Core.UPM.ManifestParser.Parse(manifestJSON);

    var dependenciesByRegistry = manifest.dependencies.GroupBy(dependency => manifest.GetRegistryForPackage(dependency.Key)).ToDictionary(a=>a.Key, a=>a.ToList());

    var outOfDatePackages = new List<(string packageName, string registryURL, PackageVersion currentVersion, PackageVersion newVersion)>();

    foreach (var (registryURL, dependencies) in dependenciesByRegistry)
    {
        var browser = new UnityVersionBump.Core.UPM.Browser(clientFactory.CreateClient("PackageRegistry"), loggerFactor.CreateLogger("PackageRegistry"), registryURL);
        foreach (var (packageName, currentVersion) in dependencies)
        {
            var latestVersion = await browser.GetLatestVersion(packageName);
            if (latestVersion > currentVersion)
            {
                outOfDatePackages.Add((packageName, registryURL, currentVersion, latestVersion));
            }
        }
    }

    foreach (var (packageName, registryURL, currentVersion, newVersion) in outOfDatePackages)
    {
        var alreadyUpToDatePR = await PackagePullRequestManager.CleanupAndCheckForAlreadyExistingPR(gitHubHttpClient, commitInfo, repositoryInfo, packageName, registryURL, currentVersion, newVersion);
        if (alreadyUpToDatePR != null)
        {
            GitHubActionsUtilities.GitHubActionsWriteLine($"PR https://github.com/{repositoryInfo.UserName}/{repositoryInfo.RepositoryName}/pull/{alreadyUpToDatePR} for package {packageName}@{registryURL} is newer or identical to {newVersion}");
            continue;
        }
        
        GitHubActionsUtilities.GitHubActionsWriteLine($"Latest Version for {packageName}@{registryURL}:  {newVersion}");
        
        var newPullRequestID = await PackagePullRequestManager.CreatePullRequestIfTargetVersionNewer(
            gitHubHttpClient,
            commitInfo,
            repositoryInfo,
            packageName,
            registryURL,
            currentVersion,
            newVersion
        );
        

        var hash = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes($"{packageName}@{registryURL}"));
        
        GitHubActionsUtilities.SetOutputVariable($"{hash}-has-newer-version", (newVersion.GetComparable() > currentVersion.GetComparable()).ToString());
        GitHubActionsUtilities.SetOutputVariable($"{hash}-current-version", currentVersion.ToString());
        GitHubActionsUtilities.SetOutputVariable($"{hash}-newest-version", newVersion.ToString());
        if (!string.IsNullOrEmpty(newPullRequestID))
        {
            GitHubActionsUtilities.GitHubActionsWriteLine($"Created new pull request at: https://github.com/{repositoryInfo.UserName}/{repositoryInfo.RepositoryName}/pull/{newPullRequestID} ---");
            GitHubActionsUtilities.SetOutputVariable("pull-request-id", newPullRequestID);
        }
    }
}

static async Task StartAnalysisAsync(ActionInputs inputs, IHttpClientFactory clientFactory, ILoggerFactory loggerFactor)
{
    if (!inputs.TargetRepository.Contains('/'))
    {
        GitHubActionsUtilities.GitHubActionsWriteLine("TargetRepository isn't set to a proper 'owner/repoName' value");
        Environment.Exit(1);
    }

    var repositoryPath = inputs.TargetRepository.Split("/");
    GitHubActionsUtilities.GitHubActionsWriteLine($"Owner:  {string.Join(",", repositoryPath)}");

    var repositoryInfo = new EditorPullRequestManager.RepositoryInfo {
        RelativePathToUnityProject = inputs.UnityProjectPath,
        UserName = repositoryPath[0],
        RepositoryName = repositoryPath[1]
    };
    var commitInfo = new EditorPullRequestManager.CommitInfo {
        APIToken = inputs.GithubToken,
        FullName = "UnityVersionBump (bot)",
        EmailAddress = "unity-version-bump@vincent.mahn.ke",
        PullRequestLabels = inputs.pullRequestLabels,
        PullRequestPrefix = inputs.PullRequestPrefix
    };

    var gitHubHttpClient = clientFactory.CreateClient("github").SetupGitHub(repositoryInfo, commitInfo);

    await HandlePackageVersionUpdate(
        clientFactory,
        loggerFactor,
        gitHubHttpClient,
        repositoryInfo,
        commitInfo,
        inputs.releaseStreams.Select(Enum.Parse<UnityVersion.ReleaseStreamType>)
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

public class PackagePullRequestManager
{
    public static async Task<int?> CleanupAndCheckForAlreadyExistingPR(HttpClient gitHubHttpClient, EditorPullRequestManager.CommitInfo commitInfo, EditorPullRequestManager.RepositoryInfo repositoryInfo, string packageName, string registryURL, PackageVersion currentVersion, PackageVersion newVersion)
    {
        // TODO: Implement logic identical to the one in UnityPullRequestManager
        return null;
    }

    public static async Task<string> CreatePullRequestIfTargetVersionNewer(HttpClient gitHubHttpClient, EditorPullRequestManager.CommitInfo commitInfo, EditorPullRequestManager.RepositoryInfo repositoryInfo, string packageName, string registryURL, PackageVersion currentVersion, PackageVersion newVersion)
    {
        // TODO: Implement logic identical to the one in UnityPullRequestManager
        return null;
    }
}