using Newtonsoft.Json;
using UnityVersionBump.Core.UPM.Models;

namespace UnityVersionBump.Core.UPM
{
    namespace Models
    {
        public class Manifest
        {
            public Dictionary<string, Core.PackageVersion> dependencies { get; set; }
            public List<ScopedRegistries> scopedRegistries { get; set; }
            public List<string> testables { get; set; }
        }
    }
    public static class ManifestParser
    {
        public static Manifest Parse(string contents)
        {
            return JsonConvert.DeserializeObject<Manifest>(contents);
        }
        public static string Generate(Manifest manifest)
        {
            return JsonConvert.SerializeObject(manifest);
        }
    }

    public class ScopedRegistries
    {
        public string name { get; set; }
        public string url { get; set; }
        public string[] scopes { get; set; }
    }
}
