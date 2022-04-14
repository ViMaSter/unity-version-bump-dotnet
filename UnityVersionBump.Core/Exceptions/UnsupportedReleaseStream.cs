namespace UnityVersionBump.Core.Exceptions;

public class UnsupportedReleaseStream : Exception
{
    public UnsupportedReleaseStream(char releaseStreamShorthand)
        : base($"Unsupported channel shorthand '{releaseStreamShorthand}'")
    {
    }
}