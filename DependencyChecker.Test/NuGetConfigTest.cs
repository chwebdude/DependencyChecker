using DependencyChecker.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System.Linq;

namespace DependencyChecker.Test
{
    [TestClass]
    public class NuGetConfigTest
    {
        [TestMethod]
        public void TestNuGetConfig()
        {

            var settings = Settings.LoadDefaultSettings(root: null);
            var sourceRepositoryProvider = new SourceRepositoryProvider(settings, Repository.Provider.GetCoreV3());
            var beforeCount = sourceRepositoryProvider.GetRepositories().Count();

            var option = new Options()
            {
                SearchPath = "TestProjects/net462",
                SearchRec = true,
                CustomNuGetFile = "TestProjects/NuGet.Config"
            };

            var runner = new Runner();
            runner.Run(option);

            Assert.IsTrue(runner.Sources.Count > 1, "Too few sources");
            Assert.IsNotNull(runner.Sources.SingleOrDefault(n => n == @"C:\NuGetTest"));
            Assert.IsNotNull(runner.Sources.SingleOrDefault(n => n == @"C:\NuGetTest2"));
            Assert.AreEqual(beforeCount + 2, runner.Sources.Count);


        }
    }
}
