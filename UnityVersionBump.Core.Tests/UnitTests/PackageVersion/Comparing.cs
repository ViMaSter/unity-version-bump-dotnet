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
            Assert.Less(earlierRelease, laterRelease);

            Assert.IsTrue(earlierRelease < laterRelease);
            Assert.IsFalse(earlierRelease > laterRelease);

            Assert.IsFalse(earlierRelease < null);
            Assert.IsTrue(null < earlierRelease);
            Assert.IsTrue(earlierRelease > null);
            Assert.IsFalse(null > earlierRelease);
        }
    }

    [TestCaseSource(nameof(OrderedReleases))]
    public void MatchingVersionsAreEqual(Core.PackageVersion packageVersion)
    {
        // ReSharper disable once EqualExpressionComparison (needed to hit coverage)
        Assert.IsTrue(packageVersion.Equals(packageVersion));
        Assert.AreEqual(packageVersion, new Core.PackageVersion(packageVersion.ToString()));

        Assert.AreEqual(packageVersion.GetHashCode(), packageVersion.GetHashCode());
        Assert.AreEqual(packageVersion.GetHashCode(), new Core.PackageVersion(packageVersion.ToString()).GetHashCode());
    }

    [TestCaseSource(nameof(OrderedReleases))]
    public void CanCompareAgainstNull(Core.PackageVersion packageVersion)
    {
        Assert.AreEqual(1, packageVersion.CompareTo(null));
        Assert.AreEqual(1, packageVersion.CompareTo("not same type"));
    }

    [TestCase]
    public void CanSortReleases()
    {
        CollectionAssert.AreEqual(OrderedReleases, OrderedReleases.Reverse().OrderByDescending(version => version));
    }

    [TestCaseSource(nameof(OrderedReleases))]
    public void SameInputCausesSameOutput(Core.PackageVersion projectVersion)
    {
        Assert.AreEqual(new Core.PackageVersion(projectVersion.ToString()), new Core.PackageVersion(projectVersion.ToString()));
    }
}