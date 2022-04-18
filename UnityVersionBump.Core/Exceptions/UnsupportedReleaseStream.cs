namespace UnityVersionBump.Core.Exceptions;

public class UnsupportedReleaseStream : Exception
{
    public UnsupportedReleaseStream(char releaseStreamShorthand)
        : base($"Unsupported release stream shorthand '{releaseStreamShorthand}'")
    {
    }
}