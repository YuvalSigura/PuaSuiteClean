using System;
using System.IO;

namespace PuaSuiteClean.Logic
{
    public class CrashAnalyzer
    {
        public string AnalyzeCrash(string exe, string crashLogPath)
        {
            if (!File.Exists(crashLogPath))
                return "No crash log found";

            string log = File.ReadAllText(crashLogPath);

            if (log.Contains("NullReference"))
                return "Likely null pointer crash";

            if (log.Contains("Access violation"))
                return "Memory access violation";

            return "Crash signature unknown";
        }
    }
}

