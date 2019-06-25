import tl = require('azure-pipelines-task-lib/task');
import trm = require('azure-pipelines-task-lib/toolrunner');

// import * as pkgLocationUtils from "./helpers/locationUtilities";
// import * as auth from "nuget-task-common/Authentication";


async function run() {
    if (process.platform != 'win32') {
        tl.setResult(tl.TaskResult.Failed, "System is " + process.platform + ". Only win32 is supported at the moment")
    }

    try {

        // Try Credential Provider
        var cpArgs = ["-u ", "https://pkgs.dev.azure.com/webdude/_packaging/TestFeed/nuget/v3/index.json", "-S", "vso.packaging_read", "-N, -V", "Detailed"];
        let toolPathcp = __dirname + "\\..\\bin\\CredentialProvider.VSS.exe";
        let toolcp: trm.ToolRunner = tl.tool(toolPathcp).arg(cpArgs);
        let resultcp: number = await toolcp.exec();



        var path = tl.getPathInput("path");
        var searchRecursive = tl.getBoolInput("searchRecursive");
        var combineProjects = tl.getBoolInput("combineProjects");
        var includePrerelease = tl.getBoolInput("includePrerelease", true);
        var createReport = tl.getBoolInput("createReport", true);
        var reportPath = tl.getPathInput("reportPath");
        var createBadge = tl.getBoolInput("createBadge");
        var createBadgePerProject = tl.getBoolInput("createBadgePerProject");
        var badgeDirPath = tl.getPathInput("badgeDirPath");
        var badgePath = tl.getPathInput("badgePath");
        var style = tl.getInput("style");
        var nuGetFile = tl.getPathInput("customNuGetfile", false);
        var useArtifacts = tl.getBoolInput("useArtifacts", true);
        var artifactsFeeds = tl.getInput("artifactsFeeds", false);
        var collectionUri = process.env.SYSTEM_COLLECTIONURI;


        let toolPath = __dirname + "\\..\\bin\\DependencyChecker.exe";

        let arg = ["--dev-ops-result-file", "--search-path", path, "--report-path", reportPath];
        if (createReport)
            arg.push("--create-report");
        if (createBadge) {
            var realStyle = "Flat";
            switch (style) {
                case "flat": realStyle = "Flat"; break;
                case "flat-square": realStyle = "FlatSquare"; break;
                case "plastic": realStyle = "Plastic"; break;
            }

            if (createBadgePerProject) {
                arg.push("--create-badge", "--badge-path", badgeDirPath, "--badge-style", realStyle);
            } else {
                arg.push("--create-badge", "--badge-path", badgePath, "--badge-style", realStyle);
            }
        }
        if (searchRecursive)
            arg.push("--search-recursive");
        if (combineProjects)
            arg.push("--combine-projects")
        if (includePrerelease)
            arg.push("--prerelease")
        if (nuGetFile != "")
            arg.push("--nuget-file", nuGetFile);

        // Azure Artifacts Feed
        if (useArtifacts) {
            var artifactsUri = collectionUri + "_packaging/" + artifactsFeeds + "/nuget/v3/index.json";
            artifactsUri = artifactsUri.replace("dev.azure.com", "pkgs.dev.azure.com");
            arg.push("--azure-artifacts-uri", artifactsUri);
        }

        let tool: trm.ToolRunner = tl.tool(toolPath).arg(arg);
        let result: number = await tool.exec();




        // let packagingLocation: pkgLocationUtils.PackagingLocation;
        // try {
        //     tl.debug("getting the uris");
        //     packagingLocation = await pkgLocationUtils.getPackagingUris(pkgLocationUtils.ProtocolType.NuGet);
        // } catch (error) {
        //     tl.debug("Unable to get packaging URIs, using default collection URI");
        //     tl.debug(JSON.stringify(error));
        //     const collectionUrl = tl.getVariable("System.TeamFoundationCollectionUri");
        //     packagingLocation = {
        //         PackagingUris: [collectionUrl],
        //         DefaultPackagingUri: collectionUrl};
        // }
        // tl.debug("got the uris");
        // let buildIdentityDisplayName: string = null;
        // let buildIdentityAccount: string = null;


        try {


            // let credProviderPath = nutil.locateCredentialProvider();
            // // Clauses ordered in this way to avoid short-circuit evaluation, so the debug info printed by the functions
            // // is unconditionally displayed
            // const useCredProvider = ngToolRunner.isCredentialProviderEnabled(quirks) && credProviderPath;
            // const useCredConfig = ngToolRunner.isCredentialConfigEnabled(quirks) && !useCredProvider;

            // Setting up auth-related variables
            // tl.debug('Setting up auth');
            // let serviceUri = tl.getEndpointUrl("SYSTEMVSSCONNECTION", false);
            // let urlPrefixes = packagingLocation.PackagingUris;
            // tl.debug(`Discovered URL prefixes: ${urlPrefixes}`);;
            // // Note to readers: This variable will be going away once we have a fix for the location service for
            // // customers behind proxies
            // let testPrefixes = tl.getVariable("NuGetTasks.ExtraUrlPrefixesForTesting");
            // if (testPrefixes) {
            //     urlPrefixes = urlPrefixes.concat(testPrefixes.split(";"));
            //     tl.debug(`All URL prefixes: ${urlPrefixes}`);
            // }
            // let accessToken = pkgLocationUtils.getSystemAccessToken();
            // const authInfo = new auth.NuGetAuthInfo(urlPrefixes, accessToken);
            // let environmentSettings: ngToolRunner.NuGetEnvironmentSettings = {
            //     authInfo: authInfo,
            //     credProviderFolder: useCredProvider ? path.dirname(credProviderPath) : null,
            //     extensionsDisabled: true
            // };

            // // Setting up sources, either from provided config file or from feed selection
            // tl.debug('Setting up sources');
            // let nuGetConfigPath : string = undefined;
            // let selectOrConfig = tl.getInput("selectOrConfig");
            // // This IF is here in order to provide a value to nuGetConfigPath (if option selected, if user provided it)
            // // and then pass it into the config helper
            // if (selectOrConfig === "config" ) {
            //     nuGetConfigPath = tl.getPathInput("nugetConfigPath", false, true);
            //     if (!tl.filePathSupplied("nugetConfigPath")) {
            //         nuGetConfigPath = undefined;
            //     }
            // }

            // If there was no nuGetConfigPath, NuGetConfigHelper will create one
            // let nuGetConfigHelper = new NuGetConfigHelper(
            //             nuGetPath,
            //             nuGetConfigPath,
            //             authInfo,
            //             environmentSettings);

            // let credCleanup = () => { return; };

            // // Now that the NuGetConfigHelper was initialized with all the known information we can proceed
            // // and check if the user picked the 'select' option to fill out the config file if needed
            // if (selectOrConfig === "select" ) {
            //     let sources: Array<IPackageSource> = new Array<IPackageSource>();
            //     let feed = getProjectAndFeedIdFromInputParam("feed");

            //     if (feed.feedId) {
            //         if(feed.projectId) {
            //             throw new Error(tl.loc("UnsupportedProjectScopedFeeds"));
            //         } else {
            //             let feedUrl:string = await nutil.getNuGetFeedRegistryUrl(packagingLocation.DefaultPackagingUri, feed.feedId, null, nuGetVersion, accessToken);
            //             sources.push(<IPackageSource>
            //             {
            //                 feedName: feed.feedId,
            //                 feedUri: feedUrl
            //             })
            //         }
            //     }

            //     let includeNuGetOrg = tl.getBoolInput("includeNuGetOrg", false);
            //     if (includeNuGetOrg) {
            //         let nuGetUrl: string = nuGetVersion.productVersion.a < 3 ? NUGET_ORG_V2_URL : NUGET_ORG_V3_URL;
            //         sources.push(<IPackageSource>
            //         {
            //             feedName: "NuGetOrg",
            //             feedUri: nuGetUrl
            //         })
            //     }

            //     // Creating NuGet.config for the user
            //     if (sources.length > 0)
            //     {
            //         tl.debug(`Adding the following sources to the config file: ${sources.map(x => x.feedName).join(';')}`)
            //         nuGetConfigHelper.setSources(sources, false);
            //         credCleanup = () => tl.rmRF(nuGetConfigHelper.tempNugetConfigPath);
            //         nuGetConfigPath = nuGetConfigHelper.tempNugetConfigPath;
            //     }
            //     else {
            //         tl.debug('No sources were added to the temp NuGet.config file');
            //     }
            // }

            // // Setting creds in the temp NuGet.config if needed
            // let configFile = nuGetConfigPath;
            // if (useCredConfig) {
            //     tl.debug('Config credentials should be used');
            //     if (nuGetConfigPath) {
            //         let nuGetConfigHelper = new NuGetConfigHelper(
            //             nuGetPath,
            //             nuGetConfigPath,
            //             authInfo,
            //             environmentSettings);
            //         const packageSources = await nuGetConfigHelper.getSourcesFromConfig();

            //         if (packageSources.length !== 0) {
            //             nuGetConfigHelper.setSources(packageSources, true);
            //             credCleanup = () => tl.rmRF(nuGetConfigHelper.tempNugetConfigPath);
            //             configFile = nuGetConfigHelper.tempNugetConfigPath;
            //         }
            //         else {
            //             tl.debug('No package sources were added');
            //         }
            //     }
            //     else {
            //         console.log(tl.loc("Warning_NoConfigForNoCredentialProvider"));
            //     }
            // }

            // try {
            //     let restoreOptions = new RestoreOptions(
            //         nuGetPath,
            //         configFile,
            //         noCache,
            //         verbosity,
            //         packagesDirectory,
            //         environmentSettings);

            //     for (const solutionFile of filesList) {
            //         await restorePackagesAsync(solutionFile, restoreOptions);
            //     }
            // } finally {
            //     credCleanup();
            // }

            // tl.setResult(tl.TaskResult.Succeeded, tl.loc("PackagesInstalledSuccessfully"));
        } catch (err) {
            // tl.error(err);

            // if (buildIdentityDisplayName || buildIdentityAccount) {
            //     tl.warning(tl.loc("BuildIdentityPermissionsHint", buildIdentityDisplayName, buildIdentityAccount));
            // }

            // tl.setResult(tl.TaskResult.Failed, tl.loc("PackagesFailedToInstall"));
        }





    }
    catch (err) {
        tl.setResult(tl.TaskResult.Failed, err.message);
    }
}

run();