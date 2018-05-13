using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace Plagiat
{
    class Program
    {
        public static List<string> matchedSentences = new List<string>();
        public static int numberOfWords = 5;
        public static int[] searchGroups = { 50, 20, 8, 5 };

        public static void Main(string[] args)
        {
            List<string> workSentences;
            string[] workWords;
            string wholeWorkString;

            if(File.Exists(@"WorkFile.txt"))
            {
                (workSentences, workWords, wholeWorkString) = TxtReader.LoadSentences(@"WorkFile.txt", numberOfWords);

                var startupPath = @"source\";

                if(Directory.Exists(startupPath))
                {
                    // This path is a directory
                    ProcessDirectory(startupPath, workSentences);
                }
                else
                {
                    Console.WriteLine("{0} is not a valid path.", startupPath);
                }

                WriteWordsCountWithPercents(workWords);
                WriteBadDateSpaces(wholeWorkString);
                WriteAllQuotes(wholeWorkString);

                StringBuilder builder = new StringBuilder();
                foreach(var word in workWords)
                {
                    builder.Append(word + " ");
                }

                HtmlGenerator generator = new HtmlGenerator();
                generator.AddText(builder.ToString());
                generator.AddMaches(matchedSentences, searchGroups);
                var html = generator.GenerateHtml();
                using(System.IO.StreamWriter file = new System.IO.StreamWriter(@"Output.html", false))
                {
                    file.Write(html);
                }
            }
            else
            {
                Console.WriteLine("WorkFile.txt not found");
            }

            Console.ReadKey();
        }

        private static void WriteWordsCountWithPercents(string[] workWords)
        {
            StringBuilder builder = new StringBuilder();
            foreach(var word in workWords)
            {
                builder.Append(word + " ");
            }

            var normalizedStr = builder.ToString();

            int wholeLength = normalizedStr.Split(' ').Length;
            Console.WriteLine("Words in WorkFile.txt: {0}", wholeLength);
            Console.WriteLine();
            Console.WriteLine("Plagiarism groups:");

            var lastlength = wholeLength;
            foreach(var i in searchGroups)
            {
                var matches = 0;
                (matches, normalizedStr) = CalculateMatchCount(i, normalizedStr);
                var currentLength = normalizedStr.Split(' ').Length;
                WritePercentMatchString(i, wholeLength, matches, wholeLength - currentLength, lastlength - currentLength);
                lastlength = currentLength;
            }
            Console.WriteLine();

        }

        public static void WritePercentMatchString(int number, int workLength, int matches, int length, int selfLength)
        {
            Console.WriteLine("{1} word repetitions: {0} -- {2}% ({3}%)",
                matches,
                number,
                (float)length * 100 / workLength,
                (float)selfLength * 100 / workLength);
        }

        public static (int matches, string normalizedStr) CalculateMatchCount(int number, string normalizedStr)
        {
            int matchesCount = 0;

            var distincteSentences = matchedSentences.Distinct();

            foreach(var match in distincteSentences.OrderByDescending(x => x.Split(' ').Length))
            {
                int words = match.Split(' ').Length;
                if(words >= number)
                {
                    matchesCount += Regex.Matches(normalizedStr, match).Count;
                    normalizedStr = normalizedStr.Replace(match, "");
                }
            }
            return (matchesCount, normalizedStr);
        }

        private static void WriteBadDateSpaces(string str)
        {
            (var badDates, var goodDates) = TxtReader.FindBadDateSpaces(str);
            Console.WriteLine();
            Console.WriteLine("Number of bad date descriptions {0} out of {1}", badDates.Count, goodDates.Count + badDates.Count);
            foreach(Match match in badDates)
            {
                Console.WriteLine(match.ToString());
            }
            Console.WriteLine();
        }

        private static void WriteAllQuotes(string str)
        {
            (MatchCollection matches, int wordCount, int wholeStringWordCount) = TxtReader.CheckQuotes(str);
            Console.WriteLine();
            Console.WriteLine("Number of quotes: {0} -- {1}%", matches.Count, (wordCount * 100) / wholeStringWordCount);
            int i = 0;
            foreach(Match match in matches)
            {
                i++;
                Console.WriteLine("Quote {0}: {1}", i, match);
            }
            Console.WriteLine();
        }

        public static void ProcessDirectory(string targetDirectory, List<string> sourceSentences)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory, "*.txt");
            string[] pdfEntries = Directory.GetFiles(targetDirectory, "*.pdf");
            foreach(string pdfName in pdfEntries)
                if(fileEntries.All(x => x.Replace(".txt", "") != pdfName.Replace(".pdf", "")))
                    TxtReader.ExtractTextFromPdfToTxt(pdfName, targetDirectory);

            fileEntries = Directory.GetFiles(targetDirectory, "*.txt");
            Console.WriteLine("Number of loaded sources: " + fileEntries.Length);
            Console.WriteLine();
            foreach(var fileName in fileEntries)
            {
                ProcessFile(fileName, sourceSentences);
            }
        }

        public static void ProcessFile(string path, List<string> sourceSentences)
        {
            // load source
            (List<string> sentences, string[] wordArray, string str) = TxtReader.LoadSentences(path, numberOfWords);

            //print filename
            Console.WriteLine(path);
            Console.WriteLine();

            //find all sentences
            var matches = sourceSentences.Intersect(sentences);
            matches = TxtReader.CombineFollowingSencences(matches.ToList(), numberOfWords);

            WriteFileMatches(matches.ToList());

            foreach(var match in matches)
            {
                matchedSentences.Add(match);
            }
        }

        public static void WriteFileMatches(List<string> matches)
        {
            int i = 0;
            foreach(var match in matches)
            {
                Console.WriteLine(i + "  -- (" + match.Split(' ').Length + ") --  " + match);
                i++;
            }

            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
