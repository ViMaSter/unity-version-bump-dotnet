namespace UnityVersionBump.Core.Exceptions;

public class InvalidDownloadURLSyntaxException : Exception
{
    public InvalidDownloadURLSyntaxException(string fullVersion)
        : base($"Invalid Unity download url '{fullVersion}': Missing 'download_unity/[revision]' or 'download/[revision]'")
    {
    }
}