public static class GitHubActionsUtilities
{
    public static void SetOutputVariable(string variable, string value)
    {
        Console.WriteLine($"::set-output name={variable}::{value}");
    }
    public static void GitHubActionsWriteLine(string line)
    {
        Console.WriteLine($"echo {line}");
    }
}