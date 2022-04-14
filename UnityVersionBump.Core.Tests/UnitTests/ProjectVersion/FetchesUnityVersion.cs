using System.IO;
using NUnit.Framework;

namespace UnityVersionBump.Core.Tests.UnitTests.ProjectVersion
{
    class FetchesUnityVersion
    {
        [TestCase]
        public void ReturnsVersionAsString()
        {
            var projectVersionTxt = new StreamReader(GetType().Assembly.GetManifestResourceStream("UnityVersionBump.Core.Tests.UnitTests.ProjectVersion.Resources.ProjectVersion.txt")!).ReadToEnd();
            var unityVersion = Core.ProjectVersion.DetermineUnityVersion(projectVersionTxt, false);
            Assert.AreEqual("2020.3.15f2", unityVersion.ToUnityString());
        }
    }
}
