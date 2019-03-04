using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyChecker.Model
{
    public class Options
    {
        public string BadgePath
        {
            get;
            set;
        }

        public string BadgeStyle
        {
            get;
            set;
        }

        public bool CreateBadge
        {
            get;
            set;
        }

        public bool CreateReport
        {
            get;
            set;
        }


        public string ReportPath
        {
            get;
            set;
        }

        public string SearchPath
        {
            get;
            set;
        }

        public bool SearchRec
        {
            get;
            set;
        }
    }
}
