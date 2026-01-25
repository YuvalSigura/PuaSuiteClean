using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace PuaSuiteClean.Logic.Patching
{
    public class PatchManagerV2
    {
        private readonly string _folder;

        public PatchManagerV2()
        {
            _folder = Path.Combine(AppContext.BaseDirectory, "patches");
            Directory.CreateDirectory(_folder);
        }

        public void SaveAutoPatches(List<PatchInstruction> patches, string name)
        {
            string path = Path.Combine(_folder, $"autopatch_{name}.json");

            string json = System.Text.Json.JsonSerializer.Serialize(
                patches,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            );

            File.WriteAllText(path, json);
        }

        public List<string> ListAutoPatches()
        {
            return Directory.GetFiles(_folder, "autopatch_*.json")
                            .Select(Path.GetFileName)
                            .ToList();
        }
    }
}
