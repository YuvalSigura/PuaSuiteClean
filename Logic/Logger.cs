using System;
using System.IO;

namespace PuaSuiteClean.Logic
{
    public static class Logger
    {
        private static readonly string logFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "activity.log");

        public static void Write(string message)
        {
            var dir = Path.GetDirectoryName(logFile);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);


            string line = $"[{DateTime.Now}] {message}{Environment.NewLine}";
            File.AppendAllText(logFile, line);
        }
    }
}

