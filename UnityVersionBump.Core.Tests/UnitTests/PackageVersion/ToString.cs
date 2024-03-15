using NUnit.Framework;

namespace UnityVersionBump.Core.Tests.UnitTests.PackageVersion;

public class ToString
{
    private static readonly string[] Versions = {
        "1.2.3",
        "1.2.3-preview.4"
    };

    [TestCaseSource(nameof(Versions))]
    public void ToStringGeneratesSemanticVersionFormat(string packageVersion)
    {
        Assert.That(new Core.PackageVersion(packageVersion).ToString(), Is.EqualTo(packageVersion));
    }

    private static readonly object[][] VersionsWithPrefix = {
        new object[]{"01.2.3", "1.2.3"},
        new object[]{"1.02.3", "1.2.3"},
        new object[]{"1.2.03", "1.2.3"},
        new object[]{"01.2.3-preview.4", "1.2.3-preview.4"},
        new object[]{"1.02.3-preview.4", "1.2.3-preview.4"},
        new object[]{"1.2.03-preview.4", "1.2.3-preview.4"},
        new object[]{"1.2.3-preview.04", "1.2.3-preview.04"}
    };

    [TestCaseSource(nameof(VersionsWithPrefix))]
    public void ToStringGeneratesSemanticVersionFormat(string versionWithLeftPadding, string versionWithoutLeftPadding)
    {
        Assert.That(new Core.PackageVersion(versionWithLeftPadding).ToString(), Is.EqualTo(versionWithoutLeftPadding));
    }
}