#if !CUSTOM_VERSIONING
Setup<BuildVersion>(context =>
{
    BuildVersion buildVersion = null;

    RequireTool(ToolSettings.GitVersionTool, () => {
        buildVersion = BuildVersion.CalculatingSemanticVersion(
                context: Context
            );
        });

    Information("Building version {0} of " + BuildParameters.Title + " ({1}, {2}) using version {3} of Cake, and version {4} of Cake.Recipe. (IsTagged: {5})",
        buildVersion.FullSemVersion,
        BuildParameters.Configuration,
        BuildParameters.Target,
        buildVersion.CakeVersion,
        BuildMetaData.Version,
        BuildParameters.IsTagged);

    if (!IsSupportedCakeVersion(BuildMetaData.CakeVersion, buildVersion.CakeVersion))
    {
        if (HasArgument("ignore-cake-version"))
        {
            Warning("Currently running Cake version {0}. This version is not supported together with Cake.Recipe.", buildVersion.CakeVersion);
            Warning("--ignore-cake-version Switch was found. Continuing execution of Cake.Recipe.");
        }
        else
        {
            throw new Exception(string.Format("Cake.Recipe currently only supports building projects using version {0} of Cake.  Please update your packages.config file (or whatever method is used to pin to a specific version of Cake) to use this version. Or use the --ignore-cake-version switch if you know what you are doing!", BuildMetaData.CakeVersion));
        }
    }

    return buildVersion;
});
#endif

Setup<BuildData>(context =>
{
    Information(Figlet(BuildParameters.Title));

    Information("Starting Setup...");

    if (BuildParameters.BranchType == BranchType.Master && (context.Log.Verbosity != Verbosity.Diagnostic)) {
        Information("Increasing verbosity to diagnostic.");
        context.Log.Verbosity = Verbosity.Diagnostic;
    }

    // Make sure build and linters run before issues task.
    IssuesBuildTasks.ReadIssuesTask
        .IsDependentOn("Build");

    // Define additional URL resolvers for Cake.Issues

    // Rules from https://github.com/cake-contrib/CakeContrib.Guidelines
    MsBuildAddRuleUrlResolver(x =>
        x.Category.ToUpperInvariant() == "CCG" ?
        new Uri("https://cake-contrib.github.io/CakeContrib.Guidelines/rules/" + x.Rule.ToLowerInvariant()) :
        null,
        5);

    return new BuildData(context);
});
