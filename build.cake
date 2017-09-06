#addin "Cake.Npm"
#addin Cake.DoInDirectory

#tool GitVersion.CommandLine

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var framework = Argument("framework", "netcoreapp2.0");

FilePath webApiProject = "./src/Fileharbor.csproj";
DirectoryPath webAppProject = "./ui";

DirectoryPath artifacts = "./dist";
GitVersion version = GitVersion();

Task("Clean")
	.Does(() =>
{
    // clean artifacts folder
    CleanDirectory(artifacts);

    // clean webapp output folder
    CleanDirectory(webAppProject.Combine("dist"));

    // clean webapi output folders
    CleanDirectories("./src/**/bin/" + configuration);
    CleanDirectories("./src/**/obj");
} );

Task("Restore")
	.Does(() =>
{
    // run dotnet restore for webapi projects
    DotNetCoreRestore(webApiProject.FullPath);

    // run npm install for webapp project
    DoInDirectory(webAppProject, () =>
    {
        NpmInstall();
    });
} );

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("Restore")
	.Does(() =>
{
    // run dotnet publish to build webapi output
    DotNetCorePublish(webApiProject.FullPath, new DotNetCorePublishSettings
    {
        Configuration = configuration,
        VersionSuffix = version.PreReleaseTag,
        OutputDirectory = "./dist/",
    });

    // run npm build to build bundled and minified webapp output
    DoInDirectory(webAppProject, () =>
    {
        NpmRunScript("build");
    });
} );

Task("Run")
	.IsDependentOn("Restore")
	.Does(() =>
{
    // start dotnet run task
    var webApi = System.Threading.Tasks.Task.Factory.StartNew(() =>
    {
        DotNetCoreRun(webApiProject.GetFilename().ToString(), null, new DotNetCoreRunSettings
        {
            WorkingDirectory = webApiProject.GetDirectory().FullPath
        });
    });

    // start npm run task
    var webApp = System.Threading.Tasks.Task.Factory.StartNew(() =>
    {
        NpmRunScript(new NpmRunScriptSettings
        {
            ScriptName = "start",
            WorkingDirectory = webAppProject.FullPath
        });
    });
    System.Threading.Tasks.Task.WaitAll(webApi, webApp);
} );

RunTarget(target);