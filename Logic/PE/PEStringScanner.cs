using System;
using System.Collections.Generic;
using System.Text;

namespace PuaSuiteClean.Logic.PE
{
    public class PEStringScanner
    {
        public List<string> ExtractAsciiStrings(byte[] data, int minLen = 4)
        {
            List<string> result = new();
            StringBuilder sb = new();

            foreach (var b in data)
            {
                if (b >= 32 && b <= 126) // printable ASCII
                {
                    sb.Append((char)b);
                    continue;
                }

                if (sb.Length >= minLen)
                    result.Add(sb.ToString());

                sb.Clear();
            }

            return result;
        }
    }
}
