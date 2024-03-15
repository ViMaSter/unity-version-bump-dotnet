using NUnit.Framework;

namespace UnityVersionBump.Core.Tests.UnitTests.ProjectVersion;

public class ToUnityStringWithRevision
{
    private static readonly object[] Versions = {
        new[]{"2022.2.0f1 (6cf78cb77498)"},
        new[]{"2021.2.0 (6cf78cb77498)"},
        new[]{"2021.1.1b15 (6cf78cb77498)"},
        new[]{"2021.1.0a15 (6cf78cb77498)"}
    };

    [TestCaseSource(nameof(Versions))]
    public void InputEqualsOutput(string unityVersionWithRevision)
    {
        Assert.That(Core.ProjectVersion.FromProjectVersionTXTSyntax(unityVersionWithRevision).ToUnityStringWithRevision(), Is.EqualTo(unityVersionWithRevision));
    }
}