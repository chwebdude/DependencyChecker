import tl = require('vsts-task-lib/task');
import trm = require('vsts-task-lib/toolrunner');

async function run() {
    if (process.platform != 'win32') {
        tl.setResult(tl.TaskResult.Failed,"System is "+process.platform+". Only win32 is supported at the moment" )
    }

    try {

        var path = "--search-path " + tl.getInput("path");
        var searchRecursive = " --search-recursive" + tl.getBoolInput("searchRecursive");
        var includePrerelease = tl.getBoolInput("includePrerelease", true) ? " --prerelease" : "" ;
        var createReport = tl.getBoolInput("createReport", true) ? " --create-report" : "";
        var reportPath = " --report-path " + tl.getInput("reportPath");
        var createBadge = tl.getBoolInput("createBadge") ? " --create-badge" : "";
        var badgePath = " --badge-path " + tl.getInput("badgePath");
        var style = " --badge-style " + tl.getInput("style");        
        
        let toolPath = __dirname+"\\..\\bin\\DependencyChecker.exe";

        let arg = path + searchRecursive + includePrerelease + createReport + reportPath + createBadge + badgePath + style;
        
        let tool: trm.ToolRunner = tl.tool(toolPath).arg(arg);
        let result:number =  await tool.exec();
    }
    catch (err) {
        tl.setResult(tl.TaskResult.Failed, err.message);
    }
}

run();