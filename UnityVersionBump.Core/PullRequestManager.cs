using System.Net;
using System.Net.Http.Json;
using System.Text;

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
                return $"Bumps the [Unity Editor](https://unity3d.com/get-unity/download) version from `{currentVersion.ToUnityStringWithRevision()}` to `{targetVersion.ToUnityStringWithRevision()}`.\r\n\r\n{GenerateReleaseNotesLink(targetVersion)}";
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
