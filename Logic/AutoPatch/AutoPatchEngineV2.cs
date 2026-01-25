using System;
using System.Collections.Generic;
using System.IO;
using PuaSuiteClean.Logic.PE;
using PuaSuiteClean.Logic.Patching;

namespace PuaSuiteClean.Logic.AutoPatch
{
    public class AutoPatchEngineV2
    {
        private readonly string _targetFile;
        private readonly PESectionResolver _resolver;
        private readonly byte[] _fileData;

        public AutoPatchEngineV2(string targetFile)
        {
            if (!File.Exists(targetFile))
                throw new FileNotFoundException($"Target file not found: {targetFile}");

            _targetFile = targetFile;
            _fileData = File.ReadAllBytes(targetFile);
            _resolver = new PESectionResolver(targetFile);
        }

        public PatchReport Run()
        {
            var plan = new PatchPlan();
            var report = new PatchReport();

            // 1. Scan for anti-debug patterns
            ScanAntiDebug(plan);

            // 2. Scan for license checks
            ScanLicenseChecks(plan);

            // 3. Scan for CRC/integrity checks
            ScanIntegrityChecks(plan);

            // 4. Apply all patches
            if (!plan.IsEmpty)
            {
                var injector = new PatchInjectorV2(_targetFile);
                report = injector.Apply(plan);

                // Save patch definition
                SavePatchDefinition(plan);
            }

            return report;
        }

        private void ScanAntiDebug(PatchPlan plan)
        {
            var patterns = PatternLibrary.GetAntiDebugPatterns();

            foreach (var pattern in patterns)
            {
                var matches = FindPattern(_fileData, pattern.Bytes, pattern.Mask);

                foreach (var offset in matches)
                {
                    var patch = PatchCreator.ForceSuccess();
                    uint rva = _resolver.OffsetToRva(offset);

                    plan.Add(rva, offset, patch, "ANTI-DEBUG", 
                        $"Neutralized anti-debug at 0x{offset:X}");

                    Logger.Write($"[AutoPatch] Anti-debug found at 0x{offset:X}");
                }
            }
        }

        private void ScanLicenseChecks(PatchPlan plan)
        {
            var patterns = PatternLibrary.GetLicensePatterns();

            foreach (var pattern in patterns)
            {
                var matches = FindPattern(_fileData, pattern.Bytes, pattern.Mask);

                foreach (var offset in matches)
                {
                    // Flip jump to always succeed
                    byte jumpOpcode = _fileData[offset];
                    var patch = PatchCreator.FlipJump(jumpOpcode);
                    uint rva = _resolver.OffsetToRva(offset);

                    plan.Add(rva, offset, patch, "LICENSE-CHECK",
                        $"Flipped license check at 0x{offset:X}");

                    Logger.Write($"[AutoPatch] License check found at 0x{offset:X}");
                }
            }
        }

        private void ScanIntegrityChecks(PatchPlan plan)
        {
            var patterns = PatternLibrary.GetIntegrityPatterns();

            foreach (var pattern in patterns)
            {
                var matches = FindPattern(_fileData, pattern.Bytes, pattern.Mask);

                foreach (var offset in matches)
                {
                    var patch = PatchCreator.Nop(pattern.Bytes.Length);
                    uint rva = _resolver.OffsetToRva(offset);

                    plan.Add(rva, offset, patch, "INTEGRITY-CHECK",
                        $"Disabled integrity check at 0x{offset:X}");

                    Logger.Write($"[AutoPatch] Integrity check found at 0x{offset:X}");
                }
            }
        }

        private List FindPattern(byte[] data, byte[] pattern, bool[] mask)
        {
            var results = new List();

            for (long i = 0; i <= data.Length - pattern.Length; i++)
            {
                bool match = true;

                for (int j = 0; j < pattern.Length; j++)
                {
                    if (mask[j] && data[i + j] != pattern[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    results.Add(i);
            }

            return results;
        }

        private void SavePatchDefinition(PatchPlan plan)
        {
            var instructions = new List();

            foreach (var entry in plan.Entries)
            {
                instructions.Add(new PatchInstruction
                {
                    Rva = entry.Rva,
                    Offset = (int)entry.Offset,
                    Bytes = entry.Data,
                    Category = entry.Type,
                    Description = entry.Description
                });
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = $"autopatch_v2_{timestamp}.json";

            var manager = new PatchManagerV2();
            manager.SaveAutoPatches(instructions, filename);
        }
    }
}
