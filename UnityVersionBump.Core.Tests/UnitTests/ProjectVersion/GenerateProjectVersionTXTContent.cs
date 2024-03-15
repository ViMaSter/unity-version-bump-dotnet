using System.IO;
using NUnit.Framework;

namespace UnityVersionBump.Core.Tests.UnitTests.ProjectVersion
{
    internal class GenerateProjectVersionTXTContent
    {
        [TestCase]
        public void InputEqualsOutput()
        {
            var projectVersionTxt = new StreamReader(GetType().Assembly.GetManifestResourceStream("UnityVersionBump.Core.Tests.UnitTests.ProjectVersion.Resources.ProjectVersion.txt")!).ReadToEnd().ReplaceLineEndings("\n");
            var unityVersion = Core.ProjectVersion.FromProjectVersionTXT(projectVersionTxt);
            Assert.That(Core.ProjectVersion.GenerateProjectVersionTXTContent(unityVersion), Is.EqualTo(projectVersionTxt));
        }
    }
}
