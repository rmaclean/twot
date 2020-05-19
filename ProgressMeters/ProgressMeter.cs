using System.Runtime.InteropServices;
namespace twot
{
    using System;
    using System.Threading;

    abstract class ProgressMeter : IDisposable
    {
        protected int cursorLine;
        readonly ConsoleColor originalColour;
        readonly bool originalCursorVisible;
        readonly Timer? timer;
        internal AutoResetEvent? doneEvent;

        protected ProgressMeter(int updateSpeedMs)
        {
            if (Console.IsOutputRedirected)
            {
                return;
            }

            cursorLine = Console.CursorTop + 1;
            if (cursorLine + 2 >= Console.WindowHeight)
            {
                Console.WriteLine();
                cursorLine -= 2;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                originalCursorVisible = Console.CursorVisible;
                Console.CursorVisible = false;
            }

            originalColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;

            doneEvent = new AutoResetEvent(false);
            timer = new Timer(UpdateUI, doneEvent, 0, updateSpeedMs);
        }

        internal void Write(string message, int offset = 0)
        {
            var target = cursorLine + offset;
            if (target >= Console.BufferHeight)
            {
                target = Console.BufferHeight - 1 - Math.Min(offset - 1, 1 - offset);
                cursorLine = target - offset;
            }

            Console.CursorTop = target;

            Console.CursorLeft = 0;
            Console.Write(message);
        }

        internal abstract void UpdateUI(object? state);

        private Boolean disposed;

        public void Dispose()
        {
            if (disposed || Console.IsOutputRedirected)
            {
                return;
            }

            doneEvent?.WaitOne();
            doneEvent?.Dispose();
            timer?.Dispose();
            Console.WriteLine();
            Console.ForegroundColor = originalColour;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.CursorVisible = originalCursorVisible;
            }

            disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}
