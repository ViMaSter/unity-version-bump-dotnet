using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml;
using UnityVersionBump.Core.GitHubActionsData;
using UnityVersionBump.Core.GitHubActionsData.Models;

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
            public string PullRequestPrefix { get; set; }
        }

        public static class EditorPRs
        {
            const string EDITOR_PACKAGE = "editor";
            private static readonly Regex contentFilter = new Regex("<!--uvb(.*)-->");

            public static async Task<IList<(int number, UnityVersion version)>> GetActivePRs(HttpClient httpClient)
            {
                var pullRequests = new List<PullRequest>();
                var nextPage = 1;
                do
                {
                    var response = await httpClient.GetAsync($"pulls?per_page=100&page={nextPage}");
                    var contentAsString = await response.Content.ReadAsStringAsync();
                    var parsedData = JsonSerializer.Deserialize<IEnumerable<PullRequest>>(contentAsString);
                    pullRequests.AddRange(parsedData);
                    if (parsedData.Count() < 100)
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

            private static (int number, UnityVersion) ConvertToPRNumberAndUnityVersion((PullRequest pullRequest, PullRequestInfo pullRequestContent) package)
            {
                return (package.pullRequest.number, ProjectVersion.FromProjectVersionTXTSyntax(package.pullRequestContent.data.version));
            }

            private static bool IsEditorPackage((PullRequest pullRequest, PullRequestInfo pullRequestContent) package)
            {
                return package.pullRequestContent.data.package == EDITOR_PACKAGE;
            }

            private static (PullRequest pullRequest, PullRequestInfo pullRequestContent) ParsePullRequestInfo(PullRequest pullRequest)
            {
                var pullRequestContent = JsonSerializer.Deserialize<PullRequestInfo>(contentFilter.Match(pullRequest.body).Groups[1].Value)!;
                return (pullRequest, pullRequestContent);
            }

            private static bool PullRequestHasInfo(PullRequest pullRequest)
            {
                return contentFilter.IsMatch(pullRequest.body);
            }
        }

        private static class GitHubAPI
        {
            public static async Task<string> GetDefaultBranchName(HttpClient httpClient, RepositoryInfo repositoryInfo)
            {
                var response = await httpClient.GetAsync($"/repos/{repositoryInfo.UserName}/{repositoryInfo.RepositoryName}");
                return System.Text.Json.Nodes.JsonNode.Parse(await response.Content.ReadAsStreamAsync())!["default_branch"]!.ToString();
            }

            public static async Task<string> CreateBlobForContent(HttpClient httpClient, string newProjectVersionTXTContent)
            {
                var response = await httpClient.PostAsync("git/blobs", JsonContent.Create(new
                {
                    encoding = "utf-8",
                    content = newProjectVersionTXTContent
                }));
                return System.Text.Json.Nodes.JsonNode.Parse(await response.Content.ReadAsStreamAsync())!["sha"]!.ToString();
            }

            public static async Task<string> CreateTree(HttpClient httpClient, string parentCommitSHAHash, string prefixToProjectSettingsTXT, string shaHashOfNewProjectVersionTXT)
            {
                const string PROJECT_VERSION_TXT_PATH = "ProjectSettings/ProjectVersion.txt";

                var body = new
                {
                    base_tree = parentCommitSHAHash,
                    tree = new object[] { new
                    {
                        mode = "100644",
                        type = "blob",
                        path = $"{(string.IsNullOrEmpty(prefixToProjectSettingsTXT) ? "" : prefixToProjectSettingsTXT + "/")}{PROJECT_VERSION_TXT_PATH}",
                        sha = shaHashOfNewProjectVersionTXT
                    }
                }
                };
                var response = await httpClient.PostAsync("git/trees", JsonContent.Create(body));

                return System.Text.Json.Nodes.JsonNode.Parse(await response.Content.ReadAsStreamAsync())!["sha"]!.ToString();
            }

            public static async Task<string> GetLastCommitSHAHash(HttpClient httpClient, string baseBranch)
            {
                var response = await httpClient.GetAsync($"branches/{baseBranch}");
                return System.Text.Json.Nodes.JsonNode.Parse(await response.Content.ReadAsStreamAsync())!["commit"]!["sha"]!.ToString();
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

                return System.Text.Json.Nodes.JsonNode.Parse(await response.Content.ReadAsStreamAsync())!["sha"]!.ToString();
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

                return System.Text.Json.Nodes.JsonNode.Parse(await response.Content.ReadAsStreamAsync())!["number"]!.GetValue<long>();
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
                var prInfo = JsonSerializer.Deserialize<PullRequest>(await (await httpClient.GetAsync("pulls/" + prNumber)).Content.ReadAsStringAsync())!;

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
                var deleteBranchResponse = await httpClient.DeleteAsync("git/refs/heads/" + prInfo.head.label.Split(":")[1]!);
                if (deleteBranchResponse.StatusCode != HttpStatusCode.NoContent)
                {
                    throw new HttpRequestException($"Expected '{HttpStatusCode.NoContent}'. Instead received '{response.StatusCode}'.\r\n{await response.Content.ReadAsStringAsync()}", null, response.StatusCode);
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
        }

        private static class TextFormatting
        {
            public static string GenerateBody(UnityVersion currentVersion, UnityVersion targetVersion)
            {
                return $"Bumps the [Unity Editor](https://unity3d.com/get-unity/download) version from `{currentVersion.ToUnityStringWithRevision()}` to `{targetVersion.ToUnityStringWithRevision()}`.\r\n\r\n{GenerateReleaseNotesLink(targetVersion)}\r\n\r\n<!--uvb {GenerateJSONInfo(targetVersion)} -->";
            }

            private static string GenerateJSONInfo(UnityVersion targetVersion)
            {
                return JsonSerializer.Serialize(new PullRequestInfo(new PullRequestInfoData(){
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
            var latestPR = await CloseAllButLatestPR(httpClient);

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

        private static async Task<(int number, UnityVersion version)?> CloseAllButLatestPR(HttpClient httpClient)
        {
            var currentEditorPullRequests = await EditorPRs.GetActivePRs(httpClient);
            var sortedFromOldestToNewestVersion = currentEditorPullRequests.OrderBy(tuple => tuple.version).ToList();
            var latestPR = sortedFromOldestToNewestVersion.FirstOrDefault();

            // if there is more than one PR
            if (latestPR != default)
            {
                // close all that are older than the latest
                var outdatedPRs = sortedFromOldestToNewestVersion.Where(tuple => tuple.number != latestPR.number);
                foreach (var (prNumber, _) in outdatedPRs)
                {
                    await GitHubAPI.ClosePullRequest(httpClient, prNumber);
                }
                return latestPR;
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
            var shaHasOfTreeForNewFileProjectVersionTXT = await GitHubAPI.CreateTree(httpClient, shaHashOfLastCommitOnBaseBranch, repositoryInfo.RelativePathToUnityProject, shaHashOfNewProjectVersionTXT);

            var commitMessage = $"build(deps): bump UnityEditor from `{currentVersion.ToUnityString()}` to `{targetVersion.ToUnityString()}`";
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
