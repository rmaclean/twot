namespace twot
{
    using System;
    using System.Threading;

    class ProgressBar : ProgressMeter
    {
        readonly int steps;
        double done;
        string message = "";
        readonly char block = '█';
        readonly char background = '░';
        int lastCursorPosition = 0;
        int lastDraw = -1;

        public ProgressBar(int steps) : base(60)
        {
            this.steps = steps;
        }

        internal override void UpdateUI(object? state)
        {
            var percentage = done / steps;
            var draw = (int)Math.Floor(percentage * Console.WindowWidth);
            if (lastCursorPosition != cursorLine)
            {
                lastCursorPosition = cursorLine;
                Write("".PadRight(Console.WindowWidth - 1, background));
            }

            if (draw != lastDraw)
            {
                lastDraw = draw;
                Write("".PadRight(draw, block));
            }

            Write("".PadRight(Console.WindowWidth - 1, ' '), 1);
            Write($"{percentage:P} {message}", 1);

            if (percentage >= 1)
            {
                doneEvent?.Set();
            }
        }

        public void Tick(string message)
        {
            this.done++;
            this.message = message;
        }
    }
}
