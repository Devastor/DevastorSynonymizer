using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace DevastorSynonimyzer
{
    class Program
    {
        public static int MULTI_RANDOMIZE = 2;
        public static bool LOG_SYNONYM = false;
        public static List<string> DevastorOriginalPhrases = new List<string>();
        public static List<string> DevastorSynonimyzedPhrases = new List<string>();
        public static List<string> DevastorTimersExceptionArray = new List<string>() { "Score" };
        public static Dictionary<string, System.Diagnostics.Stopwatch> DevastorWatchesArray = new Dictionary<string, System.Diagnostics.Stopwatch>();

        public static void DevastorWatch(bool START, string _name)
        {
            if (!DevastorTimersExceptionArray.Contains(_name))
            {
                if (START)
                {
                    var watch_OUT_STATE = System.Diagnostics.Stopwatch.StartNew();
                    try
                    {
                        DevastorWatchesArray.Add(_name, watch_OUT_STATE);
                        DevastorWatchesArray[_name].Start();
                        Console.WriteLine("---===--- Watch: " + _name + " started!");
                    }
                    catch { Console.WriteLine("TIMER " + _name + " already in array!"); }
                }
                else
                {
                    DevastorWatchesArray[_name].Stop();
                    var elapsedMs = DevastorWatchesArray[_name].ElapsedMilliseconds;
                    int seconds = (int)elapsedMs / 1000;
                    int minutes = (int)seconds / 60;
                    seconds = seconds - minutes * 60;
                    DevastorWatchesArray[_name].Start();
                    DevastorWatchesArray[_name].Reset();
                    DevastorWatchesArray[_name].Stop();
                    DevastorWatchesArray.Remove(_name);
                    Console.WriteLine("---===--- Watch: " + _name + " TIME: " + minutes + " min., " + seconds + " sec.");
                }
            }
        }

        static void Main(string[] args)
        {
            int WORDS_WITH_SYNONYMS = 0;
            int WORDS_ALL = 0;
            string path = "origin.txt";
            Console.WriteLine("Четние файла: " + path);
            string[] DevastorOriginalPhrasesList = File.ReadAllLines(path);
            DevastorWatch(true, "Рассказ");
            foreach (var DevastorOriginalPhraseElement in DevastorOriginalPhrasesList)
            {
                DevastorWatch(true, "Фраза");
                Console.WriteLine();
                foreach (var word in DevastorOriginalPhraseElement.Split(null))
                {
                    WORDS_ALL++;
                    string input = word;
                    string lang = "en";
                    string url = String.Format("https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}", "ru", lang, Uri.EscapeDataString(input));
                    WebClient wc = new WebClient();
                    string result = wc.DownloadString(url);
                    JArray json = JArray.Parse(result);
                    //Console.WriteLine("JSON ru to eng: " + json);
                    string translated_word = json[0][0][0].ToString();
                    wc.Dispose();
                    DevastorOriginalPhrases.Add(word);
                    string[] synonyms = new GroupDocs.Search.Index().Dictionaries.SynonymDictionary.GetSynonyms(translated_word);
                    //Console.WriteLine("synonyms.Length: " + synonyms.Length);
                    var Random = new Random();
                    int RAND_INT_BETWEEN_0_AND_MAX_SYNONYMS = Random.Next(0, synonyms.Length * MULTI_RANDOMIZE);
                    string randow_synonym = null;
                    string synonym_word = null;
                    try
                    {
                        randow_synonym = synonyms[RAND_INT_BETWEEN_0_AND_MAX_SYNONYMS];
                        input = randow_synonym;
                        lang = "ru";
                        url = String.Format("https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}", "en", lang, Uri.EscapeDataString(input));
                        wc = new WebClient();
                        result = wc.DownloadString(url);
                        json = JArray.Parse(result);
                        //Console.WriteLine("JSON eng to ru : " + json);
                        synonym_word = json[0][0][0].ToString();
                        wc.Dispose();
                        WORDS_WITH_SYNONYMS++;
                    }
                    catch
                    {
                        synonym_word = word;
                    }
                    DevastorSynonimyzedPhrases.Add(synonym_word);// RAND_INT_BETWEEN_0_AND_MAX_SYNONYMS]);
                    if (LOG_SYNONYM)
                    {
                        Console.Write(word);
                        Console.Write("(" + translated_word + ")");
                        Console.Write("<" + synonym_word + ">");
                        Console.Write(" ");
                    }
                }
                /*
                string[] synonyms = new GroupDocs.Search.Index().Dictionaries.SynonymDictionary.GetSynonyms(DevastorOriginalPhraseElement);
                var Random = new Random();
                int RAND_INT_BETWEEN_0_AND_10_INCLUDE_ALL_ = Random.Next(0, 11);
                */
                //Console.WriteLine("Фраза: " + DevastorOriginalPhraseElement);

                DevastorWatch(false, "Фраза");
            }
            DevastorWatch(false, "Рассказ");            
            Console.WriteLine("всего слов:" + WORDS_ALL);
            Console.WriteLine("надено синонимов: " + WORDS_WITH_SYNONYMS + " ( " + Math.Round(Convert.ToDecimal(WORDS_WITH_SYNONYMS) / WORDS_ALL, 2)+  "% )");
            //await File.WriteAllLinesAsync("WriteLines.txt", lines);
            Console.WriteLine();
            Console.WriteLine("ОРИГИНАЛ: ");
            Console.WriteLine();
            foreach (var _origin_phrase in DevastorOriginalPhrases)
            {
                Console.Write(_origin_phrase);
                Console.Write(" ");
            }
            Console.WriteLine();
            Console.WriteLine("ИЗЛОЖЕНИЕ: ");
            Console.WriteLine();
            foreach (var _synonym_phrase in DevastorSynonimyzedPhrases)
            {
                Console.Write(_synonym_phrase);
                Console.Write(" ");
            }
        }
    }
}
