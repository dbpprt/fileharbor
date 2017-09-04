Task("Dist").Does(() => {
    Information("Building project...");

	var settings = new DotNetCoreBuildSettings
	{
		Framework = "netcoreapp2.0",
		Configuration = "Production",
		OutputDirectory = "dist"
	};

    DotNetCoreBuild("./src/Fileharbor.csproj", settings);
}).OnError(ex => {
    Error("Build Failed :(");
    throw ex;
});

RunTarget("Dist");