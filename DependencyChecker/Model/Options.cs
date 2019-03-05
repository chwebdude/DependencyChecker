using CommandLine;

namespace DependencyChecker.Model
{
    public class Options
    {
        #region Properties

        [Option("badge-path", Required = false, HelpText = "Sets the path to the badge file.")]
        public string BadgePath { get; set; }

        [Option("badge-style", Required = false, HelpText = "Defines the style of the badge.")]
        public string BadgeStyle { get; set; }

        [Option("create-badge", Required = false, HelpText = "Defines if a badge needs to be generated.")]
        public bool CreateBadge { get; set; }

        [Option("create-report", Required = false, HelpText = "Defines if a report needs to be generated.")]
        public bool CreateReport { get; set; }

        [Option("report-path", Required = false, HelpText = "Sets the path to the report file.")]
        public string ReportPath { get; set; }
        
        [Option("search-path", Required = true, HelpText = "Defines the search path of the csproj files.")]
        public string SearchPath { get; set; }

        [Option("search-recursive", Required = true, HelpText = "Defines if the search is recursive.")]
        public bool SearchRec { get; set; }

        [Option("prerelease", Required = true, HelpText = "Defines if prereleases should be considered or not")]
        public bool IncludePrereleases { get; set; }

        #endregion
    }
}