using System.Linq;
using NUnit.Framework;

namespace UnityVersionBump.Core.Tests.UnitTests.UnityVersion;

internal class Comparing
{
    private static readonly Core.UnityVersion[] OrderedReleases = {
        new("2022.2.12f1", "1234567890ab", false),
        new("2022.2.11", "1234567890ab", false),
        new("2021.1.10p1", "1234567890ab", false),
        new("2021.1.9b1", "1234567890ab", false),
        new("2021.1.1a1", "1234567890ab", false)
    };

    [TestCase]
    public void CanCompareReleases()
    {
        for (var i = 1; i < OrderedReleases.Length; i++)
        {
            var laterRelease = OrderedReleases[i - 1];
            var earlierRelease = OrderedReleases[i];
            Assert.That(earlierRelease, Is.LessThan(laterRelease));
        }
    }

    [TestCaseSource(nameof(OrderedReleases))]
    public void MatchingVersionsAreEqual(Core.UnityVersion unityVersion)
    {
        // ReSharper disable once EqualExpressionComparison (needed to hit coverage)
        Assert.That(unityVersion.Equals(unityVersion), Is.True);
        Assert.That(new Core.UnityVersion(unityVersion.ToUnityString(), unityVersion.Revision, false), Is.EqualTo(unityVersion));

        Assert.That(unityVersion.GetHashCode(), Is.EqualTo(unityVersion.GetHashCode()));
        Assert.That(unityVersion.GetHashCode(), Is.EqualTo(new Core.UnityVersion(unityVersion.ToUnityString(), unityVersion.Revision, false).GetHashCode()));
    }

    [TestCase]
    public void CanSortReleases()
    {
        Assert.That(OrderedReleases, Is.EqualTo(OrderedReleases.Reverse().OrderByDescending(version => version)));
    }

    [TestCaseSource(nameof(OrderedReleases))]
    public void SameInputCausesSameOutput(Core.UnityVersion unityVersion)
    {
        Assert.That(new Core.UnityVersion(unityVersion.ToUnityString(), unityVersion.Revision, false), Is.EqualTo(new Core.UnityVersion(unityVersion.ToUnityString(), unityVersion.Revision, false)));
    }
}