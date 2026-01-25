namespace PuaSuiteClean.JavaBytecode.Encoder
{
    public class InstructionEncoder
    {
        public byte[] Encode(string opcode, params byte[] args)
        {
            return new byte[] { 0x00 };
        }
    }
}
