using System.Collections.Generic;
using System.Text;

namespace PuaSuiteClean.Logic.Patching
{
    /// <summary>
    /// PatchReport formats a human-readable summary of all applied patches.
    /// Used by AutoPatch, Batch Runner, and the Dashboard UI.
    /// </summary>
    public class PatchReport
    {
        public class ReportEntry
        {
            public uint Rva { get; set; }
            public long Offset { get; set; }
            public byte[] Data { get; set; } = [];
            public string Type { get; set; } = "";
            public string Description { get; set; } = "";
        }

        private readonly List<ReportEntry> _entries = new();
        public IReadOnlyList<ReportEntry> Entries => _entries;

        /// <summary>
        /// Adds a row to the report.
        /// </summary>
        public void Add(uint rva, long offset, byte[] bytes, string type, string desc)
        {
            _entries.Add(new ReportEntry
            {
                Rva = rva,
                Offset = offset,
                Data = bytes,
                Type = type,
                Description = desc
            });
        }

        /// <summary>
        /// Converts the report to a readable string.
        /// </summary>
        public string BuildText()
        {
            if (_entries.Count == 0)
                return "No patches applied.";

            var sb = new StringBuilder();

            sb.AppendLine($"Total patches applied: {_entries.Count}");
            sb.AppendLine(new string('-', 50));

            foreach (var e in _entries)
            {
                sb.AppendLine($"Type: {e.Type}");
                sb.AppendLine($"RVA: 0x{e.Rva:X}");
                sb.AppendLine($"Offset: 0x{e.Offset:X}");
                sb.AppendLine($"Bytes: {BitConverter.ToString(e.Data)}");
                sb.AppendLine($"Description: {e.Description}");
                sb.AppendLine(new string('-', 50));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Clears the report for reuse.
        /// </summary>
        public void Clear()
        {
            _entries.Clear();
        }
    }
}
