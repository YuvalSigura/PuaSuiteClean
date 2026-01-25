using System;
using System.Collections.Generic;

namespace PuaSuiteClean.Logic
{
    public static class PatchHistory
    {
        private static readonly List<string> history = new();

        public static void Add(string msg)
        {
            history.Add($"[{DateTime.Now}] {msg}");
        }

        public static string GetAll()
        {
            return string.Join("\n", history);
        }
    }
}

