using System;
using System.Collections.Generic;

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
        public string DefinedInFile { get; set; }

        #endregion
    }

    public class IdAndInstalledVersionComparer : IEqualityComparer<PackageStatus>
    {
        public bool Equals(PackageStatus x, PackageStatus y)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the properties are equal.
            return x.Id == y.Id && x.InstalledVersion == y.InstalledVersion;
        }

        public int GetHashCode(PackageStatus obj)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(obj, null)) return 0;

            //Get hash code for the Id field if it is not null.
            int hashCodeId = obj.Id == null ? 0 : obj.Id.GetHashCode();

            //Get hash code for the Code field.
            int hashCodeInstalledVersion = obj.InstalledVersion == null ? 0 : obj.InstalledVersion.GetHashCode();

            //Calculate the hash code for the product.
            return hashCodeId ^ hashCodeInstalledVersion;
        }
    }
}