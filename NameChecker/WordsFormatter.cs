using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace NameChecker
{
    internal static class WordsFormatter
    {
        private const int COMPARE_EQUAL = 0;

        internal static IOrderedEnumerable<string> Format(string firstWord, string lastWord, string wordsString)
        {
            string[] words = Regex.Split(wordsString, Environment.NewLine);
            Func<string, string, int> lenAlphaComparer = (x, y) =>
                x.Length == y.Length ?
                x.CompareTo(y) :
                x.Length.CompareTo(y.Length);
            Func<string, bool> afterFirstWord = x => lenAlphaComparer(x, firstWord) >= COMPARE_EQUAL;
            Func<string, bool> beforeLastWord = x => lenAlphaComparer(x, lastWord) <= COMPARE_EQUAL;
            Func<string, bool> noApostrophe = x => !x.Contains("'");
            return words.
                Where(afterFirstWord).
                Where(beforeLastWord).
                Where(noApostrophe).
                OrderBy(x => x.Length).
                ThenBy(x => x);
        }
    }
}
