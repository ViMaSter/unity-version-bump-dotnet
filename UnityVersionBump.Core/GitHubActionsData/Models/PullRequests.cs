namespace UnityVersionBump.Core.GitHubActionsData.Models.PullRequests
{
    public class PullRequest
    {
        public int number { get; set; }
        public string body { get; set; }
        public Head head { get; set; }
    }

    public class Head
    {
        public string label { get; set; }
    }
}
