using System;
using System.Collections.Generic;
using System.Linq;
using DependencyChecker.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DependencyChecker.Test
{
    [TestClass]
    public class DependecyCheckerTestNet462
    {
        List<CodeProject> _projects;
        [TestInitialize]
        public void Initalize()
        {
            var option = new Options()
            {
                SearchPath = "TestProjects/net462",
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
            Assert.IsTrue(_projects.First().NuGetFile.EndsWith("packages.config"));
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

        [TestMethod]
        public void NotFoundCheck()
        {           
            var package = _projects.First().PackageStatuses.Single(p => p.Id == "NotFound");
            Assert.AreEqual("2.4.3", package.InstalledVersion);
            Assert.IsTrue(package.NotFound);
            Assert.IsFalse(package.Outdated);
            Assert.IsFalse(package.NoLocalVersion);
            Assert.IsNull(package.ProjectUrl);
        }

        [TestMethod]
        public void NoVersionNotFound()
        {           
            var package = _projects.First().PackageStatuses.Single(p => p.Id == "NoVersionNotFound");
            Assert.IsNull(package.InstalledVersion);
            Assert.IsTrue(package.NotFound);
            Assert.IsFalse(package.Outdated);
            Assert.IsTrue(package.NoLocalVersion);
            Assert.IsNull(package.ProjectUrl);
        }

        [TestMethod]
        public void NoVersion()
        {           
            var package = _projects.First().PackageStatuses.Single(p => p.Id == "NUnit");
            Assert.IsNull(package.InstalledVersion);
            Assert.IsFalse(package.NotFound);
            Assert.IsTrue(package.Outdated);
            Assert.IsTrue(package.NoLocalVersion);
            Assert.AreEqual(new Uri("https://nunit.org/"), package.ProjectUrl);
        }
    }
}
