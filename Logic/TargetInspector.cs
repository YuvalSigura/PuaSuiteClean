using System;
using System.IO;

namespace PuaSuiteClean.Logic
{
    public static class TargetInspector
    {
        public static LanguageType Inspect(string path)
        {
            if (!File.Exists(path))
                return LanguageType.Unknown;

            string ext = Path.GetExtension(path).ToLowerInvariant();

            return ext switch
            {
                ".exe" => IsDotNet(path) ? LanguageType.DotNet : LanguageType.Native,
                ".dll" => IsDotNet(path) ? LanguageType.DotNet : LanguageType.Native,
                ".jar" => LanguageType.Java,
                ".py" => LanguageType.Python,
                ".js" => LanguageType.Node,
                _ => LanguageType.Unknown
            };
        }

        /// <summary>
        /// Detects if a PE file (EXE/DLL) is a .NET assembly by checking
        /// the CLR data directory (IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR).
        /// </summary>
        private static bool IsDotNet(string file)
        {
            try
            {
                using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var br = new BinaryReader(fs);

                // צריך שיהיה מספיק גדול בשביל כותרות PE
                if (fs.Length < 0x200)
                    return false;

                // ----- 1. קריאת מיקום כותרת PE -----
                fs.Seek(0x3C, SeekOrigin.Begin);
                int peOffset = br.ReadInt32();
                if (peOffset <= 0 || peOffset > fs.Length - 0x18)
                    return false;

                // ----- 2. חתימת "PE\0\0" -----
                fs.Seek(peOffset, SeekOrigin.Begin);
                uint peSignature = br.ReadUInt32();
                if (peSignature != 0x00004550) // "PE\0\0"
                    return false;

                // ----- 3. FileHeader -----
                br.ReadUInt16(); // Machine
                br.ReadUInt16(); // NumberOfSections
                br.ReadUInt32(); // TimeDateStamp
                br.ReadUInt32(); // PointerToSymbolTable
                br.ReadUInt32(); // NumberOfSymbols
                ushort optHeaderSize = br.ReadUInt16();
                br.ReadUInt16(); // Characteristics

                long optHeaderStart = fs.Position;
                if (optHeaderStart + optHeaderSize > fs.Length)
                    return false;

                // ----- 4. OptionalHeader (PE32 / PE32+) -----
                ushort magic = br.ReadUInt16(); // 0x10B = PE32, 0x20B = PE32+
                bool isPE32Plus = magic == 0x20B;

                // קפיצה לתחילת ה-Data Directory
                // PE32: offset 0x60 מהתחלת optional header
                // PE32+: offset 0x70 מהתחלת optional header
                int dataDirStartOffset = isPE32Plus ? 0x70 : 0x60;
                fs.Seek(optHeaderStart + dataDirStartOffset, SeekOrigin.Begin);

                const int dirEntrySize = 8;   // RVA (4) + Size (4)
                const int clrDirIndex = 14; // IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR

                // דילוג על 14 כניסות ראשונות
                fs.Seek(clrDirIndex * dirEntrySize, SeekOrigin.Current);

                uint clrRva = br.ReadUInt32();
                uint clrSize = br.ReadUInt32();

                // .NET Assembly אמיתי: שני הערכים לא אפס
                return clrRva != 0 && clrSize != 0;
            }
            catch
            {
                // אם יש שגיאה בקריאה – נניח שלא מדובר ב־.NET
                return false;
            }
        }
    }
}
