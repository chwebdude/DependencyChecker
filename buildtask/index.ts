import tl = require('vsts-task-lib/task');
import trm = require('vsts-task-lib/toolrunner');

async function run() {
    if (process.platform != 'win32') {
        tl.setResult(tl.TaskResult.Failed, "System is " + process.platform + ". Only win32 is supported at the moment")
    }

    try {

        var path = tl.getInput("path");
        var searchRecursive = tl.getBoolInput("searchRecursive") ? "--search-recursive" : "";
        var includePrerelease = tl.getBoolInput("includePrerelease", true) ? "--prerelease" : "";
        var createReport = tl.getBoolInput("createReport", true) ? "--create-report" : "";
        var reportPath = tl.getInput("reportPath");
        var createBadge = tl.getBoolInput("createBadge") ? " --create-badge" : "";
        var badgePath = tl.getInput("badgePath");
        var style = tl.getInput("style");

        let toolPath = __dirname + "\\..\\bin\\DependencyChecker.exe";

        // let arg = [] path + searchRecursive + includePrerelease + createReport + reportPath + createBadge + badgePath + style;
        let arg = ["--search-path", path, searchRecursive, includePrerelease, createReport, "--report-path", reportPath, createBadge,
            "--badge-path", badgePath, "--badge-style", style];

        let tool: trm.ToolRunner = tl.tool(toolPath).arg(arg);
        let result: number = await tool.exec();
    }
    catch (err) {
        tl.setResult(tl.TaskResult.Failed, err.message);
    }
}

run();