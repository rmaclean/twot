namespace twot
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal abstract class ProgressMeter : IDisposable
    {
        private static object writeLocker = new object();

        private readonly ConsoleColor originalColour;
        private readonly bool originalCursorVisible;
        private readonly Timer? timer;
        private bool disposed;

        protected ProgressMeter(int updateSpeedMs)
        {
            if (Console.IsOutputRedirected)
            {
                return;
            }

            this.cursorLine = Console.CursorTop + 1;
            if (this.cursorLine + 2 >= Console.WindowHeight)
            {
                Console.WriteLine();
                this.cursorLine -= 2;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                this.originalCursorVisible = Console.CursorVisible;
                Console.CursorVisible = false;
            }

            this.originalColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;

            this.DoneEvent = new AutoResetEvent(false);
            this.timer = new Timer(this.UpdateUI, this.DoneEvent, 0, updateSpeedMs);
        }

        protected AutoResetEvent? DoneEvent { get; private set; }

        protected int cursorLine { get; private set; }

        public void Dispose()
        {
            if (this.disposed || Console.IsOutputRedirected)
            {
                return;
            }

            this.DoneEvent?.WaitOne();
            this.DoneEvent?.Dispose();
            this.timer?.Dispose();
            Console.WriteLine();
            Console.ForegroundColor = this.originalColour;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.CursorVisible = this.originalCursorVisible;
            }

            this.disposed = true;

            GC.SuppressFinalize(this);
        }

        internal void Write(string message, int verticalOffset = 0, int horizontalOffset = 0)
        {
            lock (writeLocker)
            {
                var target = this.cursorLine + verticalOffset;
                if (target >= Console.BufferHeight)
                {
                    target = Console.BufferHeight - 1 - Math.Min(verticalOffset - 1, 1 - verticalOffset);
                    this.cursorLine = target - verticalOffset;
                }

                Console.CursorTop = target;

                Console.CursorLeft = horizontalOffset;
                Console.Write(message);
            }
        }

        internal abstract void UpdateUI(object? state);
    }
}
