using NUnit.Framework;
using UnityVersionBump.Core.Exceptions;

namespace UnityVersionBump.Core.Tests.UnitTests.PackageVersion;

public class ErrorHandling
{
    private static object[] _invalidSyntax = {
        new object[]{"1.2." },
        new object[]{"1..3" },
        new object[]{".2.3" },
        new object[]{"1.." },
        new object[]{".2." },
        new object[]{"..3" },
        new object[]{"a" },
        new object[]{"1" },
        new object[]{"" }
    };
    [TestCaseSource(nameof(_invalidSyntax))]
    public void ThrowsIfInvalidVersionFormat(string invalidFormat)
    {
        var exception = Assert.Throws<InvalidVersionSyntaxException>(() =>
        {
            _ = new Core.PackageVersion(invalidFormat);
        });
        Assert.NotNull(exception);
        StringAssert.Contains(invalidFormat, exception!.Message);
    }
}