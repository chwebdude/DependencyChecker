using System;

namespace DependencyChecker.Model
{
    public class PackageStatus
    {
        #region Properties

        public string CurrentVersion { get; set; }

        public string Id { get; set; }

        public string InstalledVersion { get; set; }

        public bool NoLocalVersion { get; set; }

        public bool NotFound { get; set; }

        public bool Outdated { get; set; }

        public Uri ProjectUrl { get; set; }

        #endregion
    }
}