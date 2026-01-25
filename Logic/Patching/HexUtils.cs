using System;
using System.Linq;

namespace PuaSuiteClean.Logic.Patching
{
    public static class HexUtils
    {
        public static byte[] ToBytes(string hexString)
        {
            if (string.IsNullOrWhiteSpace(hexString))
                return Array.Empty<byte>();

            return hexString
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(h => Convert.ToByte(h, 16))
                .ToArray();
        }
    }
}
