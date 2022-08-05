var target = Argument("target", "Default");
var configuration = "Release";
FilePath solutionPath = File("./ReswCodeGen.sln");

Task("Clean")
    .Does(() =>
{
    // Clean directories.
    CleanDirectory("./artifacts");
    CleanDirectories("./**/bin/**");
    CleanDirectories("./packages/");
});

Task("Restore")
    .IsDependentOn("Clean")
	.Does(() =>
{
	Information("Restoring solution...");
	NuGetRestore(solutionPath);
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() => {
        Information("Building solution...");
        MSBuild(solutionPath, settings =>
            settings.SetPlatformTarget(PlatformTarget.MSIL)
                .SetMSBuildPlatform(MSBuildPlatform.x86)
                .UseToolVersion(MSBuildToolVersion.VS2022)
                .WithProperty("TreatWarningsAsErrors","true")
                .WithTarget("Build")
                .SetConfiguration(configuration));
    });

Task("Run-Unit-Tests")
    .IsDependentOn("Restore")
    .Does(() =>
{
    MSTest("./**/bin/" + configuration + "/*.Tests.dll");
});

Task("Post-Build")
    .IsDependentOn("Build")
    .Does(() => {
        CopyFileToDirectory("./VSPackage/bin/Release/ReswFileCodeGenerator.vsix", "./Artifacts/");
    });

Task("Default")
	.IsDependentOn("Post-Build");

RunTarget(target);