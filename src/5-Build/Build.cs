// Copyright (c) CodeBlock.Dev. All rights reserved.
// See LICENSE in the project root for license information.

using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CodeBlock.DevKit.Build;

public class Build : NukeBuild
{
    /// <summary>
    /// It will be run when you run the nuke command without any target
    /// The best practice is to always run it before pushing the changes to source
    /// Run directly : cmd> nuke
    /// </summary>
    public static int Main() => Execute<Build>(x => x.RunUnitTests);

    [Parameter]
    private readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution]
    private readonly Solution Solution;

    [Parameter]
    private readonly AbsolutePath TestResultDirectory = RootDirectory + "/.nuke/Artifacts/Test-Results/";

    /// <summary>
    /// I just logs some information
    /// Run directly : cmd> nuke LogInformation
    /// </summary>
    private Target LogInformation =>
        _ =>
            _.Executes(() =>
            {
                Log.Information($"Solution path : {Solution}");
                Log.Information($"Solution directory : {Solution.Directory}");
                Log.Information($"Configuration : {Configuration}");
                Log.Information($"TestResultDirectory : {TestResultDirectory}");
            });

    /// <summary>
    /// It prepares the build artifacts
    /// Run directly : cmd> nuke Preparation
    /// </summary>
    private Target Preparation =>
        _ =>
            _.DependsOn(LogInformation)
                .Executes(() =>
                {
                    TestResultDirectory.CreateOrCleanDirectory();
                });

    /// <summary>
    /// It will restore all the dotnet tools mentioned in ./.config/dotnet-tools.json
    /// We use those tools in the following (like stryker and csharpier)
    /// Run directly : cmd> nuke RestoreDotNetTools
    /// </summary>
    private Target RestoreDotNetTools =>
        _ =>
            _.Executes(() =>
            {
                DotNet(arguments: "tool restore");
            });

    /// <summary>
    /// It will clean the solution
    /// Run directly : cmd> nuke Clean
    /// </summary>
    private Target Clean =>
        _ =>
            _.DependsOn(Preparation)
                .Executes(() =>
                {
                    DotNetClean();
                });

    /// <summary>
    /// It will restore all the nuget packages
    /// Run directly : cmd> nuke Restore
    /// </summary>
    private Target Restore =>
        _ =>
            _.DependsOn(Clean)
                .Executes(() =>
                {
                    DotNetRestore(a => a.SetProjectFile(Solution));
                });

    /// <summary>
    /// It will Compile the solution
    /// Run directly : cmd> nuke Compile
    /// </summary>
    private Target Compile =>
        _ =>
            _.DependsOn(Restore)
                .Executes(() =>
                {
                    DotNetBuild(a => a.SetProjectFile(Solution).SetNoRestore(true).SetConfiguration(Configuration));
                });

    /// <summary>
    /// It will lint the source code based on the rules specified in .editorconfig and csharpier
    /// It will run csharpier command to apply all the csharpier default rules (we can not change them)
    /// Then it will run dotnet format command to apply all the rules based on .editorconfig
    /// If there are any violations, it will try to fix them automatically. But some of them like namming rules should be done manualy
    /// In that case it will show you the file path and the reason of failure.
    /// Run directly : cmd> nuke Lint
    /// </summary>
    private Target Lint =>
        _ =>
            _.DependsOn(Compile, RestoreDotNetTools)
                .Executes(() =>
                {
                    // Only on local we want to apply linting changes to the source code
                    if (!IsLocalBuild)
                        return;

                    DotNet("csharpier .");
                    DotNet("format style  --verbosity diagnostic");
                    DotNet("format analyzers --verbosity diagnostic");
                });

    /// <summary>
    /// It is almost the same as Lint but in this step, it only checks if there is still any rule violation or not.
    /// It doesn't apply any change to the source code.
    /// If there is any violation, it will break the build and log the reason
    /// Run directly: cmd> nuke LintCheck
    /// </summary>
    private Target LintCheck =>
        _ =>
            _.DependsOn(Lint)
                .Executes(() =>
                {
                    DotNet("csharpier --check .");

                    DotNet("format style --verify-no-changes --verbosity diagnostic");

                    DotNet("format analyzers --verify-no-changes --verbosity diagnostic");
                });

    /// <summary>
    /// It will run all the unit tests
    /// Run directly : cmd> nuke RunUnitTests
    /// </summary>
    private Target RunUnitTests =>
        _ =>
            _.DependsOn(LintCheck)
                .Executes(() =>
                {
                    var testProjects = Solution.AllProjects.Where(s => s.Name.EndsWith("Tests.Unit"));

                    DotNetTest(a =>
                        a.SetConfiguration(Configuration)
                            .SetNoBuild(true)
                            .SetNoRestore(true)
                            .ResetVerbosity()
                            .SetResultsDirectory(TestResultDirectory)
                            .EnableCollectCoverage()
                            .SetCoverletOutputFormat(CoverletOutputFormat.opencover)
                            .SetExcludeByFile("*.Generated.cs")
                            .EnableUseSourceLink()
                            .CombineWith(
                                testProjects,
                                (b, z) =>
                                    b.SetProjectFile(z)
                                        .AddLoggers($"trx;LogFileName={z.Name}.trx")
                                        .SetCoverletOutput(TestResultDirectory + $"{z.Name}.xml")
                            )
                    );
                });
}
