var version = Argument<string>("packageversion", "1.0.0");

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

Task("Pack")
    .IsDependentOn("Build")
    .Does(() => 
{
    DotNetCorePack("./src/Snitch.sln", new DotNetCorePackSettings
    {
        Configuration = "Release",
        NoRestore = true,
        NoBuild = true,
        MSBuildSettings = new DotNetCoreMSBuildSettings()
            .WithProperty("PackageVersion", version)
    });
});

RunTarget("Pack")