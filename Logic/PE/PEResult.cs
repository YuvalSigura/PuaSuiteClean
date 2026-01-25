using System.Collections.Generic;

namespace PuaSuiteClean.Logic.PE
{
    public class PEResult
    {
        public List<string> Imports { get; } = new();
        public List<string> Strings { get; } = new();
        public List<string> AntiDebug { get; } = new();
        public List<string> LicenseHits { get; } = new();

        public override string ToString()
        {
            return
                $"Imports: {Imports.Count}\n" +
                $"Strings: {Strings.Count}\n" +
                $"AntiDebug: {AntiDebug.Count}\n" +
                $"License Patterns: {LicenseHits.Count}\n";
        }
    }
}
