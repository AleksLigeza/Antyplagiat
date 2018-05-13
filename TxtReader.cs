using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Path = System.IO.Path;

namespace Plagiat
{
    public class TxtReader
    {
        public static (List<string> sentences, string[] wordArray, string str) LoadSentences(string path, int sencenteLenght)
        {
            var sentences = new List<string>();
            var str = LoadTxt(path);

            var str1 = NormalizeString(str);
            var wordArray = SplitString(str1);

            for(var i = 0; i < wordArray.Length - sencenteLenght; i++)
            {
                var sentence = wordArray[i];

                for(var j = 1; j < sencenteLenght; j++)
                {
                    sentence += " " + wordArray[i + j];
                }

                sentences.Add(sentence);
            }

            return (sentences, wordArray, str);
        }

        public static string NormalizeString(string str)
        {
            var pattern = new Regex(@"(?:[a-zA-Z])\d+");
            str = pattern.Replace(str, "");
            return StripPunctuation(str).ToLower();
        }

        public static string[] SplitString(string str)
        {
            return str.Split(new string[] { " ", "\t", "\n", "\r", "\\t", "\\n" },
                StringSplitOptions.RemoveEmptyEntries);
        }

        public static string LoadTxt(string path)
        {
            var sb = new StringBuilder();
            using(StreamReader sr = new StreamReader(path))
            {
                sb.AppendLine(sr.ReadToEnd());
            }

            return sb.ToString();
        }

        public static void ExtractTextFromPdfToTxt(string path, string directoryPath)
        {
            using(PdfReader reader = new PdfReader(path))
            {
                StringBuilder text = new StringBuilder();

                for(int i = 1; i <= reader.NumberOfPages; i++)
                {
                    text.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                }

                string fileName = directoryPath + Path.GetFileName(path) + ".txt";
                fileName = fileName.Replace(".pdf", "");

                if(!File.Exists(fileName))
                {
                    File.WriteAllText(fileName, text.ToString());
                }
            }
        }

        public static List<string> CombineFollowingSencences(List<string> sentences, int count)
        {
            var newList = new List<string>();

            for(var i = 0; i < sentences.Count; i++)
            {
                var current = sentences[i];
                while(i + 1 < sentences.Count)
                {
                    var next = sentences[i + 1];
                    var splited = current.Split(new char[] { ' ' });
                    var nextSplited = next.Split(new char[] { ' ' });

                    var matchTable = new bool[count - 1];
                    for(var j = count - 2; j >= 0; j--)
                    {
                        matchTable[j] = splited[j + 1 + splited.Length - count] == nextSplited[j];
                    }

                    if(matchTable.All(x => x == true))
                    {
                        current += " " + nextSplited[count - 1];
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }
                newList.Add(current);
            }

            return newList;
        }

        public static string StripPunctuation(string s)
        {
            var sb = new StringBuilder();
            foreach(char c in s)
            {
                if(!Char.IsPunctuation(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public static (MatchCollection badDates, MatchCollection goodDates) FindBadDateSpaces(string str)
        {
            var matches = Regex.Matches(str, @"[(]\d{4}[,][\d]*[)]");
            var matches1 = Regex.Matches(str, @"[(]\d{4}[,][ ][\d]*[)]");
            return (matches, matches1);
        }

        public static (MatchCollection matches, int wordCount, int wholeStringWordCount) CheckQuotes(string str)
        {
            MatchCollection matches = Regex.Matches(str, "“(.|\n)*?”");

            int quoteWordsCount = 0;
            foreach(Match match in matches)
            {
                var normalizedMatch = NormalizeString(match.ToString());
                var wordArray = SplitString(normalizedMatch);
                quoteWordsCount += wordArray.Length;
            }

            return (matches, quoteWordsCount, SplitString(str.Normalize()).Length);
        }
    }
}
