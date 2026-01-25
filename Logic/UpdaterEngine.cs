using System;
using System.IO;

namespace PuaSuiteClean.Logic
{
    public class UpdaterEngine
    {
        public bool NeedsUpdate(string loaderPath)
        {
            string stamp = Path.Combine(loaderPath, "loader.txt");
            if (!File.Exists(stamp))
                return true;

            DateTime created = File.GetCreationTime(stamp);
            return (DateTime.Now - created).TotalDays > 3;
        }

        public void Update(string loaderPath)
        {
            string stamp = Path.Combine(loaderPath, "loader.txt");
            File.AppendAllText(stamp, $"\nUPDATED ? {DateTime.Now}");
        }
    }
}

