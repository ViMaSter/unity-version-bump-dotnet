using System.IO;
using NUnit.Framework;

namespace UnityVersionBump.Core.Tests.UnitTests.ProjectVersion
{
    internal class GenerateProjectVersionTXTContent
    {
        [TestCase]
        public void InputEqualsOutput()
        {
            var projectVersionTxt = new StreamReader(GetType().Assembly.GetManifestResourceStream("UnityVersionBump.Core.Tests.UnitTests.ProjectVersion.Resources.ProjectVersion.txt")!).ReadToEnd();
            var unityVersion = Core.ProjectVersion.FromProjectVersionTXT(projectVersionTxt);
            Assert.AreEqual(projectVersionTxt, Core.ProjectVersion.GenerateProjectVersionTXTContent(unityVersion));
        }
    }
}
