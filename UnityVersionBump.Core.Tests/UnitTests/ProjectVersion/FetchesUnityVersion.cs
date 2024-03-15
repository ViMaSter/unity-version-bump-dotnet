using System.IO;
using NUnit.Framework;

namespace UnityVersionBump.Core.Tests.UnitTests.ProjectVersion
{
    internal class FromProjectVersionTXT
    {
        [TestCase]
        public void ReturnsVersionAsString()
        {
            var projectVersionTxt = new StreamReader(GetType().Assembly.GetManifestResourceStream("UnityVersionBump.Core.Tests.UnitTests.ProjectVersion.Resources.ProjectVersion.txt")!).ReadToEnd();
            var unityVersion = Core.ProjectVersion.FromProjectVersionTXT(projectVersionTxt);
            Assert.That(unityVersion.ToUnityStringWithRevision(), Is.EqualTo("2020.3.15f2 (6cf78cb77498)"));
        }
    }
}
