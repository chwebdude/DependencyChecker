import tl = require('vsts-task-lib/task');
import trm = require('vsts-task-lib/toolrunner');

async function run() {
    if (process.platform != 'win32') {
        tl.setResult(tl.TaskResult.Failed,"System is "+process.platform+". Only win32 is supported at the moment" )
    }

    try {

        var path = tl.getInput("path");
        var searchRecursive = tl.getBoolInput("searchRecursive");
        var createReport = tl.getBoolInput("createReport", true);
        var reportPath = tl.getInput("reportPath");
        var createBadge = tl.getBoolInput("createBadge");
        var badgePath = tl.getInput("badgePath");
        var style = tl.getInput("style");
        let toolPath = __dirname+"\\..\\bin\\DependencyChecker.exe";
        
        let tool: trm.ToolRunner = tl.tool(toolPath).arg([path, searchRecursive.toString(), createReport.toString(),
             reportPath, createBadge.toString(), badgePath, style]);
        let result:number =  await tool.exec();
    }
    catch (err) {
        tl.setResult(tl.TaskResult.Failed, err.message);
    }
}

run();