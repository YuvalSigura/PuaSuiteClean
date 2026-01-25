using System;
using System.IO;

namespace PuaSuiteClean.Logic
{
    public class BackupManager
    {
        public string CreateBackup(string targetFile)
        {
            string backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backup");
            Directory.CreateDirectory(backupDir);

            string fileName = Path.GetFileName(targetFile);
            string backupPath = Path.Combine(backupDir, fileName + ".bak");

            File.Copy(targetFile, backupPath, overwrite: true);

            return backupPath;
        }
    }
}

