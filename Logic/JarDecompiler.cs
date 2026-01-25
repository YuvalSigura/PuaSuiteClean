using System;
using System.Diagnostics;
using System.IO;

namespace PuaSuiteClean.Logic
{
    public class JarDecompiler
    {
        private readonly string cfrPath = "tools/cfr.jar";

        public bool Decompile(string jarFile, string outputFolder, out string message)
        {
            try
            {
                if (!File.Exists(jarFile))
                {
                    message = "JAR file not found.";
                    return false;
                }

                if (!File.Exists(cfrPath))
                {
                    message = "Decompiler (cfr.jar) missing.";
                    return false;
                }

                Directory.CreateDirectory(outputFolder);

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "java",
                    Arguments = $"-jar \"{cfrPath}\" \"{jarFile}\" --outputdir \"{outputFolder}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                var p = Process.Start(psi);

                if (p == null)
                {
                    message = "Failed to start Java process.";
                    return false;
                }

                p.WaitForExit();

                if (p.ExitCode != 0)
                {
                    message = "Decompiler error:\n" + p.StandardError.ReadToEnd();
                    return false;
                }

                message = "Decompiled successfully.";
                return true;
            }
            catch (Exception ex)
            {
                message = $"Decompiler failed: {ex.Message}";
                return false;
            }
        }
    }
}
