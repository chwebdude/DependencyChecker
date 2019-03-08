using System.Collections.Generic;

namespace DependencyChecker.Model
{
    public class CodeProject
    {
        #region Properties

        public string Name { get; set; }

        public string NuGetFile { get; set; }

        public List<PackageStatus> PackageStatuses { get; set; } = new List<PackageStatus>();

        #endregion
    }
}