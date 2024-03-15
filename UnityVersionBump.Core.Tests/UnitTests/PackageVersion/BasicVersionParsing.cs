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

            Assert.That(version.IsPreview, Is.False);
            Assert.That(version.GetVersionPart(Core.PackageVersion.VersionPart.Major), Is.EqualTo(1));
            Assert.That(version.GetVersionPart(Core.PackageVersion.VersionPart.Minor), Is.EqualTo(2));
            Assert.That(version.GetVersionPart(Core.PackageVersion.VersionPart.Patch), Is.EqualTo(3));
            Assert.Throws<KeyNotFoundException>(() => version.GetVersionPart(Core.PackageVersion.VersionPart.Suffix));
            Assert.Throws<KeyNotFoundException>(() => version.GetVersionPart(Core.PackageVersion.VersionPart.SuffixNumber));
        }

        [TestCase]
        public void StableVersionWithSuffixWithNumberIsParsedCorrectly()
        {
            var version = new Core.PackageVersion("1.2.3-preview.4");

            Assert.That(version.IsPreview, Is.True);
            Assert.That(version.GetVersionPart(Core.PackageVersion.VersionPart.Major), Is.EqualTo(1));
            Assert.That(version.GetVersionPart(Core.PackageVersion.VersionPart.Minor), Is.EqualTo(2));
            Assert.That(version.GetVersionPart(Core.PackageVersion.VersionPart.Patch), Is.EqualTo(3));
            Assert.That(version.GetVersionPart(Core.PackageVersion.VersionPart.SuffixNumber), Is.EqualTo(5));
            Assert.Throws<KeyNotFoundException>(() => version.GetVersionPart(Core.PackageVersion.VersionPart.Suffix));
            Assert.That(version._suffix, Is.EqualTo("preview.4"));
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