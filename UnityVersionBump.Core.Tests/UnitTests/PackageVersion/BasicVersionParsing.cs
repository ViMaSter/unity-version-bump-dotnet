using System.Collections.Generic;
using NUnit.Framework;
using UnityVersionBump.Core.Exceptions;

namespace UnityVersionBump.Core.Tests.UnitTests.PackageVersion
{
    public class BasicVersionParsing
    {
        [TestCase]
        public void StableVersionWithoutSuffixIsParsedCorrectly()
        {
            var version = new Core.PackageVersion("1.2.3");

            Assert.IsFalse(version.IsPreview);
            Assert.AreEqual(1, version.GetVersionPart(Core.PackageVersion.VersionPart.Major));
            Assert.AreEqual(2, version.GetVersionPart(Core.PackageVersion.VersionPart.Minor));
            Assert.AreEqual(3, version.GetVersionPart(Core.PackageVersion.VersionPart.Patch));
            Assert.Throws<KeyNotFoundException>(() => version.GetVersionPart(Core.PackageVersion.VersionPart.Suffix));
            Assert.Throws<KeyNotFoundException>(() => version.GetVersionPart(Core.PackageVersion.VersionPart.SuffixNumber));
        }

        [TestCase]
        public void StableVersionWithSuffixWithNumberIsParsedCorrectly()
        {
            var version = new Core.PackageVersion("1.2.3-preview.4");

            Assert.IsTrue(version.IsPreview);
            Assert.AreEqual(1, version.GetVersionPart(Core.PackageVersion.VersionPart.Major));
            Assert.AreEqual(2, version.GetVersionPart(Core.PackageVersion.VersionPart.Minor));
            Assert.AreEqual(3, version.GetVersionPart(Core.PackageVersion.VersionPart.Patch));
            Assert.AreEqual(5, version.GetVersionPart(Core.PackageVersion.VersionPart.SuffixNumber));
            Assert.Throws<KeyNotFoundException>(() => version.GetVersionPart(Core.PackageVersion.VersionPart.Suffix));
            Assert.AreEqual("preview.4", version._suffix);
        }

        [TestCase]
        public void StableVersionWithoutMajorThrows()
        {
            Assert.Throws<InvalidVersionSyntaxException>(() =>
            {
                _ = new Core.PackageVersion(".0.0");
            });
            Assert.Throws<InvalidVersionSyntaxException>(() =>
            {
                _ = new Core.PackageVersion("0.0");
            });
        }

        [TestCase]
        public void StableVersionWithoutMinorThrows()
        {
            Assert.Throws<InvalidVersionSyntaxException>(() =>
            {
                _ = new Core.PackageVersion("1..0");
            });

            Assert.Throws<InvalidVersionSyntaxException>(() =>
            {
                _ = new Core.PackageVersion("1.0");
            });
        }

        [TestCase]
        public void StableVersionWithoutPatchThrows()
        {
            Assert.Throws<InvalidVersionSyntaxException>(() =>
            {
                _ = new Core.PackageVersion("1.0.");
            });
        }
    }
}