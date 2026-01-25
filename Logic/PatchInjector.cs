using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace PuaSuiteClean.Logic
{
    public class PatchInjector
    {
        public bool ApplyPatch(string patchFile, string targetFile, out string message)
        {
            message = "";

            if (!File.Exists(patchFile))
            {
                message = "Patch file not found: " + patchFile;
                return false;
            }

            if (!File.Exists(targetFile))
            {
                message = "Target file not found: " + targetFile;
                return false;
            }

            List<PatchInstruction> patches = LoadPatchFile(patchFile);

            if (patches.Count == 0)
            {
                message = "Patch file contains no instructions.";
                return false;
            }

            byte[] data = File.ReadAllBytes(targetFile);
            bool applied = false;

            foreach (var p in patches)
            {
                if (string.IsNullOrWhiteSpace(p.FindBytes) ||
                    string.IsNullOrWhiteSpace(p.ReplaceBytes))
                {
                    message += $"Skipping invalid patch: {p.Description}\n";
                    continue;
                }

                byte[] find = HexToBytes(p.FindBytes);
                byte[] replace = HexToBytes(p.ReplaceBytes);

                long offset = p.Offset;
                int index;

                if (offset > 0)
                {
                    index = (int)offset;
                }
                else
                {
                    index = FindPattern(data, find);
                }

                if (index < 0 || index + replace.Length > data.Length)
                {
                    message += $"Pattern not found: {p.FindBytes}\n";
                    continue;
                }

                Array.Copy(replace, 0, data, index, replace.Length);
                applied = true;
                message += $"Patched at 0x{index:X} ({p.Description})\n";
            }

            if (applied)
                File.WriteAllBytes(targetFile, data);

            return applied;
        }

        private List<PatchInstruction> LoadPatchFile(string patchFile)
        {
            try
            {
                string json = File.ReadAllText(patchFile);

                // אם זה OBJECT יחיד
                if (!json.TrimStart().StartsWith("["))
                {
                    var single = JsonSerializer.Deserialize<PatchInstruction>(json);
                    return single != null ? new List<PatchInstruction> { single } : new();
                }

                // אם זה מערך
                return JsonSerializer.Deserialize<List<PatchInstruction>>(json) ?? new();
            }
            catch
            {
                return new();
            }
        }

        private byte[] HexToBytes(string hex)
        {
            var parts = hex.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var bytes = new byte[parts.Length];

            for (int i = 0; i < parts.Length; i++)
                bytes[i] = Convert.ToByte(parts[i], 16);

            return bytes;
        }

        private int FindPattern(byte[] data, byte[] pattern)
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

                if (match)
                    return i;
            }

            return -1;
        }
    }
}
