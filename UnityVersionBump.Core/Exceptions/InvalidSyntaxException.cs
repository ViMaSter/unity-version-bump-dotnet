namespace UnityVersionBump.Core.Exceptions;

public class InvalidSyntaxException : Exception
{
    public InvalidSyntaxException(string fullVersion)
        : base($"Cannot parse version string '{fullVersion}'")
    {
    }
}