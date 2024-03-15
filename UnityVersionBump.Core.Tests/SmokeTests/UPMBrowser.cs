using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace UnityVersionBump.Core.Tests.SmokeTests
{
    [ExcludeFromCodeCoverage]
    class UPMBrowser
    {
        private class LoggerStub : ILogger
        {
            private readonly List<string> _loggedMessages = new (1);
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                _loggedMessages.Add(formatter.Invoke(state, exception));
            }

            public bool HasReceivedMessage(string content)
            {
                return _loggedMessages.Any(message => message.Contains(content));
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                throw new NotImplementedException();
            }
        }

        [TestCase]
        public async Task CanGetVersionFromOfficialPackageRepository()
        {
            const string PACKAGE_NAME = "com.unity.2d.ik";
            var browser = new UPM.Browser(new HttpClient(), null, UPM.Models.Manifest.UNITY_DEFAULT_PACKAGE_REPOSITORY_ROOT);

            var version = (await browser.GetLatestVersion(PACKAGE_NAME))!;
            Assert.That(version.GetVersionPart(PackageVersion.VersionPart.Major), Is.GreaterThan(0));

        }
        [TestCase]
        public async Task CanGetVersionFromOpenUPM()
        {
            const string PACKAGE_NAME = "com.inklestudios.ink-unity-integration";
            var browser = new UPM.Browser(new HttpClient(), null, "https://package.openupm.com");

            var version = (await browser.GetLatestVersion(PACKAGE_NAME))!;
            Assert.That(version.GetVersionPart(PackageVersion.VersionPart.Major), Is.GreaterThan(0));
        }
        [TestCase]
        public async Task ThrowsOn404FromOfficialPackageRepository()
        {
            const string PACKAGE_NAME = "com.inklestudios.ink-unity-integration";
            var logger = new LoggerStub();
            var browser = new UPM.Browser(new HttpClient(), logger, UPM.Models.Manifest.UNITY_DEFAULT_PACKAGE_REPOSITORY_ROOT);
            await browser.GetLatestVersion(PACKAGE_NAME);
            Assert.That(logger.HasReceivedMessage("Unexpected return code"), Is.True);
            Assert.That(logger.HasReceivedMessage("404"), Is.True);
        }
        [TestCase]
        public async Task ThrowsOn404FromOpenUPM()
        {
            const string PACKAGE_NAME = "com.unity.2d.ik";
            var logger = new LoggerStub();
            var browser = new UPM.Browser(new HttpClient(), logger, "https://package.openupm.com");
            await browser.GetLatestVersion(PACKAGE_NAME);
            Assert.That(logger.HasReceivedMessage("Unexpected return code"), Is.True);
            Assert.That(logger.HasReceivedMessage("404"), Is.True);
        }
    }
}
