using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnityVersionBump.Core.GitHubActionsData;
using UnityVersionBump.Core.GitHubActionsData.Models.Branches;
using UnityVersionBump.Core.GitHubActionsData.Models.PullRequests;
using UnityVersionBump.Core.UPM.Models;

namespace UnityVersionBump.Core
{
    public static class HttpClientGitHubExtensions
    {
        public static HttpClient SetupGitHub(this HttpClient client, PullRequestManager.RepositoryInfo repositoryInfo, PullRequestManager.CommitInfo commitInfo)
        {
            client.DefaultRequestHeaders.Authorization = new("token", commitInfo.APIToken);
            client.DefaultRequestHeaders.UserAgent.Add(new("UnityVersionBump", "1.0.0"));
            client.BaseAddress = new($"https://api.github.com/repos/{repositoryInfo.UserName}/{repositoryInfo.RepositoryName}/");
            return client;
        }
    }
    public static class PullRequestManager
    {
        public record RepositoryInfo
        {
            public string UserName { get; set; } = null!;
            public string RepositoryName { get; set; } = null!;
            public string RelativePathToUnityProject { get; set; } = null!;
        }

        public record CommitInfo
        {
            public string FullName { get; set; } = null!;
            public string EmailAddress { get; set; } = null!;
            public string APIToken { get; set; } = null!;
            public string[] PullRequestLabels { get; set; } = null!;
            public string PullRequestPrefix { get; set; } = null!;
        }

        public static class EditorPRs
        {
            const string EDITOR_PACKAGE = "editor";
            private static readonly Regex ContentFilter = new("<!--uvb(.*)-->");

            public static async Task<IList<(PullRequest pullRequest, UnityVersion version)>> GetActivePRs(HttpClient httpClient)
            {
                var pullRequests = new List<PullRequest>();
                var nextPage = 1;
                do
                {
                    var response = await httpClient.GetAsync($"pulls?per_page=100&page={nextPage}");
                    var contentAsString = await response.Content.ReadAsStringAsync();
                    var parsedData = JsonConvert.DeserializeObject<IEnumerable<PullRequest>>(contentAsString)!.ToList();
                    pullRequests.AddRange(parsedData);
                    if (parsedData.Count < 100)
                    {
                        nextPage = -1;
                        break;
                    }

                    ++nextPage;
                } while (nextPage != -1);

                return pullRequests
                    .Where(PullRequestHasInfo)
                    .Select(ParsePullRequestInfo)
                    .Where(IsEditorPackage)
                    .Select(ConvertToPRNumberAndUnityVersion)
                    .ToList();
            }

            private static (PullRequest pullRequest, UnityVersion) ConvertToPRNumberAndUnityVersion((PullRequest pullRequest, PullRequestInfo pullRequestContent) package)
            {
                return (package.pullRequest, ProjectVersion.FromProjectVersionTXTSyntax(package.pullRequestContent.data.version));
            }

            private static bool IsEditorPackage((PullRequest pullRequest, PullRequestInfo pullRequestContent) package)
            {
                return package.pullRequestContent.data.package == EDITOR_PACKAGE;
            }

            private static (PullRequest pullRequest, PullRequestInfo pullRequestContent) ParsePullRequestInfo(PullRequest pullRequest)
            {
                var pullRequestContent = JsonConvert.DeserializeObject<PullRequestInfo>(ContentFilter.Match(pullRequest.body).Groups[1].Value)!;
                return (pullRequest, pullRequestContent);
            }

            private static bool PullRequestHasInfo(PullRequest pullRequest)
            {
                return ContentFilter.IsMatch(pullRequest.body);
            }
        }

        public static class PackagePRs
        {
            public record UpdateInfo
            {
                public string packageName;
                public string registryURL;
                public PackageVersion currentVersion;
                public PackageVersion newVersion;
            };

