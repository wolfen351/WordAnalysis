using System.Collections.Concurrent;
using System.Threading;

namespace Wolfen.DevTestMain.Threads
{
    public class DataAnalysisThread
    {
        #region Internals

        private readonly ConcurrentQueue<char> _dataCache;
        private readonly Thread _internalThread;
        private StreamCommunicatorThread _dataLoader;

        /// <summary>
        ///     Gets the character frequency.
        /// </summary>
        /// <value>
        ///     The character frequency.
        /// </value>
        public ConcurrentDictionary<char, ulong> CharFrequency { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is complete.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is complete; otherwise, <c>false</c>.
        /// </value>
        public bool IsComplete { get; set; }

        /// <summary>
        ///     Gets the total chars processed.
        /// </summary>
        /// <value>
        ///     The total chars processed.
        /// </value>
        public ulong TotalCharsProcessed { get; private set; }

        /// <summary>
        ///     Gets the total words processed.
        /// </summary>
        /// <value>
        ///     The total words processed.
        /// </value>
        public ulong TotalWordsProcessed { get; private set; }

        /// <summary>
        ///     Gets the word frequency.
        /// </summary>
        /// <value>
        ///     The word frequency.
        /// </value>
        public ConcurrentDictionary<string, ulong> WordFrequency { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataAnalysisThread" /> class.
        /// </summary>
        /// <param name="dataLoader">The data loader.</param>
        public DataAnalysisThread(StreamCommunicatorThread dataLoader)
        {
            _dataCache = dataLoader.DataCache;
            _dataLoader = dataLoader;
            _internalThread = new Thread(ProcessDataStream);
            WordFrequency = new ConcurrentDictionary<string, ulong>();
            CharFrequency = new ConcurrentDictionary<char, ulong>();
        }

        #endregion

        #region Members

        /// <summary>
        ///     Cleanups the word.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        private static string CleanupWord(string word)
        {
            string lowered = word.ToLowerInvariant();

            string cleaned = "";
            foreach (char c in lowered)
                if (c >= 'a' && c <= 'z' || c == '\'' || c == '-')
                    cleaned += c;
            return cleaned;
        }

        /// <summary>
        ///     Processes the data stream.
        /// </summary>
        private void ProcessDataStream()
        {
            string word = "";
            while (!_dataLoader.IsComplete || !_dataCache.IsEmpty)
            {
                char charToProcess;
                if (_dataCache.TryDequeue(out charToProcess))
                {
                    TotalCharsProcessed++;

                    //(end of word)
                    if (charToProcess == ' ' || charToProcess == '.')
                    {
                        // add 1 for the space
                        CharFrequency.TryAdd(charToProcess, 0); // make a space for the space
                        CharFrequency[charToProcess] += 1;

                        if (word.Length > 0)
                        {
                            // process the chars in the word
                            foreach (char c in word)
                            {
                                CharFrequency.TryAdd(c, 0);
                                CharFrequency[c] += 1;
                            }

                            // switch to lower case & remove junk chars 
                            word = CleanupWord(word);

                            // do word frequency analysis
                            ulong currentCount = WordFrequency.GetOrAdd(word, 0);
                            WordFrequency.TryUpdate(word, currentCount + 1, currentCount);

                            // do word count analysis
                            TotalWordsProcessed++;

                            // we are done, flush the word buffer
                            word = "";
                        }
                    }
                    else
                    {
                        // add this char to the current word
                        word += charToProcess;
                    }
                }
            }
            IsComplete = true;
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