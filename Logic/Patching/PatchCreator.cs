using System;
using System.Collections.Generic;

namespace PuaSuiteClean.Logic.Patching
{
    /// <summary>
    /// PatchCreator generates byte-level patches used by AutoPatchEngineV2:
    /// - Flip jumps (JNE→JE, JZ→JNZ)
    /// - Force-success returns
    /// - Disable anti-debug APIs
    /// - Neutralize compare instructions (CMP→NOP or CMP→CMP reg,reg)
    /// </summary>
    public static class PatchCreator
    {
        /// <summary>
        /// Creates a NOP sequence of given length.
        /// </summary>
        public static byte[] Nop(int length)
        {
            byte[] b = new byte[length];
            for (int i = 0; i < length; i++)
                b[i] = 0x90;
            return b;
        }

        /// <summary>
        /// Generates sequence: xor eax,eax ; ret
        /// Useful for disabling anti-debug or forcing success.
        /// </summary>
        public static byte[] ForceSuccess()
        {
            return new byte[]
            {
                0x31, 0xC0, // xor eax, eax
                0xC3        // ret
            };
        }

        /// <summary>
        /// Converts JNE (0x75) into JE (0x74), or vice versa.
        /// </summary>
        public static byte[] FlipJump(byte opcode)
        {
            // JNE (75) → JE (74)
            // JNZ (75) → JZ (74)
            // opposite also works
            return opcode switch
            {
                0x75 => new byte[] { 0x74 }, // JNE/JNZ -> JE/JZ
                0x74 => new byte[] { 0x75 }, // JE/JZ -> JNE/JNZ
                _ => new byte[] { opcode }
            };
        }

        /// <summary>
        /// Turns compare instructions into "cmp eax,eax"
        /// which is always zero → predictable jump.
        /// </summary>
        public static byte[] NeutralizeCompare()
        {
            return new byte[]
            {
                0x39, 0xC0 // cmp eax, eax
            };
        }

        /// <summary>
        /// Creates a forced return value with "mov eax,01h ; ret".
        /// </summary>
        public static byte[] ForceReturnTrue()
        {
            return new byte[]
            {
                0xB8, 0x01, 0x00, 0x00, 0x00, // mov eax,1
                0xC3                          // ret
            };
        }

        /// <summary>
        /// Creates a patch to kill CALL instruction (replace with 5x NOP).
        /// </summary>
        public static byte[] KillCall()
        {
            return Nop(5);
        }

        /// <summary>
        /// Creates a patch to replace any 1-byte opcode with NOP.
        /// </summary>
        public static byte[] OneByteNop()
        {
            return new byte[] { 0x90 };
        }

        /// <summary>
        /// Creates a patch for disabling CRC comparison:
        /// replace JNZ/JNE with NOP or jump over fail block.
        /// </summary>
        public static byte[] BypassCRC()
        {
            return new byte[] { 0x90, 0x90 }; // NOP NOP (safe neutralization)
        }
    }
}

