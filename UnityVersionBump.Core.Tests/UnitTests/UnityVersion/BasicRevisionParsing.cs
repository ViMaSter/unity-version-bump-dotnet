using NUnit.Framework;
using UnityVersionBump.Core.Exceptions;

namespace UnityVersionBump.Core.Tests.UnitTests.UnityVersion
{
    public class BasicRevisionParsing
    {
        private static readonly string[] InvalidRevisions = {
            "1234567890a",   // too short
            "1234567890abc", // too long
            "1234567890aq",  // invalid character (q)
            "1234567890Ab",  // uppercase character (A)
        };

        [TestCaseSource(nameof(InvalidRevisions))]
        public void ThrowsIfRevisionIsInvalid(string invalidRevision)
        {
            Assert.Throws<InvalidRevisionSyntaxException>(() => {
                _ = new Core.UnityVersion("2022.2.0p1", invalidRevision, true);
            });
        }
    }
}