using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace UnityVersionBump.Core.Tests.UnitTests.UnityVersion;

class ReleaseStreams
{
    [TestCase]
    public void CanCompareReleasesAcrossReleaseStreams()
    {
        var releases = new[] {
            "2022.2.2f2",
            "2021.2.2f2",
            "2021.1.2f2",
            "2021.1.1f2",
            "2021.1.1",
            "2021.1.1p2",
            "2021.1.1b2",
            "2021.1.1a2",
            "2021.1.1a1"
        }.Select(versionString => new Core.UnityVersion(versionString, false)).ToArray();

        for (var i = 1; i < releases.Length; i++)
        {
            var laterRelease = releases[i - 1];
            var earlierRelease = releases[i];
            Assert.Less(earlierRelease, laterRelease);
        }
    }

    private static readonly Dictionary<string, Core.UnityVersion[]> VersionsInsideReleaseStreams = new() {
        {
            "stable-lts",
            new Core.UnityVersion[] {
                new("2022.2.1f1", true),
                new("2021.2.1f1", true),
                new("2021.1.1f1", true),
                new("2021.1.0f1", true),
                new("2021.1.0f0", true)
            }
        }, {
            "stable-nonLTS",
            new Core.UnityVersion[] {
                new("2022.2.1", false),
                new("2021.2.1", false),
                new("2021.1.1", false),
                new("2021.1.0", false)
            }
        }, {
            "patch",
            new Core.UnityVersion[] {
                new("2022.2.1p1", false),
                new("2021.2.1p1", false),
                new("2021.1.1p1", false),
                new("2021.1.0p1", false),
                new("2021.1.0p0", false)
            }
        }, {
            "beta",
            new Core.UnityVersion[] {
                new("2022.2.1b1", false),
                new("2021.2.1b1", false),
                new("2021.1.1b1", false),
                new("2021.1.0b1", false),
                new("2021.1.0b0", false)
            }
        }, {
            "alpha",
            new Core.UnityVersion[] {
                new("2022.2.1a1", false),
                new("2021.2.1a1", false),
                new("2021.1.1a1", false),
                new("2021.1.0a1", false),
                new("2021.1.0a0", false)
            }
        }
    };

    public static object[][] VersionsInsideReleaseStreamsAsArguments => VersionsInsideReleaseStreams.Select(entry => new object[] { entry.Key, entry.Value }).ToArray();

    [TestCaseSource(nameof(VersionsInsideReleaseStreamsAsArguments))]
    public void CanCompareReleasesInsideSameReleaseStream(string releaseStream, Core.UnityVersion[] unityVersions)
    {
        for (var i = 1; i < unityVersions.Length; i++)
        {
            var laterRelease = unityVersions[i - 1];
            var earlierRelease = unityVersions[i];
            Assert.Less(earlierRelease, laterRelease);
        }
    }
}