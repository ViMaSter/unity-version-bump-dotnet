public static class GitHubActionsUtilities
{
    public static void SetOutputVariable(string variable, string value)
    {
        var output = $"{variable}={value}";
        var outputFilename = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
        if (outputFilename == null)
        {
            Console.WriteLine($"Skipping writing output variable as no GITHUB_OUTPUT environment variable is set: '{output}'");
            return;
        }
        File.WriteAllText(outputFilename, output);
    }
    public static void GitHubActionsWriteLine(string line)
    {
        Console.WriteLine($"{line}");
    }
}