using DependencyChecker.Model;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DependencyChecker
{
    public class Runner
    {
        #region Fields

        private readonly ILogger _logger = new Logger();

        private readonly List<PackageMetadataResource> _packageMetadataResources = new List<PackageMetadataResource>();
        public List<string> Sources { get; } = new List<string>();

        public readonly List<CodeProject> CodeProjects = new List<CodeProject>();
        private Options _options;

        #endregion

        /// <summary>
        ///     Runs the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        public void Run(Options options)
        {
            _options = options;
            Initialize();
            RunAsync(options).Wait();
            if (options.CreateReport)
            {
                CreateOutputDocument(options.ReportPath);
            }
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
            foreach (var codeProject in CodeProjects.Where(p => p.PackageStatuses.Count != 0))
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
        private async Task<List<PackageStatus>> GetPackagesFromCsproj(string csprojFile)
        {
            // Parse file content
            var serializer = new XmlSerializer(typeof(Project));
            var data = (Project)serializer.Deserialize(new XmlTextReader(csprojFile));

            var packageStatuses = new List<PackageStatus>();


            // Crawl trough all Item Groups
            foreach (var itemGroup in data.ItemGroup)
            {
                // Crawl trough all PackageReferences
                foreach (var package in itemGroup.PackageReference)
                {
                    _logger.LogInformation($"Checking package {package.Include}");
                    var res = await GetPackageStatus(package.Include, package.Version);
                    packageStatuses.Add(res);
                    _logger.LogInformation(string.Empty); // Blank line
                }
            }

            return packageStatuses;
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
            var data = (packages)serializer.Deserialize(new XmlTextReader(packageFile));


            // Check status of each package
            var packageStatuses = new List<PackageStatus>();
            foreach (var package in data.package)
            {
                _logger.LogInformation($"Checking package {package.id}");
                var res = await GetPackageStatus(package.id, package.version);
                packageStatuses.Add(res);
                _logger.LogInformation(string.Empty); // Blank line
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
                var results = await packageMetadataResource.GetMetadataAsync(packageId, _options.IncludePrereleases, false, _logger, CancellationToken.None);
                if (results.Count() != 0)
                {
                    searchResult = results.Last();
                    break;
                }
            }

            // Parse Version information
            var parsingResult = NuGetVersion.TryParse(installedVersion, out var installedVersionParsed);

            // Return if not found
            if (searchResult == null)
            {
                _logger.LogError($"---> Package {packageId} is was not found on any source.");
                return new PackageStatus
                {
                    Id = packageId,
                    InstalledVersion = installedVersion,
                    NotFound = true,
                    NoLocalVersion = !parsingResult
                };
            }

            // Compare installed versions
            var currentVersion = searchResult.Identity.Version;
            var outdated = false;
            if (currentVersion.CompareTo(installedVersionParsed) == 1)
            {
                outdated = true;
                _logger.LogWarning($"---> Package {packageId} is out of date. Current Version: {currentVersion}. Installed Version: {installedVersionParsed}");
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
            _logger.LogInformation("Using Sources:");

            var settings = Settings.LoadDefaultSettings(root: null);
            if (!string.IsNullOrEmpty(_options.CustomNuGetFile))
            {
                var additionalSettings = Settings.LoadSpecificSettings(null, _options.CustomNuGetFile);
                var section = additionalSettings.GetSection("packageSources");
                foreach (var item in section.Items)
                {
                    settings.AddOrUpdate("packageSources", item);
                }
            }

            var sourceRepositoryProvider = new SourceRepositoryProvider(settings, Repository.Provider.GetCoreV3());
            var sourceRepository = sourceRepositoryProvider.GetRepositories();
            foreach (var repository in sourceRepository)
            {
                var resource = repository.GetResource<PackageMetadataResource>();
                _packageMetadataResources.Add(resource);
                _logger.LogInformation("  " + repository + " \t - \t" + repository.PackageSource.Source);
                Sources.Add(repository.PackageSource.Source);
            }

            _logger.LogInformation(string.Empty); // Blank line
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
                    CodeProjects.Add(new CodeProject
                    {
                        Name = file.Name,
                        NuGetFile = packageFile,
                        PackageStatuses = packages
                    });
                }
                else
                {
                    // Try to get package information from csproj file
                    var packages = await GetPackagesFromCsproj(file.FullName);
                    CodeProjects.Add(new CodeProject
                    {
                        Name = file.Name,
                        NuGetFile = file.Name,
                        PackageStatuses = packages
                    });
                }
            }
        }
    }
}