            public static async Task<Dictionary<string, string>> GeneratePRs(IHttpClientFactory clientFactory, ILoggerFactory loggerFactor, HttpClient gitHubHttpClient, RepositoryInfo repositoryInfo, CommitInfo commitInfo, bool includePreReleasePackages)
            {
                var manifestJSON = await File.ReadAllTextAsync(Path.Join(Directory.GetCurrentDirectory(), repositoryInfo.RelativePathToUnityProject, "Packages", "manifest.json"));
                var manifest = Manifest.Parse(manifestJSON);
                var updatesNeeded = await PackagePRs.GetPackagesThatNeedUpdate(manifest, clientFactory.CreateClient("PackageRegistry"), loggerFactor.CreateLogger("PackageRegistry"), includePreReleasePackages);
                var updatePRs = new Dictionary<string, string>();
                foreach (var updateInfo in updatesNeeded)
                {
                    updatePRs.Add(updateInfo.packageName, await CreatePullRequest(gitHubHttpClient, commitInfo, repositoryInfo, Manifest.Parse(Manifest.Generate(manifest)), updateInfo));
                }

                return updatePRs;
            }

            public static async Task<IEnumerable<UpdateInfo>> GetPackagesThatNeedUpdate(Manifest manifest, HttpClient httpClient, ILogger logger, bool includePreReleasePackages)
            {
                var dependenciesByRegistry = manifest.dependencies.GroupBy(dependency => manifest.GetRegistryForPackage(dependency.Key)).ToDictionary(a => a.Key, a => a.ToList());

                var outOfDatePackages = new List<UpdateInfo>();

                foreach (var (registryURL, dependencies) in dependenciesByRegistry)
                {
                    var browser = new UnityVersionBump.Core.UPM.Browser(httpClient, logger, registryURL);
                    foreach (var (packageName, currentVersion) in dependencies)
                    {
                        var latestVersion = await browser.GetLatestVersion(packageName, includePreReleasePackages);
                        if (latestVersion > currentVersion)
                        {
                            outOfDatePackages.Add(new UpdateInfo { packageName = packageName, registryURL = registryURL, currentVersion = currentVersion, newVersion = latestVersion });
                        }
                    }
                }

                return outOfDatePackages;
            }

            public static async Task<string> CreatePullRequest(HttpClient httpClient, CommitInfo commitInfo, RepositoryInfo repositoryInfo, Manifest manifest, PackagePRs.UpdateInfo updateInfo)
            {
                manifest.dependencies[updateInfo.packageName] = updateInfo.newVersion;
                
                var baseBranch = await GitHubAPI.GetDefaultBranchName(httpClient, repositoryInfo);
                var shaHashOfLastCommitOnBaseBranch = await GitHubAPI.GetLastCommitSHAHash(httpClient, baseBranch);

                var newManifestContent = Manifest.Generate(manifest);
                var shaHashOfNewProjectVersionTXT = await GitHubAPI.CreateBlobForContent(httpClient, newManifestContent);
                var shaHasOfTreeForNewFileProjectVersionTXT = await GitHubAPI.CreateTree(httpClient, "Packages/manifest.json", shaHashOfLastCommitOnBaseBranch, repositoryInfo.RelativePathToUnityProject, shaHashOfNewProjectVersionTXT);

                var commitMessage = $"build(deps): bump `{updateInfo.packageName}` from `{updateInfo.currentVersion}` to `{updateInfo.newVersion}`";
                var shaHasOfCommitOfTree = await GitHubAPI.CreateCommit(httpClient, commitMessage, shaHasOfTreeForNewFileProjectVersionTXT, shaHashOfLastCommitOnBaseBranch, commitInfo);

                var targetBranchName = $"{commitInfo.PullRequestPrefix}/{updateInfo.packageName}/{updateInfo.newVersion}";
                await GitHubAPI.CreateBranch(httpClient, targetBranchName, shaHasOfCommitOfTree);

                var body = TextFormatting.GenerateBody(updateInfo);

                var pullRequest = await GitHubAPI.CreatePullRequest(httpClient, baseBranch, targetBranchName, commitMessage, body);

                await GitHubAPI.ApplyLabels(httpClient, pullRequest, commitInfo.PullRequestLabels);

                return pullRequest.ToString("D1");
            }
        }

        private static class GitHubAPI
        {
            public static async Task<string> GetDefaultBranchName(HttpClient httpClient, RepositoryInfo repositoryInfo)
            {
                var response = await httpClient.GetAsync($"/repos/{repositoryInfo.UserName}/{repositoryInfo.RepositoryName}");
                return JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync())!["default_branch"];
            }

