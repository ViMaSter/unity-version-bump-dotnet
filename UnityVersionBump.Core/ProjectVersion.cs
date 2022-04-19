using System.Text.Json;
using System.Text.RegularExpressions;
using UnityVersionBump.Core.Exceptions;
using UnityVersionBump.Core.SerializedResponses;

namespace UnityVersionBump.Core
{
    public class ProjectVersion
    {
        public static UnityVersion FromProjectVersionTXT(string fileContent)
        {
            var values = fileContent
                .Split("\n")
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(entry => entry.Split(":"))
                .ToDictionary(entry => entry[0].Trim(), entry => entry[1].Trim());

            return FromProjectVersionTXTSyntax(values["m_EditorVersionWithRevision"]);
        }

        private static readonly Regex VersionPartsRegex = new("^(?'version'.*)\\((?'revision'.*)\\)$");
        public static UnityVersion FromProjectVersionTXTSyntax(string unityVersionWithRevision)
        {
            var versionParts = VersionPartsRegex.Match(unityVersionWithRevision);
            return new(versionParts.Groups["version"].Value, versionParts.Groups["revision"].Value, false);
        }

        private const string RELEASES_PATH = "https://public-cdn.cloud.unity3d.com/hub/prod/releases-win32.json";

        private static readonly Regex RevisionFromURLRegex = new("download(?:_unity)?\\/([a-f0-9]{12})\\/");
        public static string ExtractRevisionFromDownloadURL(string downloadURL)
        {
            var match = RevisionFromURLRegex.Match(downloadURL);
            if (!match.Success)
            {
                throw new InvalidDownloadURLSyntaxException(downloadURL);
            }
            return RevisionFromURLRegex.Match(downloadURL).Groups[1].Value;
        }

        public static string GenerateProjectVersionTXTContent(UnityVersion unityVersion)
        {
            return $"m_EditorVersion: {unityVersion.ToUnityString()}\nm_EditorVersionWithRevision: {unityVersion.ToUnityStringWithRevision()}\n";
        }

        public static UnityVersion? GetLatestFromHub(HttpClient httpClient, IEnumerable<UnityVersion.ReleaseStreamType> consideredReleaseStreams)
        {
            var qualifiedReleaseStreams = consideredReleaseStreams.ToList();
            if (!qualifiedReleaseStreams.Any())
            {
                throw new ArgumentException("Need to specify at least one release stream", nameof(consideredReleaseStreams));
            }

            var httpResponse = httpClient.GetAsync(RELEASES_PATH).ConfigureAwait(false).GetAwaiter().GetResult();
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new InvalidDataException($"'{RELEASES_PATH}' responded with status code {httpResponse.StatusCode}");
            }

            var serverResponse = JsonSerializer.Deserialize<Response>(httpResponse.Content.ReadAsStream());
            if (serverResponse == null)
            {
                throw new NotSupportedException($"'{RELEASES_PATH}' responded with data that couldn't be deserialized:{Environment.NewLine}{httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult()}");
            }

            var officialReleases = serverResponse.official.Select(release => new UnityVersion(release.version, ExtractRevisionFromDownloadURL(release.downloadUrl), release.lts));
            var betaReleases = serverResponse.beta.Select(release => new UnityVersion(release.version, ExtractRevisionFromDownloadURL(release.downloadUrl), release.lts));
            var allAvailableReleases = officialReleases.Concat(betaReleases).ToList();

            var releasesForStreams = allAvailableReleases.Where(release => qualifiedReleaseStreams.Contains(release.ReleaseStream)).ToList();
            var releasesForLTS = allAvailableReleases.Where(release => release.IsLTS).Where(release => qualifiedReleaseStreams.Contains(UnityVersion.ReleaseStreamType.LTS)).ToList();

            var qualifyingReleases = releasesForStreams.Concat(releasesForLTS).ToList();
            if (!qualifyingReleases.Any())
            {
                throw new FileNotFoundException($"No release found for the following release streams: '{(qualifiedReleaseStreams.Any() ? string.Join("','", qualifiedReleaseStreams) : "NONE")}'");
            }

            return qualifyingReleases.OrderByDescending(release=>release).First();

        }
    }
}
