using System.Linq;
using NUnit.Framework;

namespace UnityVersionBump.Core.Tests.UnitTests.PackageVersion;

internal class Comparing
{
    private static readonly Core.PackageVersion[] OrderedReleases = {
        new("2.1.1-preview.2"),
        new("2.1.1-preview.1"),
        new("2.1.1"),
        new("2.1.0"),
        new("2.0.0"),
        new("1.0.0")
    };

    [TestCase]
    public void CanCompareReleases()
    {
        for (var i = 1; i < OrderedReleases.Length; i++)
        {
            var laterRelease = OrderedReleases[i - 1];
            var earlierRelease = OrderedReleases[i];
            Assert.That(earlierRelease, Is.LessThan(laterRelease));

            Assert.That(earlierRelease < laterRelease, Is.True);
            Assert.That(earlierRelease > laterRelease, Is.False);

            Assert.That(earlierRelease < null, Is.False);
            Assert.That(null < earlierRelease, Is.True);
            Assert.That(earlierRelease > null, Is.True);
            Assert.That(null > earlierRelease, Is.False);
        }
    }

    [TestCaseSource(nameof(OrderedReleases))]
    public void MatchingVersionsAreEqual(Core.PackageVersion packageVersion)
    {
        // ReSharper disable once EqualExpressionComparison (needed to hit coverage)
        Assert.That(packageVersion.Equals(packageVersion), Is.True);
        Assert.That(new Core.PackageVersion(packageVersion.ToString()), Is.EqualTo(packageVersion));

        Assert.That(packageVersion.GetHashCode(), Is.EqualTo(packageVersion.GetHashCode()));
        Assert.That(packageVersion.GetHashCode(), Is.EqualTo(new Core.PackageVersion(packageVersion.ToString()).GetHashCode()));
    }

    [TestCaseSource(nameof(OrderedReleases))]
    public void CanCompareAgainstNull(Core.PackageVersion packageVersion)
    {
        Assert.That(packageVersion.CompareTo(null), Is.EqualTo(1));
        Assert.That(packageVersion.CompareTo("not same type"), Is.EqualTo(1));
    }

    [TestCase]
    public void CanSortReleases()
    {
        Assert.That(OrderedReleases, Is.EqualTo(OrderedReleases.Reverse().OrderByDescending(version => version)));
    }

    [TestCaseSource(nameof(OrderedReleases))]
    public void SameInputCausesSameOutput(Core.PackageVersion projectVersion)
    {
        Assert.That(new Core.PackageVersion(projectVersion.ToString()), Is.EqualTo(new Core.PackageVersion(projectVersion.ToString())));
    }
}