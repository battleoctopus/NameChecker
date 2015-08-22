using LolApi;
using NameChecker.Properties;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NameChecker
{
    internal class Program
    {
        private const string APOSTROPHE = "'";
        private const string REGION = "na";
        private const string WORDS_PATH = "Words.txt";
        private const string NAMES_PATH = "Names.txt";
        private const int CHECK_POINTS = 100;

        public static void Main(string[] args)
        {
            string apiKey = Properties.Settings.Default.apiKey;
            ParsedRequester parsedRequester = new ParsedRequester(apiKey);

            using (StreamReader streamReader = new StreamReader(WORDS_PATH))
            {
                string wordsString = streamReader.ReadToEnd();
                string[] words = Regex.Split(wordsString, Environment.NewLine);
                Func<string, bool> minLen = word => word.Length >= Settings.Default.minLen;
                Func<string, bool> maxLen = word => word.Length <= Settings.Default.maxLen;
                Func<string, bool> noApostrophe = word => !word.Contains(APOSTROPHE);
                words = words.Where(minLen).Where(maxLen).Where(noApostrophe).ToArray();
                IOrderedEnumerable<string> sortedWords = words.OrderBy(x => x.Length).ThenBy(x => x);
                StringBuilder stringBuilder = new StringBuilder();
                int sortedWordsCount = sortedWords.Count();

                for (int i = 0; i < sortedWordsCount; i++)
                {
                    string word = sortedWords.ElementAt(i);

                    try
                    {
                        if (!parsedRequester.CheckSummonerExists(word, REGION))
                        {
                            stringBuilder.Append(String.Format("{0}{1}", word, Environment.NewLine));
                        }

                        if ((i % (sortedWordsCount / CHECK_POINTS)) == 0)
                        {
                            Console.Clear();
                            Console.WriteLine("progress: {0}/{1}", i * CHECK_POINTS / sortedWordsCount, CHECK_POINTS);
                        }
                    }
                    catch (ServerUnavailableException)
                    {
                        Console.WriteLine(String.Format(
                            CultureInfo.InvariantCulture,
                            "failed on {0} at {1:M/dd H:mm:ss} because the server was unavailable",
                            word,
                            DateTime.Now));
                        i--;
                    }
                }

                File.Delete(NAMES_PATH);
                File.AppendAllText(NAMES_PATH, stringBuilder.ToString());
                System.Diagnostics.Process.Start(NAMES_PATH);
            }
        }
    }
}
