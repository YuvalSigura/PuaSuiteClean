using System.Collections.Generic;
using PuaSuiteClean.Logic.PE;

namespace PuaSuiteClean.Logic.Patching
{
    /// <summary>
    /// A PatchPlan groups patches before injection.
    /// Each patch defines: RVA, offset, bytes, description.
    /// This is used by AutoPatchEngineV2 and PatchInjectorV2.
    /// </summary>
    public class PatchPlan
    {
        public class PatchEntry
        {
            public uint Rva { get; set; }
            public long Offset { get; set; }
            public byte[] Data { get; set; } = [];
            public string Description { get; set; } = "";
            public string Type { get; set; } = "";   // JUMP-FLIP / FORCE-SUCCESS / NOP / etc.
        }

        private readonly List<PatchEntry> _entries = new();
        public IReadOnlyList<PatchEntry> Entries => _entries;

        /// <summary>
        /// Adds a patch.
        /// </summary>
        public void Add(uint rva, long offset, byte[] bytes, string type, string description)
        {
            _entries.Add(new PatchEntry
            {
                Rva = rva,
                Offset = offset,
                Data = bytes,
                Type = type,
                Description = description
            });
        }

        /// <summary>
        /// Merges another PatchPlan into this one.
        /// </summary>
        public void Merge(PatchPlan other)
        {
            _entries.AddRange(other._entries);
        }

        /// <summary>
        /// Clears all patches.
        /// </summary>
        public void Clear()
        {
            _entries.Clear();
        }

        /// <summary>
        /// Returns true if there are no patches.
        /// </summary>
        public bool IsEmpty => _entries.Count == 0;
    }
}
