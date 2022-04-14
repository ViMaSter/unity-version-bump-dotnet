using System.Text.Json;
using UnityVersionBump.Core.SerializedResponses;

namespace UnityVersionBump.Core
{
    public class ProjectVersion
    {
        public static UnityVersion DetermineUnityVersion(string projectVersionTxt, bool isLTS)
        {
            var values = projectVersionTxt
                .Split("\n")
                .Select(line=>line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(entry => entry.Split(":"))
                .ToDictionary(entry => entry[0].Trim(), entry => entry[1].Trim());
            return new(values["m_EditorVersion"], isLTS);
        }

        private const string RELEASES_PATH = "https://public-cdn.cloud.unity3d.com/hub/prod/releases-win32.json";

        public static UnityVersion GetLatestFromHub(HttpClient httpClient, IEnumerable<UnityVersion.ReleaseStreamType> consideredReleaseStreams)
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

            var officialReleases = serverResponse.official.Select(release => new UnityVersion(release.version, release.lts));
            var betaReleases = serverResponse.beta.Select(release => new UnityVersion(release.version, release.lts));
            var allAvailableReleases = officialReleases.Concat(betaReleases).ToList();

            var releasesForStreams = allAvailableReleases.Where(release => qualifiedReleaseStreams.Contains(release.ReleaseStream)).ToList();
            var releasesForLTS = allAvailableReleases.Where(release => release.IsLTS).Where(release => qualifiedReleaseStreams.Contains(UnityVersion.ReleaseStreamType.LTS)).ToList();

            var qualifyingReleases = releasesForStreams.Concat(releasesForLTS).ToList();
            if (!qualifyingReleases.Any())
            {
                throw new FileNotFoundException($"No release found for the following release streams: '{(releasesForLTS.Any() ? string.Join("','", releasesForLTS) : "NONE")}'");
            }

            return qualifyingReleases.OrderByDescending(release=>release).First();

        }
    }
}
