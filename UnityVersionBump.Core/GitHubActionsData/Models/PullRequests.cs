namespace UnityVersionBump.Core.GitHubActionsData.Models.PullRequests
{
    public class PullRequest
    {
        public int number { get; set; }
        public string body { get; set; } = null!;
        public Head head { get; set; } = null!;
    }

    public class Head
    {
        public string label { get; set; } = null!;
    }
}
