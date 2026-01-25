using System;
using System.IO;

namespace PuaSuiteClean.Logic.AutoPatch
{
    public static class AutoPatchSaver
    {
        public static void SavePatch(string lang, string name, byte[] bytes)
        {
            try
            {
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;

                // Debug logs
                Console.WriteLine("[DEBUG] EXE DIR = " + exeDir);

                string projectRoot =
                    Directory.GetParent(
                        Directory.GetParent(
                            Directory.GetParent(exeDir)!.FullName
                        )!.FullName
                    )!.FullName;

                Console.WriteLine("[DEBUG] PROJECT ROOT = " + projectRoot);

                string baseDir = Path.Combine(projectRoot, "patches", "auto", lang);

                Console.WriteLine("[DEBUG] SAVE DIR = " + baseDir);

                if (!Directory.Exists(baseDir))
                    Directory.CreateDirectory(baseDir);

                string fullPath = Path.Combine(baseDir, name + ".patch");

                Console.WriteLine("[DEBUG] SAVE FILE = " + fullPath);

                File.WriteAllBytes(fullPath, bytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[AutoPatchSaver ERROR] " + ex);
            }
        }
    }
}
