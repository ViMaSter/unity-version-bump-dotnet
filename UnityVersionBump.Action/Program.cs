using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UnityVersionBump.Action;
using UnityVersionBump.Core;
using static CommandLine.Parser;

using var host = Host.CreateDefaultBuilder(args)
    .Build();

static TService Get<TService>(IHost host)
    where TService : notnull =>
    host.Services.GetRequiredService<TService>();

var parser = Default.ParseArguments<ActionInputs>(() => new(), args);
parser.WithNotParsed(
    errors =>
    {
        Get<ILoggerFactory>(host)
            .CreateLogger("DotNet.GitHubAction.Program")
            .LogError(
                string.Join(
                    Environment.NewLine, errors.Select(error => error.ToString())));
        
        Environment.Exit(2);
    });

await parser.WithParsedAsync(StartAnalysisAsync);
await host.RunAsync();

static async Task StartAnalysisAsync(ActionInputs inputs)
{
    var projectVersionTxt = File.ReadAllText(Path.Join(inputs.ProjectPath, "ProjectSettings", "ProjectVersion.txt"));
    var currentVersion = UnityVersionBump.Core.ProjectVersion.DetermineUnityVersion(projectVersionTxt, false);
    var highestVersion = UnityVersionBump.Core.ProjectVersion.GetLatestFromHub(new(), inputs.releaseStreams.Select(Enum.Parse<UnityVersion.ReleaseStreamType>));

    Console.WriteLine($"echo Current Unity Version: {currentVersion.ToUnityString()}");
    Console.WriteLine($"echo Latest Unity Version:  {highestVersion.ToUnityString()}");
    Console.WriteLine($"::set-output name=has-never-version::{highestVersion.GetComparable()>currentVersion.GetComparable()}");
    Console.WriteLine($"::set-output name=current-unity-version::{currentVersion.ToUnityString()}");
    Console.WriteLine($"::set-output name=newest-unity-version::{highestVersion.ToUnityString()}");

    await Task.CompletedTask;

    Environment.Exit(0);
}