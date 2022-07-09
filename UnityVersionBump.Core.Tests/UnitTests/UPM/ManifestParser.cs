using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityVersionBump.Core.UPM;
using UnityVersionBump.Core.UPM.Models;

namespace UnityVersionBump.Core.Tests.UnitTests.UPM
{
    internal class ManifestParser
    {
        public readonly Dictionary<string, Core.PackageVersion> versionByPackage = new()
        {
            { "com.inklestudios.ink-unity-integration", new ("1.0.2") },
            { "com.unity.2d.animation", new ("5.0.6") },
            { "com.unity.2d.pixel-perfect", new("4.0.1") },
            { "com.unity.2d.psdimporter", new("4.1.0") },
            { "com.unity.2d.sprite", new("1.0.0") },
            { "com.unity.2d.spriteshape", new("5.1.3") },
            { "com.unity.2d.tilemap", new("1.0.0") },
            { "com.unity.collab-proxy", new("1.7.1") },
            { "com.unity.ide.rider", new("2.0.7") },
            { "com.unity.ide.visualstudio", new("2.0.9") },
            { "com.unity.ide.vscode", new("1.2.3") },
            { "com.unity.inputsystem", new("1.0.2") },
            { "com.unity.test-framework", new("1.1.27") },
            { "com.unity.testtools.codecoverage", new("1.1.1") },
            { "com.unity.textmeshpro", new("3.0.6") },
            { "com.unity.timeline", new("1.4.8") },
            { "com.unity.ugui", new("1.0.0") },
            { "com.unity.modules.ai", new("1.0.0") },
            { "com.unity.modules.androidjni", new("1.0.0") },
            { "com.unity.modules.animation", new("1.0.0") },
            { "com.unity.modules.assetbundle", new("1.0.0") },
            { "com.unity.modules.audio", new("1.0.0") },
            { "com.unity.modules.cloth", new("1.0.0") },
            { "com.unity.modules.director", new("1.0.0") },
            { "com.unity.modules.imageconversion", new("1.0.0-preview.1") },
            { "com.unity.modules.imgui", new("1.0.0") },
            { "com.unity.modules.jsonserialize", new("1.0.0") },
            { "com.unity.modules.particlesystem", new("1.0.0") },
            { "com.unity.modules.physics", new("1.0.0") },
            { "com.unity.modules.physics2d", new("1.0.0") },
            { "com.unity.modules.screencapture", new("1.0.0") },
            { "com.unity.modules.terrain", new("1.0.0") },
            { "com.unity.modules.terrainphysics", new("1.0.0") },
            { "com.unity.modules.tilemap", new("1.0.0") },
            { "com.unity.modules.ui", new("1.0.0") },
            { "com.unity.modules.uielements", new("1.0.0") },
            { "com.unity.modules.umbra", new("1.0.0") },
            { "com.unity.modules.unityanalytics", new("1.0.0") },
            { "com.unity.modules.unitywebrequest", new("1.0.0") },
            { "com.unity.modules.unitywebrequestassetbundle", new("1.0.0") },
            { "com.unity.modules.unitywebrequestaudio", new("1.0.0") },
            { "com.unity.modules.unitywebrequesttexture", new("1.0.0") },
            { "com.unity.modules.unitywebrequestwww", new("1.0.0") },
            { "com.unity.modules.vehicles", new("1.0.0") },
            { "com.unity.modules.video", new("1.0.0") },
            { "com.unity.modules.vr", new("1.0.0") },
            { "com.unity.modules.wind", new("1.0.0") },
            { "com.unity.modules.xr", new("1.0.0") },
        };

        [TestCase]
        public void ParsesPackageVersions()
        {
            var manifestContentStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("UnityVersionBump.Core.Tests.UnitTests.UPM.Resources.GG-JointJustice-manifest.json");
            using var streamReader = new StreamReader(manifestContentStream);
            var manifestContent = streamReader.ReadToEnd();
            var actualOutput = UnityVersionBump.Core.UPM.ManifestParser.Parse(manifestContent).dependencies;
            Assert.AreEqual(versionByPackage, actualOutput);
        }

        [TestCase]
        public void GeneratesOutput()
        {
            var manifestContentStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("UnityVersionBump.Core.Tests.UnitTests.UPM.Resources.GG-JointJustice-manifest.json");
            using var streamReader = new StreamReader(manifestContentStream);
            var manifestContent = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(streamReader.ReadToEnd()));
            var actualOutput = UnityVersionBump.Core.UPM.ManifestParser.Generate(new Manifest()
            {
                dependencies = versionByPackage,
                scopedRegistries = new List<ScopedRegistries>()
                {
                    new()
                    {
                        name = "OpenUPM",
                        scopes = new[] { "com.inklestudios.ink-unity-integration" },
                        url = "https://package.openupm.com"
                    }
                },
                testables = new List<string>() { "com.unity.inputsystem" }
            });
            Assert.AreEqual(manifestContent, actualOutput);
        }

        [TestCase]
        public void IsDeterministicStartingFromJSON()
        {
            var manifestContentStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("UnityVersionBump.Core.Tests.UnitTests.UPM.Resources.GG-JointJustice-manifest.json");
            using var streamReader = new StreamReader(manifestContentStream);
            var manifestContent = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(streamReader.ReadToEnd()))!;
            Assert.AreEqual(manifestContent, UnityVersionBump.Core.UPM.ManifestParser.Generate(UnityVersionBump.Core.UPM.ManifestParser.Parse(manifestContent)));
        }

        [TestCase]
        public void IsDeterministicStartingFromDependenciesObject()
        {
            Assert.AreEqual(versionByPackage, UnityVersionBump.Core.UPM.ManifestParser.Parse(UnityVersionBump.Core.UPM.ManifestParser.Generate(new Manifest() { dependencies = versionByPackage })).dependencies);
        }

        [TestCase]
        public void GetsCorrespondingRegistry()
        {
            var manifestContentStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("UnityVersionBump.Core.Tests.UnitTests.UPM.Resources.GG-JointJustice-manifest.json");
            using var streamReader = new StreamReader(manifestContentStream);
            var manifestContent = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(streamReader.ReadToEnd()))!;
            var manifest = UnityVersionBump.Core.UPM.ManifestParser.Parse(manifestContent);

            Assert.AreEqual(Manifest.UNITY_DEFAULT_PACKAGE_REPOSITORY_ROOT, manifest.GetRegistryForPackage("com.unity.2d.tilemap"));
            Assert.AreEqual("https://package.openupm.com", manifest.GetRegistryForPackage("com.inklestudios.ink-unity-integration"));
        }
    }
}
