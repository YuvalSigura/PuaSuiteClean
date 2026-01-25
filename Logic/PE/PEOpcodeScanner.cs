using System;
using System.Collections.Generic;
using System.IO;

namespace PuaSuiteClean.Logic.PE
{
    /// <summary>
    /// Scans raw opcodes inside .text section to detect:
    /// - CALL instructions
    /// - CMP / TEST patterns
    /// - Conditional jumps (JNE, JE, JZ, JNZ, etc.)
    /// - CRC loops
    /// </summary>
    public class PEOpcodeScanner
    {
        private readonly byte[] _data;
        private readonly PESectionResolver _resolver;

        public class OpcodeHit
        {
            public long Offset;        // file offset
            public uint Rva;           // rva
            public string Type = "";   // CALL / CMP / JNE / CRC-LOOP etc.
            public byte[] Bytes = Array.Empty<byte>();
            public string Description = "";
        }

        public List<OpcodeHit> Hits { get; } = new();

        public PEOpcodeScanner(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            _data = File.ReadAllBytes(filePath);
            _resolver = new PESectionResolver(filePath);
        }

        /// <summary>
        /// Entry point — scans .text section for patterns.
        /// </summary>
        public void Scan()
        {
            var text = _resolver.GetSection(".text");
            if (text == null)
                return;

            long start = text.RawAddress;
            long end = text.RawAddress + text.RawSize;

            for (long i = start; i < end; i++)
            {
                byte b = _data[i];

                // CALL (0xE8 = relative call)
                if (b == 0xE8)
                    ProcessCall(i);

                // CALL qword ptr [xxxx] (FF 15 xx xx xx xx)
                if (b == 0xFF && i + 1 < end && _data[i + 1] == 0x15)
                    ProcessIndirectCall(i);

                // CMP (0x83 F8 XX, 0x3D XXXXXXXX, 0x85 C0)
                if (b == 0x83 || b == 0x3D || b == 0x85)
                    ProcessCompare(i);

                // TEST (85 C0 etc.)
                if (b == 0x85)
                    ProcessTest(i);

                // Jumps (70–7F = short jumps), E9/E8 extended
                if (b >= 0x70 && b <= 0x7F)
                    ProcessJump(i);
                if (b == 0xE9)
                    ProcessJump(i);

                // Detect CRC loops
                DetectCRCLoop(i);
            }
        }

        private void ProcessCall(long offset)
        {
            // CALL rel32 : E8 xx xx xx xx
            if (offset + 4 >= _data.Length) return;

            byte[] patch = new byte[5];
            Array.Copy(_data, offset, patch, 0, 5);

            AddHit(offset, "CALL", patch, "Relative call instruction");
        }

        private void ProcessIndirectCall(long offset)
        {
            // FF 15 xx xx xx xx  (CALL [IAT])
            if (offset + 6 >= _data.Length) return;

            byte[] patch = new byte[6];
            Array.Copy(_data, offset, patch, 0, 6);

            AddHit(offset, "CALL-INDIRECT", patch, "Potential import-based call (IsDebuggerPresent? NtQueryInformationProcess?)");
        }

        private void ProcessCompare(long offset)
        {
            // CMP patterns commonly found in license checks
            byte[] sample = new byte[Math.Min(4, _data.Length - offset)];
            Array.Copy(_data, offset, sample, 0, sample.Length);

            AddHit(offset, "CMP/COMPARE", sample, "Compare instruction (possible license / CRC check)");
        }

        private void ProcessTest(long offset)
        {
            // TEST EAX,EAX or TEST r/m,r
            if (offset + 1 >= _data.Length) return;

            byte[] sample = { _data[offset], _data[offset + 1] };
            AddHit(offset, "TEST", sample, "Test instruction (common before JZ/JNZ)");
        }

        private void ProcessJump(long offset)
        {
            byte opcode = _data[offset];
            string type = opcode switch
            {
                0x74 => "JE/JZ",     // jump equal / zero
                0x75 => "JNE/JNZ",   // jump not equal / not zero
                0x72 => "JB/JNAE",
                0x73 => "JAE/JNB",
                0x70 => "JO",
                0x71 => "JNO",
                0x7C => "JL",
                0x7D => "JGE",
                0x7E => "JLE",
                0x7F => "JG",
                0xE9 => "JMP-NEAR",
                _ => "JUMP"
            };

            AddHit(offset, type, new byte[] { opcode }, "Jump instruction (branch check)");
        }

        private void DetectCRCLoop(long offset)
        {
            // Simple CRC loop detection: look for XOR + ADD/INC + CMP + JNE
            try
            {
                if (_data[offset] == 0x33 || _data[offset] == 0x31) // XOR reg, reg
                {
                    // Try look forward for INC/ADD
                    for (int j = 1; j < 10; j++)
                    {
                        if (_data[offset + j] == 0x40 || _data[offset + j] == 0x41) // INC
                        {
                            AddHit(offset, "CRC-LOOP", new byte[] { _data[offset] }, "Possible CRC loop pattern");
                            return;
                        }
                    }
                }
            }
            catch
            {
                // Ignore out-of-range
            }
        }

        private void AddHit(long offset, string type, byte[] bytes, string desc)
        {
            Hits.Add(new OpcodeHit
            {
                Offset = offset,
                Rva = _resolver.OffsetToRva(offset),
                Type = type,
                Bytes = bytes,
                Description = desc
            });
        }
    }
}
