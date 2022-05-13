using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using NUnit.Framework;
using UnityVersionBump.Core.Tests.Stubs;

namespace UnityVersionBump.Core.Tests.UnitTests.ProjectVersion
{
    internal class GetVersionsFromHub
    {
        private readonly HttpClient _stubHttpClient = new(new LocalFileMessageHandler("UnityVersionBump.Core.Tests.UnitTests.ProjectVersion.Resources.ExpectedHTTPResponse.json"));

        [TestCase]
        public void ThrowsWhenSpecifyingNoStream()
        {
            Assert.Throws<ArgumentException>(() => {
                Core.ProjectVersion.GetLatestFromHub(_stubHttpClient, Array.Empty<Core.UnityVersion.ReleaseStreamType>());
            });
        }

        private static Core.UnityVersion.ReleaseStreamType[] AllStreams => Enum.GetValues<Core.UnityVersion.ReleaseStreamType>();

        [TestCaseSource(nameof(AllStreams))]
        public void GetsVersionForSpecificStream(Core.UnityVersion.ReleaseStreamType stream)
        {
            if (stream == Core.UnityVersion.ReleaseStreamType.LTS)
            {
                Assert.IsTrue(Core.ProjectVersion.GetLatestFromHub(_stubHttpClient, new[]{ stream }).IsLTS);
                return;
            }
            Assert.AreEqual(stream, Core.ProjectVersion.GetLatestFromHub(_stubHttpClient, new[]{ stream }).ReleaseStream);
        }

        [TestCase]
        public void GetsVersionForAllStreams()
        {
            CollectionAssert.Contains(AllStreams, Core.ProjectVersion.GetLatestFromHub(_stubHttpClient, AllStreams).ReleaseStream);
        }

        [TestCase]
        public void SubsetOnlyContainsExpectedReleases()
        {
            var subset = new List<Core.UnityVersion.ReleaseStreamType>();
            foreach (var releaseStreamType in AllStreams)
            {
                if (!subset.Any())
                {
                    subset.Add(releaseStreamType);
                    continue;
                }
                CollectionAssert.Contains(subset, Core.ProjectVersion.GetLatestFromHub(_stubHttpClient, subset).ReleaseStream);
                subset.Add(releaseStreamType);
            }
        }


        [TestCase]
        public void ReturnsNullIfNoVersionExistsForReleaseStreams()
        {
            HttpClient clientWithoutPatchVersions = new(new LocalFileMessageHandler("UnityVersionBump.Core.Tests.UnitTests.ProjectVersion.Resources.NoPatchVersions.json"));

            Assert.IsNull(Core.ProjectVersion.GetLatestFromHub(clientWithoutPatchVersions, new List<Core.UnityVersion.ReleaseStreamType> { Core.UnityVersion.ReleaseStreamType.Patch }));
        }
    }
}
