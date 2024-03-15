using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace UnityVersionBump.Core.Tests.SmokeTests
{
    public class SymbolServerSmokeTest
    {
        private static IEnumerable<string> UnityVersionsFromSymbolServer
        {
            get
            {
                var rawSymbolServerInfo = new HttpClient().GetAsync("http://symbolserver.unity3d.com/000Admin/history.txt").ConfigureAwait(false).GetAwaiter().GetResult();
                var unityLine = new Regex(@"Unity"",""(.+)"",""");
                var allUnityVersions = unityLine.Matches(rawSymbolServerInfo.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult());
                return allUnityVersions.Select(match => match.Groups[1].Value).Distinct();
            }
        }

        [TestCaseSource(nameof(UnityVersionsFromSymbolServer))]
        public void VerifyAllVersionsOfUnityAreParsable(string unityVersion)
        {
            Assert.That(new UnityVersion(unityVersion, "1234567890ab", false).GetComparable(), Is.GreaterThan(500000000));
        }
    }
}