using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Wolfen.DevTestMain.Threads
{
    public class RenderThread
    {
        #region Internals

        private readonly Thread _internalThread;
        private readonly DataAnalysisThread _worker;

        #endregion

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="RenderThread" /> class.
        /// </summary>
        /// <param name="worker">The worker.</param>
        public RenderThread(DataAnalysisThread worker)
        {
            _worker = worker;
            _internalThread = new Thread(RenderData);
        }

        #endregion

        #region Members

        /// <summary>
        ///     Renders the character frequency.
        /// </summary>
        /// <param name="top">The top.</param>
        /// <param name="left">The left.</param>
        private void RenderCharFrequency(int top, int left)
        {
            Console.CursorTop = top;
            Console.CursorLeft = left;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("CHARACTER FREQUENCY (desc order, left to right, by frequency):");
            Console.ForegroundColor = ConsoleColor.Gray;
            int l = 0;
            foreach (char item in _worker.CharFrequency.OrderByDescending(x => x.Value).Select(x=>x.Key))
            {
                l++;
                ulong val = 0;
                _worker.CharFrequency.TryGetValue(item, out val);
                Console.Write($"[{item}]:{val}\t");
                if (l % 7 == 0) // 7 cols of data
                {
                    Console.WriteLine();
                    Console.CursorLeft = left;
                }
            }
        }

        /// <summary>
        /// Renders the data.
        /// </summary>
        private void RenderData()
        {
            int i = 0;
            while (!_worker.IsComplete)
                try
                {
                    i++;

                    RenderAllStats();


                    Thread.Sleep(100);
                    if (i % 50 == 0)
                       Console.Clear(); // clear artifacts without flickering too much
                }
                catch
                {
                    // suppress
                }
            // one last render to clean up the totals
            Console.Clear();
            RenderAllStats();

            // wait for exit
            Console.WriteLine();
            Console.WriteLine("ALL DATA PROCESSED. PRESS [ENTER] to exit");
            Console.ReadLine();
        }

        /// <summary>
        /// Renders all stats.
        /// </summary>
        private void RenderAllStats()
        {
            // display stats 
            RenderMostFrequentWords(0, 0);
            RenderTotals();
            RenderSmallestWords(0, 40);
            RenderLargestWords(0, 75);
            RenderCharFrequency(19, 0);
        }

        /// <summary>
        ///     Renders the largest words.
        /// </summary>
        /// <param name="top">The top.</param>
        /// <param name="left">The left.</param>
        private void RenderLargestWords(int top, int left)
        {
            Console.CursorTop = top;
            Console.CursorLeft = left;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("5 LARGEST WORDS:");
            Console.ForegroundColor = ConsoleColor.Gray;
            IEnumerable<KeyValuePair<string, ulong>> toPrintBig = _worker.WordFrequency
                .OrderByDescending(x => x.Key.Length)
                .ThenByDescending(x => x.Key).Take(5);
            int k = 0;
            foreach (KeyValuePair<string, ulong> item in toPrintBig)
            {
                Console.CursorLeft = left;
                Console.WriteLine($"#{++k} - {item.Key} ({item.Value} times)        ");
            }
        }

        /// <summary>
        ///     Renders the most frequent words.
        /// </summary>
        /// <param name="top">The top.</param>
        /// <param name="left">The left.</param>
        private void RenderMostFrequentWords(int top, int left)
        {
            Console.CursorTop = top;
            Console.CursorLeft = left;
            IEnumerable<KeyValuePair<string, ulong>> toPrint = _worker.WordFrequency
                .OrderByDescending(x => x.Value)
                .Take(10);
            int i = 0;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("10 MOST FREQUENT WORDS:");
            Console.ForegroundColor = ConsoleColor.Gray;
            foreach (KeyValuePair<string, ulong> item in toPrint)
            {
                Console.WriteLine($"#{++i} - {item.Key} ({item.Value} times)        ");
                Console.CursorLeft = left;
            }
            Console.WriteLine();
        }

        /// <summary>
        ///     Renders the smallest words.
        /// </summary>
        /// <param name="top">The top.</param>
        /// <param name="left">The left.</param>
        private void RenderSmallestWords(int top, int left)
        {
            Console.CursorTop = top;
            Console.CursorLeft = left;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("5 SMALLEST WORDS:");
            Console.ForegroundColor = ConsoleColor.Gray;
            IEnumerable<KeyValuePair<string, ulong>> toPrintSmall = _worker.WordFrequency
                .OrderBy(x => x.Key.Length)
                .ThenBy(x => x.Key)
                .Take(5);
            int j = 0;
            foreach (KeyValuePair<string, ulong> item in toPrintSmall)
            {
                Console.CursorLeft = left;
                Console.WriteLine($"#{++j} - {item.Key} ({item.Value} times)        ");
            }
        }

        /// <summary>
        ///     Renders the totals.
        /// </summary>
        private void RenderTotals()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("TOTALS:");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"WORDS:        {_worker.TotalWordsProcessed}");
            Console.WriteLine($"UNIQUE WORDS: {_worker.WordFrequency.Count}");
            Console.WriteLine($"CHARS:        {_worker.TotalCharsProcessed}");
            Console.WriteLine($"UNIQUE CHARS: {_worker.CharFrequency.Count}");
        }

        /// <summary>
        ///     Starts this instance.
        /// </summary>
        public void Start()
        {
            _internalThread.Start();
        }

        #endregion
    }
}