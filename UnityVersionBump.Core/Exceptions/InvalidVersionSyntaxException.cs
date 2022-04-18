namespace UnityVersionBump.Core.Exceptions;

public class InvalidVersionSyntaxException : Exception
{
    public InvalidVersionSyntaxException(string fullVersion)
        : base($"Cannot parse version string '{fullVersion}'")
    {
    }
}