import tl = require('azure-pipelines-task-lib/task');
import trm = require('azure-pipelines-task-lib/toolrunner');

// import * as pkgLocationUtils from "./helpers/locationUtilities";
// import * as auth from "nuget-task-common/Authentication";


async function run() {
    if (process.platform != 'win32') {
        tl.setResult(tl.TaskResult.Failed, "System is " + process.platform + ". Only win32 is supported at the moment")
    }

    try {
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
    }
    catch (err) {
        tl.setResult(tl.TaskResult.Failed, err.message);
    }
}

run();