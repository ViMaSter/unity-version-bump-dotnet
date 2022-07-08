// ReSharper disable InconsistentNaming

namespace UnityVersionBump.Core.SerializedResponses.UnityHub;

public class Official
{
    public string version { get; set; } = null!;
    public string downloadUrl { get; set; } = null!;
    public bool lts { get; set; }
}