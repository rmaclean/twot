using System.Text;
using System.IO;
// originally from https://stackoverflow.com/a/1187568/53236

using System;
using System.Collections.Generic;
using System.Threading;

namespace twot
{
    class ThreadedLogger : IDisposable
    {
        Queue<Action> queue = new Queue<Action>();
        ManualResetEvent hasNewItems = new ManualResetEvent(false);
        ManualResetEvent terminate = new ManualResetEvent(false);
        ManualResetEvent waiting = new ManualResetEvent(false);
        Thread? loggingThread;
        FileStream? fileStream;
        bool enabled;
        readonly byte[] newLine = Encoding.UTF8.GetBytes(Environment.NewLine);

        public ThreadedLogger(string filename, bool enabled)
        {
            this.enabled = enabled;
            if (enabled)
            {
                fileStream = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.Read);
                loggingThread = new Thread(new ThreadStart(ProcessQueue));
                loggingThread.IsBackground = true;
                loggingThread.Start();
            }
        }

        void ProcessQueue()
        {
            while (true)
            {
                waiting.Set();
                int i = ManualResetEvent.WaitAny(new WaitHandle[] { hasNewItems, terminate });
                // terminate was signaled
                if (i == 1) return;
                hasNewItems.Reset();
                waiting.Reset();

                Queue<Action> queueCopy;
                lock (queue)
                {
                    queueCopy = new Queue<Action>(queue);
                    queue.Clear();
                }

                foreach (var log in queueCopy)
                {
                    log();
                }
            }
        }

        private void WriteLogMessage(string row)
        {
            fileStream!.Write(Encoding.UTF8.GetBytes(row));
            if (!row.EndsWith(Environment.NewLine)) {
                fileStream.Write(newLine);
            }
        }

        public void LogMessage(string row)
        {
            if (!enabled)
            {
                return;
            }

            lock (queue)
            {
                queue.Enqueue(() => WriteLogMessage(row));
            }
            hasNewItems.Set();
        }

        public void Flush()
        {
            waiting.WaitOne();
        }

        public void Dispose()
        {
            terminate.Set();
            loggingThread?.Join();
            fileStream?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
