using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;

namespace PuaSuiteClean.Logic.Patching
{
    public static class PatchBatchRunnerV2
    {
        public static void ApplyPatches(string targetFile)
        {
            string folder = Path.Combine(AppContext.BaseDirectory, "patches");

            string[] files = Directory.GetFiles(folder, "autopatch_v2_*.json");

            foreach (string json in files)
            {
                var instructions = JsonSerializer.Deserialize<List<PatchInstruction>>(
                    File.ReadAllText(json)
                ) ?? new List<PatchInstruction>();

                PatchPlan plan = new PatchPlan();

                foreach (var i in instructions)
                {
                    plan.Add(i.Rva, i.Offset, i.Bytes, i.Category, i.Description);
                }

                new PatchInjectorV2(targetFile).Apply(plan);
            }
        }
    }
}
