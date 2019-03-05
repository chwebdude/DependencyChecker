import tl = require('vsts-task-lib/task');
import trm = require('vsts-task-lib/toolrunner');

async function run() {
    if (process.platform != 'win32') {
        tl.setResult(tl.TaskResult.Failed, "System is " + process.platform + ". Only win32 is supported at the moment")
    }

    try {

        var path = tl.getPathInput("path");
        var searchRecursive = tl.getBoolInput("searchRecursive");
        var includePrerelease = tl.getBoolInput("includePrerelease", true);
        var createReport = tl.getBoolInput("createReport", true);
        var reportPath = tl.getPathInput("reportPath");
        var createBadge = tl.getBoolInput("createBadge");
        var createBadgePerProject = tl.getBoolInput("createBadgePerProject");
        var badgeDirPath = tl.getPathInput("badgeDirPath");
        var badgePath = tl.getPathInput("badgePath");
        var style = tl.getInput("style");
        var nuGetFile = tl.getPathInput("customNuGetfile", false);

        let toolPath = __dirname + "\\..\\bin\\DependencyChecker.exe";

        let arg = ["--search-path", path, "--report-path", reportPath];
        if (createReport)
            arg.push("--create-report");
        if (createBadge){
            if(createBadgePerProject){
                arg.push("--create-badge", "--badge-path", badgeDirPath, "--badge-style", style);
            }else{
                arg.push("--create-badge", "--badge-path", badgePath, "--badge-style", style);
            }            
        }
        if (searchRecursive)
            arg.push("--search-recursive");
        if (includePrerelease)
            arg.push("--prerelease")
        if(nuGetFile != "")        
            arg.push("--nuget-file", nuGetFile);
        

        let tool: trm.ToolRunner = tl.tool(toolPath).arg(arg);
        let result: number = await tool.exec();
    }
    catch (err) {
        tl.setResult(tl.TaskResult.Failed, err.message);
    }
}

run();