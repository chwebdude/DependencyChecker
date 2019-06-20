using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using DependencyChecker.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DependencyChecker.Test
{
    [TestClass]
    public class CombineTest
    {

        [TestInitialize]
        public void Initialize()
        {

        }

        [TestMethod]
        public void CombineAll()
        {
            var option = new Options()
            {
                SearchPath = "TestProjects/CombineProjects/Okay",
                SearchRec = true,
                CombineProjects = true,
                CreateReport = true,
                ReportPath = "ReportResults/CombineAllReport.html"
            };

            var runner = new Runner();
            runner.Run(option);

            var projects = runner.CodeProjects;

            Assert.AreEqual(1, projects.Count);

            Assert.AreEqual("Dependency Report", projects[0].Name);
            Assert.IsNull(projects[0].NuGetFile);
            Assert.AreEqual(2, projects[0].PackageStatuses.Count);

            // Check Report File
            var reference = File.ReadAllText("TestProjects/CombineProjects/Okay/ReportReference.html");
            var reportOutput = File.ReadAllText(option.ReportPath);
            Assert.AreEqual(reference, reportOutput);
        }

        [TestMethod]
        public void CombineDifferent()
        {
            var option = new Options()
            {
                SearchPath = "TestProjects/CombineProjects/Different",
                SearchRec = true,
                CombineProjects = true,
                CreateReport = true,
                ReportPath = "ReportResults/CombineDifferentReport.html"
            };

            var runner = new Runner();
            runner.Run(option);

            var projects = runner.CodeProjects;

            Assert.AreEqual(2, projects.Count);

            Assert.AreEqual("Dependency Report", projects[0].Name);
            Assert.IsNull(projects[0].NuGetFile);

            Assert.AreEqual("NUnit", projects[1].Name);
            Assert.AreEqual(2, projects[1].PackageStatuses.Count);

            Assert.IsTrue(projects[1].PackageStatuses[0].Id.EndsWith(@"TestProjects\CombineProjects\Different\net462\packages.config"));
            Assert.AreEqual("3.11.0", projects[1].PackageStatuses[0].InstalledVersion);

            Assert.IsTrue(projects[1].PackageStatuses[1].Id.EndsWith(@"TestProjects\CombineProjects\Different\netCore21\DependencyChecker.csproj"));
            Assert.AreEqual("3.12.0", projects[1].PackageStatuses[1].InstalledVersion);

            // Check Report File
            Assert.IsTrue(File.Exists(option.ReportPath));
        }
    }
}
