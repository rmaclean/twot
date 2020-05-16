namespace twot
{
    using System;
    using System.Threading;

    abstract class ProgressMeter
    {
        int cursorLine;
        ConsoleColor originalColour;
        bool originalCursorVisible;
        Timer? timer;
        internal AutoResetEvent? doneEvent;

        internal ProgressMeter(int updateSpeedMs)
        {
            if (Console.IsOutputRedirected) {
                return;
            }

            cursorLine = Console.CursorTop + 1;
            if (cursorLine + 2 >= Console.WindowHeight)
            {
                Console.WriteLine();
                cursorLine -= 2;
            }

            originalColour = Console.ForegroundColor;
            originalCursorVisible = Console.CursorVisible;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.CursorVisible = false;

            doneEvent = new AutoResetEvent(false);
            timer = new Timer(UpdateUI, doneEvent, 0, updateSpeedMs);
        }

        internal void Write(string message, int offset = 0)
        {
            var target = cursorLine + offset;
            if (target >= Console.BufferHeight) {
                target = Console.BufferHeight - 1 - Math.Min(offset - 1, 1 - offset);
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

            timer?.Dispose();
            Console.WriteLine();
            Console.ForegroundColor = originalColour;
            Console.CursorVisible = originalCursorVisible;

            disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}
