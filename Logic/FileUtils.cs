using System.IO;

namespace PuaSuiteClean.Logic
{
    public static class FileUtils
    {
        public static void OpenFolder(string path)
        {
            if (Directory.Exists(path))
                System.Diagnostics.Process.Start("explorer.exe", path);
        }

        public static void Ensure(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }
}

