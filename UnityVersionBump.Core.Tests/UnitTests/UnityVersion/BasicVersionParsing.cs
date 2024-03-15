using System;
using NUnit.Framework;

namespace UnityVersionBump.Core.Tests.UnitTests.UnityVersion
{
    public class BasicVersionParsing
    {
        private const string CORRECT_REVISION = "1234567890ab";
        [TestCase]
        public void PatchVersionIsParsedCorrectly()
        {
            var ltsVersion = new Core.UnityVersion("2022.2.0p1", CORRECT_REVISION, true);
            var nonLTSVersion = new Core.UnityVersion("2022.2.0p1", CORRECT_REVISION, false);

            Assert.That(ltsVersion.ReleaseStream, Is.EqualTo(Core.UnityVersion.ReleaseStreamType.Patch));
            Assert.That(ltsVersion.IsLTS, Is.True);
            Assert.That(ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Major), Is.EqualTo(2022));
            Assert.That(ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Minor), Is.EqualTo(2));
            Assert.That(ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Patch), Is.EqualTo(0));
            Assert.That(ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Build), Is.EqualTo(1));
            Assert.Throws<ArgumentException>(() => ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.ReleaseStream));

            Assert.That(nonLTSVersion.ReleaseStream, Is.EqualTo(Core.UnityVersion.ReleaseStreamType.Patch));
            Assert.That(nonLTSVersion.IsLTS, Is.False);
            Assert.That(nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Major), Is.EqualTo(2022));
            Assert.That(nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Minor), Is.EqualTo(2));
            Assert.That(nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Patch), Is.EqualTo(0));
            Assert.That(nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Build), Is.EqualTo(1));
            Assert.Throws<ArgumentException>(() => nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.ReleaseStream));
        }

        [TestCase]
        public void StableVersionWithoutSuffixIsParsedCorrectly()
        {
            var ltsVersion = new Core.UnityVersion("2022.2.0", CORRECT_REVISION, true);
            var nonLTSVersion = new Core.UnityVersion("2022.2.0", CORRECT_REVISION, false);

            Assert.That(ltsVersion.ReleaseStream, Is.EqualTo(Core.UnityVersion.ReleaseStreamType.Stable));
            Assert.That(ltsVersion.IsLTS, Is.True);
            Assert.That(ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Major), Is.EqualTo(2022));
            Assert.That(ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Minor), Is.EqualTo(2));
            Assert.That(ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Patch), Is.EqualTo(0));
            Assert.That(ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Build), Is.EqualTo(0));
            Assert.Throws<ArgumentException>(() => ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.ReleaseStream));

            Assert.That(nonLTSVersion.ReleaseStream, Is.EqualTo(Core.UnityVersion.ReleaseStreamType.Stable));
            Assert.That(nonLTSVersion.IsLTS, Is.False);
            Assert.That(nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Major), Is.EqualTo(2022));
            Assert.That(nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Minor), Is.EqualTo(2));
            Assert.That(nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Patch), Is.EqualTo(0));
            Assert.That(nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Build), Is.EqualTo(0));
            Assert.Throws<ArgumentException>(() => nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.ReleaseStream));
        }

        [TestCase]
        public void StableVersionWithSuffixIsParsedCorrectly()
        {
            var ltsVersion = new Core.UnityVersion("2022.2.0f1", CORRECT_REVISION, true);
            var nonLTSVersion = new Core.UnityVersion("2022.2.0f1", CORRECT_REVISION, false);

            Assert.That(ltsVersion.ReleaseStream, Is.EqualTo(Core.UnityVersion.ReleaseStreamType.Stable));
            Assert.That(ltsVersion.IsLTS, Is.True);
            Assert.That(ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Major), Is.EqualTo(2022));
            Assert.That(ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Minor), Is.EqualTo(2));
            Assert.That(ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Patch), Is.EqualTo(0));
            Assert.That(ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Build), Is.EqualTo(1));
            Assert.Throws<ArgumentException>(() => ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.ReleaseStream));

            Assert.That(nonLTSVersion.ReleaseStream, Is.EqualTo(Core.UnityVersion.ReleaseStreamType.Stable));
            Assert.That(nonLTSVersion.IsLTS, Is.False);
            Assert.That(nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Major), Is.EqualTo(2022));
            Assert.That(nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Minor), Is.EqualTo(2));
            Assert.That(nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Patch), Is.EqualTo(0));
            Assert.That(nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Build), Is.EqualTo(1));
            Assert.Throws<ArgumentException>(() => nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.ReleaseStream));
        }

        [TestCase]
        public void BetaVersionIsParsedCorrectly()
        {
            var ltsVersion = new Core.UnityVersion("2021.1.1b15", CORRECT_REVISION, true);
            var nonLTSVersion = new Core.UnityVersion("2021.1.1b15", CORRECT_REVISION, false);

            Assert.That(ltsVersion.ReleaseStream, Is.EqualTo(Core.UnityVersion.ReleaseStreamType.Beta));
            Assert.That(ltsVersion.IsLTS, Is.True);
            Assert.That(ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Major), Is.EqualTo(2021));
            Assert.That(ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Minor), Is.EqualTo(1));
            Assert.That(ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Patch), Is.EqualTo(1));
            Assert.That(ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Build), Is.EqualTo(15));
            Assert.Throws<ArgumentException>(() => ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.ReleaseStream));

            Assert.That(nonLTSVersion.ReleaseStream, Is.EqualTo(Core.UnityVersion.ReleaseStreamType.Beta));
            Assert.That(nonLTSVersion.IsLTS, Is.False);
            Assert.That(nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Major), Is.EqualTo(2021));
            Assert.That(nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Minor), Is.EqualTo(1));
            Assert.That(nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Patch), Is.EqualTo(1));
            Assert.That(nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Build), Is.EqualTo(15));
        }

        [TestCase]
        public void AlphaVersionIsParsedCorrectly()
        {
            var ltsVersion = new Core.UnityVersion("2021.1.0a15", CORRECT_REVISION, true);
            var nonLTSVersion = new Core.UnityVersion("2021.1.0a15", CORRECT_REVISION, false);

            Assert.That(ltsVersion.ReleaseStream, Is.EqualTo(Core.UnityVersion.ReleaseStreamType.Alpha));
            Assert.That(ltsVersion.IsLTS, Is.True);
            Assert.That(ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Major), Is.EqualTo(2021));
            Assert.That(ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Minor), Is.EqualTo(1));
            Assert.That(ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Patch), Is.EqualTo(0));
            Assert.That(ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.Build), Is.EqualTo(15));
            Assert.Throws<ArgumentException>(() => ltsVersion.GetVersionPart(Core.UnityVersion.VersionPart.ReleaseStream));

            Assert.That(nonLTSVersion.ReleaseStream, Is.EqualTo(Core.UnityVersion.ReleaseStreamType.Alpha));
            Assert.That(nonLTSVersion.IsLTS, Is.False);
            Assert.That(nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Major), Is.EqualTo(2021));
            Assert.That(nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Minor), Is.EqualTo(1));
            Assert.That(nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Patch), Is.EqualTo(0));
            Assert.That(nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.Build), Is.EqualTo(15));
            Assert.Throws<ArgumentException>(() => nonLTSVersion.GetVersionPart(Core.UnityVersion.VersionPart.ReleaseStream));
        }
    }
}