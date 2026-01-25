using System.Collections.Generic;
using PuaSuiteClean.Logic.PE;

namespace PuaSuiteClean.Logic.AutoPatch
{
    public class PatternDefinition
    {
        public string Name { get; set; } = "";
        public byte[] Bytes { get; set; } = [];
        public bool[] Mask { get; set; } = [];
        public string Description { get; set; } = "";
    }

    public static class PatternLibrary
    {
        public static List GetAntiDebugPatterns()
        {
            return new List
            {
                // IsDebuggerPresent
                new PatternDefinition
                {
                    Name = "IsDebuggerPresent",
                    Bytes = new byte[] { 0xFF, 0x15 }, // CALL [IsDebuggerPresent]
                    Mask = new bool[] { true, true },
                    Description = "IsDebuggerPresent API call"
                },

                // CheckRemoteDebuggerPresent
                new PatternDefinition
                {
                    Name = "CheckRemoteDebugger",
                    Bytes = new byte[] { 0xFF, 0x15 },
                    Mask = new bool[] { true, true },
                    Description = "CheckRemoteDebuggerPresent call"
                },

                // INT 3 (debug break)
                new PatternDefinition
                {
                    Name = "DebugBreak",
                    Bytes = new byte[] { 0xCC },
                    Mask = new bool[] { true },
                    Description = "INT3 debug breakpoint"
                },

                // NtQueryInformationProcess
                new PatternDefinition
                {
                    Name = "NtQueryInfo",
                    Bytes = new byte[] { 0x68, 0x00, 0x00, 0x00, 0x00 }, // PUSH ProcessDebugPort
                    Mask = new bool[] { true, false, false, false, false },
                    Description = "NtQueryInformationProcess debug check"
                }
            };
        }

        public static List GetLicensePatterns()
        {
            return new List
            {
                // JNE/JNZ after comparison
                new PatternDefinition
                {
                    Name = "JNE_After_CMP",
                    Bytes = new byte[] { 0x39, 0x00, 0x75 }, // CMP [reg], JNE
                    Mask = new bool[] { true, false, true },
                    Description = "Jump if not equal after license check"
                },

                // JZ after TEST
                new PatternDefinition
                {
                    Name = "JZ_After_TEST",
                    Bytes = new byte[] { 0x85, 0x00, 0x74 }, // TEST [reg], JZ
                    Mask = new bool[] { true, false, true },
                    Description = "Jump if zero after validity test"
                },

                // CALL + TEST EAX, EAX + JNE
                new PatternDefinition
                {
                    Name = "ValidityCheck",
                    Bytes = new byte[] { 0xE8, 0x00, 0x00, 0x00, 0x00, 0x85, 0xC0, 0x75 },
                    Mask = new bool[] { true, false, false, false, false, true, true, true },
                    Description = "Function call with validity check"
                }
            };
        }

        public static List GetIntegrityPatterns()
        {
            return new List
            {
                // CRC32 initialization
                new PatternDefinition
                {
                    Name = "CRC32_Init",
                    Bytes = new byte[] { 0xB9, 0xFF, 0xFF, 0xFF, 0xFF }, // MOV ECX, 0xFFFFFFFF
                    Mask = new bool[] { true, true, true, true, true },
                    Description = "CRC32 initialization"
                },

                // Checksum comparison
                new PatternDefinition
                {
                    Name = "ChecksumCmp",
                    Bytes = new byte[] { 0x3B, 0x00, 0x75 }, // CMP [reg], JNE
                    Mask = new bool[] { true, false, true },
                    Description = "Checksum comparison with jump"
                }
            };
        }

        public static BytePattern Parse(string hexPattern)
        {
            return new BytePattern(hexPattern);
        }
    }
}
