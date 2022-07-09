using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UnityVersionBump.Core.UPM.SerializedResponses;

namespace UnityVersionBump.Core.UPM
{
    public class Browser
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger? _logger;
        private readonly string _repositoryRoot;

        public Browser(HttpClient httpClient, ILogger? logger, string repositoryRoot)
        {
            _httpClient = httpClient;
            _logger = logger;
            _repositoryRoot = repositoryRoot;
        }

        private async Task<PackageInfo?> GetPackage(string packageName)
        {
            var response = await _httpClient.GetAsync($"{_repositoryRoot}/{packageName}");
            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogWarning($"Unexpected return code attempting to fetch package info for '{packageName}' {(int)response.StatusCode}: '{await response.Content.ReadAsStringAsync()}'");
                return null;
            }
            return JsonConvert.DeserializeObject<PackageInfo>(await response.Content.ReadAsStringAsync())!;
        }

        public async Task<PackageVersion?> GetLatestVersion(string packageName)
        {
            var package = await GetPackage(packageName);
            return package?.versions.Keys.Select(versionString => new PackageVersion(versionString)).OrderByDescending(a => a).First();
        }
    }
}
