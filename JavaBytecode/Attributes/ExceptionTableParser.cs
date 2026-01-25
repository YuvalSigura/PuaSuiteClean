using System;
using System.Collections.Generic;
using PuaSuiteClean.JavaBytecode.IO;

namespace PuaSuiteClean.JavaBytecode.Attributes
{
    public class ExceptionEntry
    {
        public ushort StartPc;
        public ushort EndPc;
        public ushort HandlerPc;
        public ushort CatchType;
    }

    public class ExceptionTableParser
    {
        public static List<ExceptionEntry> Parse(ClassReader br, ushort count)
        {
            var list = new List<ExceptionEntry>(count);

            for (int i = 0; i < count; i++)
            {
                list.Add(new ExceptionEntry
                {
                    StartPc = br.ReadU2(),
                    EndPc   = br.ReadU2(),
                    HandlerPc = br.ReadU2(),
                    CatchType = br.ReadU2()
                });
            }

            return list;
        }
    }
}

