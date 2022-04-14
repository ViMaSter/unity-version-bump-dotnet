// ReSharper disable InconsistentNaming
namespace UnityVersionBump.Core.SerializedResponses;

public class Response
{
    public Official[] official { get; set; } = null!;
    public Beta[] beta { get; set; } = null!;
}