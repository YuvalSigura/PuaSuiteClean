using System;
using System.Collections.Generic;
using System.IO;

namespace PuaSuiteClean.Logic.PE
{
    public class PEScanner
    {
        // === כאן האליאס החדש ==========================
        public PEResult Scan(string file) => ScanFile(file);
        // ===============================================

        public PEResult ScanFile(string file)
        {
            PEResult r = new();

            if (!File.Exists(file))
                throw new FileNotFoundException(file);

            byte[] data = File.ReadAllBytes(file);

            // 1) Imports
            var importScan = new PEImportScanner();
            r.Imports.AddRange(importScan.ScanImports(file));

            // 2) Strings
            var strScan = new PEStringScanner();
            var strings = strScan.ExtractAsciiStrings(data);
            r.Strings.AddRange(strings);

            // 3) Anti-debug
            var adb = new PEAntiDebugScanner();
            r.AntiDebug.AddRange(adb.Scan(strings));

            // 4) License keys (דוגמה)
            foreach (var s in strings)
            {
                if (s.Contains("LICENSE") || s.Contains("KEY"))
                    r.LicenseHits.Add(s);
            }

            return r;
        }
    }
}
