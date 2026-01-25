using System;
using System.IO;
using System.Linq;

namespace PuaSuiteClean.Logic.Patching
{
    public static class PatchApplier
    {
        /// <summary>
        /// Apply all instructions directly to the target file.
        /// Supports Offset or FindBytes pattern.
        /// </summary>
        public static void Apply(string targetFile, PatchInstruction[] instructions)
        {
            if (!File.Exists(targetFile))
                throw new Exception("Target file does not exist: " + targetFile);

            if (instructions == null || instructions.Length == 0)
                throw new Exception("No patch instructions to apply.");

            byte[] data = File.ReadAllBytes(targetFile);

            foreach (var inst in instructions)
            {
                // Convert HEX → byte[]
                byte[] find = HexToBytes(inst.FindBytes);
                byte[] repl = HexToBytes(inst.ReplaceBytes);

                if (repl.Length == 0)
                    throw new Exception("ReplaceBytes cannot be empty for patch: " + inst.Description);

                // OFFSET patch (simple and direct)
                if (inst.Offset > 0)
                {
                    if (inst.Offset + repl.Length > data.Length)
                        throw new Exception($"Patch offset out of range for: {inst.Description}");

                    Array.Copy(repl, 0, data, inst.Offset, repl.Length);
                    continue;
                }

                // PATTERN patch (FindBytes → ReplaceBytes)
                if (find.Length > 0)
                {
                    long pos = FindPattern(data, find);

                    if (pos < 0)
                        throw new Exception($"Pattern not found for patch: {inst.Description}");

                    // Replace
                    for (int i = 0; i < repl.Length; i++)
                        data[pos + i] = repl[i];

                    continue;
                }

                throw new Exception(
                    $"Patch '{inst.Description}' has neither Offset nor FindBytes.");
            }

            // Write back to file
            File.WriteAllBytes(targetFile, data);
        }

        // ============================ HELPERS =============================

        private static long FindPattern(byte[] data, byte[] pattern)
        {
            for (int i = 0; i <= data.Length - pattern.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (data[i + j] != pattern[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return i;
            }
            return -1;
        }

        private static byte[] HexToBytes(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return Array.Empty<byte>();

            hex = hex.Replace(" ", "").Replace("-", "");

            if (hex.Length % 2 != 0)
                throw new Exception("Invalid hex length: " + hex);

            byte[] result = new byte[hex.Length / 2];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return result;
        }
    }
}
