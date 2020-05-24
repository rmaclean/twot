namespace twot
{
    using System;

    internal class Spinner : ProgressMeter, IDisposable
    {
        private readonly string[] steps = new[] { "|", "/", "-", "\\" };

        private int step;

        public Spinner()
            : base(200)
        {
            // no-op
        }

        internal override void UpdateUI(object? state)
        {
            this.Write(this.steps[this.step]);
            this.step++;
            if (this.step >= this.steps.Length)
            {
                this.step = 0;
            }
        }

        internal void Done()
        {
            this.DoneEvent?.Set();
        }
    }
}
