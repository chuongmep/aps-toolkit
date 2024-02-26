using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;

partial class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Pack);
    readonly AbsolutePath BinDirectory = RootDirectory / "APSToolkit"/ "bin";
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            BinDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .After(Clean)
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(s => s
                .SetProjectFile(Solution)
                .SetVerbosity(DotNetVerbosity.minimal));

        });

    Target Pack => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            var configurations = Configuration;
            DotNetTasks.DotNetPack(s => s
                .SetConfiguration(configurations)
                .SetOutputDirectory(BinDirectory)
                // .SetVersion(GetAssemblyVersion(BinDirectory))
                .SetNoRestore(true)
                .SetVerbosity(DotNetVerbosity.minimal));

        });
    string GetAssemblyVersion(string binDir)
    {
        string AssName = "ForgeToolkit.dll";
        string AssPath = Path.Combine(binDir,Configuration,AssName);
        var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(AssPath);
        return versionInfo.FileVersion;
    }
    // List<string> GetConfigurations(params string[] startPatterns)
    // {
    //     var configurations = Solution.Configurations
    //         .Select(pair => pair.Key)
    //         .Where(s => startPatterns.Any(s.StartsWith))
    //         .Select(s =>
    //         {
    //             var platformIndex = s.LastIndexOf('|');
    //             return s.Remove(platformIndex);
    //         })
    //         .ToList();
    //     if (configurations.Count == 0) throw new Exception($"Can't find configurations in the solution by patterns: {string.Join(" | ", startPatterns)}.");
    //     return configurations;
    // }

}
