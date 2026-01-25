using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PuaSuiteClean.Logic.Patching
{
    public class PatchReader
    {
        public static List<PatchInstruction> Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Patch file not found: " + path);

            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<List<PatchInstruction>>(json, options)
                   ?? new List<PatchInstruction>();
        }

        public static List<PatchInstruction> LoadLatest()
        {
            string dir = "patches";

            if (!Directory.Exists(dir))
                throw new Exception("Patch directory not found.");

            var files = Directory.GetFiles(dir, "autopatch_*.json");

            if (files.Length == 0)
                throw new Exception("No autopatch JSON files found.");

            Array.Sort(files);
            string latest = files[^1];

            return Load(latest);
        }
    }
}
