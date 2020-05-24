// originally from https://stackoverflow.com/a/1187568/53236

namespace twot
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;

    internal class ThreadedLogger : IDisposable
    {
        private readonly Queue<Action> queue = new Queue<Action>();
        private readonly ManualResetEvent hasNewItems = new ManualResetEvent(false);
        private readonly ManualResetEvent terminate = new ManualResetEvent(false);
        private readonly ManualResetEvent waiting = new ManualResetEvent(false);
        private readonly Thread? loggingThread;
        private readonly FileStream? fileStream;
        private readonly bool enabled;
        private readonly byte[] newLine = Encoding.UTF8.GetBytes(Environment.NewLine);

        public ThreadedLogger(string filename, bool enabled)
        {
            this.enabled = enabled;
            if (this.enabled)
            {
                this.fileStream = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.Read);
                this.loggingThread = new Thread(new ThreadStart(this.ProcessQueue));
                this.loggingThread.IsBackground = true;
                this.loggingThread.Start();
            }
        }

        public void LogMessage(string row)
        {
            if (!this.enabled)
            {
                return;
            }

            lock (this.queue)
            {
                this.queue.Enqueue(() => this.WriteLogMessage(row));
            }

            this.hasNewItems.Set();
        }

        public void Flush()
        {
            this.waiting.WaitOne();
        }

        public void Dispose()
        {
            this.terminate.Set();
            this.terminate.Dispose();
            this.waiting.Dispose();
            this.hasNewItems.Dispose();
            this.loggingThread?.Join();
            this.fileStream?.Dispose();
            GC.SuppressFinalize(this);
        }

        private void ProcessQueue()
        {
            while (true)
            {
                this.waiting.Set();
                var i = ManualResetEvent.WaitAny(new WaitHandle[] { this.hasNewItems, this.terminate });

                if (i == 1)
                {
                    return;
                }

                this.hasNewItems.Reset();
                this.waiting.Reset();

                Queue<Action> queueCopy;
                lock (this.queue)
                {
                    queueCopy = new Queue<Action>(this.queue);
                    this.queue.Clear();
                }

                foreach (var log in queueCopy)
                {
                    log();
                }
            }
        }

        private void WriteLogMessage(string row)
        {
            this.fileStream!.Write(Encoding.UTF8.GetBytes(row));
            if (!row.EndsWith(Environment.NewLine, StringComparison.InvariantCultureIgnoreCase))
            {
                this.fileStream.Write(this.newLine);
            }
        }
    }
}
