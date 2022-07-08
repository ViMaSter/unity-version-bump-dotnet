using Newtonsoft.Json;
using UnityVersionBump.Core.UPM.SerializedResponses;

namespace UnityVersionBump.Core.UPM
{
    public class Browser
    {
        public const string UNITY_DEFAULT_PACKAGE_REPOSITORY_ROOT = "https://packages.unity.com";

        private readonly HttpClient _httpClient;
        private readonly string _repositoryRoot;

        public Browser(HttpClient httpClient, string repositoryRoot)
        {
            _httpClient = httpClient;
            _repositoryRoot = repositoryRoot;
        }

        private async Task<PackageInfo> GetPackage(string packageName)
        {
            var response = await _httpClient.GetAsync($"{_repositoryRoot}/{packageName}");
            if (!response.IsSuccessStatusCode)
            {
                throw new NotSupportedException($"Unexpected return code {(int)response.StatusCode}: '{await response.Content.ReadAsStringAsync()}'");
            }
            return JsonConvert.DeserializeObject<PackageInfo>(await response.Content.ReadAsStringAsync())!;
        }

        public async Task<PackageVersion> GetLatestVersion(string packageName)
        {
            var package = await GetPackage(packageName);
            return package.versions.Keys.Select(versionString => new PackageVersion(versionString)).OrderByDescending(a => a).First();
        }
    }
}
