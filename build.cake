#!/usr/bin/env cake

// Required namespaces

using System;

// Tools and addins

#tool nuget:?package=NUnit.ConsoleRunner&version=3.12.0
#addin nuget:?package=Cake.Coverlet&version=2.5.4
#addin nuget:?package=Cake.ExtendedNuGet&version=4.0.2
#addin nuget:?package=Cake.GitVersioning&version=3.4.244
#addin nuget:?package=Cake.Git&version=1.0.1

// Parameters and arguments

var task = Argument("task", "Build");
var projectName = Argument("projectName", "Float.TinCan");
var configuration = Argument("configuration", "Debug");
var nugetUrl = Argument<Uri>("nugetUrl", null);
var nugetToken = Argument("nugetToken", string.Empty);
var restoreAssemblyInfo = Argument("restoreAssemblyInfo", true);

// Derived global parameters

var root = MakeAbsolute(new DirectoryPath("./"));
var isReleaseBuild = configuration == "Release";
var netAssemblyInfoLocation = File($"./{projectName}/Properties/AssemblyInfo.cs");
var testProjectName = $"{projectName}.Tests";
var solution = $"{projectName}.sln";

var project = new
{
    Main = $"./{projectName}/{projectName}.csproj",
    Test = $"./{testProjectName}/{testProjectName}.csproj",
};

// Parameters to be defined later

var assemblyVersion = "0.0.0.0";
var packageVersion = "0.0.0.0-feat";

// Tasks

Task("Clean")
    .Does(() =>
    {
        CleanDirectories(GetDirectories("./.vs"));
        CleanDirectories(GetDirectories("./**/obj"));
        CleanDirectories(GetDirectories("./**/bin"));
    });

Task("RestorePackages")
    .IsDependentOn("Clean")
    .Does(() =>
    {
    	NuGetRestore(solution);
    });

Task("GitVersion")
    .Does(() =>
    {
        var gitVersion = GitVersioningGetVersion();

        assemblyVersion = $"{gitVersion.AssemblyVersion}";
        packageVersion = $"{gitVersion.AssemblyFileVersion}";

        Information($"Assembly version: {assemblyVersion}");
        Information($"NuGet version: {gitVersion.AssemblyFileVersion}");
        Information($"Informational version: {gitVersion.AssemblyInformationalVersion}");

        var visible = isReleaseBuild 
            ? new string[] {}
            : new [] { testProjectName };

        CreateAssemblyInfo(netAssemblyInfoLocation, new AssemblyInfoSettings
        {
            Version = $"{gitVersion.AssemblyVersion}",
            FileVersion = $"{gitVersion.AssemblyFileVersion}",
            InformationalVersion = $"{gitVersion.AssemblyInformationalVersion}",
            ComVisible = true,
            InternalsVisibleTo = visible,
            CustomAttributes = new []
            {
                new AssemblyInfoCustomAttribute
                {
                    NameSpace = "System.Resources",
                    Name = "NeutralResourcesLanguage",
                    Value = "en",
                },
            },
        });
    });

Task("Build")
    .IsDependentOn("RestorePackages")
    .IsDependentOn("GitVersion")
    .Does(() =>
    {
        DotNetCoreBuild(project.Main, new DotNetCoreBuildSettings
        {
            Configuration = configuration,
            NoIncremental = true,
            NoRestore = true,
            MSBuildSettings = new DotNetCoreMSBuildSettings()
                .SetVersion(packageVersion)
        });
    });

Task("Test")
    .IsDependentOn("Build")
    .WithCriteria(!isReleaseBuild)
    .Does(() =>
    {
        var testSettings = new DotNetCoreTestSettings 
        {
            Loggers = new[] { "nunit" },
        };

        var coverletSettings = new CoverletSettings 
        {
            CollectCoverage = true,
            CoverletOutputFormat = CoverletOutputFormat.opencover,
        };

        DotNetCoreTest(project.Test, testSettings, coverletSettings);
    });

Task("Pack")
    .IsDependentOn("Build")
    .Does(() =>
    {
        DotNetCorePack(project.Main, new DotNetCorePackSettings
        {
            Configuration = configuration,
            NoBuild = true,
            NoRestore = true,
            IncludeSymbols = true,
            MSBuildSettings = new DotNetCoreMSBuildSettings()
                .SetVersion(packageVersion)
        });

        var packPath = File($"./{projectName}/bin/{configuration}/netstandard2.0/{projectName}.dll");
        var packHash = CalculateFileHash(packPath).ToHex();
        var packSize = $"{FileSize(packPath)}";
        Information($"  Assembly hash: {packHash}");
        Information($"  Assembly size: {packSize} bytes");
    });

Task("Deploy")
    .IsDependentOn("Test")
    .IsDependentOn("Pack")
    .Does(() =>
    {
        if (!HasArgument("nugetUrl"))
        {
            throw new Exception("--nugetUrl is required.");
        }

        if (!HasArgument("nugetToken"))
        {
            throw new Exception("--nugetToken is required.");
        }

        if (!(GetFiles($"./{projectName}/bin/{configuration}/*.nupkg").First() is FilePath packageFile))
        {
            throw new Exception("Unable to find NuGet package file.");
        }

        var packageId = GetNuGetPackageId(packageFile);
        var packageVer = GetNuGetPackageVersion(packageFile);

        Information("Publishing NuGet Package");
        Information($"Package ID: {packageId}");
        Information($"Package Version: {packageVer}");

        PublishNuGets(
            nugetUrl.AbsoluteUri,
            nugetToken,
            new PublishNuGetsSettings
            {
            },
            new []
            {
                $"./{projectName}/bin/{configuration}/*.nupkg",
            }
        );
    });

Teardown(context =>
{
    // assembly info restoration is disabled as it was throwing an exception in LibGit2Sharp
});

// Run

RunTarget(task);
