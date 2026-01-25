using System;
using System.Collections.Generic;

namespace PuaSuiteClean.JavaBytecode.IO
{
    public class ByteStream
    {
        private List<byte> buffer = new();

        public void WriteU1(byte v) => buffer.Add(v);
        public void WriteU2(ushort v)
        {
            buffer.Add((byte)(v >> 8));
            buffer.Add((byte)(v & 0xFF));
        }

        public void WriteBytes(byte[] data) => buffer.AddRange(data);

        public byte[] ToArray() => buffer.ToArray();
    }
}

