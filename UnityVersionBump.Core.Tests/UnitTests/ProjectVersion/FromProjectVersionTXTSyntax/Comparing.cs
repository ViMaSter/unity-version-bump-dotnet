using System.Linq;
using NUnit.Framework;

namespace UnityVersionBump.Core.Tests.UnitTests.ProjectVersion.FromProjectVersionTXTSyntax
{
    internal class Comparing
    {
        private static readonly Core.UnityVersion[] OrderedReleases = {
            Core.ProjectVersion.FromProjectVersionTXTSyntax("2022.2.12f1 (1234567890ab)"),
            Core.ProjectVersion.FromProjectVersionTXTSyntax("2022.2.11 (1234567890ab)"),
            Core.ProjectVersion.FromProjectVersionTXTSyntax("2021.1.10p1 (1234567890ab)"),
            Core.ProjectVersion.FromProjectVersionTXTSyntax("2021.1.9b1 (1234567890ab)"),
            Core.ProjectVersion.FromProjectVersionTXTSyntax("2021.1.1a1 (1234567890ab)")
        };

        [TestCase]
        public void CanCompareReleases()
        {
            for (var i = 1; i < OrderedReleases.Length; i++)
            {
                var laterRelease = OrderedReleases[i - 1];
                var earlierRelease = OrderedReleases[i];
                Assert.Less(earlierRelease, laterRelease);
            }
        }

        [TestCaseSource(nameof(OrderedReleases))]
        public void MatchingVersionsAreEqual(Core.UnityVersion unityVersion)
        {
            // ReSharper disable once EqualExpressionComparison (needed to hit coverage)
            Assert.IsTrue(unityVersion.Equals(unityVersion));
            Assert.AreEqual(unityVersion, new Core.UnityVersion(unityVersion.ToUnityString(), unityVersion.Revision, false));

            Assert.AreEqual(unityVersion.GetHashCode(), unityVersion.GetHashCode());
            Assert.AreEqual(unityVersion.GetHashCode(), new Core.UnityVersion(unityVersion.ToUnityString(), unityVersion.Revision, false).GetHashCode());
        }

        [TestCaseSource(nameof(OrderedReleases))]
        public void CanCompareAgainstNull(Core.UnityVersion unityVersion)
        {
            Assert.AreEqual(1, unityVersion.CompareTo(null));
            Assert.AreEqual(1, unityVersion.CompareTo("not same type"));
        }

        [TestCase]
        public void CanSortReleases()
        {
            CollectionAssert.AreEqual(OrderedReleases, OrderedReleases.Reverse().OrderByDescending(version => version));
        }

        [TestCaseSource(nameof(OrderedReleases))]
        public void SameInputCausesSameOutput(Core.UnityVersion unityVersion)
        {
            Assert.AreEqual(new Core.UnityVersion(unityVersion.ToUnityString(), unityVersion.Revision, false), new Core.UnityVersion(unityVersion.ToUnityString(), unityVersion.Revision, false));
        }
    }
}
