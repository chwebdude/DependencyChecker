using CommandLine;
using DotBadge;

namespace DependencyChecker.Model
{
    public class Options
    {
        #region Properties

        [Option("badge-path", Required = false, HelpText = "Sets the path to the badge file.")]
        public string BadgePath { get; set; }

        [Option("badge-style", Required = false, HelpText = "Defines the style of the badge.")]
        public Style BadgeStyle { get; set; }

        [Option("badge-per-project", Required = false, HelpText = "Generate a badge for each project.")]
        public bool BadgePerProject { get; set; }

        [Option("create-badge", Required = false, HelpText = "Defines if a badge needs to be generated.")]
        public bool CreateBadge { get; set; }

        [Option("create-report", Required = false, HelpText = "Defines if a report needs to be generated.")]
        public bool CreateReport { get; set; }

        [Option("report-path", Required = false, HelpText = "Sets the path to the report file.")]
        public string ReportPath { get; set; }
        
        [Option("search-path", Required = true, HelpText = "Defines the search path of the csproj files.")]
        public string SearchPath { get; set; }

        [Option("search-recursive", Required = false, HelpText = "Defines if the search is recursive.")]
        public bool SearchRec { get; set; }

        [Option("combine-projects", Required = false, HelpText = "Combine projects and distinct all packages. If a package is installed in different version it will be highlighted.")]
        public bool CombineProjects { get; set; }

        [Option("prerelease", Required = false, HelpText = "Defines if prereleases should be considered or not")]
        public bool IncludePrereleases { get; set; }

        [Option("nuget-file", Required = false, HelpText = "An additional NuGet config file")]
        public string CustomNuGetFile { get; set; }

        [Option("dev-ops-result-file", Required = false, HelpText = "Create a Azure DevOps result file.")]
        public bool CreateDevOpsResultFile { get; set; }

        [Option("azure-artifacts-uri", Required = false, HelpText = "An additional source like Azure Artifacts")]
        public string AzureArtifactsFeedUri{ get; set; }
        
        [Option("sort-by-outdated", Required = false, HelpText = "Sort by outdated packages, bringing them first in the list.")]
        public bool SortByOutdated { get; set; }

        #endregion
    }
}