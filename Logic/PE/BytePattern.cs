using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PuaSuiteClean.Logic.PE
{
    public class BytePattern
    {
        public byte[] Pattern { get; }
        public bool[] Mask { get; }

        public BytePattern(string hexPattern)
        {
            var parts = hexPattern.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            List<byte> bytes = new();
            List<bool> mask = new();

            foreach (var p in parts)
            {
                if (p == "??")
                {
                    bytes.Add(0);
                    mask.Add(false); // ignore
                }
                else
                {
                    bytes.Add(byte.Parse(p, NumberStyles.HexNumber));
                    mask.Add(true);
                }
            }

            Pattern = bytes.ToArray();
            Mask = mask.ToArray();
        }

        public bool Matches(byte[] data, int offset)
        {
            if (offset + Pattern.Length > data.Length) return false;

            for (int i = 0; i < Pattern.Length; i++)
            {
                if (Mask[i] && data[offset + i] != Pattern[i])
                    return false;
            }
            return true;
        }
    }
}
