using System;

namespace PuaSuiteClean.Logic.Patching
{
    public static class PatchExecutor
    {
        public static void Apply(byte[] fileBytes, int offset, byte[] patch)
        {
            if (offset < 0 || offset + patch.Length > fileBytes.Length)
                throw new Exception($"Invalid patch offset: {offset}");

            for (int i = 0; i < patch.Length; i++)
                fileBytes[offset + i] = patch[i];
        }
    }
}
