namespace twot
{
    using System;
    using System.Threading;

    class ProgressBar : IDisposable
    {
        Thread? uiThread;
        int steps;
        double done;
        string message = "";
        int cursorLine;
        char block = '█';
        char background = '░';
        ConsoleColor originalColour;
        bool originalCursorVisible;
        double percentage;

        public ProgressBar(int steps)
        {
            this.steps = steps;

            if (Console.IsOutputRedirected) {
                return;
            }

            cursorLine = Console.CursorTop + 1;
            if (cursorLine + 2 >= Console.WindowHeight)
            {
                cursorLine -= 2;
            }

            originalColour = Console.ForegroundColor;
            originalCursorVisible = Console.CursorVisible;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.CursorVisible = false;

            uiThread = new Thread(new ThreadStart(UpdateUI));
            uiThread.IsBackground = true;
            uiThread.Start();
        }

        void UpdateUI()
        {
            Thread.Sleep(33);

            Console.CursorTop = cursorLine;
            Console.CursorLeft = 0;
            var draw = (int)Math.Floor(percentage * Console.WindowWidth);
            Console.Write("".PadRight(Console.WindowWidth - 1, background));
            Console.CursorLeft = 0;
            Console.Write("".PadRight(draw, block));

            Console.CursorTop = cursorLine + 1;
            Console.CursorLeft = 0;
            Console.Write("".PadRight(Console.WindowWidth -1, ' '));
            Console.CursorLeft = 0;
            Console.Write($"{percentage:P} {message}");

            if (percentage < 1)
            {

                UpdateUI();
            }
        }

        public void Tick(string message)
        {
            this.done++;
            this.percentage = done / steps;
            this.message = message;
        }

        private Boolean disposed;

        public void Dispose()
        {
            if (disposed || Console.IsOutputRedirected)
            {
                return;
            }

            uiThread?.Join();
            Console.WriteLine();
            Console.ForegroundColor = originalColour;
            Console.CursorVisible = originalCursorVisible;

            disposed = true;

            GC.SuppressFinalize(this);
        }
    }
}
