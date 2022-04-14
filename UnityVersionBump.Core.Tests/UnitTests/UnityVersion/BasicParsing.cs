using NUnit.Framework;

namespace UnityVersionBump.Core.Tests.UnitTests.UnityVersion
{
    public class BasicParsing
    {
        [TestCase]
        public void PatchVersionIsParsedCorrectly()
        {
            var ltsVersion = new Core.UnityVersion("2022.2.0p1", true);
            var nonLTSVersion = new Core.UnityVersion("2022.2.0p1", false);

            Assert.AreEqual(Core.UnityVersion.ReleaseStreamType.Patch, ltsVersion.ReleaseStream);
            Assert.IsTrue(ltsVersion.IsLTS);
            Assert.AreEqual(2022, ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Major));
            Assert.AreEqual(2, ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Minor));
            Assert.AreEqual(0, ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Patch));
            Assert.AreEqual(1, ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Build));

            Assert.AreEqual(Core.UnityVersion.ReleaseStreamType.Patch, nonLTSVersion.ReleaseStream);
            Assert.IsFalse(nonLTSVersion.IsLTS);
            Assert.AreEqual(2022, nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Major));
            Assert.AreEqual(2, nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Minor));
            Assert.AreEqual(0, nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Patch));
            Assert.AreEqual(1, nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Build));
        }

        [TestCase]
        public void StableVersionWithoutSuffixIsParsedCorrectly()
        {
            var ltsVersion = new Core.UnityVersion("2022.2.0", true);
            var nonLTSVersion = new Core.UnityVersion("2022.2.0", false);

            Assert.AreEqual(Core.UnityVersion.ReleaseStreamType.Stable, ltsVersion.ReleaseStream);
            Assert.IsTrue(ltsVersion.IsLTS);
            Assert.AreEqual(2022, ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Major));
            Assert.AreEqual(2, ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Minor));
            Assert.AreEqual(0, ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Patch));
            Assert.AreEqual(0, ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Build));

            Assert.AreEqual(Core.UnityVersion.ReleaseStreamType.Stable, nonLTSVersion.ReleaseStream);
            Assert.IsFalse(nonLTSVersion.IsLTS);
            Assert.AreEqual(2022, nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Major));
            Assert.AreEqual(2, nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Minor));
            Assert.AreEqual(0, nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Patch));
            Assert.AreEqual(0, nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Build));
        }

        [TestCase]
        public void StableVersionWithSuffixIsParsedCorrectly()
        {
            var ltsVersion = new Core.UnityVersion("2022.2.0f1", true);
            var nonLTSVersion = new Core.UnityVersion("2022.2.0f1", false);

            Assert.AreEqual(Core.UnityVersion.ReleaseStreamType.Stable, ltsVersion.ReleaseStream);
            Assert.IsTrue(ltsVersion.IsLTS);
            Assert.AreEqual(2022, ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Major));
            Assert.AreEqual(2, ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Minor));
            Assert.AreEqual(0, ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Patch));
            Assert.AreEqual(1, ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Build));

            Assert.AreEqual(Core.UnityVersion.ReleaseStreamType.Stable, nonLTSVersion.ReleaseStream);
            Assert.IsFalse(nonLTSVersion.IsLTS);
            Assert.AreEqual(2022, nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Major));
            Assert.AreEqual(2, nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Minor));
            Assert.AreEqual(0, nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Patch));
            Assert.AreEqual(1, nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Build));
        }

        [TestCase]
        public void BetaVersionIsParsedCorrectly()
        {
            var ltsVersion = new Core.UnityVersion("2021.1.1b15", true);
            var nonLTSVersion = new Core.UnityVersion("2021.1.1b15", false);

            Assert.AreEqual(Core.UnityVersion.ReleaseStreamType.Beta, ltsVersion.ReleaseStream);
            Assert.IsTrue(ltsVersion.IsLTS);
            Assert.AreEqual(2021, ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Major));
            Assert.AreEqual(1, ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Minor));
            Assert.AreEqual(1, ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Patch));
            Assert.AreEqual(15, ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Build));

            Assert.AreEqual(Core.UnityVersion.ReleaseStreamType.Beta, nonLTSVersion.ReleaseStream);
            Assert.IsFalse(nonLTSVersion.IsLTS);
            Assert.AreEqual(2021, nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Major));
            Assert.AreEqual(1, nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Minor));
            Assert.AreEqual(1, nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Patch));
            Assert.AreEqual(15, nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Build));
        }

        [TestCase]
        public void AlphaVersionIsParsedCorrectly()
        {
            var ltsVersion = new Core.UnityVersion("2021.1.0a15", true);
            var nonLTSVersion = new Core.UnityVersion("2021.1.0a15", false);

            Assert.AreEqual(Core.UnityVersion.ReleaseStreamType.Alpha, ltsVersion.ReleaseStream);
            Assert.IsTrue(ltsVersion.IsLTS);
            Assert.AreEqual(2021, ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Major));
            Assert.AreEqual(1, ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Minor));
            Assert.AreEqual(0, ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Patch));
            Assert.AreEqual(15, ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Build));

            Assert.AreEqual(Core.UnityVersion.ReleaseStreamType.Alpha, nonLTSVersion.ReleaseStream);
            Assert.IsFalse(nonLTSVersion.IsLTS);
            Assert.AreEqual(2021, nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Major));
            Assert.AreEqual(1, nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Minor));
            Assert.AreEqual(0, nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Patch));
            Assert.AreEqual(15, nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Build));
        }
    }
}