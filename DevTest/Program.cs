using System;
using Wolfen.DevTestMain.Threads;

namespace Wolfen.DevTestMain
{
    public class Program
    {
        #region Members

        public static void Main(string[] args)
        {
            Console.WriteLine("Starting..");
            StreamCommunicatorThread dataSource = new StreamCommunicatorThread(500000);
            dataSource.Start();

            DataAnalysisThread worker = new DataAnalysisThread(dataSource);
            worker.Start();

            RenderThread renderTool = new RenderThread(worker);
            renderTool.Start();

        }

        #endregion
    }
}