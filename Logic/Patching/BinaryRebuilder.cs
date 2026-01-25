using System;
using System.IO;

namespace PuaSuiteClean.Logic.Patching
{
    public static class BinaryRebuilder
    {
        public static string SavePatchedBinary(byte[] bytes, string originalPath)
        {
            string dir = Path.GetDirectoryName(originalPath)!;
            string name = Path.GetFileNameWithoutExtension(originalPath);

            string newPath = Path.Combine(dir, $"{name}_patched.exe");

            File.WriteAllBytes(newPath, bytes);

            return newPath;
        }
    }
}
