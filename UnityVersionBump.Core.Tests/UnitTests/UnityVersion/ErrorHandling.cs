﻿using System;
using NUnit.Framework;
using UnityVersionBump.Core.Exceptions;

namespace UnityVersionBump.Core.Tests.UnitTests.UnityVersion;

public class ErrorHandling
{
    private static object[] _tooLongVersionPart = {
        new object[]{"20211.1.0a12",  Core.UnityVersion.VersionPart.Major},
        new object[]{"2021.123.0a12", Core.UnityVersion.VersionPart.Minor},
        new object[]{"2021.1.012a12", Core.UnityVersion.VersionPart.Patch},
        new object[]{"2021.1.0a1234", Core.UnityVersion.VersionPart.Build}
    };

    [TestCaseSource(nameof(_tooLongVersionPart))]
    public void ThrowsIfVersionIsTooLong(string invalidUnityVersion, Core.UnityVersion.VersionPart failingPart)
    {
        var exception = Assert.Throws<MismatchingLengthException>(() => {
            _ = new Core.UnityVersion(invalidUnityVersion, false);
        });
        Assert.NotNull(exception);
        StringAssert.Contains(failingPart.ToString(), exception!.Message);
    }

    private static object[] _invalidSyntax = {
        new object[]{".1.1a1" },
        new object[]{"2021..1a1" },
        new object[]{"2021.1." },
        new object[]{"..1a1" },
        new object[]{"2021.." },
        new object[]{"20211.1a1" },
        new object[]{"2021.11a1" },
        new object[]{"202111a1" },
        new object[]{"a" },
        new object[]{"1" },
        new object[]{"" },
    };
    [TestCaseSource(nameof(_invalidSyntax))]
    public void ThrowsIfInvalidVersionFormat(string invalidFormat)
    {
        var exception = Assert.Throws<InvalidSyntaxException>(() => {
            _ = new Core.UnityVersion(invalidFormat, false);
        });
        Assert.NotNull(exception);
        StringAssert.Contains(invalidFormat, exception!.Message);
    }

    private static object[] _unsupportedChannel = {
        new object[]{'c' },
        new object[]{'d' },
        new object[]{'e' },
        new object[]{'g' },
        new object[]{'h' },
        new object[]{'i' },
        new object[]{'j' },
        new object[]{'k' },
        new object[]{'l' },
        new object[]{'m' },
        new object[]{'n' },
        new object[]{'o' },
        new object[]{'q' },
        new object[]{'r' },
        new object[]{'s' },
        new object[]{'t' },
        new object[]{'u' },
        new object[]{'v' },
        new object[]{'w' },
        new object[]{'x' },
        new object[]{'y' },
        new object[]{'z' },
        new object[]{'_' },
    };
    [TestCaseSource(nameof(_unsupportedChannel))]
    public void ThrowsIfUnsupportedChannel(char shorthand)
    {
        var exception = Assert.Throws<UnsupportedReleaseStream>(() => {
            _ = new Core.UnityVersion($"2021.1.0{shorthand}12", false);
        });
        Assert.NotNull(exception);
        StringAssert.Contains($"'{shorthand}'", exception!.Message);
    }
}