using System.Collections.Generic;

namespace PuaSuiteClean.Logic.PE
{
    public class PEAntiDebugScanner
    {
        private static readonly string[] Patterns =
        {
            "CheckRemoteDebuggerPresent",
            "IsDebuggerPresent",
            "NtQueryInformationProcess",
            "ZwSetInformationThread",
            "OutputDebugStringA",
            "DebugBreak",
            "KiUserExceptionDispatcher"
        };

        public List<string> Scan(IEnumerable<string> strings)
        {
            List<string> result = new();

            foreach (var s in strings)
            {
                foreach (var p in Patterns)
                {
                    if (s.Contains(p))
                        result.Add(s);
                }
            }

            return result;
        }
    }
}
