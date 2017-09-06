using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using DevTest;

namespace Wolfen.DevTestMain.Threads
{
    public class StreamCommunicatorThread
    {
        #region Internals

        private readonly int _charsToRead = 5000;

        private readonly Thread _internalThread;
        private readonly int _totalKiloBytesToProcess;


        /// <summary>
        ///     Gets the data cache.
        /// </summary>
        /// <value>
        ///     The data cache.
        /// </value>
        public ConcurrentQueue<char> DataCache { get; } = new ConcurrentQueue<char>();

        #endregion

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="StreamCommunicatorThread" /> class.
        /// </summary>
        /// <param name="totalKiloBytesToProcess">The total kilo bytes to process.</param>
        public StreamCommunicatorThread(int totalKiloBytesToProcess)
        {
            _totalKiloBytesToProcess = totalKiloBytesToProcess;
            _internalThread = new Thread(ReadFromStream);
        }

        #endregion

        #region Members

        /// <summary>
        ///     Reads from stream.
        /// </summary>
        private void ReadFromStream()
        {
            LorumIpsumStream stream = new LorumIpsumStream(_totalKiloBytesToProcess);
            TextReader tr = new StreamReader(stream, Encoding.Unicode);
            char[] buffer = new char[_charsToRead];
            int bytesRead = 1; // prime the loop
            while (stream.CanRead && bytesRead > 0)
            {
                while (DataCache.Count > _charsToRead * 20) Thread.Sleep(100);
                bytesRead = tr.Read(buffer, 0, _charsToRead);
                for (int i = 0; i < bytesRead; i++)
                    DataCache.Enqueue(buffer[i]);
            }
            DataCache.Enqueue(' '); // send a final space to finish off the last word
            IsComplete = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is complete. (All data has been read from the stream)
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is complete; otherwise, <c>false</c>.
        /// </value>
        public bool IsComplete { get; set; }

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