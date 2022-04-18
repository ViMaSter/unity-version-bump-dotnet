using NUnit.Framework;

namespace UnityVersionBump.Core.Tests.UnitTests.UnityVersion;

public class ToUnityString
{
    private static readonly string[] Versions = {
        "2022.2.0f1",
        "2021.2.0",
        "2021.1.1b15",
        "2021.1.0a15"
    };

    [TestCaseSource(nameof(Versions))]
    public void ToUnityStringGeneratesUnityConformFormat(string unityVersion)
    {
        Assert.AreEqual(unityVersion, new Core.UnityVersion(unityVersion, "1234567890ab", false).ToUnityString());
    }
}