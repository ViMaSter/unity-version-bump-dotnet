namespace UnityVersionBump.Core.Exceptions;

public class InvalidRevisionSyntaxException : Exception
{
    public InvalidRevisionSyntaxException(string revision)
        : base($"Invalid revision '{revision}': Needs to be exactly 12 lowercase hex characters (0-9 and a-f)")
    {
    }
}