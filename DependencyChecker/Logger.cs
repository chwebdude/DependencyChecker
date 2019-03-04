using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Common;

namespace DependencyChecker
{
    public class Logger : ILogger
    {
        public void Log(LogLevel level, string data)
        {
            throw new NotImplementedException();
        }

        public Task LogAsync(LogLevel level, string data)
        {
            throw new NotImplementedException();
        }

        public void Log(ILogMessage message)
        {
            throw new NotImplementedException();
        }

        public Task LogAsync(ILogMessage message)
        {
            throw new NotImplementedException();
        }

        public void LogDebug(string data)
        {
            Console.WriteLine(data);
        }

        public void LogError(string data)
        {
            var c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(data);
            Console.ForegroundColor = c;
        }

        public void LogErrorSummary(string data)
        {
            throw new NotImplementedException();
        }

        public void LogInformation(string data)
        {
            Console.WriteLine(data);
        }

        public void LogInformationSummary(string data)
        {
            throw new NotImplementedException();
        }

        public void LogMinimal(string data)
        {
            Console.WriteLine(data);
        }

        public void LogSummary(string data)
        {
            Console.WriteLine(string.Concat("Summary: ", data));
        }

        public void LogVerbose(string data)
        {
            Console.WriteLine(data);
        }

        public void LogWarning(string data)
        {
            var c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(data);
            Console.ForegroundColor = c;
        }
    }
}
