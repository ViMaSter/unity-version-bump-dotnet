namespace UnityVersionBump.Core.Exceptions
{
    public class MismatchingLengthException : Exception {
        public MismatchingLengthException(string fullVersion, string offendingPart, int allowedMaximumLength, int actualLength)
            : base($"Cannot parse '{fullVersion}': The length of the '{offendingPart}' part is {actualLength} but the maximum allowed length is {allowedMaximumLength}")
        {
        }
    }
}