using System;
using System.IO;

namespace PuaSuiteClean.Logic.Patching
{
    public class PatchInjectorV2
    {
        private readonly string _filePath;

        public PatchInjectorV2(string filePath)
        {
            _filePath = filePath;
        }

        public PatchReport Apply(PatchPlan plan)
        {
            var report = new PatchReport();

            if (plan.IsEmpty)
                return report;

            byte[] data = File.ReadAllBytes(_filePath);

            foreach (var entry in plan.Entries)
            {
                if (entry.Offset < 0 || entry.Offset >= data.Length)
                {
                    report.Add(entry.Rva, entry.Offset, entry.Data,
                               entry.Type, "Offset out of range");
                    continue;
                }

                for (int i = 0; i < entry.Data.Length; i++)
                {
                    if (entry.Offset + i < data.Length)
                        data[entry.Offset + i] = entry.Data[i];
                }

                report.Add(entry.Rva, entry.Offset, entry.Data,
                           entry.Type, entry.Description);
            }

            File.WriteAllBytes(_filePath, data);
            return report;
        }
    }
}
