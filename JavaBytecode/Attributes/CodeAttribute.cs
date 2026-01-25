using PuaSuiteClean.JavaBytecode.IO;

namespace PuaSuiteClean.JavaBytecode.Attributes {
    public class CodeAttribute {
        public ushort MaxStack;
        public ushort MaxLocals;
        public byte[] Code;

        public CodeAttribute(ClassReader br) {
            MaxStack = br.ReadU2();
            MaxLocals = br.ReadU2();
            uint len = br.ReadU4();
            Code = new byte[len];
            for (int i = 0; i < len; i++) Code[i] = br.ReadU1();

            ushort ex = br.ReadU2();
            for (int j = 0; j < ex; j++) {
                br.ReadU2(); br.ReadU2(); br.ReadU2(); br.ReadU2();
            }

            ushort sub = br.ReadU2();
            for (int j = 0; j < sub; j++) {
                br.ReadU2();
                uint l = br.ReadU4();
                for (uint x = 0; x < l; x++) br.ReadU1();
            }
        }
    }
}

