using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JavaBytecode
{
    public class ClassFile
    {
        public List<JavaMethod> Methods { get; set; } = new();

        public static ClassFile Read(string path)
        {
            byte[] data = File.ReadAllBytes(path);

            var cls = new ClassFile();
            cls.ParseMethods(data);
            return cls;
        }

        public static void Write(ClassFile cls, string path)
        {
            File.WriteAllBytes(path, cls.BuildClassFile());
        }

        // Parse minimal  only Code attributes
        private void ParseMethods(byte[] data)
        {
            // This is NOT a full parser  only finds Code attributes in methods.
            // Enough for patching.

            for (int i = 0; i < data.Length - 4; i++)
            {
                // look for Code ASCII
                if (data[i] == (byte)'C' &&
                    data[i + 1] == (byte)'o' &&
                    data[i + 2] == (byte)'d' &&
                    data[i + 3] == (byte)'e')
                {
                    // crude method block extraction
                    int codeLength = BitConverter.ToInt32(new byte[]
                    {
                        data[i+10], data[i+9], data[i+8], data[i+7]
                    }, 0);

                    int codeStart = i + 14;

                    if (codeStart + codeLength <= data.Length)
                    {
                        Methods.Add(new JavaMethod()
                        {
                            Name = "unknown_method",
                            Code = new JavaCode()
                            {
                                Code = Extract(data, codeStart, codeLength),
                                CodeStart = codeStart
                            }
                        });
                    }
                }
            }
        }

        private byte[] Extract(byte[] src, int offset, int length)
        {
            byte[] b = new byte[length];
            Array.Copy(src, offset, b, 0, length);
            return b;
        }

        private byte[] BuildClassFile()
        {
            // This version does NOT rebuild the whole class.
            // Instead it reuses original bytes and overwrites method code inside.

            throw new Exception("Writing patched .class is not implemented in stub version.");
        }
    }
}

