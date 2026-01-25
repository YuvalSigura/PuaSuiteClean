using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace PuaSuiteClean.Logic
{
    public class JarPatcher
    {
        public class PatchBlock
        {
            public string? TargetClass { get; set; }
            public long? Offset { get; set; }
            public byte[]? Find { get; set; }
            public byte[]? Replace { get; set; }
            public byte[]? SearchPattern { get; set; }
        }

        // =====================================================
        // MAIN ENTRY
        // =====================================================
        public bool ApplyJarPatch(string patchFile, string jarFile, out string msg)
        {
            try
            {
                if (!File.Exists(jarFile))
                {
                    msg = $"JAR file not found: {jarFile}";
                    return false;
                }

                if (!File.Exists(patchFile))
                {
                    msg = $"Patch file not found: {patchFile}";
                    return false;
                }

                List<PatchBlock> blocks = ParsePatch(patchFile);
                if (blocks.Count == 0)
                {
                    msg = "Patch file contains no blocks.";
                    return false;
                }

                string tempJar = jarFile + ".patched";
                if (File.Exists(tempJar)) File.Delete(tempJar);

                using (ZipArchive input = ZipFile.OpenRead(jarFile))
                using (ZipArchive output = ZipFile.Open(tempJar, ZipArchiveMode.Create))
                {
                    foreach (var entry in input.Entries)
                    {
                        using Stream inStream = entry.Open();
                        MemoryStream ms = new MemoryStream();
                        inStream.CopyTo(ms);
                        byte[] data = ms.ToArray();


                        foreach (var block in blocks)
                        {
                            if (block.TargetClass == null ||
                                entry.FullName.EndsWith(block.TargetClass, StringComparison.OrdinalIgnoreCase))
                            {
                                if (ApplyBlock(block, ref data, out string bmsg))
                                {
                                    Logger.Write($"Jar patch [{entry.FullName}]: {bmsg}");
                                }
                                else
                                {
                                    Logger.Write($"Jar patch FAIL [{entry.FullName}]: {bmsg}");
                                }
                            }
                        }

                        var outEntry = output.CreateEntry(entry.FullName);
                        using Stream outStream = outEntry.Open();
                        outStream.Write(data, 0, data.Length);
                    }
                }

                File.Copy(tempJar, jarFile, true);
                File.Delete(tempJar);

                msg = "JAR patched successfully.";
                return true;
            }
            catch (Exception ex)
            {
                msg = $"Jar patch failed: {ex.Message}";
                return false;
            }
        }

        // =====================================================
        // PARSE PATCH FILE
        // =====================================================
        private List<PatchBlock> ParsePatch(string patchFile)
        {
            List<PatchBlock> list = new();
            PatchBlock? current = null;

            string[] lines = File.ReadAllLines(patchFile)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("#"))
                .ToArray();

            foreach (string line in lines)
            {
                if (line.Equals("[jar]", StringComparison.OrdinalIgnoreCase))
                {
                    current = new PatchBlock();
                    list.Add(current);
                    continue;
                }

                if (current == null)
                    continue;

                if (line.StartsWith("class:", StringComparison.OrdinalIgnoreCase))
                {
                    current.TargetClass = line.Split(":")[1].Trim();
                }
                else if (line.StartsWith("offset:", StringComparison.OrdinalIgnoreCase))
                {
                    string hex = line.Split(':')[1].Trim();
                    current.Offset = Convert.ToInt64(hex, 16);
                }
                else if (line.StartsWith("find:", StringComparison.OrdinalIgnoreCase))
                {
                    current.Find = ParseHexBytes(line.Split(':')[1].Trim());
                }
                else if (line.StartsWith("replace:", StringComparison.OrdinalIgnoreCase))
                {
                    current.Replace = ParseHexBytes(line.Split(':')[1].Trim());
                }
                else if (line.StartsWith("search:", StringComparison.OrdinalIgnoreCase))
                {
                    current.SearchPattern = ParseWildcard(line.Split(':')[1].Trim());
                }
            }

            return list;
        }

        // =====================================================
        // APPLY BLOCK
        // =====================================================
        private bool ApplyBlock(PatchBlock b, ref byte[] data, out string msg)
        {
            if (b.Offset != null)
                return ApplyOffset(b, ref data, out msg);

            if (b.SearchPattern != null)
                return ApplySearch(b, ref data, out msg);

            msg = "Invalid block (no offset or search).";
            return false;
        }

        // =====================================================
        // OFFSET PATCH
        // =====================================================
        private bool ApplyOffset(PatchBlock b, ref byte[] data, out string msg)
        {
            long off = b.Offset!.Value;

            if (off < 0 || off + (b.Replace?.Length ?? 0) >= data.Length)
            {
                msg = $"Invalid offset 0x{off:X}";
                return false;
            }

            if (b.Find != null)
            {
                byte[] segment = data
                    .Skip((int)off)
                    .Take(b.Find.Length)
                    .ToArray();

                if (!segment.SequenceEqual(b.Find))
                {
                    msg = $"Find mismatch at 0x{off:X}";
                    return false;
                }
            }

            Array.Copy(b.Replace!, 0, data, (int)off, b.Replace!.Length);

            msg = $"Offset patch OK at 0x{off:X}";
            return true;
        }

        // =====================================================
        // SEARCH PATCH
        // =====================================================
        private bool ApplySearch(PatchBlock b, ref byte[] data, out string msg)
        {
            byte[] pattern = b.SearchPattern!;
            byte[] replace = b.Replace!;

            for (int i = 0; i < data.Length - pattern.Length; i++)
            {
                if (MatchWildcard(data, i, pattern))
                {
                    Array.Copy(replace, 0, data, i, replace.Length);
                    msg = $"Search/replace OK at 0x{i:X}";
                    return true;
                }
            }

            msg = "Search pattern not found.";
            return false;
        }

        private bool MatchWildcard(byte[] data, int pos, byte[] pattern)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                if (pattern[i] == 0xFF) // wildcard
                    continue;

                if (data[pos + i] != pattern[i])
                    return false;
            }
            return true;
        }

        // =====================================================
        // HEX HELPERS
        // =====================================================
        private byte[] ParseHexBytes(string hex)
        {
            return hex.Split(' ')
                      .Where(b => b.Length > 0)
                      .Select(b => byte.Parse(b, NumberStyles.HexNumber))
                      .ToArray();
        }

        private byte[] ParseWildcard(string hex)
        {
            return hex.Split(' ')
                      .Select(b => b == "??" ? (byte)0xFF : byte.Parse(b, NumberStyles.HexNumber))
                      .ToArray();
        }
    }
}
