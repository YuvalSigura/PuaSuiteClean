using System;
using System.IO;
using JavaBytecode; //      

namespace PuaSuiteClean.Logic
{
    public class ClassPatcher
    {
        public bool PatchMethod(string classFile, string methodName, byte[] find, byte[] replace, out string message)
        {
            try
            {
                var cls = ClassFile.Read(classFile);

                foreach (var method in cls.Methods)
                {
                    if (method.Name == methodName && method.Code != null)
                    {
                        byte[] code = method.Code.Code;

                        int pos = FindPattern(code, find);
                        if (pos == -1)
                        {
                            message = "Pattern not found in method.";
                            return false;
                        }

                        Array.Copy(replace, 0, code, pos, replace.Length);

                        ClassFile.Write(cls, classFile);

                        message = $"Patched method '{methodName}' at offset {pos}.";
                        return true;
                    }
                }

                message = "Method not found.";
                return false;
            }
            catch (Exception ex)
            {
                message = $"Method patch error: {ex.Message}";
                return false;
            }
        }

        private int FindPattern(byte[] data, byte[] pattern)
        {
            for (int i = 0; i < data.Length - pattern.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (pattern[j] != data[i + j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return i;
            }
            return -1;
        }
    }
}

