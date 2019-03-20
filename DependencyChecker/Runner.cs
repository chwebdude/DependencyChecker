using DependencyChecker.Model;
using DotBadge;
using Newtonsoft.Json;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Stubble.Core.Builders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Csproj;

namespace DependencyChecker
{
    public class Runner
    {
        #region Fields

        private readonly ILogger _logger = new Logger();

        private readonly List<PackageMetadataResource> _packageMetadataResources = new List<PackageMetadataResource>();

        public readonly List<CodeProject> CodeProjects = new List<CodeProject>();
        private Options _options;

        #endregion

        #region Properties

        public List<string> Sources { get; } = new List<string>();

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
                CreateOutputDocument();
            }

            if (options.CreateBadge)
            {
                CreateOutputBadges();
            }

            if (options.CreateDevOpsResultFile)
            {
                CreateDevOpsResultFile();
            }
        }

        private void CreateDevOpsResultFile()
        {
            var serialized = JsonConvert.SerializeObject(new { Projects = CodeProjects });
            var fi = new FileInfo("dependcies_check_result.json");
            Directory.CreateDirectory(fi.DirectoryName);
            File.WriteAllText(fi.FullName, serialized);
            Console.WriteLine("##vso[task.addattachment type=dependcies_check_result;name=dependcies_check_result;]" + fi.FullName);
        }

        /// <summary>
        ///     Creates the output badges.
        /// </summary>
        private void CreateOutputBadges()
        {
            var bp = new BadgePainter();

            // Determine if provided path is a file
            if (_options.BadgePerProject)
            {
                Directory.CreateDirectory(_options.BadgePath);
                // Create a badge for each project
                foreach (var codeProject in CodeProjects)
                {
                    var filename = Path.Combine(_options.BadgePath, "Dependencies_" + codeProject.Name + ".svg");
                    if (codeProject.PackageStatuses.Any(p => p.NotFound))
                    {
                        File.WriteAllText(filename, bp.DrawSVG("Dependencies: " + codeProject.Name, "Some not found", ColorScheme.Red, _options.BadgeStyle));
                        continue;
                    }

                    if (codeProject.PackageStatuses.Any(p => p.NoLocalVersion))
                    {
                        File.WriteAllText(filename, bp.DrawSVG("Dependencies: " + codeProject.Name, "Local version not set", ColorScheme.Red, _options.BadgeStyle));
                        continue;
                    }

                    if (codeProject.PackageStatuses.Any(p => p.Outdated))
                    {
                        File.WriteAllText(filename, bp.DrawSVG("Dependencies: " + codeProject.Name, "Outdated", ColorScheme.Yellow, _options.BadgeStyle));
                        continue;
                    }

                    File.WriteAllText(filename, bp.DrawSVG("Dependencies: " + codeProject.Name, "Up to date", ColorScheme.Green, _options.BadgeStyle));
                }
            }
            else
            {
                // Create only one badge
                var fi = new FileInfo(_options.BadgePath);
                Directory.CreateDirectory(fi.DirectoryName); // Create directory tree
                if (CodeProjects.Any(c => c.PackageStatuses.Any(p => p.NotFound)))
                {
                    File.WriteAllText(_options.BadgePath, bp.DrawSVG("Dependencies", "Some not found", ColorScheme.Red, _options.BadgeStyle));
                    return;
                }

                if (CodeProjects.Any(c => c.PackageStatuses.Any(p => p.NoLocalVersion)))
                {
                    File.WriteAllText(_options.BadgePath, bp.DrawSVG("Dependencies", "Local version not set", ColorScheme.Red, _options.BadgeStyle));
                    return;
                }

                if (CodeProjects.Any(c => c.PackageStatuses.Any(p => p.Outdated)))
                {
                    File.WriteAllText(_options.BadgePath, bp.DrawSVG("Dependencies", "Outdated", ColorScheme.Yellow, _options.BadgeStyle));
                    return;
                }

                File.WriteAllText(_options.BadgePath, bp.DrawSVG("Dependencies", "Up to date", ColorScheme.Green, _options.BadgeStyle));
            }
        }

        /// <summary>
        ///     Creates the output document.
        /// </summary>
        private void CreateOutputDocument()
        {
            var currentDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var contentTemplate = File.ReadAllText(Path.Combine(currentDir, "Templates", "Content.html"));
            var reportTemplate = File.ReadAllText(Path.Combine(currentDir, "Templates", "Report.html"));

            // Render Content
            var stubble = new StubbleBuilder().Build();
            var projectsContent = stubble.Render(contentTemplate, new { Projects = CodeProjects });

            // Insert content into report file
            var report = reportTemplate.Replace("{{PROJECTS}}", projectsContent);
            var directory = new FileInfo(_options.ReportPath).Directory.FullName;
            Directory.CreateDirectory(directory);
            File.WriteAllText(_options.ReportPath, report);
        }


        /// <summary>
        /// Gets the packages from packges configuration.
        /// </summary>
        /// <param name="csprojFile">The csproj file.</param>
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
        /// Gets the packages from packges configuration.
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
        /// Gets the package status.
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

            var settings = Settings.LoadDefaultSettings(null);
            if (!string.IsNullOrEmpty(_options.CustomNuGetFile))
            {
                if (File.Exists(_options.CustomNuGetFile))
                {
                    var additionalSettings = Settings.LoadSpecificSettings(null, _options.CustomNuGetFile);
                    var section = additionalSettings.GetSection("packageSources");
                    foreach (var item in section.Items)
                    {
                        settings.AddOrUpdate("packageSources", item);
                    }
                }
                else
                {
                    _logger.LogWarning($"Additional NuGet Config \"{_options.CustomNuGetFile}\" not found");
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

            _logger.LogInformation("Found projects:");
            foreach (var csprojFile in csprojFiles)
            {
                _logger.LogInformation("  - " + csprojFile);
            }

            foreach (var csprojFile in csprojFiles)
            {
                // Check if a packages.config is beside project file
                var file = new FileInfo(csprojFile);
                var dir = file.DirectoryName;
                var packageFile = Path.Combine(dir, "packages.config");
                if (File.Exists(packageFile))
                {
                    try
                    {
                        // Get information in old Format
                        var packages = await GetPackagesFromPackgesConfig(packageFile);
                        CodeProjects.Add(new CodeProject
                        {
                            Name = file.Name.Replace(file.Extension, string.Empty),
                            NuGetFile = packageFile,
                            PackageStatuses = packages
                        });
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Could not parse file " + packageFile + ". " + e.Message);
                    }
                }
                else
                {
                    try
                    {
                        // Try to get package information from csproj file
                        var packages = await GetPackagesFromCsproj(file.FullName);
                        CodeProjects.Add(new CodeProject
                        {
                            Name = file.Name.Replace(file.Extension, string.Empty),
                            NuGetFile = file.Name,
                            PackageStatuses = packages
                        });
                    }
                    catch (InvalidOperationException e)
                    {
                        // Probably this is in the wrong format. Try to parse with old csproj format
                        // Parse file content
                        var serializer = new XmlSerializer(typeof(CsprojOld.Project));
                        var data = (CsprojOld.Project)serializer.Deserialize(new XmlTextReader(csprojFile));
                        _logger.LogInformation("This project type should have referenced NuGet packages with a packages.config. This file wasn't found and therefore no information could be collected.");
                    }

                    catch (Exception e)
                    {
                        _logger.LogError("Could not parse file " + file.FullName + ". " + e.Message);
                    }
                }
            }
        }
    }
}