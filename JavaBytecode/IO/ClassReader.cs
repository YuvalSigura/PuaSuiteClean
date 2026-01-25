using System;

namespace PuaSuiteClean.JavaBytecode.IO {
    public class ClassReader {
        private readonly byte[] data;
        private int pos = 0;

        public ClassReader(byte[] d) { data = d; }

        public void ExpectMagic() {
            if (ReadU4() != 0xCAFEBABE)
                throw new Exception("Not a valid class file");
        }

        public ushort ReadU2() {
            ushort v = (ushort)((data[pos] << 8) | data[pos+1]);
            pos += 2;
            return v;
        }

        public uint ReadU4() {
            uint v = (uint)((data[pos] << 24) | (data[pos+1] << 16) |
                            (data[pos+2] << 8)  | data[pos+3]);
            pos += 4;
            return v;
        }

        public byte ReadU1() => data[pos++];
    }
}

