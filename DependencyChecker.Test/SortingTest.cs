using System.IO;
using System.Linq;

using DependencyChecker.Model;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DependencyChecker.Test;

[TestClass]
public class SortingTest
{
    [TestMethod]
    [DataRow(true, DisplayName = "WithSorting")]
    [DataRow(false, DisplayName = "WithoutSorting")]
    public void SortingOptionTest(bool withSorting)
    {
        var option = new Options()
        {
            SearchPath = "TestProjects/Sorted",
            SearchRec = true,
            CombineProjects = true,
            CreateReport = true,
            ReportPath = "ReportResults/CombineDifferentReport.html",
            SortByOutdated = withSorting
        };

        var runner = new Runner();
        runner.Run(option);

        var projects = runner.CodeProjects;

        Assert.AreEqual(1, projects.Count);

        var project = projects.First();
        Assert.IsTrue(project.HasPackages);
        Assert.AreEqual(3, project.PackageStatuses.Count);
        
        Assert.AreEqual("Dependency Report", project.Name);
        Assert.IsNull(project.NuGetFile);

        if (withSorting)
        {
            Assert.AreNotEqual(project.PackageStatuses.First().Id, "EnterpriseLibrary.Common");
        }
        else
        {
            Assert.AreEqual(project.PackageStatuses.First().Id, "EnterpriseLibrary.Common");
        }
        // Check Report File
        Assert.IsTrue(File.Exists(option.ReportPath));
    }
}