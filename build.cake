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
    DotNetCoreBuild("./src/Snitch.sln", new DotNetCoreBuildSettings
    {
        Configuration = "Release",
        MSBuildSettings = new DotNetCoreMSBuildSettings()
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
    DotNetCoreTest("./src/Snitch.Tests/Snitch.Tests.csproj", new DotNetCoreTestSettings 
    {
        Configuration = "Release"
    });
});

Task("Pack")
    .IsDependentOn("Run-Tests")
    .Does(() => 
{
    DotNetCorePack("./src/Snitch.sln", new DotNetCorePackSettings
    {
        Configuration = "Release",
        NoRestore = true,
        NoBuild = true,
        MSBuildSettings = new DotNetCoreMSBuildSettings()
            .WithProperty("PackageVersion", semanticVersion)
    });
});

RunTarget("Pack")