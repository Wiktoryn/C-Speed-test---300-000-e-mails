using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SpeedTest_NET
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Run(args.Length != 0 && args[0].ToLower().Contains("original"));
        }

        Random random = new Random();

        // Generate a random string with a given size  
        // and place commas about every 12th position.
        // Note: This function is not optimized as its
        // not including in the benchmark timing
        private string RandomString(int size)
        {
            var buf = new byte[size];
            byte ch;
            int commaLocation = 0;
            bool PlaceComma = false;
            for (int i = 0; i < size; i++)
            {
                // If we're at the 12th char, get
                // a random number from 0-4 as a place
                // to stick a comma instead of
                // a character
                if (i != 0 && i % 12 == 0)
                {
                    commaLocation = random.Next(0, 4) + i;
                    PlaceComma = true;
                }
                // Get a random ASCII character
                // and stick it in the byte array unless
                // its time to add a comma
                ch = Convert.ToByte(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                if (PlaceComma && i == commaLocation)
                {
                    buf[i] = Convert.ToByte(',');
                    PlaceComma = false;
                }
                else
                {
                    buf[i] = ch;
                }
            }
            // return a string
            return Encoding.UTF8.GetString(buf, 0, buf.Length);
        }

        // create dummy e-mail addresses, in format of XXXXXXXX@YYY.com and append , at the end
        private string RandomString2(int count)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < count; i++)
            {
                for (var j = 0; j < 12; j++)
                {
                    if (j == 8) sb.Append('@');
                    else sb.Append((char)(random.Next() % (122 - 97 + 1) + 97));
                }
                sb.Append(".com,");
            }
            return sb.ToString();
        }

        // For every comma in the string, replace 
        // it with a comma and linefeed
        public static string AddLineFeeds(string text)
        {
            var sb = new StringBuilder(text);
            sb.Replace(",", ",\n");
            return sb.ToString();
        }

        struct TimingSingleResult
        {
            public long Elapsed;
            public int StringLen;
            public int StringBytesLen;
        }


        private void ShowResults(List<TimingSingleResult> results)
        {
            Console.WriteLine("\n============== Results ==============");

            // Print the final benchmark results
            var ordered = results.OrderBy(t => t.Elapsed).ToArray();

            Console.WriteLine($"Shortest time: {ordered[0].Elapsed} ms for string of size {ordered[0].StringLen} chars ({ordered[0].StringBytesLen} bytes)");
            Console.WriteLine($"Longest time: {ordered[ordered.Length - 1].Elapsed} ms for string of size {ordered[ordered.Length - 1].StringLen} chars ({ordered[ordered.Length - 1].StringBytesLen} bytes)");
            Console.WriteLine($"Average time: {results.Average(t => t.Elapsed)} ms");

            Console.WriteLine("============== Done ==============");
        }
        private void RunForStringsAndMeasure(List<string> strings)
        {
            var stopWatch = new Stopwatch();

            // repeats per one string
            var repeats = 10;

            // let us not time JITting of the method...
            // mybe we should, but still
            Console.Write($"Warmup...");
            stopWatch.Restart();
            _ = AddLineFeeds(strings[0]);
            stopWatch.Stop();
            Console.WriteLine($"done, took {stopWatch.ElapsedMilliseconds} ms");

            Console.WriteLine($"Timing, {repeats} repetitions per string...");

            // we store each execution time, and will print out slowest, fastest and average
            var times = new List<TimingSingleResult>(strings.Count * repeats);

            for (var j = 0; j < strings.Count; j++)
            {
                // contrary to the name, List is actually implemented as an array under the hood
                // so indexing is of the same speed as with arrays.
                // why List is actually an array.. ask MS :)
                var buffer = strings[j];

                Console.CursorLeft = 0;
                Console.Write($"Run against string #{j}");

                stopWatch.Restart();
                for (var i = 0; i < 10; i++)
                {
                    _ = AddLineFeeds(buffer);
                }
                stopWatch.Stop();

                times.Add(new TimingSingleResult { Elapsed = stopWatch.ElapsedMilliseconds, StringLen = buffer.Length, StringBytesLen = Encoding.UTF8.GetBytes(buffer).Length });
            }
            ShowResults(times);
        }

        private List<string> GenerateRandomStrings()
        {
            var strings = new List<string>();
            Console.Write("Generating source data...");
            for (var i = 0; i < 100; i++)
            {
                var len = 4000000 + random.Next(0, 4000000);
                var buffer = RandomString(len);
                strings.Add(buffer);
            }
            Console.WriteLine($"done, {strings.Count} random lenght strings generated.");
            return strings;
        }
        private List<string> GenerateSameLenMailStrings(int count)
        {
            var strings = new List<string>();
            Console.Write("Generating source data...");
            for (var i = 0; i < 100; i++)
            {
                var buffer = RandomString2(count);
                strings.Add(buffer);
            }
            Console.WriteLine($"done, {strings.Count} same lenght strings (random e-mail addresses) generated.");
            return strings;
        }

        private void Run(bool useOriginalCode)
        {
            // generate random strings and perform tests
            var strings = GenerateRandomStrings();

            RunForStringsAndMeasure(strings);

            // generate e-mail strings (300,000 of them) and perform tests
            strings = GenerateSameLenMailStrings(300000);

            RunForStringsAndMeasure(strings);
        }
    }
}
