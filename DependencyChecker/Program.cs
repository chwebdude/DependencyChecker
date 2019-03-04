using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DependencyChecker.Model;

namespace DependencyChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            var option = new Options()
            {
                SearchPath = args[0],
                SearchRec = Convert.ToBoolean(args[1]),
                CreateReport = Convert.ToBoolean(args[2]),
                ReportPath = args[3],
                CreateBadge = Convert.ToBoolean(args[4]),
                BadgePath = args[5],
                BadgeStyle = args[6]
            };
            var options = option;
            Console.WriteLine("Options: ");
            var properties = options.GetType().GetProperties();
            for (var i = 0; i < (int)properties.Length; i++)
            {
                var propertyInfo = properties[i];
                Console.WriteLine(string.Concat(new object[] { "\t", propertyInfo.Name, ":\t", propertyInfo.GetValue(options) }));
            }
            new Runner().Run(options);
        }
    }
}
