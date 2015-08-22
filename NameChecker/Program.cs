using log4net;
using LolApi;
using NameChecker.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NameChecker
{
    internal class Program
    {
        private const bool TEST = false;
        private const int CHECK_POINTS = 1000;
        private const int SAFE_EXIT = 0;
        private const string API_KEY = "7d9c3a9f-7d14-4d86-8ef2-471f32243fa8";
        private const string APOSTROPHE = "'";
        private const string NAMES_PATH = "Names.txt";
        private const string REGION = "na";
        private const string WORDS_PATH = "Words.txt";

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static void Main(string[] args)
        {
            Test();
            ParsedRequester parsedRequester = new ParsedRequester(API_KEY);

            using (StreamReader streamReader = new StreamReader(WORDS_PATH))
            {
                File.Delete(NAMES_PATH);
                IOrderedEnumerable<string> words = WordsFormatter.Format(
                    Settings.Default.firstWord,
                    Settings.Default.lastWord,
                    streamReader.ReadToEnd());
                int wordsCount = words.Count();
                StringBuilder strBuilder = new StringBuilder();

                for (int i = 0; i < wordsCount; i++)
                {
                    string word = words.ElementAt(i);

                    if ((i % (wordsCount / CHECK_POINTS)) == 0)
                    {
                        Console.Clear();
                        string logMsg = String.Format(
                            "checkpoint: {0}/{1}, word: {2}",
                            Convert.ToInt32(i * Convert.ToDouble(CHECK_POINTS) / wordsCount),
                            CHECK_POINTS,
                            word);
                        Console.WriteLine(logMsg);
                        log.Info(logMsg);
                        File.AppendAllText(NAMES_PATH, strBuilder.ToString());
                        strBuilder = new StringBuilder();
                    }

                    try
                    {
                        if (!parsedRequester.CheckSummonerExists(word, REGION))
                        {
                            strBuilder.Append(String.Format("{0}{1}", word, Environment.NewLine));
                        }
                    }
                    catch (ServerUnavailableException)
                    {
                        log.InfoFormat("failed on {0} because the server was unavailable", word);
                        i--;
                    }
                }

                File.AppendAllText(NAMES_PATH, strBuilder.ToString());
                Process.Start(NAMES_PATH);
            }
        }

        private static void Test()
        {
            if (!TEST) return;
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ParsedRequester requester = new ParsedRequester(API_KEY);
            Console.WriteLine("god exists: {0}", requester.CheckSummonerExists("god", REGION));
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            Console.WriteLine("done");
            Console.ReadLine();
            Environment.Exit(SAFE_EXIT);
        }
    }
}
