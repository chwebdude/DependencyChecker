using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyChecker.Model
{
    public class CodeProject
    {
        public string Name
        {
            get;
            set;
        }

        public string NuGetFile
        {
            get;
            set;
        }

        public List<PackageStatus> PackageStatuses { get; set; } = new List<PackageStatus>();
    }
}
