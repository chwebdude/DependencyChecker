using System;
using System.Collections.Generic;
using System.Linq;
using DependencyChecker.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DependencyChecker.Test
{
    [TestClass]
    public class DependecyCheckerTestNet462withoutPackages
    {
        List<CodeProject> _projects;
        [TestInitialize]
        public void Initalize()
        {
            var option = new Options()
            {
                SearchPath = "TestProjects/net462withoutPackages",
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


       
    }
}
