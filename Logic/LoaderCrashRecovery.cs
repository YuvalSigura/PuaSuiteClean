using System;

namespace PuaSuiteClean.Logic
{
    /// <summary>
    /// Handles crash recovery logic for loaders.
    /// Uses CrashAnalyzer to interpret a crash log and returns a summary.
    /// </summary>
    public class LoaderCrashRecovery
    {
        private readonly CrashAnalyzer _analyzer = new CrashAnalyzer();

        /// <summary>
        /// Analyze a crash of the given loader and return a textual summary.
        /// </summary>
        public string HandleCrash(string loaderExePath, string crashLogPath)
        {
            string summary = _analyzer.AnalyzeCrash(loaderExePath, crashLogPath);
            Logger.Write($"LoaderCrashRecovery: crash summary for '{loaderExePath}'  {summary}");
            return summary;
        }
    }
}

