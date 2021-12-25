var semanticVersion = Argument<string>("packageversion", "1.0.0");
var version = semanticVersion.Split(new[] { '-' }).FirstOrDefault() ?? semanticVersion;

Information("Version: {0}", semanticVersion);
Information("Legacy version: {0}", version);

Task("Clean")
    .Does(() =>
{
    CleanDirectory("./.artifacts");
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetBuild("./src/Snitch.sln", new DotNetBuildSettings
    {
        Configuration = "Release",
        MSBuildSettings = new DotNetMSBuildSettings()
            .TreatAllWarningsAs(MSBuildTreatAllWarningsAs.Error)
            .WithProperty("Version", version)
            .WithProperty("AssemblyVersion", version)
            .WithProperty("FileVersion", version)
    });
});

Task("Run-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetTest("./src/Snitch.Tests/Snitch.Tests.csproj", new DotNetTestSettings
    {
        Configuration = "Release"
    });
});

Task("Pack")
    .IsDependentOn("Run-Tests")
    .Does(() =>
{
    DotNetPack("./src/Snitch.sln", new DotNetPackSettings
    {
        Configuration = "Release",
        NoRestore = true,
        NoBuild = true,
        OutputDirectory = "./.artifacts",
        MSBuildSettings = new DotNetMSBuildSettings()
            .WithProperty("PackageVersion", semanticVersion)
    });
});

RunTarget("Pack")