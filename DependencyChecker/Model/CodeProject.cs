using System.Collections.Generic;
using System.Linq;

namespace DependencyChecker.Model
{
    public class CodeProject
    {
        #region Properties

        public string Name { get; set; }

        public string NuGetFile { get; set; }

        public List<PackageStatus> PackageStatuses { get; set; } = new List<PackageStatus>();
        public bool ParsingError { get; set; }
        public bool HasPackages => PackageStatuses.Any();

        #endregion
    }
}