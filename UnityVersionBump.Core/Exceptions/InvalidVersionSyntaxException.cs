namespace UnityVersionBump.Core.Exceptions;

public class InvalidVersionSyntaxException : Exception
{
    public InvalidVersionSyntaxException(string fullVersion, string hintForFix)
        : base($"Cannot parse version string '{fullVersion}'. ${hintForFix}")
    {
    }
}