namespace twot
{
    using System;
    using System.Threading;

    internal class ProgressBar : ProgressMeter
    {
        private readonly int steps;
        private readonly char block = '█';
        private readonly char background = '░';
        private int done;
        private string message = string.Empty;
        private int lastCursorPosition;
        private int lastDraw = -1;

        public ProgressBar(int steps)
            : base(60)
        {
            this.steps = steps;
        }

        public void Tick(string message)
        {
            Interlocked.Increment(ref this.done);
            this.message = message;
        }

        internal override void UpdateUI(object? state)
        {
            var percentage = (double)this.done / this.steps;
            var draw = (int)Math.Floor(percentage * Console.WindowWidth);
            if (this.lastCursorPosition != this.cursorLine)
            {
                this.lastCursorPosition = this.cursorLine;
                this.Write(string.Empty.PadRight(Console.WindowWidth - 1, this.background));
            }

            if (draw != this.lastDraw)
            {
                this.lastDraw = draw;
                this.Write(string.Empty.PadRight(draw, this.block));
            }

            this.Write(string.Empty.PadRight(Console.WindowWidth - 1, ' '), 1);
            this.Write($"{percentage:P} {this.message}", 1);

            if (percentage >= 1)
            {
                this.DoneEvent?.Set();
            }
        }
    }
}
