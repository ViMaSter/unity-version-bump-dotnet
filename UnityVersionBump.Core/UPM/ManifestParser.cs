using Newtonsoft.Json;
using UnityVersionBump.Core.UPM.Models;

namespace UnityVersionBump.Core.UPM
{
    namespace Models
    {
        public class Manifest
        {
            public Dictionary<string, string> dependencies { get; set; }
        }
    }
    public static class ManifestParser
    {
        public static Dictionary<string, PackageVersion> Parse(string contents)
        {
            return JsonConvert.DeserializeObject<Manifest>(contents)!.dependencies.ToDictionary(entry => entry.Key, entry => new PackageVersion(entry.Value));
        }
        public static string Generate(Dictionary<string, PackageVersion> dependencies, IEnumerable<ScopedRegistries> scopedRegistries, IEnumerable<string> testables)
        {
            return JsonConvert.SerializeObject(new
            {
                dependencies,
                scopedRegistries,
                testables,
            });
        }
    }

    public class ScopedRegistries
    {
        public string name { get; set; }
        public string url { get; set; }
        public string[] scopes { get; set; }
    }
}
