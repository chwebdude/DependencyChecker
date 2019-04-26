using DependencyChecker.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DependencyChecker.Test
{
    [TestClass]
    public class DependencyCheckerTestNetCore21
    {
        List<CodeProject> _projects;
        [TestInitialize]
        public void Initialize()
        {
            var option = new Options()
            {
                SearchPath = "TestProjects/netCore21",
                SearchRec = true
            };

            var runner = new Runner();
            runner.Run(option);

            _projects = runner.CodeProjects;
        }

        [TestMethod]
        public void ProjectInfo()
        {
            Assert.IsNotNull(_projects);
            Assert.IsTrue(_projects.Count == 1);

            Assert.AreEqual("DependencyChecker", _projects.First().Name);
            Assert.IsTrue(_projects.First().NuGetFile.EndsWith("DependencyChecker.csproj"));
        }

        [TestMethod]
        public void OutdatedCheck()
        {
            var package = _projects.First().PackageStatuses.Single(p => p.Id == "Newtonsoft.Json");
            Assert.AreEqual("2.4.3", package.InstalledVersion);
            Assert.IsFalse(package.NotFound);
            Assert.IsTrue(package.Outdated);
            Assert.IsFalse(package.NoLocalVersion);
            Assert.AreEqual(new Uri("https://www.newtonsoft.com/json"), package.ProjectUrl);
        }
    }
}
