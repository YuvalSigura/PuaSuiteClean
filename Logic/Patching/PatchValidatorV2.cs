using System;
using System.Collections.Generic;
using PuaSuiteClean.Logic.PE;

namespace PuaSuiteClean.Logic.Patching
{
    /// <summary>
    /// PatchValidatorV2 ensures patch safety before applying:
    /// - No overlapping patches
    /// - All offsets are inside .text
    /// - Patch bytes do not exceed boundaries
    /// </summary>
    public class PatchValidatorV2
    {
        private readonly PESectionResolver _resolver;

        public PatchValidatorV2(string filePath)
        {
            _resolver = new PESectionResolver(filePath);
        }

        public class ValidationResult
        {
            public bool Success { get; set; } = true;
            public List<string> Errors { get; set; } = new();
            public List<string> Warnings { get; set; } = new();
        }

        /// <summary>
        /// Validates an entire PatchPlan before injecting.
        /// </summary>
        public ValidationResult Validate(PatchPlan plan, long fileLength)
        {
            var result = new ValidationResult();
            var occupiedRanges = new List<(long start, long end)>();

            foreach (var p in plan.Entries)
            {
                long start = p.Offset;
                long end = p.Offset + p.Data.Length - 1;

                // 1) Bounds check
                if (start < 0 || end >= fileLength)
                {
                    result.Success = false;
                    result.Errors.Add(
                        $"Patch @0x{p.Offset:X} exceeds file bounds (len={p.Data.Length}).");
                    continue;
                }

                // 2) Check if patch is inside .text
                if (!_resolver.IsOffsetInCode(p.Offset))
                {
                    result.Warnings.Add(
                        $"Patch @0x{p.Offset:X} is outside .text — may be unsafe.");
                }

                // 3) Collision detection
                foreach (var (s, e) in occupiedRanges)
                {
                    bool overlap = !(end < s || start > e);
                    if (overlap)
                    {
                        result.Success = false;
                        result.Errors.Add(
                            $"Patch @0x{p.Offset:X} overlaps with another patch region.");
                    }
                }

                // Add patch range to list
                occupiedRanges.Add((start, end));
            }

            return result;
        }
    }
}