            public static async Task<string> CreateBlobForContent(HttpClient httpClient, string newProjectVersionTXTContent)
            {
                var response = await httpClient.PostAsync("git/blobs", JsonContent.Create(new
                {
                    encoding = "utf-8",
                    content = newProjectVersionTXTContent
                }));
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(await response.Content.ReadAsStringAsync())!["sha"];
            }

            public static async Task<string> CreateTree(HttpClient httpClient, string fileName, string parentCommitSHAHash, string prefixToProjectSettingsTXT, string shaHashOfNewProjectVersionTXT)
            {
                var body = new
                {
                    base_tree = parentCommitSHAHash,
                    tree = new object[] { new
                    {
                        mode = "100644",
                        type = "blob",
                        path = $"{(string.IsNullOrEmpty(prefixToProjectSettingsTXT) ? "" : prefixToProjectSettingsTXT + "/")}{fileName}",
                        sha = shaHashOfNewProjectVersionTXT
                    }
                }
                };
                var response = await httpClient.PostAsync("git/trees", JsonContent.Create(body));

                return JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync())!["sha"];
            }

            public static async Task<string> GetLastCommitSHAHash(HttpClient httpClient, string baseBranch)
            {
                var response = await httpClient.GetAsync($"branches/{baseBranch}");
                return JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync())!["commit"]["sha"];
            }

            public static async Task<string> CreateCommit(HttpClient httpClient, string commitMessage, string shaHasOfTreeForNewFileProjectVersionTXT, string shaHashOfLastCommitOnBaseBranch, CommitInfo commitInfo)
            {
                var response = await httpClient.PostAsync("git/commits", JsonContent.Create(new
                {
                    message = commitMessage,
                    parents = new[] { shaHashOfLastCommitOnBaseBranch },
                    tree = shaHasOfTreeForNewFileProjectVersionTXT,
                    author = new
                    {
                        name = commitInfo.FullName,
                        email = commitInfo.EmailAddress
                    }
                }));

                return JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync())!["sha"];
            }

            public static async Task CreateBranch(HttpClient httpClient, object branchName, string shaHashOfCommitOfTree)
            {
                var response = await httpClient.PostAsync("git/refs", JsonContent.Create(new
                {
                    @ref = $"refs/heads/{branchName}",
                    sha = shaHashOfCommitOfTree
                }));

                if (response.StatusCode != HttpStatusCode.Created)
                {
                    throw new HttpRequestException($"Expected '{HttpStatusCode.Created}'. Instead received '{response.StatusCode}'.\r\n{await response.Content.ReadAsStringAsync()}", null, response.StatusCode);
                }
            }

            public static async Task<long> CreatePullRequest(HttpClient httpClient, string baseBranch, string targetBranchName, string title, string body)
            {
                var response = await httpClient.PostAsync("pulls", JsonContent.Create(new
                {
                    title,
                    body,
                    @base = baseBranch,
                    head = targetBranchName
                }));

                return JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync())!["number"];
            }

            public static async Task ClosePullRequest(HttpClient httpClient, int prNumber)
            {
                // write comment
                var commentResponse = await httpClient.PostAsync($"issues/{prNumber}/comments", JsonContent.Create(new
                {
                    body = "🛑 This PR is targeting a version that is no longer up-to-date. Closing this PR and creating a new PR for the newest version in its stead. 🛑"
                }));
                if (commentResponse.StatusCode != HttpStatusCode.Created)
                {
                    throw new HttpRequestException($"Expected '{HttpStatusCode.Created}'. Instead received '{commentResponse.StatusCode}'.\r\n{await commentResponse.Content.ReadAsStringAsync()}", null, commentResponse.StatusCode);
                }

                // get PR
                var prInfo = JsonConvert.DeserializeObject<PullRequest>(await (await httpClient.GetAsync("pulls/" + prNumber)).Content.ReadAsStringAsync())!;

                // close PR
                var response = await httpClient.PostAsync("pulls/" + prNumber, JsonContent.Create(new
                {
                    state = "closed"
                }));

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new HttpRequestException($"Expected '{HttpStatusCode.OK}'. Instead received '{response.StatusCode}'.\r\n{await response.Content.ReadAsStringAsync()}", null, response.StatusCode);
                }

                // delete branch
                await DeleteBranch(httpClient, prInfo.head.label.Split(":")[1]);
            }

            public static async Task DeleteBranch(HttpClient httpClient, string branchName)
            {
                var deleteBranchResponse = await httpClient.DeleteAsync("git/refs/heads/" + branchName);
                if (deleteBranchResponse.StatusCode != HttpStatusCode.NoContent)
                {
                    throw new HttpRequestException($"Expected '{HttpStatusCode.NoContent}'. Instead received '{deleteBranchResponse.StatusCode}'.\r\n{await deleteBranchResponse.Content.ReadAsStringAsync()}", null, deleteBranchResponse.StatusCode);
                }
            }

            public static async Task ApplyLabels(HttpClient httpClient, long pullRequest, string[] commitInfoPullRequestLabels)
            {
                if (commitInfoPullRequestLabels.Length == 0)
                {
                    return;
                }

                var response = await httpClient.PostAsync($"issues/{pullRequest}/labels", JsonContent.Create(new
                {
                    labels = commitInfoPullRequestLabels
                }));

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new HttpRequestException($"Expected '{HttpStatusCode.OK}'. Instead received '{response.StatusCode}'.\r\n{await response.Content.ReadAsStringAsync()}", null, response.StatusCode);
                }
            }

            public static async Task<IEnumerable<string>> GetAllBranches(HttpClient httpClient)
            {
                var response = await httpClient.GetAsync("branches");

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new HttpRequestException($"Expected '{HttpStatusCode.OK}'. Instead received '{response.StatusCode}'.\r\n{await response.Content.ReadAsStringAsync()}", null, response.StatusCode);
                }

                return JsonConvert.DeserializeObject<IEnumerable<Branch>>(await response.Content.ReadAsStringAsync())!.Select(branch => branch.name);
            }
        }

        private static class TextFormatting
        {
            public static string GenerateBody(UnityVersion currentVersion, UnityVersion targetVersion)
            {
                return $"Bumps the [Unity Editor](https://unity3d.com/get-unity/download) version from `{currentVersion.ToUnityStringWithRevision()}` to `{targetVersion.ToUnityStringWithRevision()}`.\r\n\r\n{GenerateReleaseNotesLink(targetVersion)}\r\n\r\n<!--uvb {GenerateJSONInfo(targetVersion)} -->";
            }

            internal static string GenerateBody(PackagePRs.UpdateInfo updateInfo)
            {
                return $"Bumps the version of `{updateInfo.packageName}` version from `{updateInfo.currentVersion}` to `{updateInfo.newVersion}`.<!--uvb {GenerateJSONInfo(updateInfo)} -->";
            }

            private static string GenerateJSONInfo(PackagePRs.UpdateInfo updateInfo)
            {
                return JsonConvert.SerializeObject(new PullRequestInfo(new PullRequestInfoData()
                {
                    package = updateInfo.packageName,
                    version = updateInfo.newVersion.ToString()
                }));
            }

            private static string GenerateJSONInfo(UnityVersion targetVersion)
            {
                return JsonConvert.SerializeObject(new PullRequestInfo(new PullRequestInfoData(){
                    package = "editor", 
                    version = targetVersion.ToUnityStringWithRevision()
                }));
            }

            private static string GenerateReleaseNotesLink(UnityVersion targetVersion)
            {
                var preReleaseStreamTypes = new[] { UnityVersion.ReleaseStreamType.Alpha, UnityVersion.ReleaseStreamType.Beta };
                if (preReleaseStreamTypes.Contains(targetVersion.ReleaseStream))
                {
                    return $"- [{targetVersion.ReleaseStream} release notes](https://unity3d.com/beta/2022.1b#:~:text=Latest%20version-,Release%20notes,-Archive)";
                }

                return "- [Release notes](https://unity3d.com/get-unity/download/archive)";
            }
        }

        public static async Task<int?> CleanupAndCheckForAlreadyExistingPR(HttpClient httpClient, CommitInfo commitInfo, RepositoryInfo repositoryInfo, UnityVersion currentVersion, UnityVersion? targetVersion)
        {
            var latestPR = await CloseAllButLatestPR(httpClient, commitInfo);

            // if there's no new version, the update is done
            if (targetVersion == null)
            {
                return null;
            }

            if (latestPR != null)
            {
                // if there is a PR matching (or newer than) the target version
                if (latestPR.Value.version.CompareTo(targetVersion) >= 0)
                {
                    // return that PR number
                    return latestPR.Value.number;
                }

                // if there is a new version available and the existing PR is older, close it
                await GitHubAPI.ClosePullRequest(httpClient, latestPR.Value.number);
            }

            return null;
        }

        private static async Task<(int number, UnityVersion version)?> CloseAllButLatestPR(HttpClient httpClient, CommitInfo commitInfo)
        {
            var currentEditorPullRequests = await EditorPRs.GetActivePRs(httpClient);
            var branchesOfPRs = currentEditorPullRequests.Select(entry => entry.pullRequest.head.label.Split(":")[1]);
            var danglingBranches = (await GitHubAPI.GetAllBranches(httpClient)).Where(branch => branch.StartsWith(commitInfo.PullRequestPrefix)).Where(branch => !branchesOfPRs.Contains(branch));
            foreach (var danglingBranch in danglingBranches)
            {
                await GitHubAPI.DeleteBranch(httpClient, danglingBranch);
            }

            var sortedFromOldestToNewestVersion = currentEditorPullRequests.OrderBy(tuple => tuple.version).Select(entry => (entry.pullRequest.number, entry.version)).ToList();

            var latestVersionByPR = sortedFromOldestToNewestVersion.FirstOrDefault();

            // if there is more than one PR
            if (latestVersionByPR != default)
            {
                // close all that are older than the latest
                var outdatedPRs = sortedFromOldestToNewestVersion.Where(tuple => tuple.number!= latestVersionByPR.number);
                foreach (var (prNumber, _) in outdatedPRs)
                {
                    await GitHubAPI.ClosePullRequest(httpClient, prNumber);
                }
                return latestVersionByPR;
            }

            return null;
        }

        public static async Task<string> CreatePullRequestIfTargetVersionNewer(HttpClient httpClient, CommitInfo commitInfo, RepositoryInfo repositoryInfo, UnityVersion currentVersion, UnityVersion targetVersion)
        {
            if (currentVersion.GetComparable() >= targetVersion.GetComparable())
            {
                return "";
            }

            var baseBranch = await GitHubAPI.GetDefaultBranchName(httpClient, repositoryInfo);
            var shaHashOfLastCommitOnBaseBranch = await GitHubAPI.GetLastCommitSHAHash(httpClient, baseBranch);

            var newProjectVersionTXTContent = ProjectVersion.GenerateProjectVersionTXTContent(targetVersion);
            var shaHashOfNewProjectVersionTXT = await GitHubAPI.CreateBlobForContent(httpClient, newProjectVersionTXTContent);
            var shaHasOfTreeForNewFileProjectVersionTXT = await GitHubAPI.CreateTree(httpClient, "ProjectSettings/ProjectVersion.txt", shaHashOfLastCommitOnBaseBranch, repositoryInfo.RelativePathToUnityProject, shaHashOfNewProjectVersionTXT);

            var commitMessage = $"build(deps): bump `UnityEditor` from `{currentVersion.ToUnityString()}` to `{targetVersion.ToUnityString()}`";
            var shaHasOfCommitOfTree = await GitHubAPI.CreateCommit(httpClient, commitMessage, shaHasOfTreeForNewFileProjectVersionTXT, shaHashOfLastCommitOnBaseBranch, commitInfo);

            var targetBranchName = $"{commitInfo.PullRequestPrefix}/editor/{targetVersion.ToUnityString().Replace(".", "-")}";
            await GitHubAPI.CreateBranch(httpClient, targetBranchName, shaHasOfCommitOfTree);

            var body = TextFormatting.GenerateBody(currentVersion, targetVersion);

            var pullRequest = await GitHubAPI.CreatePullRequest(httpClient, baseBranch, targetBranchName, commitMessage, body);

            await GitHubAPI.ApplyLabels(httpClient, pullRequest, commitInfo.PullRequestLabels);

            return pullRequest.ToString("D1");
        }
    }
}
