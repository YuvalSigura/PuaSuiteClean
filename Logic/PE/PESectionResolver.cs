using System;
using System.Collections.Generic;
using System.IO;
using PeNet;

namespace PuaSuiteClean.Logic.PE
{
    /// <summary>
    /// Responsible for:
    /// - Resolving PE sections (.text, .rdata, .idata, etc.)
    /// - Converting RVA <-> FileOffset
    /// - Verifying if a patch location is safe
    /// </summary>
    public class PESectionResolver
    {
        public class SectionInfo
        {
            public string Name { get; set; } = "";
            public uint VirtualAddress { get; set; }
            public uint VirtualSize { get; set; }
            public uint RawAddress { get; set; }
            public uint RawSize { get; set; }

            public uint EndRVA => VirtualAddress + VirtualSize;
            public uint EndRaw => RawAddress + RawSize;
        }

        private readonly List<SectionInfo> _sections = new();

        public IReadOnlyList<SectionInfo> Sections => _sections;

        public bool IsLoaded { get; private set; } = false;
        private PeFile? _pe;

        public PESectionResolver(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found: " + filePath);

            _pe = new PeFile(File.ReadAllBytes(filePath));
            LoadSections();
        }

        private void LoadSections()
        {
            if (_pe == null)
                return;

            _sections.Clear();

            foreach (var s in _pe.ImageSectionHeaders)
            {
                _sections.Add(new SectionInfo
                {
                    Name = s.Name,
                    VirtualAddress = s.VirtualAddress,
                    VirtualSize = s.VirtualSize,
                    RawAddress = s.PointerToRawData,
                    RawSize = s.SizeOfRawData
                });
            }

            IsLoaded = true;
        }

        /// <summary>
        /// Find a section by its name (.text, .rdata, etc.)
        /// </summary>
        public SectionInfo? GetSection(string name)
        {
            name = name.ToLower();
            foreach (var s in _sections)
            {
                if (s.Name.ToLower().Contains(name))
                    return s;
            }
            return null;
        }

        /// <summary>
        /// Convert RVA to FileOffset safely.
        /// </summary>
        public long RvaToOffset(uint rva)
        {
            foreach (var sec in _sections)
            {
                if (rva >= sec.VirtualAddress && rva < sec.VirtualAddress + sec.VirtualSize)
                {
                    uint delta = rva - sec.VirtualAddress;
                    return sec.RawAddress + delta;
                }
            }

            throw new Exception($"RVA {rva:X} not found in any section.");
        }

        /// <summary>
        /// Convert FileOffset to RVA.
        /// </summary>
        public uint OffsetToRva(long offset)
        {
            foreach (var sec in _sections)
            {
                if (offset >= sec.RawAddress && offset < sec.RawAddress + sec.RawSize)
                {
                    uint delta = (uint)(offset - sec.RawAddress);
                    return sec.VirtualAddress + delta;
                }
            }

            throw new Exception($"Offset {offset:X} not found in any section.");
        }

        /// <summary>
        /// Checks if an RVA belongs to executable code inside .text
        /// </summary>
        public bool IsInCode(uint rva)
        {
            var text = GetSection(".text");
            if (text == null)
                return false;

            return rva >= text.VirtualAddress && rva < text.VirtualAddress + text.VirtualSize;
        }

        /// <summary>
        /// Check if FileOffset points to .text section.
        /// </summary>
        public bool IsOffsetInCode(long offset)
        {
            foreach (var sec in _sections)
            {
                if (sec.Name.ToLower().Contains(".text"))
                {
                    if (offset >= sec.RawAddress && offset < sec.RawAddress + sec.RawSize)
                        return true;
                }
            }
            return false;
        }
    }
}
