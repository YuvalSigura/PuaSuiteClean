using System;
using System.Linq;

namespace PuaSuiteClean.Logic
{
    public static class PatchSuggestions
    {
        public static string[] SuggestBest()
        {
            return PatchScoreboard
                .Summary()
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .OrderByDescending(line =>
                {
                    string[] p = line.Split('?');
                    return int.Parse(p[1]);
                })
                .Select(line => line.Split('?')[0].Trim())
                .Take(3)
                .ToArray();
        }
    }
}

