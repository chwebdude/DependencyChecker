using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using DependencyChecker.Model;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace DependencyChecker
{
    internal class Runner
    {
        #region Fields

        private readonly List<CodeProject> _codeProjects = new List<CodeProject>();

        private readonly List<PackageMetadataResource> _packageMetadataResources = new List<PackageMetadataResource>();
        private readonly ILogger logger = new Logger();

        #endregion

        /// <summary>
        ///     Runs the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        public void Run(Options options)
        {
            Initialize();
            RunAsync(options).Wait();
            CreateOutputDocument(options.ReportPath);
        }

        /// <summary>
        ///     Creates the output document.
        /// </summary>
        /// <param name="dest">The dest.</param>
        private void CreateOutputDocument(string dest)
        {
            var currentDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var packageTemplate = File.ReadAllText(Path.Combine(currentDir, "Templates", "Package.html"));
            var projectTemplate = File.ReadAllText(Path.Combine(currentDir, "Templates", "Project.html"));
            var reportTemplate = File.ReadAllText(Path.Combine(currentDir, "Templates", "Report.html"));
            var projectsContent = "";
            foreach (var codeProject in _codeProjects.Where(p => p.PackageStatuses.Count != 0))
            {
                var packageContent = "";
                foreach (var packageStatus in codeProject.PackageStatuses)
                {
                    var package = string.Copy(packageTemplate).Replace("{{INSTALLED_VERSION}}", packageStatus.InstalledVersion).Replace("{{CURRENT_VERSION}}", packageStatus.CurrentVersion);
                    package = packageStatus.ProjectUrl == null
                        ? package.Replace("{{LINK}}", string.Format("{0}", packageStatus.Id))
                        : package.Replace("{{LINK}}", string.Format("<a href=\"{0}\" target = \"_blank\">{1}</a>", packageStatus.ProjectUrl, packageStatus.Id));
                    if (packageStatus.NotFound)
                    {
                        package = package.Replace("{{REMARK}}", "Not Found").Replace("{{STYLE}}", "table-danger");
                    }
                    else if (!packageStatus.Outdated)
                    {
                        package = !packageStatus.NoLocalVersion
                            ? package.Replace("{{REMARK}}", "").Replace("{{STYLE}}", "")
                            : package.Replace("{{REMARK}}", "No local version found").Replace("{{STYLE}}", "table-warning");
                    }
                    else
                    {
                        package = package.Replace("{{REMARK}}", "Outdated").Replace("{{STYLE}}", "table-warning");
                    }

                    packageContent = string.Concat(packageContent, package, "\n");
                }

                var projectContent = string.Copy(projectTemplate).Replace("{{NAME}}", codeProject.Name).Replace("{{NUGETPATH}}", codeProject.NuGetFile).Replace("{{PACKAGES}}", packageContent);
                projectsContent = string.Concat(projectsContent, projectContent, "\n");
            }

            var report = reportTemplate.Replace("{{PROJECTS}}", projectsContent);
            var directory = new FileInfo(dest).Directory.FullName;
            Directory.CreateDirectory(directory);
            File.WriteAllText(dest, report);
        }

        /// <summary>
        ///     Gets the packages from packges configuration.
        /// </summary>
        /// <param name="packageFile">The package file.</param>
        /// <returns>Task&lt;List&lt;PackageStatus&gt;&gt;.</returns>
        private async Task<List<PackageStatus>> GetPackagesFromPackgesConfig(string packageFile)
        {
            // Parse file content
            var serializer = new XmlSerializer(typeof(packages));
            var data = (packages) serializer.Deserialize(new XmlTextReader(packageFile));


            // Check status of each package
            var packageStatuses = new List<PackageStatus>();
            foreach (var package in data.package)
            {
                logger.LogInformation($"Checking package {package.id}");
                var res = await GetPackageStatus(package.id, package.version);
                packageStatuses.Add(res);
                logger.LogInformation(string.Empty); // Blank line
            }

            return packageStatuses;
        }

        /// <summary>
        ///     Gets the package status.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="installedVersion">The installed version.</param>
        /// <returns>Task&lt;PackageStatus&gt;.</returns>
        private async Task<PackageStatus> GetPackageStatus(string packageId, string installedVersion)
        {
            IPackageSearchMetadata searchResult = null;
            foreach (var packageMetadataResource in _packageMetadataResources)
            {
                // Todo: Include Prerelease option
                var results = await packageMetadataResource.GetMetadataAsync(packageId, false, false, logger, CancellationToken.None);
                if (results.Count() != 0)
                {
                    searchResult = results.Last();
                    break;
                }
            }

            // Return if not found
            if (searchResult == null)
            {
                logger.LogError($"---> Package {packageId} is was not found on any source.");
                return new PackageStatus
                {
                    Id = packageId,
                    InstalledVersion = installedVersion,
                    NotFound = true
                };
            }

            // Compare installed versions
            NuGetVersion installedVersionParsed;
            var parsingResult = NuGetVersion.TryParse(installedVersion, out installedVersionParsed);
            var currentVersion = searchResult.Identity.Version;
            var outdated = false;
            if (currentVersion.CompareTo(installedVersionParsed) == 1)
            {
                outdated = true;
                logger.LogWarning($"---> Package {packageId} is out of date. Current Version: {currentVersion}. Installed Version: {installedVersionParsed}");
            }


            return new PackageStatus
            {
                CurrentVersion = searchResult.Identity.Version.ToString(),
                Id = packageId,
                InstalledVersion = installedVersion,
                NoLocalVersion = !parsingResult,
                NotFound = false,
                Outdated = outdated,
                ProjectUrl = searchResult.ProjectUrl
            };
        }


        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            // Todo: show nuget config
            logger.LogInformation("Using Sources:");
            var src = "https://api.nuget.org/v3/index.json";
            var sourceRepository = Repository.Factory.GetCoreV3(src, FeedType.HttpV3);
            var x = sourceRepository.GetResource<PackageMetadataResource>();
            _packageMetadataResources.Add(x);
            logger.LogInformation(src);


            logger.LogInformation(string.Empty); // Blank line
        }

        /// <summary>
        ///     run as an asynchronous operation.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>Task.</returns>
        private async Task RunAsync(Options options)
        {
            // Search for csproj files and it project files
            var searchMode = options.SearchRec ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var csprojFiles = Directory.GetFiles(options.SearchPath, "*.csproj", searchMode);

            foreach (var csprojFile in csprojFiles)
            {
                // Check if a packages.config is beside project file
                var file = new FileInfo(csprojFile);
                var dir = file.DirectoryName;
                var packageFile = Path.Combine(dir, "packages.config");
                if (File.Exists(packageFile))
                {
                    // Get information in old Format
                    var packages = await GetPackagesFromPackgesConfig(packageFile);
                    _codeProjects.Add(new CodeProject
                    {
                        Name = file.Name,
                        NuGetFile = packageFile,
                        PackageStatuses = packages
                    });
                }
            }
        }
    }
}