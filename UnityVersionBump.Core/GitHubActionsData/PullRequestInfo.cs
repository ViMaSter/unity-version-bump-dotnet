namespace UnityVersionBump.Core.GitHubActionsData
{
    public record PullRequestInfo
    {
        public string type => "unity-version-bump";
        public int version => 1;
        public PullRequestInfoData data { get; }

        public PullRequestInfo(PullRequestInfoData data)
        {
            this.data = data;
        }
    }

    public record PullRequestInfoData
    {
        public string package { get; set; } = null!;
        public string version { get; set; } = null!;
    }
}
