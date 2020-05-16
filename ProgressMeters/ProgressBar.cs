namespace twot
{
    using System;
    using System.Threading;

    class ProgressBar : ProgressMeter, IDisposable
    {
        int steps;
        double done;
        string message = "";
        char block = '█';
        char background = '░';
        double percentage;

        public ProgressBar(int steps): base(33)
        {
            this.steps = steps;
        }

        internal override void UpdateUI(object? state)
        {
            var draw = (int)Math.Floor(percentage * Console.WindowWidth);
            Write("".PadRight(Console.WindowWidth - 1, background));
            Write("".PadRight(draw, block));

            Write("".PadRight(Console.WindowWidth -1, ' '), 1);
            Write($"{percentage:P} {message}", 1);

            if (percentage >= 1)
            {
                doneEvent?.Set();
            }
        }

        public void Tick(string message)
        {
            this.done++;
            this.percentage = done / steps;
            this.message = message;
        }
    }
}
