using System.Text.Json.Serialization;

namespace PuaSuiteClean.Logic.Patching
{
    /// <summary>
    /// Represents a single patch instruction for saving JSON
    /// and for re-loading patches in PatchBatchRunnerV2.
    /// </summary>
    public class PatchInstruction
    {
        // RVA of instruction inside .text segment
        [JsonPropertyName("rva")]
        public uint Rva { get; set; }

        // File offset
        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        // The bytes to write into the file
        [JsonPropertyName("bytes")]
        public byte[] Bytes { get; set; } = [];

        // Patch category (ANTI-DEBUG / JUMP-FLIP / etc.)
        [JsonPropertyName("category")]
        public string Category { get; set; } = "";

        // Human readable description
        [JsonPropertyName("description")]
        public string Description { get; set; } = "";
    }
}
