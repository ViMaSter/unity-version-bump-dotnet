using CommandLine;

namespace UnityVersionBump.Action
{
    public class ActionInputs
    {
        public string[] releaseStreams = null!;

        [Option('p', "projectPath",
            Required = false,
            HelpText = "Path to the Unity project root, if it's not the root of the repository this action runs on with NO leading or trailing space [example: 'unity-project', 'projects/game1']")]
        public string ProjectPath
        {
            get;
            set;
        } = "/github/workspace";

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
    }
}