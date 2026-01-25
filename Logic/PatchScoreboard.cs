using System;
using System.Collections.Generic;
using System.IO;

namespace PuaSuiteClean.Logic
{
    public static class PatchScoreboard
    {
        private static readonly Dictionary<string, int> score = new();

        public static void Report(string patch, bool success)
        {
            if (!score.ContainsKey(patch)) score[patch] = 0;
            score[patch] += success ? 1 : -1;
        }

        public static int GetScore(string patch)
        {
            return score.ContainsKey(patch) ? score[patch] : 0;
        }

        public static string Summary()
        {
            string txt = "";
            foreach (var kv in score)
                txt += $"{kv.Key} ? {kv.Value}\n";
            return txt;
        }
    }
}

