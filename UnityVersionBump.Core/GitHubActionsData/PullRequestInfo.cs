using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
        public string package { get; set; }
        public string version { get; set; }
    }
}
