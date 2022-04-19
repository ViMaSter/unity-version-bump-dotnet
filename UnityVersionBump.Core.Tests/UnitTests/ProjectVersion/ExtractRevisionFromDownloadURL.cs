using NUnit.Framework;
using UnityVersionBump.Core.Exceptions;

namespace UnityVersionBump.Core.Tests.UnitTests.ProjectVersion
{
    internal class ExtractRevisionFromDownloadURL
    {
        private static readonly object[] URLAndExpectedRevision = {
            new[] {"https://download.unity3d.com/download_unity/fdbb7325fa47/Windows64EditorInstaller/UnitySetup64-2019.4.38f1.exe", "fdbb7325fa47"},
            new[] {"https://download.unity3d.com/download_unity/915a7af8b0d5/Windows64EditorInstaller/UnitySetup64-2020.3.33f1.exe", "915a7af8b0d5"},
            new[] {"https://download.unity3d.com/download_unity/602ecdbb2fb0/Windows64EditorInstaller/UnitySetup64-2021.2.19f1.exe", "602ecdbb2fb0"},
            new[] {"https://download.unity3d.com/download_unity/6eacc8284459/Windows64EditorInstaller/UnitySetup64-2021.3.0f1.exe", "6eacc8284459"},
            new[] {"https://beta.unity3d.com/download/098023fe5c31/Windows64EditorInstaller/UnitySetup64-2022.1.0b16.exe", "098023fe5c31"},
            new[] {"https://beta.unity3d.com/download/2849b868ceb7/Windows64EditorInstaller/UnitySetup64-2022.2.0a10.exe", "2849b868ceb7"}
        };

        [TestCaseSource(nameof(URLAndExpectedRevision))]
        public void CanParseValidURL(string downloadURL, string revision)
        {
            Assert.AreEqual(revision, Core.ProjectVersion.ExtractRevisionFromDownloadURL(downloadURL));
        }

        private static readonly object[] InvalidUrls = {
            "https://regex101.com/",
            "https://www.youtube.com/watch?v=H7uxmir7cZk",
            "chrome://downloads/",
            "https://go.microsoft.com/fwlink/?linkid=2087047",
            "https://cloudmedia-docs.unity3d.com/docscloudstorage/2019.4/UnityDocumentation.zip",
            "https://beta.unity3d.com/download/2849b868ceb/Windows64EditorInstaller/UnitySetup64-2022.2.0a10.exe",
            "https://beta.unity3d.com/download/2849b868ceba4/Windows64EditorInstaller/UnitySetup64-2022.2.0a10.exe"
        };
        [TestCaseSource(nameof(InvalidUrls))]
        public void ThrowsIfURLHasNoHash(string downloadURL)
        {
            Assert.Throws<InvalidDownloadURLSyntaxException>(() => {
                Core.ProjectVersion.ExtractRevisionFromDownloadURL(downloadURL);
            });
        }
    }
}
