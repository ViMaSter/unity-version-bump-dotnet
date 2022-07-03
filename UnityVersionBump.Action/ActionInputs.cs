using CommandLine;

namespace UnityVersionBump.Action
{
    public class ActionInputs
    {
        [Option('g', "githubToken",
            Required = true,
            HelpText = "When using --targetRepository, set this to a personal access token (PAT) is allowed to read code and create pull requests for the target repository")]
        public string GithubToken
        {
            get;
            set;
        } = "";

        [Option('u', "unityProjectPath",
            Required = false,
            HelpText = "Path to the Unity project root, if it's not the root of the repository this action runs on with NO leading or trailing space [example: 'unity-project', 'projects/game1']")]
        public string UnityProjectPath
        {
            get;
            set;
        } = "/github/workspace";

        [Option('t', "targetRepository",
            Required = false,
            HelpText = "When attempting to target another repository, set this to '$OWNER/$NAME' (defaults to repository this action is run in)")]
        public string TargetRepository
        {
            get;
            set;
        } = "";

        [Option('p', "pullRequestPrefix",
            Required = false,
            HelpText = "Prefix used by pull requests created by this action (defaults to 'unityversionbump')")]
        public string PullRequestPrefix
        {
            get;
            set;
        } = "unityversionbump";

        public string[] releaseStreams = null!;

        [Option('r', "releaseStreams",
            Required = true,
            HelpText = "(Comma-separated list of) release streams to consider when determining the latest version. (Possible values: 'Stable', 'LTS', 'Beta', 'Alpha') [example: 'Stable', 'LTS,Stable', 'Stable,Beta,Alpha']")]
        public string ReleaseStreams
        {
            get => string.Join(",", releaseStreams);
            set
            {
                if ((string?)value is not { Length: > 0 })
                {
                    throw new ArgumentException($"{nameof(ReleaseStreams)} can't be empty", nameof(ReleaseStreams));
                }

                releaseStreams = value.Split(",").Select(entry => entry.Trim()).ToArray();
            }
        }

        public string[] pullRequestLabels = null!;

        [Option('l', "pullRequestLabels",
            Required = false,
            HelpText = "(Comma-separated list of) labels to add to pull requests created by this action. (Ensure that all labels exist for this repository.)")]
        public string PullRequestLabels
        {
            get => string.Join(",", pullRequestLabels);
            set
            {
                if ((string?)value is not { Length: > 0 })
                {
                    pullRequestLabels = Array.Empty<string>();
                    return;
                }

                pullRequestLabels = value.Split(",").Select(entry => entry.Trim()).ToArray();
            }
        }
    }
}