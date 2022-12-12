using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityVersionBump.Core.Tests.Stubs;

namespace UnityVersionBump.Core.Tests.UnitTests.ProjectVersion
{
    internal class GetVersionsFromHub
    {
        private readonly HttpClient _stubHttpClient = new(new LocalFileMessageHandler("UnitTests.ProjectVersion.Resources.ExpectedHTTPResponse"));

        [TestCase]
        public void ThrowsWhenSpecifyingNoStream()
        {
            Assert.ThrowsAsync<ArgumentException>(async () => {
                await Core.ProjectVersion.GetLatestFromHub(_stubHttpClient, Array.Empty<Core.UnityVersion.ReleaseStreamType>());
            });
        }
        [TestCase]
        public void ThrowsWhenUnityHubReturnsUnexpectedStatusCode()
        {
            var stubHttpClient = new HttpClient(new LocalFileMessageHandler("UnitTests.ProjectVersion.Resources.InternalServerError"));
            Assert.ThrowsAsync<InvalidDataException>(async () => {
                await Core.ProjectVersion.GetLatestFromHub(stubHttpClient, AllStreams);
            });
        }
        [TestCase]
        public void ThrowsWhenUnityHubReturnsUnexpectedData()
        {
            var stubHttpClient = new HttpClient(new LocalFileMessageHandler("UnitTests.ProjectVersion.Resources.UnexpectedHTTPResponse"));
            Assert.ThrowsAsync<NotSupportedException>(async () => {
                await Core.ProjectVersion.GetLatestFromHub(stubHttpClient, AllStreams);
            });
        }

        private static Core.UnityVersion.ReleaseStreamType[] AllStreams => Enum.GetValues<Core.UnityVersion.ReleaseStreamType>();

        [TestCaseSource(nameof(AllStreams))]
        public async Task GetsVersionForSpecificStream(Core.UnityVersion.ReleaseStreamType stream)
        {
            if (stream == Core.UnityVersion.ReleaseStreamType.LTS)
            {
                Assert.IsTrue((await Core.ProjectVersion.GetLatestFromHub(_stubHttpClient, new[]{ stream })).IsLTS);
                return;
            }
            Assert.AreEqual(stream, (await Core.ProjectVersion.GetLatestFromHub(_stubHttpClient, new[] { stream })).ReleaseStream);
        }

        [TestCase]
        public async Task GetsVersionForAllStreams()
        {
            CollectionAssert.Contains(AllStreams, (await Core.ProjectVersion.GetLatestFromHub(_stubHttpClient, AllStreams)).ReleaseStream);
        }

        [TestCase]
        public async Task SubsetOnlyContainsExpectedReleases()
        {
            var subset = new List<Core.UnityVersion.ReleaseStreamType>();
            foreach (var releaseStreamType in AllStreams)
            {
                if (!subset.Any())
                {
                    subset.Add(releaseStreamType);
                    continue;
                }
                CollectionAssert.Contains(subset, (await Core.ProjectVersion.GetLatestFromHub(_stubHttpClient, subset)).ReleaseStream);
                subset.Add(releaseStreamType);
            }
        }


        [TestCase]
        public void ReturnsNullIfNoVersionExistsForReleaseStreams()
        {
            HttpClient clientWithoutPatchVersions = new(new LocalFileMessageHandler("UnitTests.ProjectVersion.Resources.NoPatchVersions"));

            Assert.ThrowsAsync<FileNotFoundException>(async () =>
            {
                await Core.ProjectVersion.GetLatestFromHub(clientWithoutPatchVersions, new List<Core.UnityVersion.ReleaseStreamType> { Core.UnityVersion.ReleaseStreamType.Patch });
            });
        }
    }
}
