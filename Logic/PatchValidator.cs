using System.IO;

namespace PuaSuiteClean.Logic
{
    public class PatchValidator
    {
        public bool ValidatePatch(string patchPath, string targetFile)
        {
            if (!File.Exists(patchPath)) return false;
            if (!File.Exists(targetFile)) return false;

            // Very simple validation for now:
            string text = File.ReadAllText(patchPath);
            return !string.IsNullOrWhiteSpace(text) && text.Length > 5;
        }
    }
}

