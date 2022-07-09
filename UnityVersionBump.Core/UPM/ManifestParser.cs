using Newtonsoft.Json;
using UnityVersionBump.Core.UPM.Models;

namespace UnityVersionBump.Core.UPM
{
    namespace Models
    {
        public class Manifest
        {
            public const string UNITY_DEFAULT_PACKAGE_REPOSITORY_ROOT = "https://packages.unity.com";

            public Dictionary<string, PackageVersion> dependencies { get; set; }
            public List<ScopedRegistries> scopedRegistries { get; set; }
            public List<string> testables { get; set; }

            public string GetRegistryForPackage(string packageName)
            {
                var hasNonDefaultRegistry = scopedRegistries.FirstOrDefault(registry => registry.scopes.Contains(packageName));
                if (hasNonDefaultRegistry == null)
                {
                    return UNITY_DEFAULT_PACKAGE_REPOSITORY_ROOT;
                }

                return hasNonDefaultRegistry.url;
            }

            public static Manifest Parse(string contents)
            {
                return JsonConvert.DeserializeObject<Manifest>(contents);
            }

            public static string Generate(Manifest manifest)
            {
                return JsonConvert.SerializeObject(manifest, Formatting.Indented);
            }
        }
    }

    public class ScopedRegistries
    {
        public string name { get; set; }
        public string url { get; set; }
        public string[] scopes { get; set; }
    }
}
