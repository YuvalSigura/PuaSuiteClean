using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PuaSuiteClean.Logic.PE
{
    public class PEImportScanner
    {
        public List<string> ScanImports(string file)
        {
            List<string> result = new();

            try
            {
                var asm = System.Reflection.AssemblyName.GetAssemblyName(file);
            }
            catch
            {
                // not .NET — continue
            }

            // Raw import table parsing
            try
            {
                var pe = new PeNet.PeFile(file);
                if (pe.ImportedFunctions != null)
                {
                    foreach (var f in pe.ImportedFunctions)
                        result.Add($"{f.DLL}|{f.Name}");
                }
            }
            catch
            {
                // ignore
            }

            return result;
        }
    }
}
